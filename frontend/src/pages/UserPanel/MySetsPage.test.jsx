import { render, screen, fireEvent } from "@testing-library/react";
import MySetsPage from "./MySetsPage";

// Test MySetsPage in isolation: replace the real SetsPageLayout (with
// hooks, MSW requests, etc.) with a stub that just records the props
// it received in a data-attribute (from the real SetsPageLayout component).
vi.mock("../../components/Layouts/SetsPageLayout", () => ({
  default: ({ extraCard, endpoint, isMine }) => (
    <div data-testid="layout" data-endpoint={endpoint} data-is-mine={isMine}>
      {extraCard}
    </div>
  ),
}));
vi.mock("../../features/sets/SetForm", () => ({
  default: ({ isOpen }) => isOpen ? <div data-testid="set-form">SetForm Mock</div> : null,
}));

it("renders SetsPageLayout with endpoint='/my-sets' and isMine=true", () => {
  render(<MySetsPage />);
  const layout = screen.getByTestId("layout");

  expect(layout.getAttribute("data-endpoint")).toBe("/my-sets");
  expect(layout.getAttribute("data-is-mine")).toBe("true");
});

it("opens SetForm when clicking on 'extraCard' button", () => {
  render(<MySetsPage />);

  expect(screen.queryByTestId("set-form")).not.toBeInTheDocument();

  // use a case-insensitive regular expression to flexibly find the button by text
  const createButton = screen.getByText(/Створити новий сет/i);
  expect(createButton).toBeInTheDocument();

  fireEvent.click(createButton);

  expect(screen.getByTestId("set-form")).toBeInTheDocument();
});