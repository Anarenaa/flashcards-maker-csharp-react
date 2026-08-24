import { render, fireEvent, waitFor } from "@testing-library/react";
import { BrowserRouter } from "react-router";
import { http, HttpResponse } from "msw";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { server } from "../../setupTests";
import SetForm from "./SetForm";

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
vi.mock("../../hooks/useSetTypes", () => ({
  useSetTypes: () => [
    { id: 0, name: "Мовні сети" },
    { id: 1, name: "Предметні сети" },
  ],
}));

describe("SetForm - Normal Mode & Language Selects", () => {
  it("should render default fields and default language values correctly", () => {
    const { container } = renderWithProviders(
      <SetForm isOpen={true} onClose={() => {}} />,
    );

    expect(container.querySelector('input[id="set-name-input"]').value).toBe(
      "",
    );
    expect(
      container.querySelector('textarea[id="set-description-input"]').value,
    ).toBe("");

    const toLangSelect = container.querySelector('select[id="to-lang-select"]');
    const fromLangSelect = container.querySelector(
      'select[id="from-lang-select"]',
    );

    expect(toLangSelect).not.toBeNull();
    expect(toLangSelect.value).toBe("uk");

    if (fromLangSelect) {
      expect(fromLangSelect.value).toBe("en");
    }
  });

  it("should toggle term language select visibility and format languages correctly based on set type", async () => {
    const { container } = renderWithProviders(
      <SetForm isOpen={true} onClose={() => {}} />,
    );
    const typeSelect = container.querySelector('select[id="set-type-select"]');

    fireEvent.change(typeSelect, { target: { value: "0" } });
    expect(
      container.querySelector('select[id="from-lang-select"]'),
    ).not.toBeNull();

    fireEvent.change(typeSelect, { target: { value: "1" } });
    expect(container.querySelector('select[id="from-lang-select"]')).toBeNull();
  });

  it("should show validation errors when submitting invalid data", async () => {
    const { container } = renderWithProviders(
      <SetForm isOpen={true} onClose={() => {}} />,
    );
    const submitButton = container.querySelector('button[type="submit"]');

    fireEvent.click(submitButton);

    await waitFor(() => {
      const errorMsg = container.querySelector(".text-danger");
      expect(errorMsg).not.toBeNull();
      expect(errorMsg.textContent).toContain("Введіть назву сету");
    });
  });

  it("should successfully fill all fields and send correct payload", async () => {
    const mockOnClose = vi.fn();
    const { container } = renderWithProviders(
      <SetForm isOpen={true} onClose={mockOnClose} />,
    );

    const typeSelect = container.querySelector('select[id="set-type-select"]');
    const nameInput = container.querySelector('input[id="set-name-input"]');
    const toLangSelect = container.querySelector('select[id="to-lang-select"]');
    const publicCheckbox = container.querySelector(
      'input[id="is-public-checkbox-input"]',
    );
    const submitButton = container.querySelector('button[type="submit"]');

    let sentPayload = null;
    server.use(
      http.post("*/api/my-sets", async ({ request }) => {
        sentPayload = await request.json();
        return HttpResponse.json({ id: 1, ...sentPayload }, { status: 201 });
      }),
    );

    const firstTypeId = typeSelect.options[0].value;
    fireEvent.change(typeSelect, { target: { value: firstTypeId } });

    fireEvent.change(nameInput, {
      target: { value: "Тестовий сет для перевірки" },
    });
    fireEvent.change(toLangSelect, { target: { value: "uk" } });
    fireEvent.click(publicCheckbox);

    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(mockOnClose).toHaveBeenCalled();
      expect(sentPayload).toMatchObject({
        name: "Тестовий сет для перевірки",
        isPublic: true,
        toLang: "uk",
      });
    });
  });
});

