import { render, fireEvent, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter } from "react-router";
import { http, HttpResponse } from "msw";
import { server } from "../../../setupTests";
import SetHeader from "./SetHeader";

const renderWithProviders = (ui) => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return render(
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>{ui}</BrowserRouter>
    </QueryClientProvider>,
  );
};

it("should render set info, categories, and action buttons correctly", () => {
  const mockSetInfo = {
    id: 1,
    name: "Test Set",
    description: "Test Description",
    isGenerated: false,
    categories: [{ id: 10, name: "React" }],
    flashcardsCount: 5,
    lastUpdatedAt: "2026-01-01",
  };

  const { container } = renderWithProviders(
    <SetHeader
      setInfo={mockSetInfo}
      onBack={() => {}}
      backLabel="Back"
      isMine={true}
      onAddCard={() => {}}
      onAddCategory={() => {}}
      onPractice={() => {}}
    />,
  );

  // Check title and description
  expect(container.querySelector("h1").textContent).toBe("Test Set");
  expect(container.querySelector(".set-header__description").textContent).toBe(
    "Test Description",
  );

  // Check category tag rendering
  expect(container.querySelector(".set-header__tag").textContent).toContain(
    "React",
  );

  // Check action buttons when isMine is true
  const buttons = container.querySelectorAll("button");
  const addCardBtn = Array.from(buttons).find((b) =>
    b.textContent.includes("+ Картка"),
  );
  const addTagBtn = Array.from(buttons).find((b) =>
    b.textContent.includes("+ Категорія"),
  );

  expect(addCardBtn).not.toBeUndefined();
  expect(addTagBtn).not.toBeUndefined();

  const practiceBtn = Array.from(buttons).find((b) =>
    b.textContent.includes("Практикувати"),
  );
  expect(practiceBtn).not.toBeDisabled();
});

it("should disable practice button when flashcardsCount is 0", () => {
  const mockSetInfo = {
    id: 1,
    name: "Empty Set",
    categories: [],
    flashcardsCount: 0,
  };

  const { container } = renderWithProviders(
    <SetHeader
      setInfo={mockSetInfo}
      onBack={() => {}}
      backLabel="Back"
      isMine={false}
      onPractice={() => {}}
    />,
  );

  const practiceBtn = container.querySelector(".btn-practice");
  expect(practiceBtn).toBeDisabled();
});

it("should call onAddCard, onAddCategory, and onPractice callbacks when respective buttons are clicked", () => {
  const handleAddCard = vi.fn();
  const handleAddCategory = vi.fn();
  const handlePractice = vi.fn();
  const mockSetInfo = {
    id: 1,
    name: "Test Set",
    categories: [],
    flashcardsCount: 3,
  };

  const { container } = renderWithProviders(
    <SetHeader
      setInfo={mockSetInfo}
      onBack={() => {}}
      backLabel="Back"
      isMine={true}
      onAddCard={handleAddCard}
      onAddCategory={handleAddCategory}
      onPractice={handlePractice}
    />,
  );

  const buttons = container.querySelectorAll("button");
  const addCardBtn = Array.from(buttons).find((b) =>
    b.textContent.includes("+ Картка"),
  );
  const addCategoryBtn = Array.from(buttons).find((b) =>
    b.textContent.includes("+ Категорія"),
  );
  const practiceBtn = Array.from(buttons).find((b) =>
    b.textContent.includes("Практикувати"),
  );

  fireEvent.click(addCardBtn);
  expect(handleAddCard).toHaveBeenCalledTimes(1);

  fireEvent.click(addCategoryBtn);
  expect(handleAddCategory).toHaveBeenCalledTimes(1);

  fireEvent.click(practiceBtn);
  expect(handlePractice).toHaveBeenCalledTimes(1);
});

it("should send remove category request when clicking category remove button", async () => {
  let apiCalled = false;
  server.use(
    http.post("*/api/my-sets/1/remove-category", ({ request }) => {
      const url = new URL(request.url);
      if (url.searchParams.get("categoryId") === "10") {
        apiCalled = true;
      }
      return HttpResponse.json({ success: true }, { status: 200 });
    }),
  );

  const mockSetInfo = {
    id: 1,
    name: "Test Set",
    categories: [{ id: 10, name: "React" }],
    flashcardsCount: 2,
  };

  const { container } = renderWithProviders(
    <SetHeader
      setInfo={mockSetInfo}
      onBack={() => {}}
      backLabel="Back"
      isMine={true}
      onAddCard={() => {}}
      onAddCategory={() => {}}
      onPractice={() => {}}
    />,
  );

  const removeCategoryBtn = container.querySelector(".set-header__tag button");
  fireEvent.click(removeCategoryBtn);

  await waitFor(() => {
    expect(apiCalled).toBe(true);
  });
});
