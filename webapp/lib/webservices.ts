import axios, { AxiosError } from 'axios';

/**
 * Paginated result wrapper returned by list endpoints.
 */
export interface Paged<T> {
  items: T[];
  total: number;
}

/**
 * Basic pagination + filter params sent as URL query string.
 */
export interface QueryRequest {
  limit?: number;
  pageIndex?: number;
  includeTotal?: boolean;
}

export interface MetaEnvelope {
  // Always populated by diagnostics filter
  timestamp?: string; // ISO string from backend (DateTimeOffset)
  requestId?: string;
  processingTimeMs?: number;

  // Optional pagination fields
  limit?: number;
  pageIndex?: number;
  total?: number;
  remaining?: number;

  custom?: Record<string, unknown>;
}

export interface ErrorInfo {
  code: number;
  message: string;
  details?: unknown;
}

export type ApiStatus = 'success' | 'error';

export interface ApiResponse<T> {
  status: ApiStatus;
  meta: MetaEnvelope;
  error?: ErrorInfo;
  data?: T;
}

// src/lib/caseConvert.ts

const toCamelKey = (key: string) => {
  // First convert snake_case to camelCase
  let result = key.replace(/_([a-z])/g, (_, c: string) => c.toUpperCase());
  // Then convert PascalCase to camelCase
  result = result.charAt(0).toLowerCase() + result.slice(1);
  return result;
};

const toSnakeKey = (key: string) =>
  key.replace(/[A-Z]/g, (c) => `_${c.toLowerCase()}`);

export function keysToCamel<T = any>(input: any): T {
  if (Array.isArray(input)) {
    return input.map((v) => keysToCamel(v)) as any;
  }
  if (input !== null && typeof input === 'object') {
    return Object.entries(input).reduce((acc, [k, v]) => {
      const newKey = toCamelKey(k);
      (acc as any)[newKey] = keysToCamel(v);
      return acc;
    }, {} as any);
  }
  return input;
}

export function keysToSnake<T = any>(input: any): T {
  if (Array.isArray(input)) {
    return input.map((v) => keysToSnake(v)) as any;
  }
  if (input !== null && typeof input === 'object') {
    return Object.entries(input).reduce((acc, [k, v]) => {
      const newKey = toSnakeKey(k);
      (acc as any)[newKey] = keysToSnake(v);
      return acc;
    }, {} as any);
  }
  return input;
}


export class Result<T, E> {
  protected constructor(
    private readonly _value: T | null,
    private readonly _error: E | null,
    public readonly isSuccess: boolean
  ) {}

  public get isFailure(): boolean {
    return !this.isSuccess;
  }

  public get value(): T {
    if (!this.isSuccess) {
      throw new Error('No value present on a failed Result');
    }
    return this._value as T;
  }

  public get error(): E {
    if (this.isSuccess) {
      throw new Error('No error present on a successful Result');
    }
    return this._error as E;
  }

  // C# Result<T,E>.Ok(...)
  public static ok<T, E>(value: T): Result<T, E> {
    return new Result<T, E>(value, null, true);
  }

  // C# Result<T,E>.Fail(...)
  public static fail<T, E>(error: E): Result<T, E> {
    return new Result<T, E>(null, error, false);
  }

  // AndThen: chain another Result-producing function if this is success
  public andThen<U>(f: (value: T) => Result<U, E>): Result<U, E> {
    return this.isSuccess ? f(this.value) : Result.fail<U, E>(this.error);
  }

  // OrElse: transform the error type if this is failure
  public orElse<F>(f: (error: E) => Result<T, F>): Result<T, F> {
    return this.isFailure ? f(this.error) : Result.ok<T, F>(this.value);
  }

  // Transform: map T -> U (keeps same error type)
  public transform<U>(f: (value: T) => U): Result<U, E> {
    return this.isSuccess
      ? Result.ok<U, E>(f(this.value))
      : Result.fail<U, E>(this.error);
  }

  public map<U>(f: (value: T) => U): Result<U, E> {
    return this.transform(f);
  }

  public flatMap<U>(f: (value: T) => Result<U, E>): Result<U, E> {
    return this.andThen(f);
  }
}

export type ApiErrorKind =
  | 'Network'
  | 'Timeout'
  | 'Server' // 5xx
  | 'Client' // 4xx
  | 'Canceled'
  | 'Unknown';

export type ApiError = WebError;

// Wrapper type: just a Result<T, ApiError>
export type ApiResult<T> = Result<T, ApiError>;

// ---------- ApiResult helper namespace ----------

export namespace ApiResult {
  // small helper: status -> kind
  function statusToKind(status?: number): ApiErrorKind {
    if (typeof status !== 'number') return 'Unknown';
    if (status >= 500) return 'Server';
    if (status >= 400) return 'Client';
    return 'Unknown';
  }

  function buildError(
    err?: Partial<WebError>,
    statusOverride?: number,
    fallbackMessage = 'Unknown error'
  ): ApiError {
    const status = statusOverride ?? err?.status ?? err?.code;
    return {
      kind: statusToKind(status),
      code: err?.code,
      status,
      message: err?.message ?? fallbackMessage,
      details: err?.details,
    };
  }

  /** For endpoints that return a single T */
  export function fromEnvelope<T>(rawEnvelope: ApiResponse<T>): ApiResult<T> {
    const envelope = keysToCamel<ApiResponse<T>>(rawEnvelope);

    if (envelope.status === 'success' && envelope.data !== undefined) {
      return Result.ok<T, ApiError>(envelope.data);
    }

    const apiError = buildError(envelope.error);
    return Result.fail<T, ApiError>(apiError);
  }

