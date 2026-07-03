import { useState } from "react";
import { useSearchParams, useNavigate, Link } from "react-router";
import { useGoogleAuthError } from "../../hooks/useGoogleAuthError";
import { createInputChangeHandler } from "../../utils/formHandlers";
import GoogleLoginButton from "../../components/GoogleLoginButton";
import PasswordField from "../../components/PasswordField";
import Alert from "../../components/Alert";
import api from "../../services/api";
import "./RegisterPage.scss";

export default function RegisterPage() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();

  const [formData, setFormData] = useState({
    userName: "",
    email: "",
    password: "",
    confirmPassword: "",
  });

  const [errors, setErrors] = useState({});
  const [isLoading, setIsLoading] = useState(false);

  const returnUrl = searchParams.get("returnUrl") || "";
  useGoogleAuthError(returnUrl, setErrors);

  const handleInputChange = createInputChangeHandler(setFormData);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setErrors({});

    // Validation -----------
    const validationErrors = {};

    if (!formData.userName.trim()) {
      validationErrors.UserName = ["Введіть ім'я користувача"];
    }

    if (!formData.email.trim()) {
      validationErrors.Email = ["Введіть email"];
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email.trim())) {
      validationErrors.Email = ["Невірний формат email"];
    }

    if (!formData.password.trim()) {
      validationErrors.Password = ["Введіть пароль"];
    }

    if (!formData.confirmPassword.trim()) {
      validationErrors.ConfirmPassword = ["Підтвердіть пароль"];
    }

    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }

    if (formData.password !== formData.confirmPassword) {
      setErrors({ ConfirmPassword: ["Паролі не збігаються"] });
      return;
    }
    // ---------------------

    try {
      setIsLoading(true);
      // Instead of fetch we use axios through the services/api.js
      await api.post("/auth/register", formData);

      window.location.href = "/main";

      setFormData({
        userName: "",
        email: "",
        password: "",
        confirmPassword: "",
      });
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
    <>
      <Alert
        type="error"
        message={errors.global}
        onClose={() => setErrors((prev) => ({ ...prev, global: null }))}
      />
      <div className="auth-body">
        <div className="auth-card">
          <h2 className="title">Реєстрація</h2>

          <form onSubmit={handleSubmit} autoComplete="off">
            <div className="form-group">
              <input
                placeholder="Вигадайте нікнейм"
                className={hasError("UserName") ? "error-input" : ""}
                type="text"
                value={formData.userName}
                name="userName"
                onChange={handleInputChange}
              />
              {errors.UserName && (
                <span className="text-danger">{errors.UserName[0]}</span>
              )}
            </div>
            <div className="form-group">
              <input
                type="text"
                className={hasError("Email") ? "error-input" : ""}
                placeholder="example@mail.com"
                value={formData.email}
                name="email"
                onChange={handleInputChange}
              />
              {errors.Email && (
                <span className="text-danger">{errors.Email[0]}</span>
              )}
            </div>

              <hr/>

            <div className="form-group">
              <PasswordField
                placeholder="Пароль"
                inputClassName={hasError("Password") ? "error-input" : ""}
                value={formData.password}
                name="password"
                onChange={handleInputChange}
              />
              {errors.Password && (
                <span className="text-danger">{errors.Password[0]}</span>
              )}
            </div>
            <div className="form-group">
              <PasswordField
                placeholder="Повторіть пароль"
                inputClassName={hasError("ConfirmPassword") ? "error-input" : ""}
                value={formData.confirmPassword}
                name="confirmPassword"
                onChange={handleInputChange}
              />

              {errors.ConfirmPassword && (
                <span className="text-danger">{errors.ConfirmPassword[0]}</span>
              )}
            </div>

            <button type="submit" className="btn-auth" disabled={isLoading}>
              {isLoading ? "Реєстрація..." : "Зареєструватися"}
            </button>
          </form>

          <div className="separator">або</div>

          <GoogleLoginButton />

          <div className="auth-footer">
            Вже маєте акаунт? <Link to="/login">Увійти</Link>
          </div>
        </div>
      </div>
    </>
  );
}
