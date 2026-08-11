// getVoices() is synchronous but the underlying voice list often loads
// asynchronously (Chrome especially returns [] right after page load).
// This waits for the real list once, and every caller shares the same
// promise instead of each button racing its own check.

let voicesPromise = null;

export function loadVoices() {
  if (voicesPromise) return voicesPromise;

  if (!window.speechSynthesis) {
    voicesPromise = Promise.resolve([]);
    return voicesPromise;
  }

  voicesPromise = new Promise((resolve) => {
    const existing = window.speechSynthesis.getVoices();
    if (existing.length > 0) {
      resolve(existing);
      return;
    }
    window.speechSynthesis.addEventListener(
      "voiceschanged",
      () => resolve(window.speechSynthesis.getVoices()),
      { once: true },
    );
  });

  return voicesPromise;
}