import { useState, useCallback, useRef } from "react";
import { useQuery, keepPreviousData } from "@tanstack/react-query";
import api from "../services/api";

export function useSetsList(endpoint) {
  const [filters, setFilters] = useState({
    searchText: "",
    categoryId: "",
    setType: "",
    fromLangCode: "",
    progress: "",
  });

  const [page, setPage] = useState(1);
  const pageSize = 20;

  // ─────────────────────────────────────────────────────────
  // ДОВІДНИКИ (categories, types)
  //
  // БУЛО (services/dictionariesCache.js, ~25 рядків):
  //   ручний модульний кеш + TTL-таймстемп для categories,
  //   вічний проміс-кеш для types, окрема функція invalidateCategories().
  //
  // СТАЛО: useQuery сам кешує за queryKey. staleTime замінює
  // наш ручний TTL. Інвалідація — queryClient.invalidateQueries(),
  // без ручного скидання таймстемпа.
  // ─────────────────────────────────────────────────────────
  const { data: categories = [] } = useQuery({
    queryKey: ["categories"],
    queryFn: () => api.get("/categories").then((res) => res.data),
    staleTime: 5 * 60 * 1000, // 5 хв — те саме, що CATEGORIES_TTL раніше
  });

  const { data: types = [] } = useQuery({
    queryKey: ["setTypes"],
    queryFn: () => api.get("/sets/types").then((res) => res.data),
    staleTime: Infinity, // "вічний" кеш — те саме, що typesPromise раніше
  });

  // ─────────────────────────────────────────────────────────
  // СПИСОК СЕТІВ
  //
  // БУЛО (useSetsList.js, ~90 рядків):
  //   useState для sets/isLoading, useRef для AbortController,
  //   ручний try/catch/finally, ручна перевірка error.name === "CanceledError".
  //
  // СТАЛО: useQuery сам робить AbortController і скасування застарілих
  // запитів — signal передається в queryFn автоматично, скасування
  // відбувається під капотом при зміні queryKey чи розмонтуванні.
  // isLoading/error теж дає з коробки.
  // ─────────────────────────────────────────────────────────
  const { data, isLoading, isFetching } = useQuery({
    // queryKey — це "адреса" кешу. Зміна filters/page/endpoint
    // автоматично означає новий запит (і скасування попереднього,
    // якщо він ще летить) — так само, як робив наш AbortController.
    queryKey: ["sets", endpoint, filters, page],
    queryFn: ({ signal }) =>
      api
        .get(endpoint, {
          signal, // React Query сама скасовує застарілі запити через це
          params: {
            page,
            perPage: pageSize,
            searchText: filters.searchText || null,
            categoryId: filters.categoryId || null,
            setType: filters.setType || null,
            fromLangCode: filters.fromLangCode || null,
            progress: filters.progress || null,
          },
        })
        .then((res) => res.data),
    // Поки вантажиться нова сторінка/фільтр, не показуємо порожній
    // стан — лишаємо попередні дані на екрані. Це заміна ручного
    // "не блимати" підходу.
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

  // ─────────────────────────────────────────────────────────
  // Все, що нижче, — та сама логіка керування UI-станом,
  // яка не змінюється залежно від того, руками ви фетчите
  // дані чи бібліотекою. React Query відповідає тільки
  // за ЗАПИТИ, не за те, як ви оновлюєте filters/page.
  // ─────────────────────────────────────────────────────────

  const lastSearchedTextRef = useRef("");

  const updateFilters = useCallback((patch, opts = {}) => {
    setFilters((prev) => {
      const updated = { ...prev, ...patch };
      if (opts.reload !== false) {
        lastSearchedTextRef.current = updated.searchText;
        setPage(1);
      }
      return updated;
    });
  }, []);

  const clearSearch = useCallback(() => {
    updateFilters(
      { searchText: "" },
      { reload: lastSearchedTextRef.current !== "" }
    );
  }, [updateFilters]);

  const handleSubmit = useCallback((e) => {
    e.preventDefault();
    setFilters((prev) => {
      lastSearchedTextRef.current = prev.searchText;
      return prev;
    });
    setPage(1);
  }, []);

  const handlePageChange = useCallback(
    (direction) => {
      setPage((prevPage) => {
        if (direction === "prev" && pagination.hasPrev) return prevPage - 1;
        if (direction === "next" && pagination.hasNext) return prevPage + 1;
        return prevPage;
      });
      window.scrollTo({ top: 0, behavior: "smooth" });
    },
    [pagination.hasPrev, pagination.hasNext]
  );

  return {
    sets,
    isLoading,      // тільки для першого завантаження без даних
    isFetching,      // фоновий рефетч — окремий прапорець для subtle-індикатора
    categories,
    types,
    filters,
    pagination,
    updateFilters,
    clearSearch,
    handleSubmit,
    handlePageChange,
  };
}