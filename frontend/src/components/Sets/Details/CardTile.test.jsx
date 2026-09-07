import { render, fireEvent, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { http, HttpResponse } from "msw";
import { server } from "../../../setupTests";
import CardTile from "./CardTile";

const renderWithProviders = (ui) => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return render(
    <QueryClientProvider client={queryClient}>{ui}</QueryClientProvider>,
  );
};

// Mock complex child components for isolation
vi.mock("../../Shared/PronounceButton", () => ({
  default: () => <button data-testid="pronounce-btn">Pronounce</button>,
}));

vi.mock("../../../features/flashcards/FlashcardContextsPanel", () => ({
  default: ({ isOpen }) =>
    isOpen ? <div data-testid="context-modal">Context Modal</div> : null,
}));

vi.mock("../../../features/flashcards/FlashcardForm", () => ({
  default: ({ isOpen }) =>
    isOpen ? <div data-testid="edit-card-form">Edit Form</div> : null,
}));

vi.mock("../../Shared/ConfirmModal", () => ({
  default: ({ onConfirm, onCancel }) => (
    <div data-testid="confirm-modal">
      <button data-testid="confirm-del" onClick={onConfirm}>
        Confirm
      </button>
      <button data-testid="cancel-del" onClick={onCancel}>
        Cancel
      </button>
    </div>
  ),
}));

vi.mock("../../Shared/ActionsDropdown", () => ({
  default: ({ onEdit, onDelete }) => (
    <div data-testid="actions-dropdown">
      <button data-testid="edit-action" onClick={onEdit}>
        Edit
      </button>
      <button data-testid="delete-action" onClick={onDelete}>
        Delete
      </button>
    </div>
  ),
}));

const mockCard = {
  id: 1,
  term: "Test Term",
  definition: "Тестове визначення",
  fromLang: "en",
};

it("should render card term, definition, and pronounce button correctly", () => {
  const { container } = renderWithProviders(
    <CardTile
      card={mockCard}
      setId={10}
      isMine={false}
      isLanguageType={false}
    />,
  );

  expect(container.querySelector("h3").textContent).toBe("Test Term");
  expect(container.querySelector("p").textContent).toBe("Тестове визначення");
  expect(
    container.querySelector('[data-testid="pronounce-btn"]'),
  ).not.toBeNull();
});

it("should show context button when isLanguageType is true", () => {
  const { container } = renderWithProviders(
    <CardTile
      card={mockCard}
      setId={10}
      isMine={false}
      isLanguageType={true}
    />,
  );

  expect(container.querySelector(".card-tile__context")).not.toBeNull();
});

it("should open context modal when clicking context button", () => {
  const { container } = renderWithProviders(
    <CardTile
      card={mockCard}
      setId={10}
      isMine={false}
      isLanguageType={true}
    />,
  );

  expect(container.querySelector('[data-testid="context-modal"]')).toBeNull();

  fireEvent.click(container.querySelector(".card-tile__context"));

  expect(
    container.querySelector('[data-testid="context-modal"]'),
  ).not.toBeNull();
});

it("should render actions and open edit form when isMine is true", () => {
  const { container } = renderWithProviders(
    <CardTile
      card={mockCard}
      setId={10}
      isMine={true}
      isLanguageType={false}
    />,
  );

  expect(
    container.querySelector('[data-testid="actions-dropdown"]'),
  ).not.toBeNull();
  expect(container.querySelector('[data-testid="edit-card-form"]')).toBeNull();

  fireEvent.click(container.querySelector('[data-testid="edit-action"]'));

  expect(
    container.querySelector('[data-testid="edit-card-form"]'),
  ).not.toBeNull();
});

it("should open confirmation modal and delete card successfully", async () => {
  server.use(
    http.delete("*/api/sets/10/flashcards/1", () => {
      return new HttpResponse(null, { status: 204 });
    }),
  );

  const { container } = renderWithProviders(
    <CardTile
      card={mockCard}
      setId={10}
      isMine={true}
      isLanguageType={false}
    />,
  );

  expect(container.querySelector('[data-testid="confirm-modal"]')).toBeNull();

  fireEvent.click(container.querySelector('[data-testid="delete-action"]'));
  expect(
    container.querySelector('[data-testid="confirm-modal"]'),
  ).not.toBeNull();

  fireEvent.click(container.querySelector('[data-testid="confirm-del"]'));

  await waitFor(() => {
    expect(container.querySelector('[data-testid="confirm-modal"]')).toBeNull();
  });
});
