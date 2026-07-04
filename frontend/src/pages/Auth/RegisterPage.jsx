import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { 
  userNameRule, 
  emailRule, 
  passwordRule, 
  confirmPasswordRule, 
  withConfirmPassword 
} from "../../utils/validationRules";
import { useSearchParams, Link } from "react-router";
import { useGoogleAuthError } from "../../hooks/useGoogleAuthError";
import { handleServerErrors } from "../../utils/formHandlers";
import GoogleLoginButton from "../../components/GoogleLoginButton";
import PasswordField from "../../components/PasswordField";
import Alert from "../../components/Alert";
import api from "../../services/api";
import "./RegisterPage.scss";

const registerSchema = withConfirmPassword(
  z.object({
    userName: userNameRule,
    email: emailRule,
    password: passwordRule,
    confirmPassword: confirmPasswordRule,
  })
);

export default function RegisterPage() {
  const [searchParams] = useSearchParams();
  const returnUrl = searchParams.get("returnUrl") || "";

  // React Hook Form
  const { 
    register, 
    handleSubmit, 
    setError, 
    formState: { errors, isSubmitting } 
  } = useForm({
    resolver: zodResolver(registerSchema) // for parsing data between react hook form and zod
  });

  useGoogleAuthError(returnUrl, setError);

  const onSubmit = async (data) => {
    try {
      // data — { userName, email, password, confirmPassword } with successfull validation
      await api.post("/auth/register", data);

      window.location.href = "/main";
    } catch (err) {
      handleServerErrors(err, setError);
    }
  };

  return (
    <>
      <Alert
        type="error"
        message={errors.root?.serverError?.message}
        onClose={() => setError("root.serverError", { message: null })}
      />

      <div className="auth-body">
        <div className="auth-card">
          <h2 className="title">Реєстрація</h2>

          <form onSubmit={handleSubmit(onSubmit)}>
            
            <div className="form-group">
              <input
                placeholder="Вигадайте нікнейм"
                className={errors.userName ? "error-input" : ""}
                type="text"
                {...register("userName")}
              />
              {errors.userName && (
                <span className="text-danger">{errors.userName.message}</span>
              )}
            </div>

            <div className="form-group">
              <input
                type="text"
                placeholder="example@mail.com"
                className={errors.email ? "error-input" : ""}
                {...register("email")}
              />
              {errors.email && (
                <span className="text-danger">{errors.email.message}</span>
              )}
            </div>

            <hr />

            <div className="form-group">
              <PasswordField
                placeholder="Пароль"
                inputClassName={errors.password ? "error-input" : ""}
                name="password"
                register={register}
              />
              {errors.password && (
                <span className="text-danger">{errors.password.message}</span>
              )}
            </div>

            <div className="form-group">
              <PasswordField
                placeholder="Повторіть пароль"
                inputClassName={errors.confirmPassword ? "error-input" : ""}
                name="confirmPassword"
                register={register}
              />
              {errors.confirmPassword && (
                <span className="text-danger">{errors.confirmPassword.message}</span>
              )}
            </div>

            <button type="submit" className="btn-auth" disabled={isSubmitting}>
              {isSubmitting ? "Реєстрація..." : "Зареєструватися"}
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