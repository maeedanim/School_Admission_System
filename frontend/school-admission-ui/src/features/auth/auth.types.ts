export type UserRole = "Student" | "Admin";

export interface AuthUser {
  userId: string;
  fullName: string;
  email: string;
  role: UserRole;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterStudentRequest {
  fullName: string;
  email: string;
  password: string;
  confirmPassword: string;
  birthRegistrationNumber: string;
}

export interface AuthResponse extends AuthUser {
  accessToken: string;
  expiresAtUtc: string;
}

export interface AuthMeResponse {
  userId: string;
  fullName: string;
  email: string;
  birthRegistrationNumber: string;
  roles: string[];
}

export interface AuthMessageResponse {
  message: string;
  userId?: string;
  fullName?: string;
  email?: string;
}