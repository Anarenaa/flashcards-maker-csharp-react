import React from "react";
import { BookPlus, Loader2, Sparkles } from "lucide-react";
import CardTile from "./CardTile";
import "./CardsGrid.scss";
import Loader from "../../Shared/Loader";

function CardsGrid({
  cards,
  flashcardsCount,
  isLoading,
  isMine,
  isLanguageType,
  emptyText,
  loadingText = "Завантажуємо картки...",
}) {
  if (isLoading && flashcardsCount > 0) {
    return (
      <div className="cards-grid">
        <div className="grid-status-message">
          <Loader loadingText={loadingText} />
        </div>
      </div>
    );
  }

  const isEmpty = flashcardsCount === 0 || !cards || cards.length === 0;
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
        />)
      ))}
    </div>
  );
}

export default React.memo(CardsGrid);
