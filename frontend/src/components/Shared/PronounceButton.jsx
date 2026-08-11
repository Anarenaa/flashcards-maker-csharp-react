import { useEffect, useState } from "react";
import { Volume2 } from "lucide-react";
import { loadVoices } from "../../utils/loadVoices";
import api from "../../services/api";
import "./PronounceButton.scss";

const audioCache = new Map(); // word_lang -> object URL
const pendingRequests = new Map(); // word_lang -> in-flight promise, dedupes rapid double clicks

export default function PronounceButton({ word, lang, size = 18 }) {
  // null = still checking, true/false = resolved
  const [isSupported, setIsSupported] = useState(null);
  const cacheKey = `${word}_${lang || "default"}`;

  useEffect(() => {
    let cancelled = false;

    if (!lang) {
      // No target language to match against — always use the backend fallback.
      setIsSupported(false);
      return;
    }

    loadVoices().then((voices) => {
      if (cancelled) return;
      const supported = voices.some((voice) =>
        voice.lang.toLowerCase().includes(lang.toLowerCase()),
      );
      setIsSupported(supported);
    });

    return () => {
      cancelled = true;
    };
  }, [lang]);

  const speakViaBrowser = () => {
    const utterance = new SpeechSynthesisUtterance(word);
    utterance.lang = lang;
    window.speechSynthesis.cancel(); // stop any ongoing speech to handle rapid clicks
    window.speechSynthesis.speak(utterance);
  };

  const speakViaBackend = async () => {
    const cachedUrl = audioCache.get(cacheKey);

    if (cachedUrl) {
      new Audio(cachedUrl).play();
      return;
    }

    // Dedupe: if a request for this word/lang is already in flight
    // (e.g. rapid double click), reuse it instead of firing a second one.
    let requestPromise = pendingRequests.get(cacheKey);
    if (!requestPromise) {
      requestPromise = api
        .post(
          "/tts",
          { text: word, language: lang },
          { responseType: "blob" },
        )
        .then((response) => {
          const audioUrl = URL.createObjectURL(response.data);
          audioCache.set(cacheKey, audioUrl);
          return audioUrl;
        })
        .finally(() => pendingRequests.delete(cacheKey));

      pendingRequests.set(cacheKey, requestPromise);
    }

    const audioUrl = await requestPromise;
    new Audio(audioUrl).play();
  };

  const handleSpeak = async (e) => {
    e.stopPropagation();
    
    try {
      if (isSupported) {
        speakViaBrowser();
      } else {
        await speakViaBackend();
      }
    } catch (error) {
      console.error("TTS Error:", error.globalMessage || error.message);
    }
  };

  return (
    <button
      className="speak-btn"
      onClick={handleSpeak}
      aria-label="Прослухати вимову"
      title={
        isSupported === false
          ? "Цей браузер не має вбудованої озвучки для цієї мови, тому ми використовуємо хмарний сервіс. Для необмеженого й безкоштовного озвучення рекомендуємо відкрити додаток у Microsoft Edge"
          : undefined
      }
    >
      <Volume2 size={size} />
    </button>
  );
}