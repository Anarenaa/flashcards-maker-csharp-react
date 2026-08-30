import { NavLink, useNavigate, useParams } from "react-router";
import { ChevronLeft } from "lucide-react";
import { useSetsListWithoutFilters } from "../../../hooks/useSetsListWithoutFilters";
import Loader from "../../../components/Shared/Loader";
import SetsGrid from "../../../components/Sets/SetsGrid";
import PaginationFooter from "../../../components/Sets/PaginationFooter";
import "./UserSetsPage.scss";

export default function UserProfilePage() {
  const { id } = useParams();
  const navigate = useNavigate();

  const { sets, isLoading, isFetching, isError, pagination, handlePageChange } =
    useSetsListWithoutFilters(`sets/public/${id}`, 12);

  const userName = sets[0]?.userName;

  if (isLoading) {
    return <Loader scale={1.2} fullHeight={true} />;
  }

  if (isError) {
    return (
      <div className="user-profile-page error-state">
        <p>Не вдалося завантажити сети користувача.</p>
        <button
          className="primary-button"
          onClick={() => navigate(`/users/${id}`)}
        >
          Повернутися назад
        </button>
      </div>
    );
  }

  return (
    <div className="user-sets-page">
      <div className="top-panel">
        <NavLink to={`/users/${id}`} className="back-link">
          <ChevronLeft size={18} />
          Повернутись
        </NavLink>
        <p className="title">
          Всі публічні сети{" "}
          {sets.length !== 0 ? (
            <span className="accent-name">@{userName}</span>
          ) : (
            "користувача"
          )}
        </p>
        <div className="top-pagination-counter">
          {`${pagination.startItem}–${pagination.endItem} з ${pagination.totalItems}`}
        </div>
      </div>

      <main className="user-sets">
        <SetsGrid
          sets={sets}
          withTop={false}
          isLoading={isLoading}
          isFetching={isFetching}
          emptyText="Сетів не знайдено"
        />
      </main>

      <PaginationFooter
        pagination={pagination}
        onPageChange={handlePageChange}
      />
    </div>
  );
}
