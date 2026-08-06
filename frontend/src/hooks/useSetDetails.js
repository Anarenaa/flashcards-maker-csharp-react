import { useState, useCallback, useEffect, useRef } from "react";
import { useQuery } from "@tanstack/react-query";
import api from "../services/api";
import { usePracticeCards } from "./usePracticeCards";

const PAGE_SIZE_OPTIONS = [5, 10, 15, 20, 25, "all"];

export function useSetDetails(
  endpoint,
  setId,
  initialPage,
  initialPageSize,
  // optional options object (not a positional bool) so calls
  // stay self-documenting and easy to extend later; the outer `= {}` lets callers omit
  // the whole options argument without a "cannot destructure undefined" crash.
  // (used by Practice pages, where page changes via setSearchParams without a remount).
  { keepPrevious = true, controlled = false } = {},
) {
  const [internalPage, setInternalPage] = useState(Number(initialPage) || 1);
  const [internalPageSize, setInternalPageSize] = useState(initialPageSize || 10);

  const page = controlled ? Number(initialPage) || 1 : internalPage;
  const pageSize = controlled ? initialPageSize || 10 : internalPageSize;

  const { data: setInfo, isLoading: isSetInfoLoading } = useQuery({
    queryKey: ["setInfo", endpoint, setId],
    queryFn: () => api.get(`${endpoint}/${setId}`).then((res) => res.data),
  });

  // paginatedFlashcards via usePracticeCards
  const {
    cards,
    isLoading: isCardsLoading,
    isFetching,
    pagination,
  } = usePracticeCards(endpoint, setId, page, pageSize, { keepPrevious });

  // Only relevant in uncontrolled mode (SetDetailsLayout). In controlled mode the caller
  // owns page/pageSize via the URL, so these are no-ops — Practice pages don't use them.
  const changePageSize = useCallback((value) => {
    if (controlled) return;
    setInternalPageSize(value);
    setInternalPage(1);
  }, [controlled]);

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
      if (controlled) return;
      document.activeElement?.blur(); //зняли фокус з кнопки (- конфлікт скрола і рендера)
      setInternalPage((prevPage) => {
        if (direction === "prev" && pagination.hasPrev) return prevPage - 1;
        if (direction === "next" && pagination.hasNext) return prevPage + 1;
        return prevPage;
      });
    },
    [controlled, pagination.hasPrev, pagination.hasNext],
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