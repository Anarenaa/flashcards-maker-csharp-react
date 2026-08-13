import { PRACTICE_MODES } from "../../../constants/practiceConstants";
import React, { useState } from "react";
import {
  useParams,
  useSearchParams,
  useLocation,
  useNavigate,
} from "react-router";
import {
  useQuery,
  useQueryClient,
  useMutation,
  keepPreviousData,
} from "@tanstack/react-query";
import toast from "react-hot-toast";
import { ArrowLeft, RefreshCcw, Trash } from "lucide-react";
import { useSetDetails } from "../../../hooks/useSetDetails";
import { useUrlPagination } from "../../../hooks/useUrlPagination";
import PracticeNode from "../../../components/Practice/PracticeNode";
import ResetProgressModal from "../../../features/practice/ResetProgressModal";
import api from "../../../services/api";
import Loader from "../../../components/Shared/Loader";
import "./PracticeMapPage.scss";

export default function PracticeMapPage() {
  const { id: setId } = useParams();
  const [searchParams, setSearchParams] = useSearchParams();
  const location = useLocation();
  const navigate = useNavigate();
  const [isModalOpen, setIsModalOpen] = useState(false);

  const isMySet = location.pathname.startsWith("/my-sets");
  const endpoint = isMySet ? "/my-sets" : "/sets";

  const { page, pageSize } = useUrlPagination();
  const isReversed = searchParams.get("isReversed") === "true";

  const {
    setInfo,
    cards: flashcards,
    isLoading: isCardsLoading,
    pagination,
  } = useSetDetails(endpoint, setId, page, pageSize);

  const getBackNavigation = () => {
    const params = `page=${page}&pageSize=${pageSize}`;
    if (isMySet) {
      return {
        url: `/my-sets/${setId}?${params}`,
        text: "Назад до моїх карток",
      };
    }
    return { url: `/sets/${setId}?${params}`, text: "Назад до перегляду" };
  };
  const backNav = getBackNavigation();

  const flashcardIds = flashcards.map((f) => f.id);

  const { data: progressData, isLoading: isProgressLoading } = useQuery({
    queryKey: ["set-progress", setId, flashcardIds],
    queryFn: () => {
      const isAll =
        pageSize === "all" || flashcards.length === pagination.totalItems;

      return isAll
        ? api.get(`/sets/${setId}/get-progress`).then((res) => res.data)
        : api
            .post(`/practice/progress-batch`, flashcardIds)
            .then((res) => res.data);
    },
    placeholderData: keepPreviousData,
    enabled: !!setId && flashcards.length > 0,
  });
  const progress = progressData?.progress ?? 0;

  const { data: limits, isLoading: isLimitsLoading } = useQuery({
    queryKey: ["practice-limits"],
    queryFn: () => api.get("/practice/get-limits").then((res) => res.data),
    staleTime: Infinity,
  });

  const handleToggleReverse = () => {
    setSearchParams({
      page,
      pageSize,
      isReversed: !isReversed,
    });
  };

  const queryClient = useQueryClient();

  const resetMutation = useMutation({
    mutationFn: (resetType) => {
      return resetType === "all"
        ? api.post(`/sets/${setId}/reset-set-progress`)
        : api.post(`/practice/reset-batch-progress`, flashcardIds);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["set-progress", setId] });
      queryClient.invalidateQueries({ queryKey: ["sets"] });
      setIsModalOpen(false);
    },
    onError: (error) => {
      toast.error("Не вдалося скинути прогрес.", {
        className: "toast-error"
      });
    },
  });

  if (isCardsLoading || !setInfo) {
    return <Loader scale={1.2} fullHeight={true} />;
  }

  if (flashcards.length === 0) {
    return (
      <div className="map-page-container">
        <p>На цій сторінці немає карток.</p>
        <button
          onClick={() =>
            navigate(
              `${endpoint}/${setId}/practice?page=1&pageSize=${pageSize}`,
            )
          }
        >
          На першу сторінку
        </button>
      </div>
    );
  }

  // before opening Map page
  if (isProgressLoading || isLimitsLoading || !progressData || !limits) {
    return <Loader scale={1.2} fullHeight={true} />;
  }

  const isUnlocked = (mode) => {
    switch (mode) {
      case 1:
        return true;
      case 2:
        return progress >= limits.reviewLimit;
      case 3:
        return progress >= limits.quizLimit;
      case 4:
        return progress >= limits.matchingLimit;
      case 5:
        return progress >= limits.writingLimit;
      default:
        return false;
    }
  };
  const getModeName = (mode) => {
    return PRACTICE_MODES.find((m) => m.id === mode)?.name || "";
  };

  return (
    <div className="map-page-container">
      <div className="map-top-bar">
        <button onClick={() => navigate(backNav.url)} className="back-link">
          <ArrowLeft />
          {backNav.text}
        </button>

        <div className="map-top-actions">
          <button
            type="button"
            onClick={handleToggleReverse}
            className={`btn-top btn-top--reverse ${isReversed ? "btn-top--reverse-active" : ""}`}
          >
            <span className="reverse-label">Зворотний режим</span>
            <div className="reverse-icon-bg">
              <RefreshCcw size={16} />
            </div>
          </button>

          <button
            onClick={() => setIsModalOpen(true)}
            className="btn-top--reset btn-top"
          >
            <span>Скинути прогрес</span>
            <div className="reset-icon-bg">
              <Trash size={16} />
            </div>
          </button>
        </div>
      </div>
      {isModalOpen && (
        <ResetProgressModal
          isOpen={isModalOpen}
          onConfirmBatch={() => resetMutation.mutate()}
          onConfirmAll={() => resetMutation.mutate("all")}
          onClose={() => setIsModalOpen(false)}
        />
      )}

      <div className="progress-header">
        <h2>{setInfo.name}</h2>
        <div className="flashcards-count">
          {`(${pagination.startItem}–${pagination.endItem} з ${pagination.totalItems} карток)`}
        </div>
        <div className="percentage">{Math.round(progress * 100)}%</div>
        <p style={{ opacity: 0.5 }}>Рівень володіння набором</p>
      </div>

      <div className="roadmap">
        {PRACTICE_MODES.map((mode, i) => (
          <PracticeNode
            key={mode.id}
            modeId={mode.id}
            unlocked={isUnlocked(mode.id)}
            nextWillBeUnlocked={
              i < PRACTICE_MODES.length - 1 &&
              isUnlocked(PRACTICE_MODES[i + 1].id)
            }
            isLast={i === PRACTICE_MODES.length - 1}
            getModeName={getModeName}
            setId={setId}
            isMySet={isMySet}
            isReversed={isReversed}
            page={pagination.currentPage}
            pageSize={pageSize}
          />
        ))}
      </div>
    </div>
  );
}
