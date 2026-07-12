import '@testing-library/jest-dom';
import { beforeAll, afterEach, afterAll } from 'vitest';
import { setupServer } from 'msw/node';
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render } from "@testing-library/react";
import { BrowserRouter } from "react-router";

export const server = setupServer();

beforeAll(() => server.listen());
afterEach(() => server.resetHandlers()); // Очищає фейкові відповіді після кожного тесту
afterAll(() => server.close());

// Creates a NEW QueryClient per test — critical for isolation:
// reusing one client would leak cached data (e.g. applied search)
// between tests.
export function createTestQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
        staleTime: 0,
        gcTime: 0,
      },
    },
  });
}

// Wraps a component in the same providers used in the real app.
export function renderWithProviders(ui, { queryClient = createTestQueryClient() } = {}) {
  return {
    ...render(
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>{ui}</BrowserRouter>
      </QueryClientProvider>
    ),
    queryClient,
  };
}