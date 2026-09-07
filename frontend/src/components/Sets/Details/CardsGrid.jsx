import React from "react";
import { Sparkles } from "lucide-react";
import { useQueryClient } from "@tanstack/react-query";
import { useMutation } from "@tanstack/react-query";
import toast from "react-hot-toast";
import CardTile from "./CardTile";
import Loader from "../../Shared/Loader";
import api from "../../../services/api";
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
  page,
  pagination,
  onPageChange,
}) {

  const queryClient= useQueryClient();

  const deleteMutation = useMutation({
    mutationFn: (cardId) => api.delete(`/sets/${setId}/flashcards/${cardId}`),
    onSuccess: () => {
      queryClient.invalidateQueries(["setCards"]);

      if (pagination && onPageChange && cards.length === 1 && page > 1) {
        onPageChange(page - 1);
      }

      setTimeout(()=>{
        toast.success("Картку успішно видалено");
      }, 200);
    },
  });

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
          onDelete={(cardId) => deleteMutation.mutate(cardId)}
        />)
      ))}
    </div>
  );
}

export default React.memo(CardsGrid);
