
import { GraduationCap } from "lucide-react";
import { NavLink, useNavigate } from "react-router-dom";
import { useEffect, useState } from "react";

import { useAuth } from "../../features/auth/AuthContext";

const Navbar = () => {
  const [scrollProgress, setScrollProgress] = useState(0);

  const { user, isAuthenticated, logout } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    const handleScroll = () => {
      const scrollTop = window.scrollY;
      const docHeight =
        document.documentElement.scrollHeight - window.innerHeight;

      const progress = docHeight > 0 ? (scrollTop / docHeight) * 100 : 0;

      setScrollProgress(progress);
    };

    window.addEventListener("scroll", handleScroll, { passive: true });

    return () => window.removeEventListener("scroll", handleScroll);
  }, []);

  const handleLogout = async () => {
    try {
      await logout();
    } catch {
      // Local authentication state is already cleared.
    } finally {
      navigate("/login", { replace: true });
    }
  };

  return (
    <header className="navbar">
      <div className="navbar-container">
        <NavLink to="/classes" className="brand">
          <div className="brand-icon">
            <GraduationCap size={25} />
          </div>

          <span>
            Edu<span className="brand-highlight">Apply</span>
          </span>
        </NavLink>

        <nav className="nav-links">
          {isAuthenticated ? (
            <>
              <NavLink to="/classes">
                Admission Classes
              </NavLink>

              {user?.role === "Student" && (
                <NavLink to="/my-applications">
                  My Applications
                </NavLink>
              )}

              {user?.role === "Admin" && (
  <>
    <NavLink to="/admin/classes">
      Manage Classes
    </NavLink>

    <NavLink to="/admin/applications">
      Applications
    </NavLink>
  </>
)}
            </>
          ) : (
            <>
              <NavLink to="/login">
                Student Login
              </NavLink>

              <NavLink to="/register">
                Register
              </NavLink>

              <NavLink to="/admin/login">
                Admin Login
              </NavLink>
            </>
          )}
        </nav>

        {isAuthenticated ? (
          <div className="navbar-label">
            <span className="status-dot"></span>

            <span>{user?.fullName}</span>

            <button
              type="button"
              onClick={handleLogout}
              className="nav-logout-button"
            >
              Logout
            </button>
          </div>
        ) : (
          <div className="navbar-label">
            <span className="status-dot"></span>
            Admission Portal
          </div>
        )}
      </div>

      <div className="scroll-progress-track">
        <div
          className="scroll-progress-bar"
          style={{ width: `${scrollProgress}%` }}
          role="progressbar"
          aria-valuenow={Math.round(scrollProgress)}
          aria-valuemin={0}
          aria-valuemax={100}
          aria-label="Page scroll progress"
        />
      </div>
    </header>
  );
};

export default Navbar;
