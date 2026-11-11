
import { Navigate, Route, Routes, useLocation } from "react-router-dom";

import AdminLoginPage from "../features/auth/AdminLoginPage";
import LoginPage from "../features/auth/LoginPage";
import RegisterPage from "../features/auth/RegisterPage";
import ProtectedRoute from "../features/auth/ProtectedRoute";
import AdminClassesPage from "../features/admin/AdminClassesPage";
import ClassesPage from "../features/classes/ClassesPage";
import MyApplicationsPage from "../features/my-applications/MyApplicationsPage";
import AdminApplicationsPage from "../features/admin/AdminApplicationsPage";

const App = () => {
  const location = useLocation();

  return (
    <div key={location.pathname} className="page-transition">
      <Routes>
        <Route path="/" element={<Navigate to="/classes" replace />} />

        {/* Public authentication routes */}
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/admin/login" element={<AdminLoginPage />} />

        {/* Routes available to authenticated users */}
        <Route
          element={
            <ProtectedRoute allowedRoles={["Student", "Admin"]} />
          }
        >
          <Route path="/classes" element={<ClassesPage />} />
        </Route>

        {/* Student-only routes */}
        <Route element={<ProtectedRoute allowedRoles={["Student"]} />}>
          <Route
            path="/my-applications"
            element={<MyApplicationsPage />}
          />
        </Route>

        {/* Admin-only routes */}
<Route element={<ProtectedRoute allowedRoles={["Admin"]} />}>
  <Route
    path="/admin/classes"
    element={<AdminClassesPage />}
  />

  <Route
    path="/admin/applications"
    element={<AdminApplicationsPage />}
  />
</Route>

        {/* Fallback route */}
        <Route path="*" element={<Navigate to="/classes" replace />} />
      </Routes>
    </div>
  );
};

export default App;
