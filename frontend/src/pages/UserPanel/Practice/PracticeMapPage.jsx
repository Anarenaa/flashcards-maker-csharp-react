import React, { useState } from "react";
import { useParams, useLocation, useNavigate } from "react-router";
import {
  useQuery,
  useQueryClient,
  useMutation,
  keepPreviousData,
} from "@tanstack/react-query";
import { ArrowLeft, RefreshCcw, Trash } from "lucide-react";
import PracticeNode from "../../../components/Practice/PracticeNode";
import api from "../../../services/api";
import "./PracticeMapPage.scss";

export default function PracticeMapPage() {
  const { id: setId } = useParams();
  const [isReversed, setIsReversed] = useState(false);
  const location = useLocation();
  console.log(location);
  const navigate = useNavigate();

  const isMySet = location.pathname.startsWith("/my-sets");

  const getBackNavigation = () => {
    if (isMySet) {
      return { url: `/my-sets/${setId}`, text: "Назад до моїх карток" };
    }
    return { url: `/sets/${setId}`, text: "Назад до перегляду" };
  };
  const backNav = getBackNavigation();

  const { data: progressData, isLoading: isProgressLoading } = useQuery({
    queryKey: ["set-progress", setId],
    queryFn: () =>
      api.get(`/sets/${setId}/get-progress`).then((res) => res.data),
    placeholderData: keepPreviousData,
    enabled: !!setId,
  });

  const { data: limits, isLoading: isLimitsLoading } = useQuery({
    queryKey: ["practice-limits"],
    queryFn: () => api.get("/practice/get-limits").then((res) => res.data),
    staleTime: Infinity,
  });

  const queryClient = useQueryClient();

  const resetMutation = useMutation({
    mutationFn: () => api.post(`/practice/reset`, null, { params: { setId } }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["set-progress", setId] });
    },
    onError: (error) => {
      console.error(error);
      alert("Не вдалося скинути прогрес.");
    },
  });

  const handleResetProgress = () => {
    if (
      !window.confirm(
        "Ви впевнені? Це обнулить увесь ваш прогрес навчання для цього сету!",
      )
    )
      return;
    resetMutation.mutate();
  };

  if (isProgressLoading || isLimitsLoading || !progressData || !limits) {
    return (
      <div style={{ textAlign: "center", marginTop: "50px" }}>
        Завантаження...
      </div>
    );
  }

  const progress = progressData.progress ?? 0;

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
    switch (mode) {
      case 1:
        return "Флеш картки";
      case 2:
        return "Вікторина";
      case 3:
        return "З'єднання";
      case 4:
        return "Письмо";
      case 5:
        return "Mixed-режим";
      default:
        return "";
    }
  };

  const visibleModes = [1, 2, 3, 4, 5];

  return (
    <div className="flex-center-wrapper">
      <div className="map-page-container">
        <div className="map-top-bar">
          <button onClick={() => navigate(backNav.url)} className="back-link">
            <ArrowLeft />
            {backNav.text}
          </button>

          <div className="map-top-actions">
            <button
              type="button"
              onClick={() => setIsReversed(!isReversed)}
              className={`btn-top btn-top--reverse ${isReversed ? "btn-top--reverse-active" : ""}`}
            >
              <span className="reverse-label">Зворотний режим</span>
              <div className="reverse-icon-bg">
                <RefreshCcw size={16} />
              </div>
            </button>

            <button
              onClick={handleResetProgress}
              className="btn-top--reset btn-top"
            >
              <span>Скинути прогрес</span>
              <div className="reset-icon-bg">
                <Trash size={16} />
              </div>
            </button>
          </div>
        </div>

        <div className="progress-header">
          <h2>{location.state?.name}</h2>
          <div className="percentage">{Math.round(progress * 100)}%</div>
          <p style={{ opacity: 0.5 }}>Рівень володіння набором</p>
        </div>

        <div className="roadmap">
          {visibleModes.map((modeId, i) => {
            const unlocked = isUnlocked(modeId);
            const nextWillBeUnlocked =
              i < visibleModes.length - 1 && isUnlocked(visibleModes[i + 1]);

            return (
              <PracticeNode
                key={modeId}
                modeId={modeId}
                unlocked={unlocked}
                nextWillBeUnlocked={nextWillBeUnlocked}
                isLast={i === visibleModes.length - 1}
                getModeName={getModeName}
                setId={setId}
                isReversed={isReversed}
                navigate={navigate}
              />
            );
          })}
        </div>
      </div>
    </div>
  );
}
