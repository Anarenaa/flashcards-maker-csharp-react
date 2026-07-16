import { MoreVertical, Volume2, BookOpen } from "lucide-react";
import "./CardTile.scss";

function formatDate(dateString) {
  if (!dateString) return null;
  return new Date(dateString).toLocaleDateString(undefined, {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
  });
}
export default function CardTile({ card, isMine, isLanguageType, lang }) {
  const handleSpeak = (e) => {
    e.stopPropagation(); // щоб клік по озвучці не зачепив можливий onClick картки в майбутньому

    // Захист для браузерів/середовищ без підтримки Web Speech API
    if (!window.speechSynthesis) return;

    const utterance = new SpeechSynthesisUtterance(card.term);
    if (lang) utterance.lang = lang; // напр. "de-DE", "en-US"

    window.speechSynthesis
      .getVoices()
      .forEach((v) => console.log(v.name, v.lang));
    window.speechSynthesis.cancel(); // перериває попередню озвучку, якщо ще звучить
    window.speechSynthesis.speak(utterance);
  };

  return (
    <div className="card-tile">
      {isMine && (
        <button type="button" className="card-tile__menu card-tile__icon">
          <MoreVertical size={18} />
        </button>
      )}
      <div className="card-tile__term-row">
        <h3>{card.term}</h3>
        <button
          type="button"
          className="card-tile__speak card-tile__icon"
          onClick={handleSpeak}
          aria-label="Прослухати вимову"
        >
          <Volume2 size={18} />
        </button>
        {isLanguageType && (
          <button type="button" className="card-tile__context card-tile__icon">
            <BookOpen size={18} />
          </button>
        )}
      </div>
      <hr class="card-tile__divider" />
      <p>{card.definition}</p>

      {/* Додати в дто поле і парсити */}
      {card.updatedAt && (
        <span className="card-tile__updated">
          Оновлено: {formatDate(card.updatedAt)}
        </span>
      )}
    </div>
  );
}
