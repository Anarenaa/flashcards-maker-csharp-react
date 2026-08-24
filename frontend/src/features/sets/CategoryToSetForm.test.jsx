import { render, fireEvent, waitFor } from "@testing-library/react";
import { http, HttpResponse } from "msw";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { server } from "../../setupTests";
import CategoryToSetForm from "./CategoryToSetForm";

const renderWithProviders = (ui) => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return render(
    <QueryClientProvider client={queryClient}>{ui}</QueryClientProvider>,
  );
};

vi.mock("../../hooks/useCategories", () => ({
  useCategories: () => [
    { id: 10, name: "Програмування" },
    { id: 20, name: "Мови" },
  ],
}));

it("should render categories select and disabled submit button initially", () => {
  const { container } = renderWithProviders(
    <CategoryToSetForm isOpen={true} onClose={() => {}} setId={1} />,
  );

  const select = container.querySelector('select[name="category"]');
  const submitBtn = container.querySelector('button[type="submit"]');

  expect(select).not.toBeNull();
  expect(submitBtn).toBeDisabled();
});

it("should enable submit button when category is selected and successfully submit data", async () => {
  const mockOnClose = vi.fn();
  const { container } = renderWithProviders(
    <CategoryToSetForm isOpen={true} onClose={mockOnClose} setId={1} />,
  );

  const select = container.querySelector('select[name="category"]');
  const submitBtn = container.querySelector('button[type="submit"]');

  let apiCalled = false;
  server.use(
    http.post("*/api/my-sets/1/add-category", ({ request }) => {
      const url = new URL(request.url);
      if (url.searchParams.get("categoryId") === "10") {
        apiCalled = true;
      }
      return HttpResponse.json({ success: true }, { status: 200 });
    }),
  );

  fireEvent.change(select, { target: { value: "10" } });
  expect(submitBtn).not.toBeDisabled();

  fireEvent.click(submitBtn);

  await waitFor(() => {
    expect(apiCalled).toBe(true);
    expect(mockOnClose).toHaveBeenCalled();
  });
});
