import { useState, useCallback, useEffect, useRef } from "react";
import { useQuery, keepPreviousData } from "@tanstack/react-query";
import api from "../services/api";

const PAGE_SIZE_OPTIONS = [5, 10, 15, 20, 25, "all"];

export function useSetDetails(endpoint, setId) {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10); 

  const { data: setInfo, isLoading: isSetInfoLoading } = useQuery({
    queryKey: ["setInfo", endpoint, setId],
    queryFn: () => api.get(`${endpoint}/${setId}`).then((res) => res.data),
  });
  
  // paginatedFlashcards
  const { data, isLoading, isFetching } = useQuery({
    queryKey: ["setCards", endpoint, setId, page, pageSize],
    queryFn: () =>
      api
        .get(`${endpoint}/${setId}/flashcards`, {
          params: {
            page,
            pageSize: pageSize === "all" ? 0 : pageSize,
          },
        })
        .then((res) => res.data),
    placeholderData: keepPreviousData,
    enabled: !!setId,
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
        document.activeElement?.blur(); //зняли фокус з кнопки (- конфлікт скрола і рендера)
        setPage((prevPage) => {
          if (direction === "prev" && pagination.hasPrev) return prevPage - 1;
          if (direction === "next" && pagination.hasNext) return prevPage + 1;
          return prevPage;
        });
      },
      [pagination.hasPrev, pagination.hasNext]
    );

  return {
    setInfo,
    isSetInfoLoading,
    cards,
    isLoading: isLoading || isSetInfoLoading,
    isFetching,
    pagination,
    pageSize,
    pageSizeOptions: PAGE_SIZE_OPTIONS,
    changePageSize,
    handlePageChange,
  };
}