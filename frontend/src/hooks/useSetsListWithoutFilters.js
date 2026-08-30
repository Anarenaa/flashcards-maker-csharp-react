import { useState, useCallback, useRef, useEffect } from "react";
import { useQuery, keepPreviousData } from "@tanstack/react-query";
import api from "../services/api";

export function useSetsListWithoutFilters(endpoint, pageSize = 20) {
  const [page, setPage] = useState(1);

  const { data, isLoading, isFetching, isError } = useQuery({
    queryKey: ["sets-pagination", endpoint, page],
    queryFn: ({ signal }) =>
      api
        .get(endpoint, {
          signal,
          params: {
            page,
            perPage: pageSize,
          },
        })
        .then((res) => res.data),
    placeholderData: keepPreviousData,
  });

  const sets = data?.items ?? [];
  const pagination = {
    currentPage: data?.currentPage ?? 1,
    pageSize,
    totalItems: data?.totalItems ?? 0,
    startItem: data?.startItem ?? 0,
    endItem: data?.endItem ?? 0,
    hasPrev: data?.hasPreviousPage ?? false,
    hasNext: data?.hasNextPage ?? false,
  };

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
    [pagination.hasPrev, pagination.hasNext]
  );

  return {
    sets,
    isLoading,
    isFetching,
    isError,
    pagination,
    handlePageChange,
  };
}