import { render, fireEvent, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { MemoryRouter, Route, Routes } from "react-router";
import { http, HttpResponse } from "msw";
import { server } from "../../setupTests";
import FlashcardContextsPanel from "./FlashcardContextsPanel";

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

vi.mock("../../components/Shared/PronounceButton", () => ({
  default: () => <button data-testid="pronounce-btn">Pronounce</button>,
}));

const mockCard = { id: 5, term: "Apple", fromLang: "en" };

it("should auto-generate contexts when list is empty on load", async () => {
  let generateCalled = false;
  server.use(
    http.get("*/api/sets/10/flashcards/5/contexts", () => {
      return HttpResponse.json([], { status: 200 });
    }),
    http.post("*/api/sets/10/flashcards/5/generate-context", () => {
      generateCalled = true;
      return HttpResponse.json(
        [
          {
            id: 1,
            sentence: "I like <strong>apple</strong>.",
            translation: "Я люблю яблуко.",
            isGenerated: true,
          },
        ],
        { status: 200 },
      );
    }),
  );

  const { container } = renderWithRouterAndProviders(
    <FlashcardContextsPanel
      isOpen={true}
      onClose={() => {}}
      card={mockCard}
      isSetMine={true}
    />,
  );

  await waitFor(() => {
    expect(generateCalled).toBe(true);
    expect(container.textContent).toContain("I like");
  });
});

it("should render fetched contexts list with translation correctly and allow opening add form", async () => {
  server.use(
    http.get("*/api/sets/10/flashcards/5/contexts", () => {
      return HttpResponse.json(
        [
          {
            id: 1,
            sentence: "Eat an <strong>apple</strong>.",
            translation: "З'їж яблуко.",
            isGenerated: false,
          },
        ],
        { status: 200 },
      );
    }),
  );

  const { container } = renderWithRouterAndProviders(
    <FlashcardContextsPanel
      isOpen={true}
      onClose={() => {}}
      card={mockCard}
      isSetMine={true}
    />,
  );

  await waitFor(() => {
    expect(container.textContent).toContain("Eat an");
    expect(container.textContent).toContain("З'їж яблуко.");
    expect(container.querySelector("hr")).not.toBeNull();
  });

  const addButton = container.querySelector(".add-new-button");
  fireEvent.click(addButton);

  await waitFor(() => {
    expect(container.textContent).toContain("Додати контекст");
  });
});

it("should not render translation block if translation is missing", async () => {
  server.use(
    http.get("*/api/sets/10/flashcards/5/contexts", () => {
      return HttpResponse.json(
        [
          {
            id: 1,
            sentence: "Only sentence without translation.",
            translation: null,
            isGenerated: false,
          },
        ],
        { status: 200 },
      );
    }),
  );

  const { container } = renderWithRouterAndProviders(
    <FlashcardContextsPanel
      isOpen={true}
      onClose={() => {}}
      card={mockCard}
      isSetMine={true}
    />,
  );

  await waitFor(() => {
    expect(container.textContent).toContain(
      "Only sentence without translation.",
    );
    expect(container.querySelector("hr")).toBeNull();
  });
});

it("should open edit form when clicking edit button on a context block", async () => {
  server.use(
    http.get("*/api/sets/10/flashcards/5/contexts", () => {
      return HttpResponse.json(
        [
          {
            id: 1,
            sentence: "Red <strong>apple</strong>.",
            translation: "Червоне яблуко.",
            isGenerated: false,
          },
        ],
        { status: 200 },
      );
    }),
  );

  const { container } = renderWithRouterAndProviders(
    <FlashcardContextsPanel
      isOpen={true}
      onClose={() => {}}
      card={mockCard}
      isSetMine={true}
    />,
  );

  await waitFor(() => {
    expect(container.querySelector('[aria-label="Редагувати"]')).not.toBeNull();
  });

  fireEvent.click(container.querySelector('[aria-label="Редагувати"]'));

  await waitFor(() => {
    expect(container.textContent).toContain("Редагувати");
  });
});

it("should not render add button and action buttons when isSetMine is false", async () => {
  server.use(
    http.get("*/api/sets/10/flashcards/5/contexts", () => {
      return HttpResponse.json(
        [
          {
            id: 1,
            sentence: "Other user context.",
            translation: "Контекст іншого користувача.",
            isGenerated: false,
          },
        ],
        { status: 200 },
      );
    }),
  );

  const { container } = renderWithRouterAndProviders(
    <FlashcardContextsPanel
      isOpen={true}
      onClose={() => {}}
      card={mockCard}
      isSetMine={false}
    />,
  );

  await waitFor(() => {
    expect(container.textContent).toContain("Other user context.");
  });

  expect(container.querySelector(".add-new-button")).toBeNull();
  expect(container.querySelector('[aria-label="Редагувати"]')).toBeNull();
  expect(container.querySelector('[aria-label="Видалити"]')).toBeNull();
});

it("should open confirmation modal and delete context successfully", async () => {
  let deleteCalled = false;
  server.use(
    http.get("*/api/sets/10/flashcards/5/contexts", () => {
      return HttpResponse.json(
        [
          {
            id: 1,
            sentence: "Fresh <strong>apple</strong>.",
            translation: "Свіже яблуко.",
            isGenerated: false,
          },
        ],
        { status: 200 },
      );
    }),
    http.delete("*/api/sets/10/flashcards/5/contexts/1", () => {
      deleteCalled = true;
      return new HttpResponse(null, { status: 204 });
    }),
  );

  const { container } = renderWithRouterAndProviders(
    <FlashcardContextsPanel
      isOpen={true}
      onClose={() => {}}
      card={mockCard}
      isSetMine={true}
    />,
  );

  await waitFor(() => {
    expect(container.querySelector('[aria-label="Видалити"]')).not.toBeNull();
  });

  fireEvent.click(container.querySelector('[aria-label="Видалити"]'));

  await waitFor(() => {
    const confirmButtons = container.querySelectorAll("button");
    const confirmBtn = Array.from(confirmButtons).find((b) =>
      ["Так", "Підтвердити", "Видалити"].some((text) =>
        b.textContent.includes(text),
      ),
    );
    expect(confirmBtn).toBeDefined();
  });
});
