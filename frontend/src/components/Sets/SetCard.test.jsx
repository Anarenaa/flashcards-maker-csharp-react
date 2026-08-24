import { render, fireEvent, waitFor } from "@testing-library/react";
import { BrowserRouter } from "react-router";
import { http, HttpResponse } from "msw";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { server } from "../../setupTests";
import SetCard from "./SetCard";

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

// SetCard isolation
vi.mock("../Shared/ActionsDropdown", () => ({
  default: ({ onEdit, onDelete }) => (
    <div data-testid="actions-dropdown">
      <button data-testid="edit-btn" onClick={onEdit}>
        Edit
      </button>
      <button data-testid="delete-btn" onClick={onDelete}>
        Delete
      </button>
    </div>
  ),
}));

vi.mock("../../features/sets/SetForm", () => ({
  default: ({ isOpen }) =>
    isOpen ? <div data-testid="edit-modal">Edit Form Modal</div> : null,
}));

vi.mock("../Shared/ConfirmModal", () => ({
  default: ({ isOpen, onConfirm, onCancel }) =>
    isOpen ? (
      <div data-testid="confirm-modal">
        <button data-testid="confirm-delete-btn" onClick={onConfirm}>
          Confirm
        </button>
        <button data-testid="cancel-delete-btn" onClick={onCancel}>
          Cancel
        </button>
      </div>
    ) : null,
}));

const mockMineSet = {
  id: 1,
  name: "Мій тестовий сет",
  description: "Опис мого сету",
  isPublic: true,
  isGenerated: false,
  progress: 50,
  flashcardsCount: 10,
  createdAt: "2026-08-01T10:00:00Z",
};

const mockPublicSet = {
  id: 2,
  name: "Чужий публічний сет",
  description: "Опис чужого сету",
  userName: "ivan",
  avatarUrl: null,
  progress: 20,
  flashcardsCount: 5,
  createdAt: "2026-08-02T10:00:00Z",
};

it("should render mine set correctly with privacy status, actions dropdown, progress, and correct baseLink", () => {
  const { container } = renderWithProviders(
    <SetCard set={mockMineSet} isMine={true} />,
  );

  expect(container.querySelector(".set-card__title").textContent).toContain(
    "Мій тестовий сет",
  );
  expect(container.querySelector(".set-card__description").textContent).toBe(
    "Опис мого сету",
  );
  expect(container.querySelector(".privacy-status").textContent).toContain(
    "Публічний",
  );
  expect(container.textContent).toContain("50%");
  expect(container.querySelector(".progress-fill").style.width).toBe("50%");

  expect(
    container.querySelector('[data-testid="actions-dropdown"]'),
  ).not.toBeNull();

  const link = container.querySelector("a.set-card");
  expect(link.getAttribute("href")).toBe("/my-sets/1?page=1&pageSize=10");
});

it("should render public set of another user with author info, progress, instead of actions", () => {
  const { container } = renderWithProviders(
    <SetCard set={mockPublicSet} isMine={false} />,
  );

  expect(container.querySelector(".set-card__title").textContent).toContain(
    "Чужий публічний сет",
  );
  expect(container.querySelector(".author-name").textContent).toContain(
    "Автор: ivan",
  );
  expect(container.textContent).toContain("20%");
  expect(container.querySelector(".progress-fill").style.width).toBe("20%");

  expect(
    container.querySelector('[data-testid="actions-dropdown"]'),
  ).toBeNull();

  const link = container.querySelector("a.set-card");
  expect(link.getAttribute("href")).toBe("/sets/2?page=1&pageSize=10");
});

it("should render sparkle icon when set is generated", () => {
  const generatedSet = {
    ...mockMineSet,
    isGenerated: true,
  };

  const { container } = renderWithProviders(
    <SetCard set={generatedSet} isMine={true} />,
  );

  const titleElement = container.querySelector(".set-card__title");
  expect(titleElement.textContent).toContain("✨");
});

it("should open edit modal when clicking edit in actions dropdown", () => {
  const { container } = renderWithProviders(
    <SetCard set={mockMineSet} isMine={true} />,
  );

  expect(container.querySelector('[data-testid="edit-modal"]')).toBeNull();

  const editBtn = container.querySelector('[data-testid="edit-btn"]');
  fireEvent.click(editBtn);

  expect(container.querySelector('[data-testid="edit-modal"]')).not.toBeNull();
});

it("should open confirmation modal and successfully delete set on confirmation", async () => {
  server.use(
    http.delete("*/api/my-sets/1", () => {
      return new HttpResponse(null, { status: 204 });
    }),
  );

  const { container } = renderWithProviders(
    <SetCard set={mockMineSet} isMine={true} />,
  );

  expect(container.querySelector('[data-testid="confirm-modal"]')).toBeNull();

  const deleteBtn = container.querySelector('[data-testid="delete-btn"]');
  fireEvent.click(deleteBtn);

  expect(
    container.querySelector('[data-testid="confirm-modal"]'),
  ).not.toBeNull();

  const confirmBtn = container.querySelector(
    '[data-testid="confirm-delete-btn"]',
  );
  fireEvent.click(confirmBtn);

  await waitFor(() => {
    expect(container.querySelector('[data-testid="confirm-modal"]')).toBeNull();
  });
});
