import { useEffect, useState } from "react";
import api from "../services/api";

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

  const loadSets = async (currentFilters, page) => {
    setIsLoading(true);
    try {
      const response = await api.get(endpoint, {
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
      console.error(error);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    const loadInitialData = async () => {
      try {
        const [catRes, typeRes] = await Promise.all([
          api.get("/categories"),
          api.get("/sets/types"),
        ]);
        setCategories(catRes.data);
        setTypes(typeRes.data);
      } catch (error) {
        console.error(error);
      }
    };

    loadInitialData();
    loadSets(filters, 1);
  }, []);

  const updateFilters = (patch, opts = {}) => {
    const updated = { ...filters, ...patch };
    setFilters(updated);
    if (opts.reload !== false) loadSets(updated, 1);
    return updated;
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    loadSets(filters, 1);
  };

  const handlePageChange = (direction) => {
    let nextPage = pagination.currentPage;

    if (direction === "prev" && pagination.hasPrev) {
      nextPage -= 1;
    } else if (direction === "next" && pagination.hasNext) {
      nextPage += 1;
    }

    if (nextPage !== pagination.currentPage) {
      loadSets(filters, nextPage);
      window.scrollTo({ top: 0, behavior: "smooth" });
    }
  };

  return {
    sets,
    isLoading,
    categories,
    types,
    filters,
    pagination,
    updateFilters,
    handleSubmit,
    handlePageChange,
  };
}