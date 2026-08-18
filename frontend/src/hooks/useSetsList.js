import { useState, useCallback, useRef, useEffect } from "react";
import { useQuery, keepPreviousData } from "@tanstack/react-query";
import api from "../services/api";
import { useSetTypes } from "./useSetTypes";
import { useCategories } from "./useCategories";

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

  const categories = useCategories();
  const types = useSetTypes();

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