describe("SetForm - AI Generation Mode", () => {
  it("should display AI fields (prompt, file, cardsCount) when AI mode is enabled", () => {
    const { container } = renderWithProviders(
      <SetForm isOpen={true} onClose={() => {}} />,
    );

    const aiToggle = container.querySelector('input[id="ai-checkbox-input"]');
    fireEvent.click(aiToggle);

    expect(
      container.querySelector('textarea[id="set-promt-input"]'),
    ).not.toBeNull();
    expect(
      container.querySelector("button.set-form-generate-button").textContent,
    ).toContain("Згенерувати");
  });

  it("should handle AI generation, show loader, populate form with generated data, and reset AI state properly", async () => {
    const { container } = renderWithProviders(
      <SetForm isOpen={true} onClose={() => {}} />,
    );

    const aiToggle = container.querySelector('input[id="ai-checkbox-input"]');
    fireEvent.click(aiToggle);

    const promptInput = container.querySelector(
      'textarea[id="set-promt-input"]',
    );
    const generateButton = container.querySelector(
      "button.set-form-generate-button",
    );

    server.use(
      http.post("*/api/my-sets/generate", () => {
        return HttpResponse.json({
          setInfo: {
            type: 0,
            toLang: "uk",
            fromLang: "en",
            name: "AI Generated Set",
            description: "Generated description",
            isPublic: false,
          },
          flashcards: [{ term: "Test", definition: "Тест" }],
        });
      }),
    );

    fireEvent.change(promptInput, {
      target: { value: "Зроби словник IT термінів" },
    });
    fireEvent.click(generateButton);

    await waitFor(() => {
      expect(container.querySelector(".generated-cards")).not.toBeNull();

      const nameInputAfter = container.querySelector(
        'input[id="set-name-input"]',
      );
      expect(nameInputAfter.value).toBe("AI Generated Set");
      expect(aiToggle.checked).toBe(false);
    });
  });
  it("should handle optional AI fields (file and cardsCount) via checkboxes correctly", async () => {
    const { container } = renderWithProviders(
      <SetForm isOpen={true} onClose={() => {}} />,
    );

    const aiToggle = container.querySelector('input[id="ai-checkbox-input"]');
    fireEvent.click(aiToggle);

    const promptInput = container.querySelector(
      'textarea[id="set-promt-input"]',
    );
    const generateButton = container.querySelector(
      "button.set-form-generate-button",
    );

    const checkboxes = container.querySelectorAll(
      '.ai-controls input[type="checkbox"]',
    );
    const limitCheckbox = checkboxes[1];

    let sentAiPayload = null;
    server.use(
      http.post("*/api/my-sets/generate", async ({ request }) => {
        const formData = await request.formData();
        sentAiPayload = {
          prompt: formData.get("prompt"),
          file: formData.get("file"),
          cardsCount: formData.get("cardsCount"),
        };

        return HttpResponse.json({
          setInfo: {
            type: 0,
            toLang: "uk",
            fromLang: "en",
            name: "Set",
            description: "",
            isPublic: false,
          },
          flashcards: [{ term: "A", definition: "Б" }],
        });
      }),
    );

    fireEvent.change(promptInput, {
      target: { value: "Промт для генерації з опціями" },
    });

    fireEvent.click(limitCheckbox);

    fireEvent.click(generateButton);

    await waitFor(() => {
      expect(sentAiPayload).not.toBeNull();
      expect(sentAiPayload.prompt).toBe("Промт для генерації з опціями");
      expect(sentAiPayload.file).toBeNull();
      expect(sentAiPayload.cardsCount).toBe("10");
    });
  });
  it("should handle file upload when file checkbox is active and cardsCount is null", async () => {
    const { container } = renderWithProviders(
      <SetForm isOpen={true} onClose={() => {}} />,
    );

    const aiToggle = container.querySelector('input[id="ai-checkbox-input"]');
    fireEvent.click(aiToggle);

    const promptInput = container.querySelector(
      'textarea[id="set-promt-input"]',
    );
    const generateButton = container.querySelector(
      "button.set-form-generate-button",
    );

    const checkboxes = container.querySelectorAll(
      '.ai-controls input[type="checkbox"]',
    );
    const fileCheckbox = checkboxes[0];

    let sentAiPayload = null;
    server.use(
      http.post("*/api/my-sets/generate", async ({ request }) => {
        const formData = await request.formData();
        sentAiPayload = {
          prompt: formData.get("prompt"),
          file: formData.get("file"),
          cardsCount: formData.get("cardsCount"),
        };

        return HttpResponse.json({
          setInfo: {
            type: 0,
            toLang: "uk",
            fromLang: "en",
            name: "File Set",
            description: "",
            isPublic: false,
          },
          flashcards: [{ term: "FileTerm", definition: "ФайлВизначення" }],
        });
      }),
    );

    fireEvent.change(promptInput, { target: { value: "Згенеруй з файлу" } });

    fireEvent.click(fileCheckbox);

    const fileInput = container.querySelector('input[type="file"]');
    const fakeFile = new File(["dummy content"], "test-notes.txt", {
      type: "text/plain",
    });

    fireEvent.change(fileInput, { target: { files: [fakeFile] } });

    fireEvent.click(generateButton);

    await waitFor(() => {
      expect(sentAiPayload).not.toBeNull();
      expect(sentAiPayload.prompt).toBe("Згенеруй з файлу");
      expect(sentAiPayload.file).not.toBeNull();
      expect(sentAiPayload.cardsCount).toBeNull();
    });
  });
});

describe("SetForm - Edit Mode (Full Update)", () => {
  it("should prepopulate fields and send updated payload with all changed properties", async () => {
    const mockInitialSet = {
      id: 5,
      name: "Old Name",
      description: "Old Description",
      type: 0,
      toLang: "uk",
      fromLang: "en",
      isPublic: false,
    };

    const mockOnClose = vi.fn();
    const { container } = renderWithProviders(
      <SetForm
        isOpen={true}
        onClose={mockOnClose}
        initialData={mockInitialSet}
      />,
    );

    // AI toggle is not present in edit mode
    const aiToggle = container.querySelector('input[id="ai-checkbox-input"]');
    expect(aiToggle).toBeNull();

    const nameInput = container.querySelector('input[id="set-name-input"]');
    const descInput = container.querySelector(
      'textarea[id="set-description-input"]',
    );
    const typeSelect = container.querySelector('select[id="set-type-select"]');
    const toLangSelect = container.querySelector('select[id="to-lang-select"]');
    const fromLangSelect = container.querySelector(
      'select[id="from-lang-select"]',
    );
    const publicCheckbox = container.querySelector(
      'input[id="is-public-checkbox-input"]',
    );
    const submitButton = container.querySelector('button[type="submit"]');

    let sentPayload = null;
    server.use(
      http.put("*/api/my-sets/5", async ({ request }) => {
        sentPayload = await request.json();
        return HttpResponse.json({ success: true }, { status: 200 });
      }),
    );

    // Modify all form fields/props
    fireEvent.change(nameInput, { target: { value: "Fully Updated Name" } });
    fireEvent.change(descInput, {
      target: { value: "Fully Updated Description" },
    });
    fireEvent.change(typeSelect, { target: { value: "0" } });
    fireEvent.change(toLangSelect, { target: { value: "en" } });

    if (fromLangSelect) {
      fireEvent.change(fromLangSelect, { target: { value: "uk" } });
    }

    fireEvent.click(publicCheckbox); // toggle to true

    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(mockOnClose).toHaveBeenCalled();
      expect(sentPayload).toMatchObject({
        name: "Fully Updated Name",
        description: "Fully Updated Description",
        toLang: "en",
        fromLang: "uk",
        isPublic: true,
      });
    });
  });
});
