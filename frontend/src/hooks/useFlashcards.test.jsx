import { renderHook, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { http, HttpResponse } from "msw";
import { server } from "../setupTests";
import { useFlashcards } from "./useFlashcards";

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return ({ children }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
};

it("should fetch flashcards and correctly map pagination and items", async () => {
  let capturedParams = null;

  server.use(
    http.get("*/api/sets/1/flashcards", ({ request }) => {
      const url = new URL(request.url);
      capturedParams = {
        page: url.searchParams.get("page"),
        pageSize: url.searchParams.get("pageSize"),
      };
      return HttpResponse.json(
        {
          items: [
            {
              id: 1493,
              term: "bang",
              definition: "гуп",
              fromLang: "en",
              toLang: "uk",
            },
          ],
          currentPage: 1,
          pageSize: 10,
          totalItems: 19,
          totalPages: 2,
          startItem: 1,
          endItem: 10,
          hasPreviousPage: false,
          hasNextPage: true,
        },
        { status: 200 },
      );
    }),
  );

  const { result } = renderHook(() => useFlashcards(1, 1, "all", 5), {
    wrapper: createWrapper(),
  });

  await waitFor(() => {
    expect(result.current.isLoading).toBe(false);
  });

  // Check if query parameters were passed correctly ("all" should map to 0)
  expect(capturedParams).toEqual({
    page: "1",
    pageSize: "0",
  });

  // Check cards items structure and pagination mapping
  expect(result.current.cards).toEqual([
    {
      id: 1493,
      term: "bang",
      definition: "гуп",
      fromLang: "en",
      toLang: "uk",
    },
  ]);
  expect(result.current.pagination).toEqual({
    currentPage: 1,
    pageSize: "all",
    totalItems: 19,
    startItem: 1,
    endItem: 10,
    hasPrev: false,
    hasNext: true,
  });
});

it("should not execute query if flashcardsCount is 0", () => {
  let apiCalled = false;
  server.use(
    http.get("*/api/sets/2/flashcards", () => {
      apiCalled = true;
      return HttpResponse.json({ items: [] }, { status: 200 });
    }),
  );

  const { result } = renderHook(() => useFlashcards(2, 1, 10, 0), {
    wrapper: createWrapper(),
  });

  expect(result.current.isLoading).toBe(false);
  expect(result.current.cards).toEqual([]);
  expect(apiCalled).toBe(false);
});
