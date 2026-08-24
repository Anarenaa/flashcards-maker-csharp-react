import { renderHook, waitFor, act } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { http, HttpResponse } from "msw";
import { server } from "../setupTests";
import { useSetDetails } from "./useSetDetails";
import { useFlashcards } from "./useFlashcards";

vi.mock("./useFlashcards", () => ({
  useFlashcards: vi.fn(),
}));

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return ({ children }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
};

it("should fetch set info and handle pagination changes correctly", async () => {
  server.use(
    http.get("*/api/sets/1", () => {
      return new HttpResponse(
        JSON.stringify({ id: 1, name: "Test Set", flashcardsCount: 15 }),
        {
          status: 200,
          headers: { "Content-Type": "application/json" },
        },
      );
    }),
  );

  useFlashcards.mockImplementation(( page, pageSize) => {
    return {
      cards: [{ id: 1, term: "Apple", definition: "Яблуко" }],
      isLoading: false,
      isFetching: false,
      pagination: {
        currentPage: page,
        pageSize,
        totalItems: 15,
        hasPrev: page > 1,
        hasNext: page < 2,
      },
    };
  });

  const { result } = renderHook(() => useSetDetails("/api/sets", 1, 1, 10), {
    wrapper: createWrapper(),
  });

  await waitFor(() => {
    expect(result.current.isSetInfoLoading).toBe(false);
  });

  expect(result.current.setInfo).toEqual({
    id: 1,
    name: "Test Set",
    flashcardsCount: 15,
  });
  expect(result.current.page).toBe(1);

  act(() => {
    result.current.changePageSize(20);
  });

  expect(result.current.pageSize).toBe(20);
  expect(result.current.page).toBe(1);

  act(() => {
    result.current.handlePageChange("next");
  });

  expect(result.current.page).toBe(2);
});
