import { Volume2 } from "lucide-react";
import "./PronounceButton.scss";

export default function PronounceButton({ word, lang, size = 18 }) {
  const handleSpeak = (e, term) => {
    e.stopPropagation(); // щоб клік по озвучці не зачепив можливий onClick картки в майбутньому

    // Захист для браузерів/середовищ без підтримки Web Speech API
    if (!window.speechSynthesis) return;

    const utterance = new SpeechSynthesisUtterance(term);
    if (lang) utterance.lang = lang; // напр. "de-DE", "en-US"
    
    window.speechSynthesis.cancel(); // перериває попередню озвучку, якщо ще звучить
    window.speechSynthesis.speak(utterance);
  };

  return (
    <button
      className="speak-btn"
      onClick={(e) => handleSpeak(e, word)}
      aria-label="Прослухати вимову"
    >
      <Volume2 size={size} />
    </button>
  );
}
