import axios, { type AxiosError } from "axios";

export interface Paged<T> {
  items: T[];
  total: number;
}

interface ErrorInfo {
  code: number;
  message: string;
  details?: unknown;
}

interface ApiResponse<T> {
  status: "success" | "error";
  meta?: {
    total?: number;
  };
  error?: ErrorInfo;
  data?: T;
}

type ApiErrorKind =
  | "Network"
  | "Timeout"
  | "Server"
  | "Client"
  | "Canceled"
  | "Unknown";

interface WebError {
  kind: ApiErrorKind;
  code?: number;
  status?: number;
  message: string;
  details?: unknown;
}

function toCamelKey(key: string): string {
  const camelKey = key.replace(/_([a-z])/g, (_, character: string) => (
    character.toUpperCase()
  ));
  return camelKey.charAt(0).toLowerCase() + camelKey.slice(1);
}

function toSnakeKey(key: string): string {
  return key.replace(/[A-Z]/g, (character) => `_${character.toLowerCase()}`);
}

function convertKeys(
  input: unknown,
  convertKey: (key: string) => string
): unknown {
  if (Array.isArray(input)) {
    return input.map((value) => convertKeys(value, convertKey));
  }

  if (input !== null && typeof input === "object") {
    return Object.fromEntries(
      Object.entries(input).map(([key, value]) => [
        convertKey(key),
        convertKeys(value, convertKey),
      ])
    );
  }

  return input;
}

function keysToCamel<T>(input: unknown): T {
  return convertKeys(input, toCamelKey) as T;
}

export function keysToSnake(input: unknown): unknown {
  return convertKeys(input, toSnakeKey);
}

class Result<T, E> {
  protected constructor(
    private readonly resultValue: T | null,
    private readonly resultError: E | null,
    public readonly isSuccess: boolean
  ) {}

  public get isFailure(): boolean {
    return !this.isSuccess;
  }

  public get value(): T {
    if (!this.isSuccess) {
      throw new Error("No value present on a failed Result");
    }

    return this.resultValue as T;
  }

  public get error(): E {
    if (this.isSuccess) {
      throw new Error("No error present on a successful Result");
    }

    return this.resultError as E;
  }
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
    if (status === undefined) return "Unknown";
    if (status >= 500) return "Server";
    if (status >= 400) return "Client";
    return "Unknown";
  }

  private static makeError(
    error?: ErrorInfo,
    statusOverride?: number,
    fallback = "Unknown error"
  ): WebError {
    const status = statusOverride ?? error?.code;
    return {
      kind: EnvelopeResult.kindFromStatus(status),
      code: error?.code,
      status,
      message: error?.message ?? fallback,
      details: error?.details,
    };
  }

  static fromObject<T>(rawEnvelope: ApiResponse<T>): EnvelopeResult<T> {
    const { status, data, error } = keysToCamel<ApiResponse<T>>(rawEnvelope);
    if (status === "success" && data !== undefined) {
      return EnvelopeResult.success<T>(data);
    }

    return EnvelopeResult.failure<T>(EnvelopeResult.makeError(error));
  }

  static fromList<T>(rawEnvelope: ApiResponse<T[]>): EnvelopeResult<Paged<T>> {
    const { status, data, meta, error } = keysToCamel<ApiResponse<T[]>>(rawEnvelope);
    if (status === "success" && Array.isArray(data)) {
      return EnvelopeResult.success<Paged<T>>({
        items: data,
        total: meta?.total ?? data.length,
      });
    }

    return EnvelopeResult.failure<Paged<T>>(EnvelopeResult.makeError(error));
  }

  static fromError<T>(error: unknown): EnvelopeResult<T> {
    if (!axios.isAxiosError(error)) {
      return EnvelopeResult.failure<T>({
        kind: "Unknown",
        message: error instanceof Error ? error.message : "Unknown error",
      });
    }

    const axiosError = error as AxiosError<ApiResponse<unknown>>;

    if (axiosError.code === "ERR_CANCELED") {
      return EnvelopeResult.failure<T>({
        kind: "Canceled",
        message: "Request canceled",
      });
    }

    if (axiosError.code === "ECONNABORTED") {
      return EnvelopeResult.failure<T>({
        kind: "Timeout",
        status: 504,
        message: "Request timeout",
      });
    }

    if (!axiosError.response) {
      return EnvelopeResult.failure<T>({
        kind: "Network",
        status: 502,
        message: "Network error",
      });
    }

    return EnvelopeResult.failure<T>(
      EnvelopeResult.makeError(
        axiosError.response.data?.error,
        axiosError.response.status,
        axiosError.message || "Request failed"
      )
    );
  }
}
