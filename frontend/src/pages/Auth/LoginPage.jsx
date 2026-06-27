import { useState } from "react";
import { useSearchParams, useNavigate, Link } from "react-router";
import { useGoogleAuthError } from "../../hooks/useGoogleAuthError";
import { createInputChangeHandler } from "../../utils/formHandlers";
import GoogleLoginButton from "../../components/GoogleLoginButton";
import PasswordField from "../../components/PasswordField";
import Alert from "../../components/Alert";
import api from "../../services/api";
import "./LoginPage.scss";

export default function LoginPage() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();

  const [formData, setFormData] = useState({
    userNameOrEmail: "",
    password: "",
  });
  const [showPassword, setShowPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [errors, setErrors] = useState({});

  const returnUrl = searchParams.get("returnUrl") || "";
  useGoogleAuthError(returnUrl);
  
  const handleInputChange = createInputChangeHandler(setFormData);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setErrors({});

    // Validation -----------
    const validationErrors = {};

    if (!formData.userNameOrEmail.trim()) {
      validationErrors.UserNameOrEmail = ["Введіть ім'я користувача"];
    }

    if (!formData.password.trim()) {
      validationErrors.Password = ["Введіть пароль"];
    }

    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }
    // ---------------------

    try {
      setIsLoading(true);

      const url = returnUrl
        ? `/auth/login?returnUrl=${encodeURIComponent(returnUrl)}`
        : "/auth/login";
      const response = await api.post(url, formData);

      navigate(response.data.redirectTo);
    } catch (err) {
        if (err.globalMessage) {
          setErrors({ global: err.globalMessage });
        }
        else if (err.response && err.response.data) {
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
          <h2 className="title">Вхід</h2>

          <form onSubmit={handleSubmit}>
            <div className="form-group">
              <input
                type="text"
                placeholder="Введіть логін або email"
                className={hasError("UserNameOrEmail") ? "error-input" : ""}
                value={formData.userNameOrEmail}
                name="userNameOrEmail"
                onChange={handleInputChange}
              />
              {errors.UserNameOrEmail && (
                <span className="text-danger">{errors.UserNameOrEmail[0]}</span>
              )}
            </div>

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
              <Link
                to="/verify-email"
                className="link-reser-password"
              >
                Забули пароль?
              </Link>
            </div>


            <button type="submit" className="btn-auth">
              {isLoading ? "Вхід..." : "Увійти"}
            </button>
          </form>

          <div className="separator">або</div>

          <GoogleLoginButton/>

          <div className="auth-footer">
            Ще не маєте акаунта? <Link to="/register">Зареєструватися</Link>
          </div>
        </div>
      </div>
    </>
  );
}
