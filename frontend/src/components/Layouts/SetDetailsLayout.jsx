import { useSetDetails } from "../../hooks/useSetDetails";
import SetHeader from "../Sets/Details/SetHeader";
import PageSizeSelector from "../Sets/Details/PageSizeSelector";
import CardsGrid from "../Sets/Details/CardsGrid";
import PaginationFooter from "../Sets/PaginationFooter";
import "./SetDetailsLayout.scss";
import { useNavigate } from "react-router";
import { useEffect } from "react";

export default function SetDetailsLayout({
  endpoint,
  setId,
  initialPage,
  initialPageSize,
  onParamsChange,
  isMine,
  backHref,
  backLabel,
  onPracticeLink,
}) {
  const {
    setInfo,
    cards,
    isLoading,
    pagination,
    pageSize,
    pageSizeOptions,
    changePageSize,
    handlePageChange,
    page,
  } = useSetDetails(endpoint, setId, initialPage, initialPageSize);

  // sync url params with user actions
  useEffect(() => {
    if (onParamsChange) {
      onParamsChange(page, pageSize);
    }
  }, [page, pageSize]);

  const navigate = useNavigate();
  return (
    <div className="set-details-page">
      <SetHeader
        title={setInfo?.name}
        description={setInfo?.description}
        flashcardsCount={setInfo?.flashcardsCount}
        tags={setInfo?.categories}
        lastUpdatedAt={setInfo?.lastUpdatedAt}
        backHref={backHref}
        backLabel={backLabel}
        isMine={isMine}
        onAddCard={() => {
          /* TODO: модалка додавання картки */
        }}
        onAddCategory={() => {
          /* TODO */
        }}
        onPractice={() => {
          navigate(
            `${onPracticeLink}?page=${pagination.currentPage}&pageSize=${pagination.pageSize}`,
          );
        }}
      />

      <PageSizeSelector
        pagination={pagination}
        options={pageSizeOptions}
        onChange={changePageSize}
      />

      <CardsGrid
        cards={cards}
        flashcardsCount={setInfo?.flashcardsCount}
        isLoading={isLoading}
        isMine={isMine}
        isLanguageType={setInfo?.type === 0}
        emptyText="Створіть свою першу картку"
      />

      <PaginationFooter
        pagination={pagination}
        onPageChange={handlePageChange}
      />
    </div>
  );
}
