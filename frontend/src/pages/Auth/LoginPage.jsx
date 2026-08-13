import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { userNameOrEmailRule, passwordRule } from "../../utils/validationRules";
import { useSearchParams, Link } from "react-router";
import { useGoogleAuthError } from "../../hooks/useGoogleAuthError";
import { handleServerErrors } from "../../utils/formHandlers";
import GoogleLoginButton from "../../components/Auth/GoogleLoginButton";
import PasswordField from "../../components/Auth/PasswordField";
import api from "../../services/api";
import "./LoginPage.scss";

const loginSchema = z.object({
  userNameOrEmail: userNameOrEmailRule,
  password: passwordRule,
});

export default function LoginPage() {
  const [searchParams] = useSearchParams();
  const returnUrl = searchParams.get("returnUrl") || "";

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm({
    resolver: zodResolver(loginSchema), // for parsing data between react hook form and zod
  });

  useGoogleAuthError(returnUrl, setError);

  const onSubmit = async (data) => {
    try {
      const url = returnUrl
        ? `/auth/login?returnUrl=${encodeURIComponent(returnUrl)}`
        : "/auth/login";
      const response = await api.post(url, data);

      window.location.href = returnUrl ? returnUrl : "/main";
    } catch (err) {
      handleServerErrors(err, setError);
    }
  };

  return (
    <div className="auth-body">
      <div className="auth-card">
        <h2 className="title">Вхід</h2>

        <form onSubmit={handleSubmit(onSubmit)}>
          <div className="form-group">
            <input
              type="text"
              placeholder="Введіть нікнейм або email"
              className={errors.userNameOrEmail ? "error-input" : ""}
              {...register("userNameOrEmail")}
            />
            {errors.userNameOrEmail && (
              <span className="text-danger">
                {errors.userNameOrEmail.message}
              </span>
            )}
          </div>

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
            <Link to="/verify-email" className="link-reser-password">
              Забули пароль?
            </Link>
          </div>

          <button type="submit" className="btn-auth" disabled={isSubmitting}>
            {isSubmitting ? "Вхід..." : "Увійти"}
          </button>
        </form>

        <div className="separator">або</div>

        <GoogleLoginButton />

        <div className="auth-footer">
          Ще не маєте акаунта? <Link to="/register">Зареєструватися</Link>
        </div>
      </div>
    </div>
  );
}
