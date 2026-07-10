import { ChevronLeft, ChevronRight } from "lucide-react";
import './PaginationFooter.scss';

export default function PaginationFooter({ pagination, onPageChange }) {
  if (pagination.totalItems <= pagination.pageSize) return null;

  return (
    <footer className="main-page-footer">
      <div className="action-pagination">
        <button
          className="pagination-button"
          type="button"
          disabled={!pagination.hasPrev}
          onClick={() => onPageChange("prev")} // useSetsList (використається в SetsPageLayout) повертає також функцію handlePageChange, яка керує стейтами попередньої та наступної сторінки а також викликає функцію оновлення списку сетів. Тут вказується лише за який напрямок відповідає кнопка
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
          onClick={() => onPageChange("next")}
        >
          <ChevronRight size={24} />
        </button>
      </div>
    </footer>
  );
}