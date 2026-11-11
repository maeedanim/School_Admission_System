let accessToken: string | null = null;

export const AUTH_SESSION_EXPIRED_EVENT = "auth:session-expired";

export const getAccessToken = (): string | null => {
  return accessToken;
};

export const setAccessToken = (token: string): void => {
  accessToken = token;
};

export const clearAccessToken = (): void => {
  accessToken = null;
};