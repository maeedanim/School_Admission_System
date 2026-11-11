
import api from "../../services/api";

import type {
  AuthMeResponse,
  AuthMessageResponse,
  AuthResponse,
  LoginRequest,
  RegisterStudentRequest,
} from "./auth.types";

const AUTH_BASE_URL = "/api/auth";

export const loginStudent = async (
  credentials: LoginRequest
): Promise<AuthResponse> => {
  const response = await api.post<AuthResponse>(
    `${AUTH_BASE_URL}/student/login`,
    credentials,
    { withCredentials: true }
  );

  return response.data;
};

export const loginAdmin = async (
  credentials: LoginRequest
): Promise<AuthResponse> => {
  const response = await api.post<AuthResponse>(
    `${AUTH_BASE_URL}/admin/login`,
    credentials,
    { withCredentials: true }
  );

  return response.data;
};

export const registerStudent = async (
  data: RegisterStudentRequest
): Promise<AuthMessageResponse> => {
  const response = await api.post<AuthMessageResponse>(
    `${AUTH_BASE_URL}/student/register`,
    data
  );

  return response.data;
};

export const refreshSession = async (): Promise<AuthResponse> => {
  const response = await api.post<AuthResponse>(
    `${AUTH_BASE_URL}/refresh`,
    {},
    { withCredentials: true }
  );

  return response.data;
};

export const getCurrentUser = async (): Promise<AuthMeResponse> => {
  const response = await api.get<AuthMeResponse>(
    `${AUTH_BASE_URL}/me`
  );

  return response.data;
};

export const logout = async (): Promise<void> => {
  await api.post(
    `${AUTH_BASE_URL}/logout`,
    {},
    { withCredentials: true }
  );
};
