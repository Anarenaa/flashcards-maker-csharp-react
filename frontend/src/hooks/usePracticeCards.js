import { useQuery, keepPreviousData } from "@tanstack/react-query";
import api from "../services/api";

export function usePracticeCards( setId, page, pageSize, flashcardsCount, { keepPrevious = true } = {}) {
  const { data, isLoading, isFetching, isError, refetch } = useQuery({
    queryKey: ["setCards", setId, page, pageSize],
    queryFn: () =>
      api
        .get(`sets/${setId}/flashcards`, {
          params: {
            page,
            pageSize: pageSize === "all" ? 0 : pageSize,
          },
        })
        .then((res) => res.data),
    placeholderData: keepPrevious ? keepPreviousData : undefined,
    enabled: Boolean(setId && (flashcardsCount === undefined || flashcardsCount > 0)),
  });

  const cards = data?.items ?? [];
  const pagination = {
    currentPage: data?.currentPage ?? 1,
    pageSize,
    totalItems: data?.totalItems ?? 0,
    startItem: data?.startItem ?? 0,
    endItem: data?.endItem ?? 0,
    hasPrev: data?.hasPreviousPage ?? false,
    hasNext: data?.hasNextPage ?? false,
  };

  return {
    cards,
    isLoading,
    isFetching,
    isError,
    refetch,
    pagination,
  };
}