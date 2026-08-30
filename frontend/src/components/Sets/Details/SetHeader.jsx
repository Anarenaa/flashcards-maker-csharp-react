import { useState, useEffect } from "react";
import { NavLink } from "react-router";
import { Play, ChevronLeft, X } from "lucide-react";
import { formatLocalDate } from "../../../utils/formatLocalDate";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import api from "../../../services/api";
import toast from "react-hot-toast";
import "./SetHeader.scss";

export default function SetHeader({
  setInfo,
  onBack,
  backLabel,
  isMine,
  onAddCard,
  onAddCategory,
  onPractice,
}) {
  const [isScrolled, setIsScrolled] = useState(false);

  useEffect(() => {
    const handleScroll = () => {
      if (window.scrollY > 0) {
        setIsScrolled(true);
      } else {
        setIsScrolled(false);
      }
    };

    window.addEventListener("scroll", handleScroll);
    return () => window.removeEventListener("scroll", handleScroll);
  }, []);

  const queryClient = useQueryClient();

  const removeCategoryMutation = useMutation({
    mutationFn: (catId) =>
      api.post(`my-sets/${setInfo.id}/remove-category`, null, {
        params: { categoryId: catId },
      }),
    onSuccess: () => {
      queryClient.invalidateQueries(["setInfo", setInfo.id]);
    },
  });

  return (
    <header className="set-header">
      <button
        type="button"
        className={`set-header__back-link ${isScrolled ? "hidden" : ""}`}
        onClick={onBack}
      >
        <ChevronLeft size={18} />
        {backLabel}
      </button>

      <div className="set-header__row">
        <div className={`set-header__info ${isScrolled ? "hidden" : ""}`}>
          <h1>
            {setInfo.name}
            {setInfo.isGenerated && <span>{"\u2728"}</span>}
          </h1>
          {setInfo.description && (
            <p className="set-header__description">{setInfo.description}</p>
          )}

          <div className="set-header__meta">
            <div className="set-header__tags">
              {setInfo.categories.map((tag) => (
                <span key={tag.id} className={`set-header__tag ${!isMine ? "extra-padding" : ""}`} >
                  {tag.name}
                  {isMine && (
                    <button
                      onClick={() => {
                        removeCategoryMutation.mutate(tag.id);
                      }}
                    >
                      <X size={18} className="icon" />
                    </button>
                  )}
                </span>
              ))}
            </div>
            {setInfo.lastUpdatedAt && (
              <span className="set-header__updated">
                Останнє оновлення: {formatLocalDate(setInfo.lastUpdatedAt)}
              </span>
            )}
          </div>
        </div>

        <div className="set-header__actions">
          {isMine && (
            <>
              <button
                type="button"
                className="btn-secondary add-btn"
                onClick={onAddCard}
              >
                + Картка
              </button>
              <button
                type="button"
                className="btn-secondary add-btn"
                onClick={onAddCategory}
              >
                + Категорія
              </button>
            </>
          )}
          <button
            type="button"
            className="btn-secondary btn-practice"
            onClick={onPractice}
            disabled={setInfo.flashcardsCount === 0}
          >
            <Play size={16} />
            Практикувати
          </button>
        </div>
      </div>
    </header>
  );
}
