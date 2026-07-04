import { NavLink } from "react-router";
import { SendIcon } from "lucide-react";
import "../AuthShared.scss";
import "./EmailSentPage.scss";

export default function EmailSentPage() {
  return (
    <div className="auth-body">
      <div className="status-card">
        <div className="icon-box">
          <SendIcon className="icon"/>
        </div>

        <h2>Готово!</h2>

        <p>
          Ми надіслали інструкції для відновлення пароля на вашу пошту.
          <br />
          Будь ласка, перевірте скриньку.
        </p>

        <NavLink to="/login" className="btn-auth">
          Повернутися до входу
        </NavLink>
      </div>
    </div>
  );
}
