import { useState } from "react";
import { Link, useNavigate } from "react-router";
import { Eye, EyeOff } from "lucide-react";
import Alert from "../components/Alert";
import api from "../services/api";
import "./RegisterPage.scss";

export default function RegisterPage() {
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");

  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);

  const [errors, setErrors] = useState({});
  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setErrors({});

    // Validation -----------
    const validationErrors = {};

    if (!name.trim()) {
      validationErrors.UserName = ["Введіть ім'я користувача"];
    }

    if (!email.trim()) {
      validationErrors.Email = ["Введіть email"];
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      validationErrors.Email = ["Невірний формат email"];
    }

    if (!password) {
      validationErrors.Password = ["Введіть пароль"];
    }

    if (!confirmPassword) {
      validationErrors.ConfirmPassword = ["Підтвердіть пароль"];
    }

    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }

    if (password !== confirmPassword) {
      setErrors({ ConfirmPassword: ["Паролі не збігаються"] });
      return;
    }
    // ---------------------

    try {
      setIsLoading(true);

      // Instead of fetch we use axios through the services/api.js 
      const response = await api.post("/auth/register", { 
        userName: name, 
        email, 
        password 
      });
      
      navigate("/");

      setName("");
      setEmail("");
      setPassword("");
      setConfirmPassword("");

    } catch (error) {

      if (error.globalMessage) {
        setErrors({ global: error.globalMessage });
      } 
      else if (error.response && error.response.data) {
        const responseData = error.response.data;

        if (responseData.errors) {
          setErrors(responseData.errors);
        } 
        else {
          setErrors(responseData);
        }
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
        onClose={() => setErrors(prev => ({ ...prev, global: null }))} 
      />
      <div className="auth-card">
        <h1 className="title">Реєстрація</h1>

        <form onSubmit={handleSubmit} autoComplete="off">
          <div className="flexs-container">
            <div className="flex-container">
              <div className="form-group">
                <input
                  placeholder="Вигадайте нікнейм"
                  name="userName"
                  className={hasError("UserName") ? "error-input" : ""}
                  type="text"
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                />
                {errors.UserName && (
                  <span className="text-danger">{errors.UserName[0]}</span>
                )}
              </div>
              <div className="form-group">
                <input
                  type="text"
                  name="email"
                  className={hasError("Email") ? "error-input" : ""}
                  placeholder="example@mail.com"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                />
                {errors.Email && (
                  <span className="text-danger">{errors.Email[0]}</span>
                )}
              </div>
            </div>

            <div className="separator">
              <span></span>
            </div>

            <div className="flex-container">
              <div className="form-group">
                <div className="password-container">
                  <input
                    type={showPassword ? "text" : "password"}
                    name="password"
                    className={hasError("Password") ? "error-input" : ""}
                    placeholder="Пароль"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                  />
                  <span
                    className="toggle-password"
                    onClick={() => setShowPassword(!showPassword)}
                  >
                    {showPassword ? <EyeOff size={20} /> : <Eye size={20} />}
                  </span>
                </div>
                {errors.Password && (
                  <span className="text-danger">{errors.Password[0]}</span>
                )}
              </div>
              <div className="form-group">
                <div className="password-container">
                  <input
                    type={showConfirmPassword ? "text" : "password"}
                    name="confirm"
                    className={hasError("ConfirmPassword") ? "error-input" : ""}
                    value={confirmPassword}
                    onChange={(e) => setConfirmPassword(e.target.value)}
                    placeholder="Повторіть пароль"
                  />
                  <span
                    className="toggle-password"
                    onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                  >
                    {showConfirmPassword ? (
                      <EyeOff size={20} />
                    ) : (
                      <Eye size={20} />
                    )}
                  </span>
                </div>

                {errors.ConfirmPassword && (
                  <span className="text-danger">
                    {errors.ConfirmPassword[0]}
                  </span>
                )}
              </div>
            </div>
          </div>
          
          <button
            type="submit"
            className="btn-auth"
            id="btn-auth"
            disabled={isLoading}
          >
            {isLoading ? "Реєстрація..." : "Зареєструватися"}
          </button>
        </form>
        <div className="separator">
          <span></span>
          або
          <span></span>
        </div>

        <a className="btn-auth-google">
          <img
            src="https://upload.wikimedia.org/wikipedia/commons/c/c1/Google_%22G%22_logo.svg"
            width="18"
            height="18"
          />
          через Google
        </a>

        <div className="auth-footer">
          Вже маєте акаунт? <Link to="/login">Увійти</Link>
        </div>
      </div>
    </>
  );
}
