import React from "react";
import "./PracticeReview.scss";
import PronounceButton from "../../Shared/PronounceButton";

export default function PracticeReview({
  card,
  isFlipped,
  onFlip,
  isReversed,
}) {
  return (
    <div
      className={`flashcard ${isFlipped ? "is-flipped" : ""}`}
      onClick={onFlip}
    >
      <div className="card-face card-front">
        <div className="term-container">
          <div className="term-text">{card.term}</div>
          <PronounceButton
            word={card.term}
            lang={isReversed ? card.toLang : card.fromLang}
            size={32}
          />
        </div>
      </div>
      <div className="card-face card-back">
        <div className="def-text">{card.definition}</div>
      </div>
    </div>
  );
}
