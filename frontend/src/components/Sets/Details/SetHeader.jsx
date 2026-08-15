import { useState, useEffect } from "react";
import { NavLink } from "react-router";
import { Play, ChevronLeft } from "lucide-react";
import "./SetHeader.scss";
import { formatLocalDate } from "../../../utils/formatLocalDate";

export default function SetHeader({
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
                </span>
              ))}
            </div>
            {lastUpdatedAt && (
              <span className="set-header__updated">
                Останнє оновлення:{" "}
                {formatLocalDate(lastUpdatedAt)}
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
