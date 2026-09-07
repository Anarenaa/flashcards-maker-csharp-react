import { useState } from "react";
import { BookOpen } from "lucide-react";
import FlashcardContextsPanel from "../../../features/flashcards/FlashcardContextsPanel";
import PronounceButton from "../../Shared/PronounceButton";
import ActionsDropdown from "../../Shared/ActionsDropdown";
import FlashcardForm from "../../../features/flashcards/FlashcardForm";
import ConfirmModal from "../../Shared/ConfirmModal";
import "./CardTile.scss";

export default function CardTile({ card, setId, isMine, isLanguageType, onDelete }) {
  const [isContextModalOpen, setIsContextModalOpen] = useState(false);
  const [isFlashcardFormOpen, setIsFlashcardFormOpen] = useState(false);
  const [deletedCardId, setDeletedCardId] = useState(null);

  return (
    <>
      <div className="card-tile-wrapper">
        {isMine && (
          <div className="card-tile-actions">
            <ActionsDropdown
              onEdit={() => setIsFlashcardFormOpen(true)}
              onDelete={() => {
                setDeletedCardId(card.id);
              }}
            />
          </div>
        )}
        <div className="card-tile">
          <div className="card-tile__term-row">
            <h3>{card.term}</h3>
            <PronounceButton word={card.term} lang={card.fromLang} />
            {isLanguageType && (
              <button
                type="button"
                className="card-tile__context icon"
                onClick={() => setIsContextModalOpen(true)}
              >
                <BookOpen size={18} />
              </button>
            )}
          </div>
          <hr className="card-tile__divider" />
          <p>{card.definition}</p>
        </div>
      </div>
      {isFlashcardFormOpen && (
        <FlashcardForm
          isOpen={isFlashcardFormOpen}
          onClose={() => setIsFlashcardFormOpen(false)}
          setId={setId}
          initialData={card}
        />
      )}
      {isContextModalOpen && (
        <FlashcardContextsPanel
          isOpen={isContextModalOpen}
          onClose={() => setIsContextModalOpen(false)}
          card={card}
        />
      )}
      {deletedCardId === card.id && (
        <ConfirmModal
          onConfirm={() => {
            onDelete(card.id);
            setDeletedCardId(null);
          }}
          onCancel={() => setDeletedCardId(null)}
        />
      )}
    </>
  );
}
