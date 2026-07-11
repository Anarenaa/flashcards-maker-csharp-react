import { useSetsList } from "../../hooks/useSetsList";
import FilterPanel from "../Sets/FilterPanel";
import SetsGrid from "../Sets/SetsGrid";
import PaginationFooter from "../Sets/PaginationFooter";

export default function SetsPageLayout({
  endpoint,
  isMine = false,
  loadingText,
  emptyText,
  extraCard,
}) {
  const {
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
  } = useSetsList(endpoint);

  const hasActiveFilters = Object.values(filters).some((v) => v !== "");
  const isEmpty = !sets || sets.length === 0;

  // Новий юзер на "Мої сети" без фільтрів і без сетів — не показуємо
  // "не знайдено", лишаємо тільки кнопку "+"
  const shouldShowEmptyText = !(isMine && isEmpty && !hasActiveFilters);

  return (
    <div className="main-page-container">
      <FilterPanel
        filters={filters}
        categories={categories}
        types={types}
        pagination={pagination}
        onChange={updateFilters}
        onClearSearch={clearSearch}
        onSubmit={handleSubmit}
      />

      <SetsGrid
        sets={sets}
        isLoading={isLoading} // грід ховається тільки коли даних взагалі нема
        isFetching={isFetching} // новий проп — для легкого індикатора поверх
        isMine={isMine}
        loadingText={loadingText}
        emptyText={shouldShowEmptyText ? emptyText : null}
        extraCard={isMine && filters.progress === "" ? extraCard : null}
      />

      {!isLoading && (
        <PaginationFooter
          pagination={pagination}
          onPageChange={handlePageChange}
        />
      )}
    </div>
  );
}
