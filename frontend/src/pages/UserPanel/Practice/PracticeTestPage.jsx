import { PRACTICE_MODES } from "../../../constants/practiceConstants";
import {
  useParams,
  useSearchParams,
  useLocation,
  useNavigate,
} from "react-router";
import { ArrowLeft, AlertCircle } from "lucide-react";
import { useFlashcards } from "../../../hooks/useFlashcards";
import { usePracticeSession } from "../../../hooks/usePracticeSession";
import { useUrlPagination } from "../../../hooks/useUrlPagination";
import PracticeSummary from "../../../components/Practice/PracticeSummary";
import TaskRenderer from "../../../components/Practice/TaskRenderer";
import Loader from "../../../components/Shared/Loader";
import "./PracticeTestPage.scss";

export default function PracticeTestPage() {
  const { id: setId } = useParams();
  const [searchParams, setSearchParams] = useSearchParams();
  const location = useLocation();
  const navigate = useNavigate();

  const isMySet = location.pathname.startsWith("/my-sets");
  const endpoint = isMySet ? "/my-sets" : "/sets";

  const { page, pageSize } = useUrlPagination();
  const globalMode = Number(searchParams.get("mode"));
  const isReversed = searchParams.get("isReversed") === "true";

  const {
    cards,
    isLoading: isCardsLoading,
    isError: isCardsError,
    refetch: refetchCards,
    pagination,
  } = useFlashcards( setId, page, pageSize, undefined, {
    keepPrevious: false,
  });

  const currentModeIndex = PRACTICE_MODES.findIndex((m) => m.id === globalMode);
  const isLastMode = currentModeIndex === PRACTICE_MODES.length - 1;
  const isLastPage = !pagination.hasNext;
  const isFinalMixedRound =
    isLastMode &&
    page === 1 &&
    (pageSize === "all" || pageSize >= pagination.totalItems);

  const session = usePracticeSession({
    setId,
    cards,
    isCardsLoading,
    globalMode,
    isReversed,
  });

  const handleNextSession = () => {
    if (
      currentModeIndex !== -1 &&
      currentModeIndex < PRACTICE_MODES.length - 1
    ) {
      const nextMode = PRACTICE_MODES[currentModeIndex + 1].id;
      setSearchParams({ page, pageSize, mode: nextMode, isReversed });
    } else {
      if (isFinalMixedRound) {
        navigate(`${endpoint}/${setId}?page=1&pageSize=10`);
        return;
      }

      if (isLastPage) {
        setSearchParams({ page: 1, pageSize: "all", mode: 5, isReversed });
        return;
      }

      const firstMode = PRACTICE_MODES[0].id;
      setSearchParams({
        page: page + 1,
        pageSize,
        mode: firstMode,
        isReversed,
      });
    }
  };

  const exitPractice = () => {
    session.finishAndSave();
    navigate(
      `${endpoint}/${setId}/practice?page=${page}&pageSize=${pageSize}&isReversed=${isReversed}`,
    );
  };

  if (
    isCardsLoading ||
    (!session.isReady && !session.isError && !isCardsError)
  ) {
    return <Loader scale={1.2} fullHeight={true} />;
  }

  if (isCardsError) {
    return (
      <div className="practice-loader error-container">
        <AlertCircle size={32} />
        <p>Не вдалося завантажити картки для практики.</p>
        <div>
          <button onClick={() => refetchCards()} className="btn-nav-arrow">
            Спробувати знову
          </button>
          <button onClick={exitPractice} className="btn-exit-top">
            До мапи
          </button>
        </div>
      </div>
    );
  }

  if (session.isError) {
    return (
      <div className="practice-loader error-container">
        <AlertCircle size={32} />
        <p>Не вдалося завантажити сесію практики. Спробуйте ще раз.</p>
        <div>
          <button onClick={session.retryStartSession} className="btn-nav-arrow">
            Повторити
          </button>
          <button onClick={exitPractice} className="btn-exit-top">
            До мапи
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="practice-page">
      <div className="exit-container">
        <button onClick={exitPractice} className="btn-exit-top">
          <ArrowLeft size={20} /> До мапи
        </button>
      </div>
      {session.isFinished ? (
        session.saveError ? (
          <div className="practice-loader error-container">
            <AlertCircle size={32} />
            <p>Не вдалося зберегти результати практики.</p>
            <div>
              <button onClick={session.retrySave} className="btn-nav-arrow">
                Спробувати зберегти знову
              </button>
              <button onClick={exitPractice} className="btn-exit-top">
                До мапи
              </button>
            </div>
          </div>
        ) : (
          <PracticeSummary
            wrongCount={session.wrongCount}
            canRetry={session.wrongCount > 0}
            onRetry={session.retry}
            onRestart={session.restart}
            onExit={exitPractice}
            onNextSession={handleNextSession}
            isLastMode={isLastMode}
            isLastPage={isLastPage}
            isFinalMixedRound={isFinalMixedRound}
          />
        )
      ) : session.currentTask ? (
        <>
          <div className="top-panel">
            <div className="progress-container">
              <div
                className="p-bar-fill"
                style={{
                  width: `${((session.currentIndex + 1) / session.totalTasks) * 100}%`,
                }}
              />
            </div>
            <div className="card-counter">
              {session.currentIndex + 1} / {session.totalTasks}
            </div>
          </div>

          <TaskRenderer
            task={session.currentTask}
            currentIndex={session.currentIndex}
            flashcards={cards}
            shuffle={session.shuffle}
            saveStep={session.saveStep}
            markWrong={session.markWrong}
            nextTask={session.nextTask}
            goToPrev={session.goToPrev}
            isReversed={isReversed}
          />
        </>
      ) : (
        <div className="practice-loader">
          Немає карток для проходження. Спробуйте ще раз або додайте нові
          картки.
        </div>
      )}
    </div>
  );
}
