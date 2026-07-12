import { render, screen } from "@testing-library/react";
import SetsGrid from "./SetsGrid";

vi.mock("./SetCard", () => ({
  default: ({ set }) => <div data-testid="set-card">{set.name}</div>,
}));

const sampleSets = [
  { id: 1, name: "German A1" },
  { id: 2, name: "Spanish B2" },
];

it("shows a spinner and loadingText while loading", () => {
  render(<SetsGrid sets={[]} isLoading loadingText="Loading..." />);

  expect(screen.getByText("Loading...")).toBeInTheDocument();
  expect(screen.queryByTestId("set-card")).not.toBeInTheDocument();
});

it("renders cards when data is present", () => {
  render(<SetsGrid sets={sampleSets} isLoading={false} />);

  expect(screen.getAllByTestId("set-card")).toHaveLength(2);
  expect(screen.getByText("German A1")).toBeInTheDocument();
});

it("shows emptyText when the list is empty", () => {
  render(<SetsGrid sets={[]} isLoading={false} emptyText="Nothing found" />);

  expect(screen.getByText("Nothing found")).toBeInTheDocument();
});

it("hides the empty message when emptyText is null (new-user case)", () => {
  const { container } = render(
    <SetsGrid sets={[]} isLoading={false} emptyText={null} />,
  );

  expect(
    container.querySelector(".grid-status-message"),
  ).not.toBeInTheDocument();
});

it("renders extraCard even when the list is empty", () => {
  render(
    <SetsGrid
      sets={[]}
      isLoading={false}
      emptyText={null}
      extraCard={<div data-testid="add-card">+</div>}
    />,
  );

  expect(screen.getByTestId("add-card")).toBeInTheDocument();
});
