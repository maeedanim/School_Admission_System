import { GraduationCap } from "lucide-react";
import { NavLink } from "react-router-dom";
import { useEffect, useState } from "react";

const Navbar = () => {
  const [scrollProgress, setScrollProgress] = useState(0);

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
          <NavLink to="/classes">
            Admission Classes
          </NavLink>

          <NavLink to="/my-applications">
            My Applications
          </NavLink>
        </nav>

        <div className="navbar-label">
          <span className="status-dot"></span>
          Admission Portal
        </div>
      </div>

      {/* Scroll progress bar */}
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
