
import { GraduationCap, Menu, X } from "lucide-react";
import { NavLink, useNavigate } from "react-router-dom";
import { useEffect, useState } from "react";

import { useAuth } from "../../features/auth/AuthContext";

const Navbar = () => {
  const [scrollProgress, setScrollProgress] = useState(0);
  const [menuOpen, setMenuOpen] = useState(false);

  const { user, isAuthenticated, logout } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    const handleScroll = () => {
      const scrollTop = window.scrollY;
      const docHeight =
        document.documentElement.scrollHeight - window.innerHeight;

      const progress = docHeight > 0 ? (scrollTop / docHeight) * 100 : 0;

      setScrollProgress(progress);

      // Close mobile menu on scroll
      if (menuOpen) setMenuOpen(false);
    };

    window.addEventListener("scroll", handleScroll, { passive: true });

    return () => window.removeEventListener("scroll", handleScroll);
  }, [menuOpen]);

  // Close menu when route changes
  const closeMenu = () => setMenuOpen(false);

  const handleLogout = async () => {
    closeMenu();
    try {
      await logout();
    } catch {
      // Local authentication state is already cleared.
    } finally {
      navigate("/login", { replace: true });
    }
  };

  return (
    <>
      <header className="navbar">
        <div className="navbar-container">
          <NavLink to="/classes" className="brand" onClick={closeMenu}>
            <div className="brand-icon">
              <GraduationCap size={25} />
            </div>
            <span>
              Edu<span className="brand-highlight">Apply</span>
            </span>
          </NavLink>

          {/* Desktop nav links */}
          <nav className="nav-links nav-links-desktop">
            {isAuthenticated ? (
              <>
                <NavLink to="/classes">Admission Classes</NavLink>
                {user?.role === "Student" && (
                  <NavLink to="/my-applications">My Applications</NavLink>
                )}
                {user?.role === "Admin" && (
                  <>
                    <NavLink to="/admin/classes">Manage Classes</NavLink>
                    <NavLink to="/admin/applications">Applications</NavLink>
                  </>
                )}
              </>
            ) : (
              <>
                <NavLink to="/login">Student Login</NavLink>
                <NavLink to="/register">Register</NavLink>
                <NavLink to="/admin/login">Admin Login</NavLink>
              </>
            )}
          </nav>

          <div className="navbar-right">
            {isAuthenticated && (
              <div className="navbar-label navbar-label-desktop">
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
            )}

            {!isAuthenticated && (
              <div className="navbar-label navbar-label-desktop">
                <span className="status-dot"></span>
                Admission Portal
              </div>
            )}

            {/* Hamburger button — mobile only */}
            <button
              type="button"
              className="nav-hamburger"
              aria-label={menuOpen ? "Close menu" : "Open menu"}
              aria-expanded={menuOpen}
              onClick={() => setMenuOpen((prev) => !prev)}
            >
              {menuOpen ? <X size={22} /> : <Menu size={22} />}
            </button>
          </div>
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

      {/* Mobile drawer */}
      {menuOpen && (
        <div className="mobile-menu-overlay" onClick={closeMenu}>
          <nav
            className="mobile-menu"
            onClick={(e) => e.stopPropagation()}
          >
            {isAuthenticated ? (
              <>
                <NavLink to="/classes" className="mobile-nav-link" onClick={closeMenu}>
                  Admission Classes
                </NavLink>
                {user?.role === "Student" && (
                  <NavLink to="/my-applications" className="mobile-nav-link" onClick={closeMenu}>
                    My Applications
                  </NavLink>
                )}
                {user?.role === "Admin" && (
                  <>
                    <NavLink to="/admin/classes" className="mobile-nav-link" onClick={closeMenu}>
                      Manage Classes
                    </NavLink>
                    <NavLink to="/admin/applications" className="mobile-nav-link" onClick={closeMenu}>
                      Applications
                    </NavLink>
                  </>
                )}
                <div className="mobile-menu-divider" />
                <div className="mobile-user-info">
                  <span className="status-dot"></span>
                  <span className="mobile-username">{user?.fullName}</span>
                </div>
                <button
                  type="button"
                  onClick={handleLogout}
                  className="mobile-logout-button"
                >
                  Logout
                </button>
              </>
            ) : (
              <>
                <NavLink to="/login" className="mobile-nav-link" onClick={closeMenu}>
                  Student Login
                </NavLink>
                <NavLink to="/register" className="mobile-nav-link" onClick={closeMenu}>
                  Register
                </NavLink>
                <NavLink to="/admin/login" className="mobile-nav-link" onClick={closeMenu}>
                  Admin Login
                </NavLink>
                <div className="mobile-menu-divider" />
                <div className="mobile-user-info">
                  <span className="status-dot"></span>
                  <span>Admission Portal</span>
                </div>
              </>
            )}
          </nav>
        </div>
      )}
    </>
  );
};

export default Navbar;
