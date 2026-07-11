import React from "react";
import { ChevronLeft, ChevronRight } from "lucide-react";

function PaginationFooter({ pagination, onPageChange }) {
  if (pagination.totalItems <= pagination.pageSize) return null;

  return (
    <footer className="main-page-footer">
      <div className="action-pagination">
        <button
          className="pagination-button"
          type="button"
          disabled={!pagination.hasPrev}
          onClick={() => onPageChange("prev")}
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

export default React.memo(PaginationFooter);