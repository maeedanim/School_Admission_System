
import axios, {
  type InternalAxiosRequestConfig,
} from "axios";

import {
  AUTH_SESSION_EXPIRED_EVENT,
  clearAccessToken,
  getAccessToken,
  setAccessToken,
} from "../features/auth/access-token";

import type { AuthResponse } from "../features/auth/auth.types";

interface RetryableRequestConfig extends InternalAxiosRequestConfig {
  _retry?: boolean;
}

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;

const api = axios.create({
  baseURL: apiBaseUrl,
  headers: {
    "Content-Type": "application/json",
  },
  timeout: 15000,
});

// Endpoints that must not trigger automatic token refresh.
const excludedAuthEndpoints = [
  "/api/auth/student/login",
  "/api/auth/admin/login",
  "/api/auth/student/register",
  "/api/auth/refresh",
  "/api/auth/logout",
];

// Attach the access token to outgoing requests.
api.interceptors.request.use((config) => {
  const token = getAccessToken();

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

// Shared refresh promise prevents simultaneous refresh requests.
let refreshPromise: Promise<string> | null = null;

const refreshAccessToken = async (): Promise<string> => {
  if (!refreshPromise) {
    refreshPromise = axios
      .post<AuthResponse>(
        `${apiBaseUrl}/api/auth/refresh`,
        {},
        {
          withCredentials: true,
          timeout: 15000,
        }
      )
      .then((response) => {
        const newToken = response.data.accessToken;

        if (!newToken) {
          throw new Error("Refresh response did not contain an access token.");
        }

        setAccessToken(newToken);

        return newToken;
      })
      .catch((error: unknown) => {
  clearAccessToken();

  window.dispatchEvent(
    new Event(AUTH_SESSION_EXPIRED_EVENT)
  );

  throw error;
})
      .finally(() => {
        refreshPromise = null;
      });
  }

  return refreshPromise;
};

// Refresh expired tokens and retry the original request once.
api.interceptors.response.use(
  (response) => response,
  async (error: unknown) => {
    if (!axios.isAxiosError(error) || error.response?.status !== 401) {
      return Promise.reject(error);
    }

    const originalRequest = error.config as
      | RetryableRequestConfig
      | undefined;

    if (!originalRequest || originalRequest._retry) {
      return Promise.reject(error);
    }

    const isExcludedEndpoint = excludedAuthEndpoints.some((endpoint) =>
      originalRequest.url?.includes(endpoint)
    );

    if (isExcludedEndpoint) {
      return Promise.reject(error);
    }

    originalRequest._retry = true;

    try {
      const newToken = await refreshAccessToken();

      originalRequest.headers.Authorization = `Bearer ${newToken}`;

      return api(originalRequest);
    } catch (refreshError) {
      return Promise.reject(refreshError);
    }
  }
);

export default api;
