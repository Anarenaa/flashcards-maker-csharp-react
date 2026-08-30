import { NavLink, useNavigate, useParams } from "react-router";
import { useQuery } from "@tanstack/react-query";
import { formatLocalDate } from "../../../utils/formatLocalDate";
import { ArrowRight, ChevronLeft, Lock } from "lucide-react";
import api from "../../../services/api";
import StatsGrid from "../../../components/Profile/StatsGrid";
import DefaultProfileImage from "../../../components/Profile/DefaultProfileImage";
import Loader from "../../../components/Shared/Loader";
import "./UserProfilePage.scss";

export default function UserProfilePage() {
  const { id } = useParams();
  const navigate = useNavigate();

  const {
    data: user,
    isLoading,
    isError,
  } = useQuery({
    queryKey: ["user-data", id],
    queryFn: () => api.get(`users/${id}`).then((res) => res.data),
  });

  if (isLoading) {
    return <Loader scale={1.2} fullHeight={true} />;
  }

  if (isError) {
    return (
      <div className="user-profile-page error-state">
        <p>Не вдалося завантажити сторінку користувача.</p>
        <button className="primary-button" onClick={() => navigate("/main")}>
          Повернутися назад
        </button>
      </div>
    );
  }

  return (
    <div className={`${user?.isPublic ? "center-container" : ""}`}>
      <div className="user-profile-page">
        <NavLink to="/main" className="back-link">
          <ChevronLeft size={18} />
          На головну
        </NavLink>

        {user?.isPublic && (
          <div className="profile-header">
            <div className="avatar-section">
              <div className="profile-avatar">
                {user?.avatarUrl ? (
                  <img src={user?.avatarUrl} id="avatarPreview" alt="Avatar" />
                ) : (
                  <div className="avatar-placeholder">
                    <DefaultProfileImage />
                  </div>
                )}
              </div>
            </div>

            <table className="profile-info-table">
              <tbody className="profile-main-info">
                <tr>
                  <td className="label">Нікнейм:</td>
                  <td>
                    <span id="userNameText" className="profile-user-name">
                      {user?.userName}
                    </span>
                  </td>
                </tr>
                <tr>
                  <td className="label">Email:</td>
                  <td>
                    <span className="info-value info-value--email">
                      {user?.email}
                    </span>
                  </td>
                </tr>
                <tr>
                  <td className="label">Реєстрація:</td>
                  <td>
                    <span className="info-value info-value--date">
                      {formatLocalDate(user?.createdAt, {
                        withTime: false,
                      })}
                    </span>
                    {user?.lastActivity && (
                      <span className="info-value info-value--date">
                        {" "}
                        (остання активність{" "}
                        {formatLocalDate(user?.lastActivity, {
                          withTime: false,
                        })}
                        )
                      </span>
                    )}
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        )}

        {!user?.isPublic && (
          <div className="private-placeholder">
            <Lock size={28} />
            <p>Цей профіль є приватним. Інформація про користувача схована.</p>
          </div>
        )}

        {user?.setsCount !== 0 && user?.publicSetsCount > 0 ? (
          <NavLink to={`sets`} className="to-sets-link">
            <p className="to-sets-link__label">
              Всі публічні сети цього користувача{" "}
              <span>{user?.publicSetsCount}</span>
            </p>
            <ArrowRight size={26} className="icon" />
          </NavLink>
        ) : user?.publicSetsCount === 0 && user?.setsCount ? (
          <p className=" to-sets-link to-sets-link__label">
            Всі сети цього користувача є приватними
          </p>
        ) : null}

        {user?.isPublic && <StatsGrid user={user} columnCount={4} />}
      </div>
    </div>
  );
}
