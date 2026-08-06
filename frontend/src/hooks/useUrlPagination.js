import { useSearchParams } from "react-router";

export function useUrlPagination(defaultPageSize = 10) {
  const [searchParams, setSearchParams] = useSearchParams();

  const page = Number(searchParams.get("page")) || 1;
  const pageSizeParam = searchParams.get("pageSize");
  
  const pageSize = pageSizeParam === "all" ? "all" : Number(pageSizeParam) || defaultPageSize;

  const setPaginationParams = (newPage, newPageSize) => {
    setSearchParams({ page: newPage, pageSize: newPageSize });
  };

  return { page, pageSize, setPaginationParams };
}