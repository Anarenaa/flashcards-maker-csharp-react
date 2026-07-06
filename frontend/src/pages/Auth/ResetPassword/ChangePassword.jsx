import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import {
  confirmPasswordRule,
  passwordRule,
  withConfirmPassword,
} from "../../../utils/validationRules";
import { NavLink, useSearchParams, Navigate } from "react-router";
import { handleServerErrors } from "../../../utils/formHandlers";
import api from "../../../services/api";
import Alert from "../../../components/Alert";
import PasswordField from "../../../components/PasswordField";
import "../AuthShared.scss";
import "./ChangePassword.scss";

const changePasswordSchema = z
  .object({
    newPassword: passwordRule,
    confirmPassword: confirmPasswordRule,
  })
  .refine((data) => data.newPassword === data.confirmPassword, {
    message: "Паролі не збігаються",
    path: ["confirmPassword"],
  });

export default function ChangePassword() {
  const [searchParams] = useSearchParams();
  const email = searchParams.get("email") || "";
  const token = searchParams.get("token") || "";

  if (!email || !token) {
    return <Navigate to="/login" replace />;
  }

  // React Hook Form
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm({
    resolver: zodResolver(changePasswordSchema), // for parsing data between react hook form and zod
  });

  const onSubmit = async (data) => {
    try {
      const requestBody = {
        email: email,
        token: token,
        newPassword: data.newPassword,
        confirmPassword: data.confirmPassword,
      };

      await api.post("/auth/change-password", requestBody);

      window.location.href = "/login";
    } catch (err) {
      handleServerErrors(err, setError);
    }
  };

  return (
    <>
      <Alert
        type="error"
        message={errors.root?.serverError?.message || errors.email?.message}
        onClose={() => setError("root.serverError", { message: null })}
      />
      <div className="auth-body">
        <div className="auth-card">
          <h2 className="title">Новий пароль</h2>

          <form onSubmit={handleSubmit(onSubmit)}>
            <input type="hidden" />
            <div className="text-danger"></div>
            <div className="form-group">
              <input value={email} readOnly />
            </div>
            <div className="form-group">
              <PasswordField
                placeholder="Новий пароль"
                inputClassName={errors.newPassword ? "error-input" : ""}
                name="newPassword"
                register={register}
              />
              {errors.newPassword && (
                <span className="text-danger">
                  {errors.newPassword.message}
                </span>
              )}
            </div>
            <div className="form-group">
              <PasswordField
                placeholder="Підтвердження паролю"
                inputClassName={errors.confirmPassword ? "error-input" : ""}
                name="confirmPassword"
                register={register}
              />
              {errors.confirmPassword && (
                <span className="text-danger">
                  {errors.confirmPassword.message}
                </span>
              )}
            </div>
            <button type="submit" className="btn-auth" disabled={isSubmitting}>
              {isSubmitting ? "Оновлення..." : "Оновити пароль"}
            </button>
          </form>
          <NavLink to="/login" className="back-link">
            ← Скасувати та повернутися
          </NavLink>
        </div>
      </div>
    </>
  );
}
