import React from "react";
import { Sparkles } from "lucide-react";
import CardTile from "./CardTile";
import Loader from "../../Shared/Loader";
import "./CardsGrid.scss";

function CardsGrid({
  cards,
  setId,
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
          setId={setId}
          isMine={isMine}
          isLanguageType={isLanguageType}
        />)
      ))}
    </div>
  );
}

export default React.memo(CardsGrid);
