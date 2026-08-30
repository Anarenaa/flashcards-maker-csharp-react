import { useSearchParams } from "react-router";

export function useUrlPagination(defaultPageSize = 10) {
  const [searchParams, setSearchParams] = useSearchParams();

  const page = Number(searchParams.get("page")) || 1;
  const pageSizeParam = searchParams.get("pageSize");
  
  const pageSize = pageSizeParam === "all" ? "all" : Number(pageSizeParam) || defaultPageSize;

  const setPaginationParams = (newPage, newPageSize) => {
    // { replace: true } — this syncs the URL to match the current page/pageSize
    // state without pushing a new history entry. Without it, every mount of
    // a page using this hook adds a duplicate history entry for what is
    // visually the same page, requiring two clicks on "back" to actually
    // leave the page (the first click just lands on the duplicate entry).
    setSearchParams({ page: newPage, pageSize: newPageSize }, { replace: true });
  };

  return { page, pageSize, setPaginationParams };
}