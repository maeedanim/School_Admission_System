
import { useState, type FormEvent } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";

import PageLayout from "../../components/layout/PageLayout";
import { useAuth } from "./AuthContext";
import { getApiErrorMessage } from "../../utils/getApiErrorMessage";

interface LoginLocationState {
  from?: {
    pathname?: string;
  };
}

const LoginPage = () => {
  const { loginStudent } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const locationState = location.state as LoginLocationState | null;
  const redirectPath = locationState?.from?.pathname ?? "/classes";

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    setError("");
    setIsSubmitting(true);

    try {
      await loginStudent({
        email: email.trim(),
        password,
      });

      navigate(redirectPath, { replace: true });
    } catch (err: unknown) {
      setError(getApiErrorMessage(err, "Login failed. Please try again."));
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <PageLayout>
      <section className="auth-page">
        <div className="auth-card">
          <div className="auth-heading">
            <h1>Student Login</h1>
            <p>Sign in to manage your admission applications.</p>
          </div>

          <form onSubmit={handleSubmit} className="auth-form">
            <div className="auth-field">
              <label htmlFor="email">Email Address</label>
              <input
                id="email"
                type="email"
                autoComplete="email"
                placeholder="Enter your email"
                value={email}
                onChange={(event) => setEmail(event.target.value)}
                required
              />
            </div>

            <div className="auth-field">
              <label htmlFor="password">Password</label>
              <input
                id="password"
                type="password"
                autoComplete="current-password"
                placeholder="Enter your password"
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                required
              />
            </div>

            {error && (
              <p className="auth-error" role="alert">
                {error}
              </p>
            )}

            <button
              type="submit"
              className="auth-submit-button"
              disabled={isSubmitting}
            >
              {isSubmitting ? "Signing in..." : "Sign In"}
            </button>
          </form>

          <p className="auth-footer">
            Don't have an account?{" "}
            <Link to="/register">Create an account</Link>
          </p>

          <p className="auth-footer">
            Administrator?{" "}
            <Link to="/admin/login">Admin Login</Link>
          </p>
        </div>
      </section>
    </PageLayout>
  );
};

export default LoginPage;
