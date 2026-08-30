import { render, screen, fireEvent } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { describe, it, expect, vi } from "vitest";
import UserSetsPage from "./UserSetsPage";
import { useSetsListWithoutFilters } from "../../../hooks/useSetsListWithoutFilters";

vi.mock("../../../hooks/useSetsListWithoutFilters", () => ({
  useSetsListWithoutFilters: vi.fn(),
}));

const mockPagination = {
  currentPage: 1,
  pageSize: 12,
  totalItems: 2,
  totalPages: 1,
  startItem: 1,
  endItem: 2,
  hasPrev: false,
  hasNext: false,
};

const renderWithRouter = (initialRoute = "/users/2/sets") => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
    },
  });

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[initialRoute]}>
        <Routes>
          <Route path="/users/:id/sets" element={<UserSetsPage />} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
};

it("should render loader when loading sets", () => {
  useSetsListWithoutFilters.mockReturnValue({
    sets: [],
    isLoading: true,
    isFetching: false,
    isError: false,
    pagination: mockPagination,
    handlePageChange: vi.fn(),
  });

  renderWithRouter();

  const loader = document.querySelector(".loading-container");
  expect(loader).toBeInTheDocument();
});

it("should render user sets list and username in title correctly", () => {
  useSetsListWithoutFilters.mockReturnValue({
    sets: [
      {
        id: 101,
        name: "Основи SQL #1",
        userName: "jane_smith",
        description: "Test",
        flashcardsCount: 5,
      },
    ],
    isLoading: false,
    isFetching: false,
    isError: false,
    pagination: mockPagination,
    handlePageChange: vi.fn(),
  });

  renderWithRouter();

  expect(screen.getByText("Всі публічні сети")).toBeInTheDocument();
  expect(screen.getByText("@jane_smith")).toBeInTheDocument();
  expect(screen.getByText("Основи SQL #1")).toBeInTheDocument();
  expect(screen.getByText("1–2 з 2")).toBeInTheDocument();
});

it("should render fallback text in title when sets list is empty", () => {
  useSetsListWithoutFilters.mockReturnValue({
    sets: [],
    isLoading: false,
    isFetching: false,
    isError: false,
    pagination: {
      ...mockPagination,
      totalItems: 0,
      startItem: 0,
      endItem: 0,
    },
    handlePageChange: vi.fn(),
  });

  renderWithRouter();

  expect(screen.getByText("Всі публічні сети користувача")).toBeInTheDocument();
  expect(screen.getByText("Сетів не знайдено")).toBeInTheDocument();
});

it("should call handlePageChange when pagination is used", () => {
  const handlePageChangeMock = vi.fn();
  useSetsListWithoutFilters.mockReturnValue({
    sets: [
      { id: 101, name: "Set 1", userName: "jane_smith", flashcardsCount: 2 },
    ],
    isLoading: false,
    isFetching: false,
    isError: false,
    pagination: {
      currentPage: 1,
      pageSize: 1,
      totalItems: 2,
      totalPages: 2,
      startItem: 1,
      endItem: 1,
      hasPrev: true,
      hasNext: true,
    },
    handlePageChange: handlePageChangeMock,
  });

  renderWithRouter();

  const buttons = screen.getAllByRole("button");
  // Друга кнопка — це стрілочка вперед ("next")
  fireEvent.click(buttons[1]);

  expect(handlePageChangeMock).toHaveBeenCalledWith("next");
});
