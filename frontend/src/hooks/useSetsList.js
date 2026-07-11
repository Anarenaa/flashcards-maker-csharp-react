import { useEffect, useRef, useState, useCallback } from "react";
import api from "../services/api";
import { getCategories, getTypes } from "../services/dictionariesCache";

export function useSetsList(endpoint) {
  const [sets, setSets] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [categories, setCategories] = useState([]);
  const [types, setTypes] = useState([]);

  const [filters, setFilters] = useState({
    searchText: "",
    categoryId: "",
    setType: "",
    fromLangCode: "",
    progress: "",
  });

  const [pagination, setPagination] = useState({
    currentPage: 1,
    pageSize: 20,
    totalItems: 0,
    startItem: 0,
    endItem: 0,
    hasPrev: false,
    hasNext: false,
  });

  // Контролер останнього запиту — потрібен, щоб скасовувати
  // застарілі запити при швидкій зміні фільтрів (race condition).
  const abortRef = useRef(null);

  // Пошуковий текст, з яким був реально відправлений останній запит
  // (а не те, що зараз в інпуті). Потрібен, щоб не робити зайвий
  // запит, коли юзер щось надрукував, не шукав, і стер назад.
  const lastSearchedTextRef = useRef("");

  const loadSets = useCallback(
    async (currentFilters, page) => {
      lastSearchedTextRef.current = currentFilters.searchText;

      abortRef.current?.abort();
      const controller = new AbortController();
      abortRef.current = controller;

      setIsLoading(true);
      try {
        const response = await api.get(endpoint, {
          signal: controller.signal,
          params: {
            page,
            perPage: pagination.pageSize,
            searchText: currentFilters.searchText || null,
            categoryId: currentFilters.categoryId || null,
            setType: currentFilters.setType || null,
            fromLangCode: currentFilters.fromLangCode || null,
            progress: currentFilters.progress || null,
          },
        });

        setSets(response.data.items);
        setPagination((prev) => ({
          ...prev,
          currentPage: response.data.currentPage,
          totalItems: response.data.totalItems,
          startItem: response.data.startItem,
          endItem: response.data.endItem,
          hasPrev: response.data.hasPreviousPage,
          hasNext: response.data.hasNextPage,
        }));
      } catch (error) {
        // Скасований запит — очікувана поведінка, не помилка.
        if (error.name === "CanceledError" || error.name === "AbortError") {
          return;
        }
        console.error(error);
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false);
        }
      }
    },
    [endpoint, pagination.pageSize]
  );

  useEffect(() => {
    const loadInitialData = async () => {
      try {
        const [cats, typesData] = await Promise.all([
          getCategories(),
          getTypes(),
        ]);
        setCategories(cats);
        setTypes(typesData);
      } catch (error) {
        console.error(error);
      }
    };

    loadInitialData();
    loadSets(filters, 1);

    return () => abortRef.current?.abort();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const updateFilters = useCallback(
    (patch, opts = {}) => {
      setFilters((prev) => {
        const updated = { ...prev, ...patch };
        if (opts.reload !== false) loadSets(updated, 1);
        return updated;
      });
    },
    [loadSets]
  );

  // Очищення пошуку. Робить запит тільки якщо востаннє
  // реально шукали щось непорожнє — інакше просто чистить інпут.
  const clearSearch = useCallback(() => {
    updateFilters(
      { searchText: "" },
      { reload: lastSearchedTextRef.current !== "" }
    );
  }, [updateFilters]);

  const handleSubmit = useCallback(
    (e) => {
      e.preventDefault();
      loadSets(filters, 1);
    },
    [filters, loadSets]
  );

  const handlePageChange = useCallback(
    (direction) => {
      setPagination((prevPagination) => {
        let nextPage = prevPagination.currentPage;

        if (direction === "prev" && prevPagination.hasPrev) {
          nextPage -= 1;
        } else if (direction === "next" && prevPagination.hasNext) {
          nextPage += 1;
        }

        if (nextPage !== prevPagination.currentPage) {
          loadSets(filters, nextPage);
          window.scrollTo({ top: 0, behavior: "smooth" });
        }

        return prevPagination;
      });
    },
    [filters, loadSets]
  );

  return {
    sets,
    isLoading,
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