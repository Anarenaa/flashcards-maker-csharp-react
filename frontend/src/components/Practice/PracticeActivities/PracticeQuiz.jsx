import React from "react";
import PronounceButton from "../../Shared/PronounceButton";
import "./PracticeQuiz.scss";

// навіщо тут flashErrors?
export default function PracticeQuiz({
  card,
  flashcards,
  shuffle,
  flashErrors,
  onAnswer,
  isReversed
}) {
  const correct = card.definition;

  return (
    <div className="flashcard-static">
      <div className="card-face-static">
        <div className="term-container">
          <div className="term-text">{card.term}</div>
          <PronounceButton
            word={card.term}
            lang={isReversed ? card.toLang : card.fromLang}
            size={32}
          />
        </div>
        <div className="quiz-options">
          {card.options.map((opt, idx) => {
            const status = flashErrors[idx];
            return (
              <button
                key={idx}
                className={`btn-practice-outline quiz-btn ${
                  status === "correct"
                    ? "quiz-correct"
                    : status === "wrong"
                      ? "quiz-wrong"
                      : ""
                }`}
                onClick={() => onAnswer(opt === correct, idx)}
              >
                {opt}
              </button>
            );
          })}
        </div>
      </div>
    </div>
  );
}
