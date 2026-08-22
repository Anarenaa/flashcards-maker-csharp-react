import { useSetDetails } from "../../hooks/useSetDetails";
import SetHeader from "../Sets/Details/SetHeader";
import PageSizeSelector from "../Sets/Details/PageSizeSelector";
import CardsGrid from "../Sets/Details/CardsGrid";
import PaginationFooter from "../Sets/PaginationFooter";
import { useNavigate } from "react-router";
import { useEffect, useState } from "react";
import Loader from "../Shared/Loader";
import FlashcardForm from "../../features/flashcards/FlashcardForm";
import CategoryToSetForm from "../../features/sets/CategoryToSetForm";
import "./SetDetailsLayout.scss";

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
  const [isFlashcardFormOpen, setIsFlashcardFormOpen] = useState(false);
  const [isAddCategoryFormOpen, setIsAddCategoryFormOpen] = useState(false);

  if (isLoading && !setInfo) {
    return <Loader fullHeight={true} />;
  }

  return (
    <>
      <div className="set-details-page">
        <SetHeader
          setInfo={setInfo}
          backHref={backHref}
          backLabel={backLabel}
          isMine={isMine}
          onAddCard={() => {
            setIsFlashcardFormOpen(true);
          }}
          onAddCategory={() => setIsAddCategoryFormOpen(true)}
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
          setId={setId}
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
      {isFlashcardFormOpen && (
        <FlashcardForm
          isOpen={isFlashcardFormOpen}
          onClose={() => setIsFlashcardFormOpen(false)}
          setId={setId}
        />
      )}
      {isAddCategoryFormOpen && (
        <CategoryToSetForm
          isOpen={isAddCategoryFormOpen}
          onClose={() => setIsAddCategoryFormOpen(false)}
          setId={setId}
        />
      )}
    </>
  );
}
