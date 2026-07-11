import { useState, useCallback } from "react";
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

  // searchText, який реально пішов у запит. Оновлюється тільки
  // при submit чи очищенні — на відміну від filters.searchText,
  // що міняється на кожен keystroke. Це і використовується в queryKey.
  const [appliedSearchText, setAppliedSearchText] = useState("");

  const [page, setPage] = useState(1);
  const pageSize = 20;

  const { data: categories = [] } = useQuery({
    queryKey: ["categories"],
    queryFn: () => api.get("/categories").then((res) => res.data),
    staleTime: 5 * 60 * 1000,
  });

  const { data: types = [] } = useQuery({
    queryKey: ["setTypes"],
    queryFn: () => api.get("/sets/types").then((res) => res.data),
    staleTime: Infinity,
  });

  // Ключ запиту: усі фільтри, КРІМ searchText, беруться напряму з filters
  // (вони й так змінюються "миттєво" — select одразу тригерить запит).
  // searchText свідомо береться з appliedSearchText, а не з filters,
  // щоб keystroke НЕ впливав на queryKey і НЕ тригерив автофетч.
  const queryFilters = { ...filters, searchText: appliedSearchText };

  const { data, isLoading, isFetching } = useQuery({
    queryKey: ["sets", endpoint, queryFilters, page],
    queryFn: ({ signal }) =>
      api
        .get(endpoint, {
          signal,
          params: {
            page,
            perPage: pageSize,
            searchText: queryFilters.searchText || null,
            categoryId: queryFilters.categoryId || null,
            setType: queryFilters.setType || null,
            fromLangCode: queryFilters.fromLangCode || null,
            progress: queryFilters.progress || null,
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

  // patch без searchText (селекти/таби) -> одразу оновлює filters,
  // і оскільки ці поля напряму йдуть у queryFilters — запит летить
  // автоматично через зміну queryKey. Це замінює колишній
  // "reload: true за замовчуванням".
  //
  // patch з searchText і reload:false (keystroke) -> оновлює тільки
  // filters (те, що бачить юзер в інпуті), НЕ чіпає appliedSearchText,
  // тому queryKey не змінюється і запит не летить.
  const updateFilters = useCallback((patch, opts = {}) => {
    setFilters((prev) => ({ ...prev, ...patch }));

    if (opts.reload !== false) {
      setPage(1);
    }
  }, []);

  const handleSubmit = useCallback(
    (e) => {
      e.preventDefault();
      setAppliedSearchText(filters.searchText); // ось тут пошук реально "застосовується"
      setPage(1);
    },
    [filters.searchText]
  );

  const clearSearch = useCallback(() => {
    setFilters((prev) => ({ ...prev, searchText: "" }));

    // Запит летить, тільки якщо до цього був реально застосований
    // непорожній пошук — той самий фікс, що й раніше, тільки без
    // useRef: appliedSearchText вже є станом, порівнюємо напряму.
    if (appliedSearchText !== "") {
      setAppliedSearchText("");
      setPage(1);
    }
  }, [appliedSearchText]);

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
    isLoading,
    isFetching,
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