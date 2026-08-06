import React, { useEffect, useRef } from "react";
import { KeyboardIcon } from "lucide-react";
import Keyboard from "react-simple-keyboard";
import "react-simple-keyboard/build/css/index.css";
import { useVirtualKeyboard } from "../../../hooks/useVirtualKeyboard";
import "./PracticeWriting.scss";

export default function PracticeWriting({
  card,
  inputVal,
  setInputVal,
  isChecked,
  onCheck,
  isReversed,
}) {
  // ---------------------------------------------------------------------
  // Answer check state
  // ---------------------------------------------------------------------

  const correctTerm = card.term;
  const isCorrect = inputVal.trim().toLowerCase() === correctTerm.toLowerCase();

  // ---------------------------------------------------------------------
  // Physical input focus
  // ---------------------------------------------------------------------

  const inputRef = useRef(null);

  useEffect(() => {
    if (!isChecked && inputRef.current) {
      inputRef.current.focus();
    }
  }, [isChecked]);

  // ---------------------------------------------------------------------
  // Virtual keyboard
  // ---------------------------------------------------------------------

  const {
    keyboardRef,
    showKeyboard,
    setShowKeyboard,
    keyboardLayoutName,
    keyboardLangName,
    layout,
    handleKeyPress,
    syncCursorFromDom,
  } = useVirtualKeyboard(
    card,
    isReversed,
    inputVal,
    setInputVal,
    onCheck,
    inputRef,
  );

  // ---------------------------------------------------------------------
  // Render
  // ---------------------------------------------------------------------

  return (
    <div className="flashcard-static card-face-static">
      <div className={`term-text ${card.definition.length > 50 ? "long-text" : ""}`}>{card.definition}</div>

      <div className="input-zone">
        <div className="input-field">
          <input
            ref={inputRef}
            type="text"
            lang={keyboardLangName}
            className={`text-input ${
              isChecked ? (isCorrect ? "input-correct" : "input-wrong") : ""
            }`}
            autoComplete="off"
            placeholder="Введіть відповідь..."
            value={inputVal}
            disabled={isChecked}
            onChange={(e) => setInputVal(e.target.value)}
            onClick={syncCursorFromDom}
            onKeyUp={syncCursorFromDom}
            onSelect={syncCursorFromDom}
            onKeyDown={(e) => {
              if (e.key === "Enter") onCheck();
            }}
          />
          <button
            type="button"
            className="keyboard-toggle-btn"
            // not taking focus away from input
            onMouseDown={(e) => e.preventDefault()}
            onClick={() => setShowKeyboard(!showKeyboard)}
          >
            <KeyboardIcon size={28} />
          </button>
        </div>
        {isChecked && !isCorrect && (
        <div className="input-feedback">
          Правильно: <span className="correct-val">{correctTerm}</span>
        </div>
        )}
      </div>

      {showKeyboard && layout && (
        <div className="virtual-keyboard">
          <Keyboard
            keyboardRef={(r) => (keyboardRef.current = r)}
            layout={layout}
            layoutName={keyboardLayoutName}
            input={inputVal}
            onKeyPress={handleKeyPress}
            preventMouseDownDefault={true}
            theme="hg-theme-default myApp-keyboard-theme"
          />
        </div>
      )}

      <button
        className="primary-button btn-check"
        onClick={onCheck}
      >
        Перевірити
      </button>
    </div>
  );
}
