import { useState, useEffect } from "react";
import { NavLink } from "react-router";
import { Play, ChevronLeft, X } from "lucide-react";
import { formatLocalDate } from "../../../utils/formatLocalDate";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import api from "../../../services/api";
import toast from "react-hot-toast";
import "./SetHeader.scss";

export default function SetHeader({
  setId,
  title,
  description,
  flashcardsCount,
  tags = [],
  lastUpdatedAt,
  backHref,
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
      api.post(`my-sets/${setId}/remove-category`, null, { params: { categoryId: catId } }),
    onSuccess: () => {
      queryClient.invalidateQueries(["setInfo", setId]);
    },
    onError: (error) => {
      const errorMessage =
        error.response?.data?.message || error.response?.data;
      toast.error(
        typeof errorMessage === "string" ? errorMessage : "Сталася помилка",
      );
    },
  });

  return (
    <header className="set-header">
      <NavLink
        to={backHref}
        className={`set-header__back-link ${isScrolled ? "hidden" : ""}`}
      >
        <ChevronLeft size={18} />
        {backLabel}
      </NavLink>

      <div className="set-header__row">
        <div className={`set-header__info ${isScrolled ? "hidden" : ""}`}>
          <h1>{title}</h1>
          {description && (
            <p className="set-header__description">{description}</p>
          )}

          <div className="set-header__meta">
            <div className="set-header__tags">
              {tags.map((tag) => (
                <span key={tag.id} className="set-header__tag">
                  {tag.name}
                  <button>
                    <X
                      size={18}
                      className="icon"
                      onClick={() => {
                        removeCategoryMutation.mutate(tag.id);
                      }}
                    />
                  </button>
                </span>
              ))}
            </div>
            {lastUpdatedAt && (
              <span className="set-header__updated">
                Останнє оновлення: {formatLocalDate(lastUpdatedAt)}
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
            disabled={flashcardsCount === 0}
          >
            <Play size={16} />
            Практикувати
          </button>
        </div>
      </div>
    </header>
  );
}