  export function fromList<T>(
    rawEnvelope: ApiResponse<T[]>
  ): ApiResult<Paged<T>> {
    const envelope = keysToCamel<ApiResponse<T[]>>(rawEnvelope);

    if (envelope.status === 'success' && Array.isArray(envelope.data)) {
      const items = envelope.data ?? [];
      const total = envelope.meta?.total ?? items.length;
      return Result.ok<Paged<T>, ApiError>({ items, total });
    }
    const apiError = buildError(envelope.error);
    return Result.fail<Paged<T>, ApiError>(apiError);
  }

  /** Convert any thrown error (axios or not) into a failed ApiResult */
  export function fromError<T>(error: unknown): ApiResult<T> {
    if (!axios.isAxiosError(error)) {
      return Result.fail<T, ApiError>({
        kind: 'Unknown',
        status: undefined,
        message: error instanceof Error ? error.message : 'Unknown error',
      });
    }

    const axiosErr = error as AxiosError<ApiResponse<unknown>>;

    if (axiosErr.code === 'ERR_CANCELED') {
      return Result.fail<T, ApiError>({
        kind: 'Canceled',
        status: undefined,
        message: axiosErr.message || 'Request canceled',
      });
    }

    if (axiosErr.code === 'ECONNABORTED') {
      return Result.fail<T, ApiError>({
        kind: 'Timeout',
        status: 504,
        message: axiosErr.message || 'Request timeout',
      });
    }

    // No response at all (network / CORS / server down)
    if (!axiosErr.response) {
      return Result.fail<T, ApiError>({
        kind: 'Network',
        status: 502,
        message: axiosErr.message || 'Network error',
      });
    }

    // HTTP error with your ApiResponse shape
    const status = axiosErr.response.status;
    const envelope = axiosErr.response.data;
    const apiError = buildError(
      envelope?.error,
      status,
      axiosErr.message || 'Request failed'
    );

    return Result.fail<T, ApiError>(apiError);
  }
}

// WebError replaces ErrorInfo and expands it for frontend needs
export interface WebError {
  kind: ApiErrorKind;
  code?: number; // from backend
  status?: number; // HTTP status
  message: string;
  details?: unknown;
}

export class EnvelopeResult<T> extends Result<T, WebError> {
  private constructor(
    value: T | null,
    error: WebError | null,
    isSuccess: boolean
  ) {
    super(value, error, isSuccess);
  }

  private static success<T>(value: T): EnvelopeResult<T> {
    return new EnvelopeResult<T>(value, null, true);
  }

  private static failure<T>(error: WebError): EnvelopeResult<T> {
    return new EnvelopeResult<T>(null, error, false);
  }

  private static kindFromStatus(status?: number): ApiErrorKind {
    if (status === undefined) return 'Unknown';
    if (status >= 500) return 'Server';
    if (status >= 400) return 'Client';
    return 'Unknown';
  }

  private static makeError(
    err?: ErrorInfo,
    statusOverride?: number,
    fallback = 'Unknown error'
  ): WebError {
    const status = statusOverride ?? err?.code;
    return {
      kind: EnvelopeResult.kindFromStatus(status),
      code: err?.code,
      status,
      message: err?.message ?? fallback,
      details: err?.details,
    };
  }

  /**
   * Parse a single-item envelope response from the backend.
   */
  static fromObject<T>(rawEnvelope: ApiResponse<T>): EnvelopeResult<T> {
    const { status, data, error } = keysToCamel<ApiResponse<T>>(rawEnvelope);
    if (status === 'success' && data !== undefined)
      return EnvelopeResult.success<T>(data);
    return EnvelopeResult.failure<T>(EnvelopeResult.makeError(error));
  }

  /**
   * Parse a paginated list envelope response from the backend.
   */
  static fromList<T>(rawEnvelope: ApiResponse<T[]>): EnvelopeResult<Paged<T>> {
    const { status, data, meta, error } =
      keysToCamel<ApiResponse<T[]>>(rawEnvelope);
    if (status === 'success' && Array.isArray(data))
      return EnvelopeResult.success<Paged<T>>({
        items: data,
        total: meta?.total ?? data.length,
      });
    return EnvelopeResult.failure<Paged<T>>(EnvelopeResult.makeError(error));
  }

  /**
   * Convert any thrown error (axios or otherwise) into a failed WebResult.
   */
  static fromError<T>(error: unknown): EnvelopeResult<T> {
    if (!axios.isAxiosError(error)) {
      return EnvelopeResult.failure<T>({
        kind: 'Unknown',
        message: error instanceof Error ? error.message : 'Unknown error',
      });
    }

    const err = error as AxiosError<ApiResponse<unknown>>;

    if (err.code === 'ERR_CANCELED')
      return EnvelopeResult.failure<T>({
        kind: 'Canceled',
        message: 'Request canceled',
      });

    if (err.code === 'ECONNABORTED')
      return EnvelopeResult.failure<T>({
        kind: 'Timeout',
        status: 504,
        message: 'Request timeout',
      });

    if (!err.response)
      return EnvelopeResult.failure<T>({
        kind: 'Network',
        status: 502,
        message: 'Network error',
      });

    return EnvelopeResult.failure<T>(
      EnvelopeResult.makeError(
        err.response.data?.error,
        err.response.status,
        err.message || 'Request failed'
      )
    );
  }
}
