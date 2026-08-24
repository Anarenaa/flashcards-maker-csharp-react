import { render, fireEvent, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { http, HttpResponse } from "msw";
import { server } from "../../setupTests";
import FlashcardForm from "./FlashcardForm";

const renderWithProviders = (ui) => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return render(
    <QueryClientProvider client={queryClient}>{ui}</QueryClientProvider>,
  );
};

describe("FlashcardForm - Create Mode", () => {
  it("should render empty fields when creating a new card", () => {
    const { container } = renderWithProviders(
      <FlashcardForm isOpen={true} onClose={() => {}} setId={1} />,
    );

    expect(container.querySelector("h2.title").textContent).toBe("Нова картка");
    expect(container.querySelector('input[id="term-input"]').value).toBe("");
    expect(
      container.querySelector('textarea[id="definition-input"]').value,
    ).toBe("");
  });

  it("should show validation errors when submitting empty form", async () => {
    const { container } = renderWithProviders(
      <FlashcardForm isOpen={true} onClose={() => {}} setId={1} />,
    );

    const submitButton = container.querySelector('button[type="submit"]');
    fireEvent.click(submitButton);

    await waitFor(() => {
      const errors = container.querySelectorAll(".text-danger");
      expect(errors.length).toBeGreaterThan(0);
    });
  });

  it("should successfully create a new flashcard via POST request", async () => {
    const mockOnClose = vi.fn();
    let sentPayload = null;

    server.use(
      http.post("*/api/sets/1/flashcards", async ({ request }) => {
        sentPayload = await request.json();
        return HttpResponse.json({ id: 100, ...sentPayload }, { status: 201 });
      }),
    );

    const { container } = renderWithProviders(
      <FlashcardForm isOpen={true} onClose={mockOnClose} setId={1} />,
    );

    const termInput = container.querySelector('input[id="term-input"]');
    const defInput = container.querySelector('textarea[id="definition-input"]');
    const submitButton = container.querySelector('button[type="submit"]');

    fireEvent.change(termInput, { target: { value: "New Term" } });
    fireEvent.change(defInput, { target: { value: "New Definition" } });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(mockOnClose).toHaveBeenCalled();
      expect(sentPayload).toEqual({
        term: "New Term",
        definition: "New Definition",
      });
    });
  });

  it("should fetch and fill definition hint when clicking hint button", async () => {
    server.use(
      http.post("*/api/sets/1/flashcards/hint", () => {
        return HttpResponse.json("Generated Hint Definition", { status: 200 });
      }),
    );

    const { container } = renderWithProviders(
      <FlashcardForm isOpen={true} onClose={() => {}} setId={1} />,
    );

    const termInput = container.querySelector('input[id="term-input"]');
    const defInput = container.querySelector('textarea[id="definition-input"]');
    const hintButton = container.querySelector(".definition-hint-button");

    expect(hintButton).toBeDisabled();

    fireEvent.change(termInput, { target: { value: "Apple" } });
    expect(hintButton).not.toBeDisabled();

    fireEvent.click(hintButton);

    await waitFor(() => {
      expect(defInput.value).toBe("Generated Hint Definition");
    });
  });
});

describe("FlashcardForm - Edit Mode", () => {
  it("should prepopulate fields with initialData and send PUT request on submit", async () => {
    const mockCard = {
      id: 42,
      term: "Old Term",
      definition: "Old Definition",
    };

    const mockOnClose = vi.fn();
    let sentPayload = null;
    let requestedId = null;

    server.use(
      http.put("*/api/sets/1/flashcards/42", async ({ request }) => {
        requestedId = "42";
        sentPayload = await request.json();
        return HttpResponse.json({ success: true }, { status: 200 });
      }),
    );

    const { container } = renderWithProviders(
      <FlashcardForm
        isOpen={true}
        onClose={mockOnClose}
        setId={1}
        initialData={mockCard}
      />,
    );

    expect(container.querySelector("h2.title").textContent).toBe(
      "Редагування картки",
    );

    const termInput = container.querySelector('input[id="term-input"]');
    const defInput = container.querySelector('textarea[id="definition-input"]');
    const submitButton = container.querySelector('button[type="submit"]');

    expect(termInput.value).toBe("Old Term");
    expect(defInput.value).toBe("Old Definition");

    fireEvent.change(termInput, { target: { value: "Updated Term" } });
    fireEvent.change(defInput, { target: { value: "Updated Definition" } });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(mockOnClose).toHaveBeenCalled();
      expect(requestedId).toBe("42");
      expect(sentPayload).toEqual({
        term: "Updated Term",
        definition: "Updated Definition",
      });
    });
  });
});
