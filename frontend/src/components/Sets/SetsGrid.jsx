import React from "react";
import { Loader2, FolderSearch } from "lucide-react";
import SetCard from "./SetCard";
import "./SetsGrid.scss";

function SetsGrid({
  sets,
  isLoading,
  isMine = false,
  loadingText = "Шукаємо сети...",
  emptyText = "Упс! Сетів із такими параметрами не знайдено",
  extraCard = null,
}) {
  if (isLoading) {
    return (
      <div className="sets-grid">
        <div className="grid-status-message">
          <Loader2 className="spinner-icon" size={32} />
          <p>{loadingText}</p>
        </div>
      </div>
    );
  }

  const isEmpty = !sets || sets.length === 0;

  return (
    <div className="sets-grid">
      {extraCard}
      {isEmpty ? (
        emptyText && (
          <div className="grid-status-message empty-state">
            <FolderSearch className="empty-icon" size={48} />
            <p>{emptyText}</p>
          </div>
        )
      ) : (
        sets.map((set) => <SetCard key={set.id} set={set} isMine={isMine} />)
      )}
    </div>
  );
}

// Пропускає ре-рендер, якщо жоден проп не змінився. Корисно тут,
// бо SetsGrid — "сусід" FilterPanel: коли юзер друкує в пошуку,
// міняється тільки filters у батьківському SetsPageLayout,
// а sets/isLoading лишаються тими самими.
export default React.memo(SetsGrid);