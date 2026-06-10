import { useState } from "react";
import { Link, useNavigate } from "react-router";
import { Eye, EyeOff } from "lucide-react";
import "./RegisterPage.scss";

export default function RegisterPage() {
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");

  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);

  const [errors, setErrors] = useState({});
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setErrors({});

    if (password !== confirmPassword) {
      setErrors({ ConfirmPassword: ["Паролі не збігаються"] });
      setPassword("");
      setConfirmPassword("");
      return;
    }

    try {
      const response = await fetch("/api/auth/register", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ userName: name, email, password }),
      });

      const data = await response.json();

      if (response.ok) {
        navigate("/");
      } else {
        setErrors(data || {});
      }
    } catch (error) {
      setErrors({
        global: "Не вдалося з'єднатися з сервером. Спробуйте пізніше.",
      });
    }
  };

  return (
    <div className="auth-card">
      <h2>Реєстрація</h2>
      {errors.global &&
        alert(errors.global[0])
      }
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <input
            placeholder="Вигадайте нікнейм"
            name="userName"
            type="text"
            value={name}
            onChange={(e) => setName(e.target.value)}
            required
          />
          {errors.UserName && (
            <span className="text-danger">{errors.UserName[0]}</span>
          )}
        </div>

        <div className="form-group">
          <input
            type="email"
            name="email"
            placeholder="Email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
          />
          {errors.Email && (
            <span className="text-danger">{errors.Email[0]}</span>
          )}
        </div>

        <div className="separator">
          <span></span>
        </div>

        <div className="form-group">
          <div className="password-container">
            <input
              type={showPassword ? "text" : "password"}
              name="password"
              placeholder="Пароль"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
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
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              placeholder="Повторіть пароль"
              required
            />
            <span
              className="toggle-password"
              onClick={() => setShowConfirmPassword(!showConfirmPassword)}
            >
              {showConfirmPassword ? <EyeOff size={20} /> : <Eye size={20} />}
            </span>
          </div>

          {errors.ConfirmPassword && (
            <span className="text-danger">{errors.ConfirmPassword[0]}</span>
          )}
        </div>

        <button type="submit" className="btn-auth">
          Зареєструватися
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
  );
}
