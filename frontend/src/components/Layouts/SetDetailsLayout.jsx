import { useSetDetails } from "../../hooks/useSetDetails";
import SetHeader from "../Sets/Details/SetHeader";
import PageSizeSelector from "../Sets/Details/PageSizeSelector";
import CardsGrid from "../Sets/Details/CardsGrid";
import PaginationFooter from "../Sets/PaginationFooter";
import './SetDetailsLayout.scss';

export default function SetDetailsLayout({
  endpoint,
  setId,
  isMine,
  backHref,
  backLabel,
}) {
  const {
    setInfo,
    cards,
    isLoading,
    pagination,
    pageSizeOptions,
    changePageSize,
    handlePageChange,
  } = useSetDetails(endpoint, setId);

  return (
    <div className="set-details-page">
      <SetHeader
        title={setInfo?.name}
        description={setInfo?.description}
        tags={setInfo?.categories}
        lastUpdatedAt={setInfo?.lastUpdatedAt}
        backHref={backHref}
        backLabel={backLabel}
        isMine={isMine}
        onAddCard={() => {
          /* TODO: модалка додавання картки — наступний крок */
        }}
        onAddCategory={() => {
          /* TODO */
        }}
        onPractice={() => {
          /* TODO: навігація на /Practice/:id */
        }}
      />

      <PageSizeSelector
        pagination={pagination}
        options={pageSizeOptions}
        onChange={changePageSize}
      />
      
      <CardsGrid
        cards={cards}
        isLoading={isLoading}
        isMine={isMine}
        isLanguageType={setInfo?.type === 0}
        emptyText="Створіть свою першу картку"
        lang={setInfo?.fromLang}
      />

      <PaginationFooter
        pagination={pagination}
        onPageChange={handlePageChange}
      />
    </div>
  );
}
