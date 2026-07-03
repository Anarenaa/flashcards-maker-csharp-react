import { useState } from "react";
import { useGoogleAuthError } from "../../../hooks/useGoogleAuthError";
import { NavLink } from "react-router";
import api from "../../../services/api";
import "../AuthShared.scss";
import "./VerifyEmailPage.scss";

export default function VerifyEmailPage() {
  const [errors, setErrors] = useState({});
  const [isLoading, setIsLoading] = useState(false);
  const [email, setEmail] = useState("");

  const handleInputChange = (e) => setEmail(e.target.value);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setErrors({});

    // Validation -----------
    const validationErrors = {};

    if (!email.trim()) {
      validationErrors.Email = ["Введіть email"];
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim())) {
      validationErrors.Email = ["Невірний формат email"];
    }

    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }
    // ---------------------

    try {
      setIsLoading(true);
      await api.post("/auth/verify-email", { email });

      window.location.href = "/main";
    } catch (err) {
      if (err.globalMessage) {
        setErrors({ global: err.globalMessage });
      } else if (err.response && err.response.data) {
        const serverErrors = err.response.data.errors || err.response.data;
        setErrors(serverErrors);
      }
    } finally {
      setIsLoading(false);
    }
  };

  const hasError = (field) => !!errors[field];

  return (
    <div className="auth-body">
      <div className="auth-card">
        <h2 className="title">Забули пароль?</h2>
        <p>
          Введіть ваш Email, і ми надішлемо вам посилання для відновлення
          доступу.
        </p>

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <input
              name="email"
              type="text"
              className={hasError("Email") ? "error-input" : ""}
              placeholder="example@mail.com"
              value={email}
              onChange={handleInputChange}
            />
            { errors.Email && <span className="text-danger">{errors.Email[0]}</span> }
          </div>
          <button type="submit" className="btn-auth" disabled={isLoading}>
            {isLoading ? "Надсилається..." : "Надіслати код"} 
          </button>
        </form>

        <NavLink to="/login" className="back-link">
          ← Назад до входу
        </NavLink>
      </div>
    </div>
  );
}
