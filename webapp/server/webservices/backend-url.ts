export function getBackendUrl(): string {
  if (typeof window !== "undefined") {
    return "";
  }

  return (process.env.BACKEND_URL ?? "http://localhost:8080")
    .replace(/\/+$/, "");
}
