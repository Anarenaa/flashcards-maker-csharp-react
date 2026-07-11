import { X } from "lucide-react";
import { LANGUAGES } from "../../constants/languages";
import "./FilterPanel.scss";

const TABS = [
  { value: "", label: "Усі сети" },
  { value: "notstarted", label: "Не початі" },
  { value: "inprogress", label: "В процесі" },
  { value: "completed", label: "Вивчені" },
];

export default function FilterPanel({
  filters,
  categories,
  types,
  pagination,
  onChange,
  onClearSearch,
  onSubmit,
}) {
  return (
    <>
      <div className="tabs">
        {TABS.map(({ value, label }) => (
          <button
            key={value}
            className={`tab-item ${filters.progress === value ? "active" : ""}`}
            onClick={() => onChange({ progress: value })}
          >
            {label}
          </button>
        ))}
      </div>

      <div className="search-container">
        <form onSubmit={onSubmit} className="search-bar-form">
          <div className="search-row-top">
            <div className="search-input-container">
              <input
                type="text"
                placeholder="Шукати сети..."
                value={filters.searchText}
                onChange={(e) => {
                  const value = e.target.value;
                  if (value.trim() === "") {
                    onClearSearch();
                  } else {
                    onChange({ searchText: value }, { reload: false });
                  }
                }}
              />
              {filters.searchText && (
                <button
                  type="button"
                  className="search-clear-icon"
                  onClick={onClearSearch}
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
              onChange={(e) => onChange({ categoryId: e.target.value })}
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
              onChange={(e) => onChange({ setType: e.target.value })}
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
              onChange={(e) => onChange({ fromLangCode: e.target.value })}
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
    </>
  );
}