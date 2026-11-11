
import { Navigate, Outlet, useLocation } from "react-router-dom";

import { useAuth } from "./AuthContext";
import type { UserRole } from "./auth.types";

interface ProtectedRouteProps {
  allowedRoles?: UserRole[];
}

const ProtectedRoute = ({ allowedRoles }: ProtectedRouteProps) => {
  const { user, isLoading } = useAuth();
  const location = useLocation();

  // Wait for the existing session to be restored.
  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <p>Checking your session...</p>
      </div>
    );
  }

  // Redirect unauthenticated users to login.
  if (!user) {
    return (
      <Navigate
        to="/login"
        replace
        state={{ from: location }}
      />
    );
  }

  // Prevent users with the wrong role from accessing this route.
  if (allowedRoles && !allowedRoles.includes(user.role)) {
    return <Navigate to="/classes" replace />;
  }

  return <Outlet />;
};

export default ProtectedRoute;
