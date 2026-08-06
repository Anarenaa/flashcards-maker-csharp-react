import React from "react";
import "./PracticeSummary.scss";

export default function PracticeSummary({
  wrongCount,
  canRetry,
  onRetry,
  onRestart,
  onExit,
  onNextSession,
  isLastMode,
  isLastPage,
  isFinalMixedRound,
}) {
  return (
    <div className="summary-card">
      <div className="summary-icon">🎉</div>
      <h2>Завершено!</h2>
      {canRetry && <p className="summary-desc">Помилок: {wrongCount}</p>}

      <div className="summary-actions">
        {canRetry && (
          <button onClick={onRetry} className="btn-practice-main btn-retry">
            Виправити помилки
          </button>
        )}
        <button onClick={onRestart} className="btn-practice-main">
          Пройти спочатку
        </button>
        <button
          onClick={onNextSession}
          className="btn-practice-main btn-next-session"
        >
          {isFinalMixedRound
            ? "Повернутись до сету"
            : isLastMode && isLastPage
              ? "Повторити всі картки"
              : isLastMode
                ? "Наступні картки"
                : "Далі"}
        </button>
      </div>
    </div>
  );
}
