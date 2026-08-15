import { useState } from "react";
import { MoreVertical, Volume2, BookOpen } from "lucide-react";
import FlashcardContextsPanel from "../../../features/flashcards/FlashcardContextsPanel";
import PronounceButton from "../../Shared/PronounceButton";
import "./CardTile.scss";

export default function CardTile({ card, isMine, isLanguageType }) {
  const [isContextModalOpen, setIsContextModalOpen] = useState(false);
  
  return (
    <>
      <div className="card-tile">
        {isMine && (
          <button type="button" className="card-tile__menu card-tile__icon">
            <MoreVertical size={18} />
          </button>
        )}
        <div className="card-tile__term-row">
          <h3>{card.term}</h3>
          <PronounceButton word={card.term} lang={card.fromLang} />
          {isLanguageType && (
            <button
              type="button"
              className="card-tile__context card-tile__icon"
              onClick={() => setIsContextModalOpen(true)}
            >
              <BookOpen size={18} />
            </button>
          )}
        </div>
        <hr className="card-tile__divider" />
        <p>{card.definition}</p>
      </div>

      {isContextModalOpen && (
        <FlashcardContextsPanel
          isOpen={isContextModalOpen}
          onClose={() => setIsContextModalOpen(false)}
          card={card}
        />
      )}
    </>
  );
}
