import { renderHook, act, waitFor } from "@testing-library/react";
import { QueryClientProvider } from "@tanstack/react-query";
import { http, HttpResponse } from "msw";
import { server, createTestQueryClient } from "../setupTests";
import { useSetsListWithFilters } from "./useSetsListWithFilters";

const mockSetsResponse = (overrides = {}) => ({
  items: [{ id: 1, name: "German A1" }],
  currentPage: 1,
  totalItems: 1,
  startItem: 1,
  endItem: 1,
  hasPreviousPage: false,
  hasNextPage: false,
  ...overrides,
});

function wrapper({ children }) {
  const queryClient = createTestQueryClient();
  return (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
}

beforeEach(() => {
  server.use(
    http.get("*/api/categories", () =>
      HttpResponse.json([{ id: 1, name: "Household" }]),
    ),
    http.get("*/api/sets/types", () =>
      HttpResponse.json([{ id: 0, name: "Words" }]),
    ),
    http.get("*/api/sets", () => HttpResponse.json(mockSetsResponse())),
  );
});

it("loads sets, categories and types right after mount", async () => {
  const { result } = renderHook(() => useSetsListWithFilters("/sets"), {
    wrapper,
  });

  await waitFor(() => expect(result.current.isLoading).toBe(false));

  expect(result.current.sets).toHaveLength(1);
  expect(result.current.categories).toEqual([{ id: 1, name: "Household" }]);
  expect(result.current.types).toEqual([{ id: 0, name: "Words" }]);
});

it("does not fire a request when searchText changes without submit", async () => {
  let requestCount = 0;
  server.use(
    http.get("*/api/sets", ({ request }) => {
      const url = new URL(request.url);
      if (url.searchParams.get("searchText")) requestCount += 1;
      return HttpResponse.json(mockSetsResponse());
    }),
  );

  const { result } = renderHook(() => useSetsListWithFilters("/sets"), {
    wrapper,
  });
  await waitFor(() => expect(result.current.isLoading).toBe(false));

  act(() => {
    result.current.updateFilters({ searchText: "German" }, { reload: false });
  });

  await new Promise((r) => setTimeout(r, 50));

  expect(result.current.filters.searchText).toBe("German");
  expect(requestCount).toBe(0);
});

it("fires exactly one request on submit — not one per letter", async () => {
  let searchRequests = [];
  server.use(
    http.get("*/api/sets", ({ request }) => {
      const url = new URL(request.url);
      const searchText = url.searchParams.get("searchText");
      if (searchText) searchRequests.push(searchText);
      return HttpResponse.json(mockSetsResponse());
    }),
  );

  const { result } = renderHook(() => useSetsListWithFilters("/sets"), {
    wrapper,
  });
  await waitFor(() => expect(result.current.isLoading).toBe(false));

  act(() => {
    result.current.updateFilters({ searchText: "German" }, { reload: false });
  });
  act(() => {
    result.current.handleSubmit({ preventDefault: () => {} });
  });

  await waitFor(() => expect(searchRequests).toContain("German"));
  expect(searchRequests).toHaveLength(1);
});

it("clearSearch does not fire a request when no search was ever applied", async () => {
  let requestCount = 0;
  server.use(
    http.get("*/api/sets", () => {
      requestCount += 1;
      return HttpResponse.json(mockSetsResponse());
    }),
  );

  const { result } = renderHook(() => useSetsListWithFilters("/sets"), {
    wrapper,
  });
  await waitFor(() => expect(result.current.isLoading).toBe(false));

  const countBeforeClear = requestCount;

  act(() => {
    result.current.updateFilters(
      { searchText: "something" },
      { reload: false },
    );
  });
  act(() => {
    result.current.clearSearch();
  });

  await new Promise((r) => setTimeout(r, 50));

  expect(requestCount).toBe(countBeforeClear);
  expect(result.current.filters.searchText).toBe("");
});

it("clearSearch fires a request when a search was previously applied", async () => {
  let requestCount = 0;
  server.use(
    http.get("*/api/sets", () => {
      requestCount += 1;
      return HttpResponse.json(mockSetsResponse());
    }),
  );

  const { result } = renderHook(() => useSetsListWithFilters("/sets"), {
    wrapper,
  });
  await waitFor(() => expect(result.current.isLoading).toBe(false));

  act(() => {
    result.current.updateFilters(
      { searchText: "something" },
      { reload: false },
    );
  });
  act(() => {
    result.current.handleSubmit({ preventDefault: () => {} });
  });
  await waitFor(() => expect(result.current.isLoading).toBe(false));

  const countAfterSubmit = requestCount;

  act(() => {
    result.current.clearSearch();
  });

  await waitFor(() => expect(requestCount).toBeGreaterThan(countAfterSubmit));
});

it("handlePageChange respects hasPrev/hasNext boundaries", async () => {
  server.use(
    http.get("*/api/sets", () =>
      HttpResponse.json(
        mockSetsResponse({
          hasPreviousPage: false,
          hasNextPage: false,
          currentPage: 1,
        }),
      ),
    ),
  );

  const { result } = renderHook(() => useSetsListWithFilters("/sets"), {
    wrapper,
  });
  await waitFor(() => expect(result.current.isLoading).toBe(false));

  act(() => {
    result.current.handlePageChange("prev");
  });
  act(() => {
    result.current.handlePageChange("next");
  });

  expect(result.current.pagination.currentPage).toBe(1);
});
