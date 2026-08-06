import { useState, useEffect, useRef } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import api from "../services/api";

const shuffle = (arr) => [...arr].sort(() => Math.random() - 0.5);

/**
 * The entire logic of the practice session: starting the session on the backend,
 * building the task queue (taskQueue), navigating through it, saving
 * results, and finally saving progress.
 *
 * The component using the hook remains "dumb" — it simply renders
 * what the hook provides.
 */
export function usePracticeSession({
  setId,
  cards: initialFlashcards,
  isCardsLoading,
  globalMode,
  isReversed,
}) {
  const [taskQueue, setTaskQueue] = useState([]);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [isRestart, setIsRestart] = useState(false);
  // Set is a collection with unique values
  const [wrongIds, setWrongIds] = useState(new Set());
  const [isFinished, setIsFinished] = useState(false);
  const [isLoaded, setIsLoaded] = useState(false);
  const [cardsToProcess, setCardsToProcess] = useState([]);
  const resultsRef = useRef([]);

  const queryClient = useQueryClient();

  const saveMutation = useMutation({
    mutationFn: (data) => api.post("/practice/results", data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["set-progress", setId] });
      queryClient.invalidateQueries({ queryKey: ["sets"] });
    },
  });

  const {
    data: sessionData,
    isLoading: isSessionLoading,
    isFetching: isSessionFetching,
    isError: isSessionError,
    refetch: retryStartSession,
  } = useQuery({
    queryKey: ["practice-session", setId, globalMode, isReversed, initialFlashcards.length],
    queryFn: () =>
      api
        .post(`/sets/${setId}/sessions`, initialFlashcards, {
          params: { mode: globalMode, isReversed },
        })
        .then((res) => res.data),
    enabled: !isCardsLoading && initialFlashcards.length > 0,
    staleTime: Infinity,
  });

  // Populates taskQueue. For matching (mode 3), cards are grouped into batches of 5
  // per task; for other modes, each card constitutes a separate task.
  const buildQueue = (cards, mode) => {
    const tasks =
      mode === 3
        ? Array.from({ length: Math.ceil(cards.length / 5) }, (_, i) => ({
            assignedMode: 3,
            batch: cards.slice(i * 5, i * 5 + 5),
          }))
        : shuffle(cards).map((card) => ({ assignedMode: mode, card }));

    setTaskQueue(mode === 3 ? tasks : shuffle(tasks));
    setCurrentIndex(0);
  };

  useEffect(() => {
    if (isCardsLoading || isSessionLoading) {
      setIsLoaded(false);
      return;
    }

    if (initialFlashcards.length === 0) {
      setIsLoaded(true);
      setIsFinished(true);
      return;
    }

    if (sessionData) {
      if (sessionData.completed) {
        setIsFinished(true);
        setIsLoaded(true);
        return;
      }

      const cards = sessionData.flashcards || [];
      const mode = sessionData.selectedActivity ?? globalMode;

      setIsFinished(false);
      setCurrentIndex(0);
      setWrongIds(new Set());
      resultsRef.current = [];
      setTaskQueue([]);
      setIsRestart(false);
      setCardsToProcess(cards);
      buildQueue(cards, mode);
      setIsLoaded(true);
    }
  }, [isCardsLoading, isSessionLoading, sessionData, globalMode]);

  const saveStep = (id, correct, mode) => {
    resultsRef.current.push({ FlashcardId: id, IsCorrect: correct, ReviewType: globalMode === 5 ? 5 : mode });
  };

  const nextTask = () => {
    if (currentIndex + 1 >= taskQueue.length) {
      setIsFinished(true);
      if (!isRestart) {
        saveMutation.mutate(resultsRef.current);
      }
    } else {
      setCurrentIndex(i => i + 1);
    }
  };

  const markWrong = (id) => setWrongIds((prev) => new Set(prev).add(id));

  const retry = () => {
    const wrongCards = cardsToProcess.filter((c) => wrongIds.has(c.id));
    
    setWrongIds(new Set());
    resultsRef.current = [];
    setIsFinished(false);

    if (wrongCards.length > 0) {
      // build the queue locally to avoid an unnecessary backend request with the same output
      buildQueue(wrongCards, globalMode);
    } else {
      // fallback: request session from the backend if local cards are missing
      retryStartSession();
    }
  };

  const restart = () => {
    setWrongIds(new Set());
    resultsRef.current = [];
    setIsFinished(false);
    setIsRestart(true);
    buildQueue(cardsToProcess, globalMode);
  };

  const finishAndSave = () => {
    if (resultsRef.current.length > 0 && !isFinished) 
      saveMutation.mutate(resultsRef.current);
  };
  const retrySave = () => {
    if (resultsRef.current.length > 0) {
      saveMutation.mutate(resultsRef.current);
    }
  };

  return {
    isReady: isLoaded && !isSessionFetching,
    isError: isSessionError,
    currentTask: taskQueue[currentIndex],
    currentIndex,
    totalTasks: taskQueue.length,
    isFinished,
    wrongCount: wrongIds.size,
    isSaving: saveMutation.isPending,
    saveError: saveMutation.isError,
    retryStartSession,
    shuffle,
    saveStep,
    markWrong,
    nextTask,
    goToPrev: () => currentIndex > 0 && setCurrentIndex((i) => i - 1),
    retry,
    restart,
    finishAndSave,
    retrySave
  };
}