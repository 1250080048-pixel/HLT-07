import { UserManager, WebStorageStateStore } from "oidc-client-ts";

export const userManager = new UserManager({
  authority: "http://localhost:8081/realms/tuvan",
  client_id: "tuvan-web",
  redirect_uri: `${window.location.origin}/callback`,
  post_logout_redirect_uri: window.location.origin,
  response_type: "code",              // Authorization Code + PKCE
  scope: "openid profile",
  userStore: new WebStorageStateStore({ store: window.sessionStorage }),
});

/** Lấy danh sách vai trò từ access token (claim "roles" đã được cấu hình trong Keycloak). */
export function rolesCua(accessToken?: string): string[] {
  if (!accessToken) return [];
  try {
    const payload = JSON.parse(atob(accessToken.split(".")[1].replace(/-/g, "+").replace(/_/g, "/")));
    return Array.isArray(payload.roles) ? payload.roles : [];
  } catch {
    return [];
  }
}
