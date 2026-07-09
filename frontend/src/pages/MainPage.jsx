import { useEffect, useState } from "react";
import {
  ChevronLeft,
  ChevronRight,
  X,
  Loader2,
  FolderSearch,
} from "lucide-react";
import { LANGUAGES } from "../constants/languages";
import SetCard from "../components/SetCard";
import api from "../services/api";
import "./MainPage.scss";

export default function MainPage() {
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
      const response = await api.get(`/sets`, {
        params: {
          page: page,
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

  const handleSubmit = (e) => {
    e.preventDefault();
    loadSets(filters, 1);
  };

  const handleTabChange = (value) => {
    const updated = { ...filters, progress: value };
    setFilters(updated);
    loadSets(updated, 1);
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

  return (
    <div className="main-page-container">
      <nav className="tabs-nav">
        {["", "notstarted", "inprogress", "completed"].map((type) => {
          const labels = {
            "": "Усі сети",
            notstarted: "Не початі",
            inprogress: "В процесі",
            completed: "Вивчені",
          };
          return (
            <button
              key={type}
              className={`tab-item ${filters.progress === type ? "active" : ""}`}
              onClick={() => handleTabChange(type)}
            >
              {labels[type]}
            </button>
          );
        })}
      </nav>

      <div className="search-container">
        <form onSubmit={handleSubmit} className="search-bar-form">
          <div className="search-row-top">
            <div className="search-input-container">
              <input
                type="text"
                placeholder="Шукати сети..."
                value={filters.searchText}
                onChange={(e) => {
                  const updated = { ...filters, searchText: e.target.value };
                  setFilters(updated);
                  if (e.target.value.trim() === "") loadSets(updated, 1);
                }}
              />
              {filters.searchText && (
                <button
                  type="button"
                  className="search-clear-icon"
                  onClick={() => {
                    const updated = { ...filters, searchText: "" };
                    setFilters(updated);
                    loadSets(updated, 1);
                  }}
                >
                  <X size={20} />
                </button>
              )}
            </div>

            <div className="top-pagination-counter">
              {`${pagination.startItem}–${pagination.endItem} з ${pagination.totalItems}`}
            </div>

            <button type="submit" className="btn-search">
              Шукати
            </button>
          </div>

          <div className="search-row-bottom">
            <select
              className="sort-select"
              value={filters.categoryId}
              onChange={(e) => {
                const updated = { ...filters, categoryId: e.target.value };
                setFilters(updated);
                loadSets(updated, 1);
              }}
            >
              <option value="">Усі категорії</option>
              {categories.map((cat) => (
                <option key={cat.id} value={cat.id}>
                  {cat.name}
                </option>
              ))}
            </select>

            <select
              className="sort-select"
              value={filters.setType}
              onChange={(e) => {
                const updated = { ...filters, setType: e.target.value };
                setFilters(updated);
                loadSets(updated, 1);
              }}
            >
              <option value="">Усі типи</option>
              {types.map((t) => (
                <option key={t.id} value={t.id}>
                  {t.name}
                </option>
              ))}
            </select>

            <select
              className="sort-select"
              value={filters.fromLangCode}
              onChange={(e) => {
                const updated = { ...filters, fromLangCode: e.target.value };
                setFilters(updated);
                loadSets(updated, 1);
              }}
            >
              <option value="">Усі мови</option>
              {LANGUAGES.map((lang) => (
                <option key={lang.code} value={lang.code}>
                  {lang.name}
                </option>
              ))}
            </select>
          </div>
        </form>
      </div>

      <div className="sets-grid">
        {isLoading ? (
          <div className="grid-status-message">
            <Loader2 className="spinner-icon" size={32} />
            <p>Шукаємо твої сети...</p>
          </div>
        ) : sets && sets.length > 0 ? (
          sets.map((set) => <SetCard key={set.id} set={set} isMine={false} />)
        ) : (
          <div className="grid-status-message empty-state">
            <FolderSearch className="empty-icon" size={48} />
            <p>Упс! Сетів із такими параметрами не знайдено</p>
          </div>
        )}
      </div>

      {pagination.totalItems > pagination.pageSize && !isLoading && (
        <footer className="main-page-footer">
          <div className="action-pagination">
            <button
              className="pagination-button"
              type="button"
              disabled={!pagination.hasPrev}
              onClick={() => handlePageChange("prev")}
            >
              <ChevronLeft size={24} />
            </button>
            <span className="pagination-current-label">
              Сторінка {pagination.currentPage}
            </span>
            <button
              className="pagination-button"
              type="button"
              disabled={!pagination.hasNext}
              onClick={() => handlePageChange("next")}
            >
              <ChevronRight size={24} />
            </button>
          </div>
        </footer>
      )}
    </div>
  );
}
