import { render, fireEvent, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { MemoryRouter, Route, Routes } from "react-router";
import { http, HttpResponse } from "msw";
import { server } from "../../setupTests";
import FlashcardContextForm from "./FlashcardContextForm";

const renderWithRouterAndProviders = (
  ui,
  { route = "/sets/10/cards" } = {},
) => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[route]}>
        <Routes>
          <Route path="/sets/:id/cards" element={ui} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
};

describe("FlashcardContextForm - Create Mode", () => {
  const mockCard = { id: 5, term: "Apple" };

  it("should render empty fields and placeholder with term correctly", () => {
    const { container } = renderWithRouterAndProviders(
      <FlashcardContextForm isOpen={true} onClose={() => {}} card={mockCard} />,
    );

    expect(container.querySelector("h2.title").textContent).toBe(
      "Додати контекст",
    );

    const sentenceInput = container.querySelector(
      'input[id="context-sentence"]',
    );
    expect(sentenceInput.value).toBe("");
    expect(sentenceInput.placeholder).toContain("Apple");

    expect(
      container.querySelector('input[id="context-translation"]').value,
    ).toBe("");
  });

  it("should show validation errors when submitting empty form", async () => {
    const { container } = renderWithRouterAndProviders(
      <FlashcardContextForm isOpen={true} onClose={() => {}} card={mockCard} />,
    );

    const submitButton = container.querySelector('button[type="submit"]');
    fireEvent.click(submitButton);

    await waitFor(() => {
      const errors = container.querySelectorAll(".text-danger");
      expect(errors.length).toBeGreaterThan(0);
    });
  });

  it("should successfully create context via POST request", async () => {
    const mockOnClose = vi.fn();
    let sentPayload = null;

    server.use(
      http.post("*/api/sets/10/flashcards/5/contexts", async ({ request }) => {
        sentPayload = await request.json();
        return HttpResponse.json({ id: 100, ...sentPayload }, { status: 201 });
      }),
    );

    const { container } = renderWithRouterAndProviders(
      <FlashcardContextForm
        isOpen={true}
        onClose={mockOnClose}
        card={mockCard}
      />,
    );

    const sentenceInput = container.querySelector(
      'input[id="context-sentence"]',
    );
    const translationInput = container.querySelector(
      'input[id="context-translation"]',
    );
    const submitButton = container.querySelector('button[type="submit"]');

    fireEvent.change(sentenceInput, { target: { value: "I ate an apple." } });
    fireEvent.change(translationInput, { target: { value: "Я зїв яблуко." } });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(mockOnClose).toHaveBeenCalled();
      expect(sentPayload).toEqual({
        sentence: "I ate an apple.",
        translation: "Я зїв яблуко.",
      });
    });
  });
});

describe("FlashcardContextForm - Edit Mode", () => {
  const mockCard = { id: 5, term: "Apple" };
  const mockInitialContext = {
    id: 42,
    sentence: "Old sentence",
    translation: "Старий переклад",
  };

  it("should prepopulate fields and send PUT request on submit", async () => {
    const mockOnClose = vi.fn();
    let sentPayload = null;
    let requestedContextId = null;

    server.use(
      http.put(
        "*/api/sets/10/flashcards/5/contexts/42",
        async ({ request }) => {
          requestedContextId = "42";
          sentPayload = await request.json();
          return HttpResponse.json({ success: true }, { status: 200 });
        },
      ),
    );

    const { container } = renderWithRouterAndProviders(
      <FlashcardContextForm
        isOpen={true}
        onClose={mockOnClose}
        card={mockCard}
        initialData={mockInitialContext}
      />,
    );

    expect(container.querySelector("h2.title").textContent).toBe("Редагувати");

    const sentenceInput = container.querySelector(
      'input[id="context-sentence"]',
    );
    const translationInput = container.querySelector(
      'input[id="context-translation"]',
    );
    const submitButton = container.querySelector('button[type="submit"]');

    expect(sentenceInput.value).toBe("Old sentence");
    expect(translationInput.value).toBe("Старий переклад");

    fireEvent.change(sentenceInput, { target: { value: "Updated sentence" } });
    fireEvent.change(translationInput, {
      target: { value: "Оновлений переклад" },
    });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(mockOnClose).toHaveBeenCalled();
      expect(requestedContextId).toBe("42");
      expect(sentPayload).toEqual({
        sentence: "Updated sentence",
        translation: "Оновлений переклад",
      });
    });
  });
});
