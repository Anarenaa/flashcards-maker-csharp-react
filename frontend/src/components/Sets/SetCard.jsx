import { NavLink } from "react-router";
import { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { getCardsCountLabel } from "../../utils/getCardsCountLabel";
import { formatLocalDate } from "../../utils/formatLocalDate";
import toast from "react-hot-toast";
import DefaultProfileImage from "../Profile/DefaultProfileImage";
import ActionsDropdown from "../Shared/ActionsDropdown";
import SetForm from "../../features/sets/SetForm";
import ConfirmModal from "../Shared/ConfirmModal";
import api from "../../services/api";
import "./SetCard.scss";

export default function SetCard({ set, isMine }) {
  const baseLink = isMine ? `/my-sets/${set.id}` : `/sets/${set.id}`;
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [deletingSetId, setDeletingSetId] = useState(null);

  const queryClient = useQueryClient();

  const deleteMutation = useMutation({
    mutationFn: (setId) => api.delete(`/my-sets/${setId}`),
    onSuccess: () => {
      queryClient.invalidateQueries(["sets"]);
      setDeletingSetId(null);
      setTimeout(() => {
        toast.success("Сет успішно видалено");
      }, 200);
    },
  });

  return (
    <>
      <div className="set-card-wrapper">
        {isMine && (
          <div className="set-card-actions">
            <ActionsDropdown
              onEdit={() => setIsEditModalOpen(true)}
              onDelete={() => setDeletingSetId(set.id)}
              disabled={deleteMutation.isPending}
            />
          </div>
        )}
        <NavLink className="set-card" to={`${baseLink}?page=1&pageSize=10`}>
          {isMine ? (
            <span
              className={`privacy-status ${set.isPublic ? "public" : "private"}`}
            >
              {set.isPublic ? "🌐 Публічний" : "🔒 Приватний"}
            </span>
          ) : (
            <div className="author-wrapper">
              <div className="author-avatar-mini">
                {set.avatarUrl ? (
                  <img src={set.avatarUrl} alt={set.userName} />
                ) : (
                  <DefaultProfileImage />
                )}
              </div>
              <span className="author-name">{set.userName}</span>
            </div>
          )}
          <h3 className="set-card__title">
            {set.name}
            {set.isGenerated && <span>{"\u2728"}</span>}
          </h3>
          <div class="description-wrapper">
            <p class="set-card__description">{set.description}</p>
          </div>

          <div className="progress">
            <div className="progress__label">
              <span>Прогрес вивчення</span>
              <span>{set.progress}%</span>
            </div>
            <div className="progress__track">
              <div
                className="progress-fill"
                style={{ width: `${set.progress}%` }}
              ></div>
            </div>
          </div>

          <footer className="set-card__footer">
            <span className="flashcards-count">
              {set.flashcardsCount} {getCardsCountLabel(set.flashcardsCount)}
            </span>
            <span className="created-at">
              {formatLocalDate(set.createdAt, { withTime: false })}
            </span>
          </footer>
        </NavLink>
      </div>
      {isEditModalOpen && (
        <SetForm
          isOpen={isEditModalOpen}
          onClose={() => setIsEditModalOpen(false)}
          initialData={set}
        />
      )}
      {deletingSetId === set.id && (
        <ConfirmModal
          isOpen={Boolean(deletingSetId)}
          onConfirm={() => {
            deleteMutation.mutate(set.id);
          }}
          onCancel={() => setDeletingSetId(null)}
        />
      )}
    </>
  );
}
