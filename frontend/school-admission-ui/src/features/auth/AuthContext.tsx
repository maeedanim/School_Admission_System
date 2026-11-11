
import {
  createContext,
  useContext,
  useEffect,
  useState,
  type ReactNode,
} from "react";

import {
  AUTH_SESSION_EXPIRED_EVENT,
  clearAccessToken,
  setAccessToken,
} from "./access-token";

import {
  getCurrentUser,
  loginAdmin as loginAdminService,
  loginStudent as loginStudentService,
  logout as logoutService,
  refreshSession,
  registerStudent as registerStudentService,
} from "./auth.service";

import type {
  AuthUser,
  LoginRequest,
  RegisterStudentRequest,
  UserRole,
} from "./auth.types";

// Authentication context state and operations.
interface AuthContextType {
  user: AuthUser | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  loginStudent: (credentials: LoginRequest) => Promise<void>;
  loginAdmin: (credentials: LoginRequest) => Promise<void>;
  registerStudent: (
    data: RegisterStudentRequest
  ) => ReturnType<typeof registerStudentService>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

// Convert the backend profile response into our frontend user model.
const mapAuthUser = (
  profile: Awaited<ReturnType<typeof getCurrentUser>>
): AuthUser => {
  const role = profile.roles.find(
    (item): item is UserRole =>
      item === "Student" || item === "Admin"
  );

  if (!role) {
    throw new Error("Your account does not have a supported role.");
  }

  return {
    userId: profile.userId,
    fullName: profile.fullName,
    email: profile.email,
    role,
  };
};

// Share session restoration across React StrictMode effect executions.
let sessionRestorePromise: Promise<AuthUser | null> | null = null;

const restoreSession = (): Promise<AuthUser | null> => {
  if (!sessionRestorePromise) {
    sessionRestorePromise = (async () => {
      try {
        const authResponse = await refreshSession();

        if (!authResponse.accessToken) {
          throw new Error("Refresh response did not contain an access token.");
        }

        setAccessToken(authResponse.accessToken);

        const profile = await getCurrentUser();

        return mapAuthUser(profile);
      } catch {
        clearAccessToken();
        return null;
      }
    })().finally(() => {
      sessionRestorePromise = null;
    });
  }

  return sessionRestorePromise;
};

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider = ({ children }: AuthProviderProps) => {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // Restore an existing session when the application starts.
  useEffect(() => {
    let isMounted = true;

    restoreSession().then((restoredUser) => {
      if (isMounted) {
        setUser(restoredUser);
        setIsLoading(false);
      }
    });

    return () => {
      isMounted = false;
    };
  }, []);

  // Clear the authenticated user when automatic token refresh fails.
  useEffect(() => {
    const handleSessionExpired = () => {
      clearAccessToken();
      setUser(null);
    };

    window.addEventListener(
      AUTH_SESSION_EXPIRED_EVENT,
      handleSessionExpired
    );

    return () => {
      window.removeEventListener(
        AUTH_SESSION_EXPIRED_EVENT,
        handleSessionExpired
      );
    };
  }, []);

  // Authenticate a student account.
  const loginStudent = async (
    credentials: LoginRequest
  ): Promise<void> => {
    const response = await loginStudentService(credentials);

    if (response.role !== "Student") {
      try {
        await logoutService();
      } catch {
        // Keep the client unauthenticated if server logout fails.
      }

      clearAccessToken();

      throw new Error(
        "This account is not authorized for student login."
      );
    }

    setAccessToken(response.accessToken);

    setUser({
      userId: response.userId,
      fullName: response.fullName,
      email: response.email,
      role: response.role,
    });
  };

  // Authenticate an administrator account.
  const loginAdmin = async (
    credentials: LoginRequest
  ): Promise<void> => {
    const response = await loginAdminService(credentials);

    if (response.role !== "Admin") {
      try {
        await logoutService();
      } catch {
        // Keep the client unauthenticated if server logout fails.
      }

      clearAccessToken();

      throw new Error(
        "This account is not authorized for admin login."
      );
    }

    setAccessToken(response.accessToken);

    setUser({
      userId: response.userId,
      fullName: response.fullName,
      email: response.email,
      role: response.role,
    });
  };

  // Register a new student account.
  const registerStudent = (data: RegisterStudentRequest) => {
    return registerStudentService(data);
  };

  // Clear local authentication state and invalidate the server session.
  const logout = async (): Promise<void> => {
    clearAccessToken();
    setUser(null);

    await logoutService();
  };

  const value: AuthContextType = {
    user,
    isLoading,
    isAuthenticated: user !== null,
    loginStudent,
    loginAdmin,
    registerStudent,
    logout,
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};

// Access authentication state and operations from any component.
export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider.");
  }

  return context;
};
