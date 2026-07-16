import React from "react";
import { BookPlus, Loader2, Sparkles } from "lucide-react";
import CardTile from "./CardTile";
import "./CardsGrid.scss";

function CardsGrid({
  cards,
  isLoading,
  isMine,
  isLanguageType,
  lang,
  emptyText,
  loadingText = "Завантажуємо картки...",
}) {
  if (isLoading) {
    return (
      <div className="cards-grid">
        <div className="grid-status-message">
          <Loader2 className="spinner-icon" size={32} />
          <p>{loadingText}</p>
        </div>
      </div>
    );
  }

  const isEmpty = !cards || cards.length === 0;
  return (
    <div className="cards-grid">
      {isEmpty ? (
          <div className="grid-status-message empty-state">
            <Sparkles className="empty-icon" size={40} />
            <p>{emptyText}</p>
          </div>
      ) : (
        cards?.map((card) => (
        <CardTile
          key={card.id}
          card={card}
          isMine={isMine}
          isLanguageType={isLanguageType}
          lang={lang}
        />)
      ))}
    </div>
  );
}

export default React.memo(CardsGrid);
