import { render, screen, fireEvent } from "@testing-library/react";
import FilterPanel from "./FilterPanel";

vi.mock("../../constants/languages", () => ({
  LANGUAGES: [{ code: "de", name: "Німецька" }],
}));

const defaultProps = {
  filters: {
    searchText: "",
    categoryId: "",
    setType: "",
    fromLangCode: "",
    progress: "",
  },
  categories: [{ id: 1, name: "Побутова" }],
  types: [{ id: 0, name: "Слова" }],
  pagination: { startItem: 1, endItem: 20, totalItems: 100 },
  onChange: vi.fn(),
  onClearSearch: vi.fn(),
  onSubmit: vi.fn(),
};

beforeEach(() => vi.clearAllMocks());

it("clicking a tab calls onChange with the progress value", () => {
  render(<FilterPanel {...defaultProps} />);
  fireEvent.click(screen.getByText("Не початі"));
  expect(defaultProps.onChange).toHaveBeenCalledWith({
    progress: "notstarted",
  });
});

it("typing in the search input calls onChange with reload: false", () => {
  render(<FilterPanel {...defaultProps} />);
  const input = screen.getByPlaceholderText("Шукати сети...");

  fireEvent.change(input, { target: { value: "Німецька" } });

  expect(defaultProps.onChange).toHaveBeenCalledWith(
    { searchText: "Німецька" },
    { reload: false },
  );
});

it("clearing the field to empty calls onClearSearch instead of onChange", () => {
  render(
    <FilterPanel
      {...defaultProps}
      filters={{ ...defaultProps.filters, searchText: "abc" }}
    />,
  );
  const input = screen.getByPlaceholderText("Шукати сети...");

  fireEvent.change(input, { target: { value: "" } });

  expect(defaultProps.onClearSearch).toHaveBeenCalledTimes(1);
  expect(defaultProps.onChange).not.toHaveBeenCalled();
});

it("shows the clear ('X') icon only when searchText is not empty", () => {
  const { rerender } = render(<FilterPanel {...defaultProps} />);
  expect(document.querySelectorAll(".search-clear-icon")).toHaveLength(0);

  rerender(
    <FilterPanel
      {...defaultProps}
      filters={{ ...defaultProps.filters, searchText: "щось" }}
    />,
  );
  expect(document.querySelectorAll(".search-clear-icon")).toHaveLength(1);
});

it("changing the category select calls onChange without reload: false (i.e. reloads immediately)", () => {
  render(<FilterPanel {...defaultProps} />);
  fireEvent.change(screen.getByDisplayValue("Усі категорії"), {
    target: { value: "1" },
  });
  expect(defaultProps.onChange).toHaveBeenCalledWith({ categoryId: "1" });
});

it("submitting the form calls onSubmit", () => {
  render(<FilterPanel {...defaultProps} />);
  fireEvent.click(screen.getByText("Шукати"));
  expect(defaultProps.onSubmit).toHaveBeenCalledTimes(1);
});

it("shows the item count from pagination", () => {
  render(<FilterPanel {...defaultProps} />);
  expect(screen.getByText("1–20 з 100")).toBeInTheDocument();
});
