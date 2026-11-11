
import { useState, type FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";

import PageLayout from "../../components/layout/PageLayout";
import { useAuth } from "./AuthContext";
import { getApiErrorMessage } from "../../utils/getApiErrorMessage";

const RegisterPage = () => {
  const { registerStudent } = useAuth();
  const navigate = useNavigate();

  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [birthRegistrationNumber, setBirthRegistrationNumber] = useState("");

  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    setError("");
    setSuccess("");

    if (password !== confirmPassword) {
      setError("Passwords do not match.");
      return;
    }

    if (!/^[0-9]{17}$/.test(birthRegistrationNumber)) {
      setError("Birth registration number must contain exactly 17 digits.");
      return;
    }

    setIsSubmitting(true);

    try {
      const response = await registerStudent({
        fullName: fullName.trim(),
        email: email.trim(),
        password,
        confirmPassword,
        birthRegistrationNumber,
      });

      setSuccess(
        response.message || "Registration successful. Please sign in."
      );

      // Redirect to login after a short delay.
      setTimeout(() => {
        navigate("/login", { replace: true });
      }, 1500);
    } catch (err: unknown) {
      setError(
        getApiErrorMessage(err, "Registration failed. Please try again.")
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <PageLayout>
      <section className="auth-page">
        <div className="auth-card">
          <div className="auth-heading">
            <h1>Create Student Account</h1>
            <p>Register to apply for admission programs.</p>
          </div>

          <form onSubmit={handleSubmit} className="auth-form">
            <div className="auth-field">
              <label htmlFor="fullName">Full Name</label>
              <input
                id="fullName"
                type="text"
                autoComplete="name"
                placeholder="Enter your full name"
                value={fullName}
                onChange={(event) => setFullName(event.target.value)}
                minLength={2}
                maxLength={200}
                required
              />
            </div>

            <div className="auth-field">
              <label htmlFor="registerEmail">Email Address</label>
              <input
                id="registerEmail"
                type="email"
                autoComplete="email"
                placeholder="Enter your email"
                value={email}
                onChange={(event) => setEmail(event.target.value)}
                required
              />
            </div>

            <div className="auth-field">
              <label htmlFor="birthRegistrationNumber">
                Birth Registration Number
              </label>
              <input
                id="birthRegistrationNumber"
                type="text"
                inputMode="numeric"
                autoComplete="off"
                placeholder="Enter your 17-digit number"
                value={birthRegistrationNumber}
                onChange={(event) =>
                  setBirthRegistrationNumber(
                    event.target.value.replace(/\D/g, "").slice(0, 17)
                  )
                }
                maxLength={17}
                minLength={17}
                required
              />
            </div>

            <div className="auth-field">
              <label htmlFor="registerPassword">Password</label>
              <input
                id="registerPassword"
                type="password"
                autoComplete="new-password"
                placeholder="Create a password"
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                minLength={8}
                required
              />
            </div>

            <div className="auth-field">
              <label htmlFor="confirmPassword">Confirm Password</label>
              <input
                id="confirmPassword"
                type="password"
                autoComplete="new-password"
                placeholder="Confirm your password"
                value={confirmPassword}
                onChange={(event) => setConfirmPassword(event.target.value)}
                minLength={8}
                required
              />
            </div>

            {error && (
              <p className="auth-error" role="alert">
                {error}
              </p>
            )}

            {success && (
              <p className="auth-success" role="status">
                {success}
              </p>
            )}

            <button
              type="submit"
              className="auth-submit-button"
              disabled={isSubmitting || Boolean(success)}
            >
              {isSubmitting ? "Creating account..." : "Create Account"}
            </button>
          </form>

          <p className="auth-footer">
            Already have an account? <Link to="/login">Sign in</Link>
          </p>
        </div>
      </section>
    </PageLayout>
  );
};

export default RegisterPage;
