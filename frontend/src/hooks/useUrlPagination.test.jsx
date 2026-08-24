import { renderHook, act } from "@testing-library/react";
import { MemoryRouter } from "react-router";
import { useUrlPagination } from "./useUrlPagination";

const createWrapper = (initialEntries = ["/"]) => {
  return ({ children }) => (
    <MemoryRouter initialEntries={initialEntries}>{children}</MemoryRouter>
  );
};

it("should return default pagination values when url params are empty", () => {
  const { result } = renderHook(() => useUrlPagination(10), {
    wrapper: createWrapper(),
  });

  expect(result.current.page).toBe(1);
  expect(result.current.pageSize).toBe(10);
});

it("should parse page and pageSize from url params correctly", () => {
  const { result } = renderHook(() => useUrlPagination(10), {
    wrapper: createWrapper(["/?page=3&pageSize=25"]),
  });

  expect(result.current.page).toBe(3);
  expect(result.current.pageSize).toBe(25);
});

it("should handle 'all' pageSize parameter correctly", () => {
  const { result } = renderHook(() => useUrlPagination(10), {
    wrapper: createWrapper(["/?page=1&pageSize=all"]),
  });

  expect(result.current.pageSize).toBe("all");
});

it("should update url search params when setPaginationParams is called", () => {
  const { result } = renderHook(() => useUrlPagination(10), {
    wrapper: createWrapper(["/"]),
  });

  act(() => {
    result.current.setPaginationParams(2, 20);
  });

  expect(result.current.page).toBe(2);
  expect(result.current.pageSize).toBe(20);
});
