import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { handleServerErrors } from "../../../utils/formHandlers";
import { emailRule } from "../../../utils/validationRules";
import { NavLink } from "react-router";
import api from "../../../services/api";
import "../AuthShared.scss";
import "./VerifyEmailPage.scss";

const verifyEmailSchema = z.object({
  email: emailRule,
});

export default function VerifyEmailPage() {
 
  const {
    register,
    handleSubmit,
    setError,
    formState: {errors, isSubmitting}
  } = useForm({
    resolver: zodResolver(verifyEmailSchema) // for parsing data between react hook form and zod
  })

  const onSubmit = async (data) => {
    try {
      await api.post("/auth/verify-email", data);

      window.location.href = "/email-sent";
    } catch (err) {
      handleServerErrors(err, setError);
    }
  };

  return (
    <div className="auth-body">
      <div className="auth-card">
        <h2 className="title">Забули пароль?</h2>
        <p>
          Введіть ваш Email, і ми надішлемо вам посилання для відновлення
          доступу.
        </p>

        <form onSubmit={handleSubmit(onSubmit)}>
          <div className="form-field">
            <input
              type="text"
              className={errors.email ? "error-input" : ""}
              placeholder="example@mail.com"
              {...register("email")}
            />
            { errors.email && <span className="text-danger">{errors.email.message}</span> }
          </div>
          <button type="submit" className="btn-auth" disabled={isSubmitting}>
            {isSubmitting ? "Надсилається..." : "Надіслати код"} 
          </button>
        </form>

        <NavLink to="/login" className="back-link">
          ← Назад до входу
        </NavLink>
      </div>
    </div>
  );
}
