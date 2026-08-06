import React, { useEffect, useState } from "react";
import { ArrowLeft, ArrowRight } from "lucide-react";
import PracticeReview from "./PracticeActivities/PracticeReview";
import PracticeQuiz from "./PracticeActivities/PracticeQuiz";
import PracticeMatching from "./PracticeActivities/PracticeMatching";
import PracticeWriting from "./PracticeActivities/PracticeWriting";
import "./TaskRenderer.scss";

const INSTRUCTIONS = {
  1: "Переверніть картку",
  2: "Оберіть варіант",
  3: "Знайдіть пари",
  4: "Напишіть відповідь",
};

/**
 * Renders the UI for the current task based on its type.
 * Local UI state for each activity (flip, checked answer, input)
 * resides here and resets when the task changes; this is distinct from
 * session progress, which is why it is separated from usePracticeSession.
 */
export default function TaskRenderer({
  task,
  currentIndex,
  flashcards,
  shuffle,
  saveStep,
  markWrong,
  nextTask,
  goToPrev,
  isReversed,
}) {
  const [isFlipped, setIsFlipped] = useState(false);
  const [flashErrors, setFlashErrors] = useState({});
  const [writingInput, setWritingInput] = useState("");
  const [writingChecked, setWritingChecked] = useState(false);

  useEffect(() => {
    setIsFlipped(false);
    setFlashErrors({});
    setWritingInput("");
    setWritingChecked(false);
  }, [task]);

  return (
    <>
      <div className="instruction-text">{INSTRUCTIONS[task.assignedMode]}</div>

      {task.assignedMode === 1 && (
        <div className="review-container">
          <PracticeReview
            card={task.card}
            isFlipped={isFlipped}
            onFlip={() => {
              setIsFlipped(!isFlipped);
            }}
            isReversed={isReversed}
          />
          <div className="nav-zone">
            <button
              className={`btn-nav-arrow btn-nav-arrow--left ${currentIndex === 0 ? "transparent" : ""}`}
              onClick={() => {
                setIsFlipped(false);
                setTimeout(goToPrev, 200); // wait for the flip-back CSS transition to finish
              }}
            >
              <ArrowLeft />
            </button>
            <button
              className="btn-nav-arrow btn-nav-arrow--right"
              onClick={() => {
                setIsFlipped(false);
                saveStep(task.card.id, true, 1);
                setTimeout(nextTask, 200); // wait for the flip-back CSS transition to finish
              }}
            >
              <ArrowRight />
            </button>
          </div>
        </div>
      )}

      {(task.assignedMode === 2 || task.card?.cardActivityType === 2) && (
        <PracticeQuiz
          card={task.card}
          flashcards={flashcards}
          shuffle={shuffle}
          flashErrors={flashErrors}
          onAnswer={(isCorrect, idx) => {
            setFlashErrors({ [idx]: isCorrect ? "correct" : "wrong" });
            if (!isCorrect) markWrong(task.card.id);
            saveStep(task.card.id, isCorrect, 2);
            setTimeout(nextTask, 600);
          }}
          isReversed={isReversed}
        />
      )}

      {task.assignedMode === 3 && (
        <PracticeMatching
          batch={task.batch}
          shuffle={shuffle}
          onComplete={nextTask}
          onSaveStep={saveStep}
        />
      )}

      {(task.assignedMode === 4 || task.card?.cardActivityType === 4) && (
        <PracticeWriting
          card={task.card}
          inputVal={writingInput}
          setInputVal={setWritingInput}
          isChecked={writingChecked}
          onCheck={() => {
            const isCorrect =
              writingInput.trim().toLowerCase() ===
              task.card.term.toLowerCase();
            setWritingChecked(true);
            saveStep(task.card.id, isCorrect, 4);
            if (!isCorrect) {
              markWrong(task.card.id);
              setTimeout(() => {
                setWritingChecked(false);
                setWritingInput("");
                nextTask();
              }, 1800);
            } else {
              setTimeout(() => {
                setWritingChecked(false);
                setWritingInput("");
                nextTask();
              }, 800);
            }
          }}
          isReversed={isReversed}
        />
      )}
    </>
  );
}
