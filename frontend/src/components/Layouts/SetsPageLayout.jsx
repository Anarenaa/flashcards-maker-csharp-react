import { useSetsList } from "../../hooks/useSetsList";
import FilterPanel from "../Sets/FilterPanel";
import SetsGrid from "../Sets/SetsGrid";
import PaginationFooter from "../Sets/PaginationFooter";

export default function SetsPageLayout({
  endpoint,
  isMine = false,
  loadingText,
  emptyText,
  extraCard
}) {
  const {
    sets,
    isLoading,
    categories,
    types,
    filters,
    pagination,
    updateFilters,
    handleSubmit,
    handlePageChange,
  } = useSetsList(endpoint);

  const hasActiveFilters = Object.values(filters).some((v) => v !== "");

  const isEmpty = !sets || sets.length === 0;
  const shouldShowSets = !(isMine && isEmpty && !hasActiveFilters); // не показує повідомлення "Сети не знайдено" (emptyText) у випадку нового юзера

  return (
    <div className="main-page-container">
      <FilterPanel
        filters={filters}
        categories={categories}
        types={types}
        pagination={pagination}
        onChange={updateFilters}
        onSubmit={handleSubmit}
      />

      <SetsGrid
        sets={sets}
        isLoading={isLoading}
        isMine={isMine}
        loadingText={loadingText}
        emptyText={shouldShowSets ? emptyText : null}
        extraCard={isMine && !hasActiveFilters ? extraCard : null}
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