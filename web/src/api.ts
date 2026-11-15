import { userManager } from "./auth";

const API = "http://localhost:5000";

export class ApiError extends Error {
  constructor(public status: number, message: string) {
    super(message);
  }
}

/** Gọi API kèm token. Lỗi nghiệp vụ (422) được trả về thông điệp tiếng Việt từ server. */
export async function goi<T = unknown>(path: string, init: RequestInit = {}): Promise<T> {
  const user = await userManager.getUser();
  const res = await fetch(`${API}${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${user?.access_token ?? ""}`,
      ...(init.headers ?? {}),
    },
  });
  if (res.status === 204) return undefined as T;
  const body = await res.json().catch(() => ({}));
  if (!res.ok) throw new ApiError(res.status, body.loi ?? `Lỗi ${res.status}`);
  return body as T;
}
