import { useState, useRef, useEffect, useLayoutEffect } from "react";
import { SimpleKeyboardLayouts } from "simple-keyboard-layouts";
import { LANGUAGES } from "../constants/languages";

/**
 * Drives an on-screen virtual keyboard bound to a text input, in sync with
 * the card's language and reversed/normal practice direction.
 *
 * Handles:
 * - resolving the correct layout for the card's language
 * - keeping the keyboard's internal buffer in sync with physical typing
 * - shift / caps lock state
 * - key press -> input value updates, inserted at the caret position
 * - restoring the caret position after each programmatic value change
 *   (the browser resets it to the end whenever .value is set via code)
 */
export function useVirtualKeyboard(card, isReversed, inputVal, setInputVal, onCheck, inputRef) {
  // ---------------------------------------------------------------------
  // Refs
  // ---------------------------------------------------------------------

  const keyboardRef = useRef(null);

  // Lazily create a single SimpleKeyboardLayouts instance instead of
  // recreating it on every render.
  const keyboardLayoutsRef = useRef(null);
  if (!keyboardLayoutsRef.current) {
    keyboardLayoutsRef.current = new SimpleKeyboardLayouts();
  }

  const cursorPosRef = useRef(inputVal.length);
  const isProgrammaticEditRef = useRef(false);

  // ---------------------------------------------------------------------
  // State
  // ---------------------------------------------------------------------

  const [showKeyboard, setShowKeyboard] = useState(false);
  const [keyboardLayoutName, setKeyboardLayoutName] = useState("default");
  // Separate from keyboardLayoutName: caps lock persists across keys,
  // while shift is a one-shot modifier.
  const [isCapsLocked, setIsCapsLocked] = useState(false);

  // ---------------------------------------------------------------------
  // Effects
  // ---------------------------------------------------------------------

  // Keep the on-screen keyboard's internal buffer in sync when the user
  // types with a physical keyboard (react-simple-keyboard has its own
  // internal input state, separate from our controlled `inputVal`).
  useEffect(() => {
    keyboardRef.current?.setInput(inputVal);
  }, [inputVal]);

  // Reset shift/caps state whenever we move to a new card.
  useEffect(() => {
    setKeyboardLayoutName("default");
    setIsCapsLocked(false);
    cursorPosRef.current = 0;
  }, [card.id]);


  useLayoutEffect(() => {
    if (!isProgrammaticEditRef.current || !inputRef?.current) return;
    inputRef.current.setSelectionRange(cursorPosRef.current, cursorPosRef.current);
    isProgrammaticEditRef.current = false;
  }, [inputVal, inputRef]);

  // ---------------------------------------------------------------------
  // Derived values
  // ---------------------------------------------------------------------

  const langCode = isReversed ? card.toLang : card.fromLang;
  const langEntry = LANGUAGES.find((el) => el.code === langCode);
  const keyboardLangName = langEntry?.fullCode ?? "english";

  // simple-keyboard-layouts returns { layout: { default: [...], shift: [...] } }
  const layout = keyboardLayoutsRef.current.get(keyboardLangName)?.layout;

  // ---------------------------------------------------------------------
  // Caret-aware text editing
  // ---------------------------------------------------------------------

  const syncCursorFromDom = () => {
    if (!inputRef?.current) return;
    cursorPosRef.current = inputRef.current.selectionStart ?? inputVal.length;
  };

  const insertAtCursor = (text) => {
    isProgrammaticEditRef.current = true;
    setInputVal((prevVal) => {
      const pos = cursorPosRef.current;
      const nextVal = prevVal.slice(0, pos) + text + prevVal.slice(pos);
      cursorPosRef.current = pos + text.length;
      return nextVal;
    });
  };

  const deleteAtCursor = () => {
    isProgrammaticEditRef.current = true;
    setInputVal((prevVal) => {
      const pos = cursorPosRef.current;
      if (pos === 0) return prevVal; // nothing to delete
      const nextVal = prevVal.slice(0, pos - 1) + prevVal.slice(pos);
      cursorPosRef.current = pos - 1;
      return nextVal;
    });
  };

  // ---------------------------------------------------------------------
  // Key press handling
  // ---------------------------------------------------------------------

  const toggleShift = () => {
    // Shift has no effect while caps lock is engaged.
    if (isCapsLocked) return;
    setKeyboardLayoutName((prev) => (prev === "default" ? "shift" : "default"));
  };

  const toggleCapsLock = () => {
    setIsCapsLocked((prev) => {
      const next = !prev;
      setKeyboardLayoutName(next ? "shift" : "default");
      return next;
    });
  };

  const appendChar = (char) => {
    insertAtCursor(char);

    // A one-shot shift resets after a character is typed;
    // a locked caps state persists.
    if (keyboardLayoutName === "shift" && !isCapsLocked) {
      setKeyboardLayoutName("default");
    }
  };

  const handleKeyPress = (button) => {
    switch (button) {
      case "{enter}":
        onCheck();
        return;
      case "{shift}":
        toggleShift();
        return;
      case "{lock}":
        toggleCapsLock();
        return;
      case "{space}":
        insertAtCursor(" ");
        return;
      case "{bksp}":
        deleteAtCursor();
        return;
      default:
        // Any other function key (e.g. {tab}) is intentionally ignored;
        // only single-character keys are inserted into the input.
        if (button.length === 1) {
          appendChar(button);
        }
    }
  };

  return {
    keyboardRef,
    showKeyboard,
    setShowKeyboard,
    keyboardLayoutName,
    keyboardLangName,
    layout,
    handleKeyPress,
    syncCursorFromDom
  };
}