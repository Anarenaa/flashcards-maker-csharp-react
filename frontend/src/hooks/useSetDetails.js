import { useState, useCallback, useEffect, useRef } from "react";
import { useQuery } from "@tanstack/react-query";
import api from "../services/api";
import { useFlashcards } from "./useFlashcards";

const PAGE_SIZE_OPTIONS = [5, 10, 15, 20, 25, "all"];

export function useSetDetails(endpoint, setId, initialPage, initialPageSize) {
  const [page, setPage] = useState(Number(initialPage) || 1);
  const [pageSize, setPageSize] = useState(initialPageSize || 10);

  const { data: setInfo, isLoading: isSetInfoLoading } = useQuery({
    queryKey: ["setInfo", endpoint, setId],
    queryFn: () => api.get(`${endpoint}/${setId}`).then((res) => res.data),
  });

  const {
    cards,
    isLoading: isCardsLoading,
    isFetching,
    pagination,
  } = useFlashcards(setId, page, pageSize, setInfo?.flashcardsCount, { keepPrevious: true });

  const changePageSize = useCallback((value) => {
    setPageSize(value);
    setPage(1);
  }, []);

  const isFirstRender = useRef(true);
  useEffect(() => {
    if (isFirstRender.current) {
      isFirstRender.current = false;
      return;
    }
    window.scrollTo({ top: 0, behavior: "smooth" });
  }, [page]);

  const handlePageChange = useCallback(
    (direction) => {
      document.activeElement?.blur();
      setPage((prevPage) => {
        if (direction === "prev" && pagination.hasPrev) return prevPage - 1;
        if (direction === "next" && pagination.hasNext) return prevPage + 1;
        return prevPage;
      });
    },
    [pagination.hasPrev, pagination.hasNext],
  );

  return {
    setInfo,
    isSetInfoLoading,
    cards,
    isLoading: isCardsLoading || isSetInfoLoading,
    isFetching,
    pagination,
    pageSize,
    pageSizeOptions: PAGE_SIZE_OPTIONS,
    changePageSize,
    handlePageChange,
    page,
  };
}