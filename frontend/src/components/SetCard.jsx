import { useState, useEffect, useRef } from "react";
import { useNavigate } from "react-router";
import { getCardsCountLabel } from "../utils/getCardsCountLabel";
import "./SetCard.scss";

export default function SetCard({ set, isMine }) {
  const navigate = useNavigate();

  const handleCardClick = () => {
    navigate(`/sets/${set.id}`);
  };

  return (
    <div className="set-card" onClick={handleCardClick}>
      {isMine ? (
        <span className={`privacy-status ${set.isPublic ? "public" : "private"}`}>
            {set.isPublic ? "🌐 Публічний" : "🔒 Приватний"}
        </span>
      ) : (
        <div className="author-wrapper">
          <div className="author-avatar-mini">
            {set.authorAvatar ? (
              <img src={set.authorAvatar} alt={set.userName} />
            ) : (
              <span>👤</span>
            )}
          </div>
          <span className="author-name">Автор: {set.userName}</span>
        </div>
      )}
      <h3 className="set-card__title">{set.name}</h3>
      <p className="set-card__description">{set.description}</p>

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
        <span className="flashcards-count">{set.flashcardsCount} {getCardsCountLabel(set.flashcardsCount)}</span>
        <span className="created-at">
          {new Date(set.createdAt).toLocaleDateString(undefined, {
            year: "numeric",
            month: "2-digit",
            day: "2-digit",
          })}
        </span>
      </footer>
    </div>
  );
}
