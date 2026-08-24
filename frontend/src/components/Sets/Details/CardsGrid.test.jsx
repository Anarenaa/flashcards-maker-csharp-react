import { render } from "@testing-library/react";
import CardsGrid from "./CardsGrid";

// Mock child components for isolation
vi.mock("./CardTile", () => ({
  default: ({ card }) => <div data-testid="card-tile">{card.term}</div>,
}));

vi.mock("../../Shared/Loader", () => ({
  default: ({ loadingText }) => <div data-testid="loader">{loadingText}</div>,
}));

it("should show loader when loading and flashcardsCount > 0", () => {
  const { container } = render(
    <CardsGrid isLoading={true} flashcardsCount={5} loadingText="Custom loading..." />
  );

  expect(container.querySelector('[data-testid="loader"]')).not.toBeNull();
  expect(container.querySelector('[data-testid="loader"]').textContent).toBe("Custom loading...");
});

it("should show empty state message when cards list is empty", () => {
  const { container } = render(
    <CardsGrid isLoading={false} flashcardsCount={0} cards={[]} emptyText="Немає карток" />
  );

  expect(container.querySelector(".empty-state")).not.toBeNull();
  expect(container.querySelector("p").textContent).toBe("Немає карток");
});

it("should render list of cards when data is present", () => {
  const mockCards = [
    { id: 1, term: "Apple", definition: "Яблуко" },
    { id: 2, term: "Car", definition: "Машина" },
  ];

  const { container } = render(
    <CardsGrid isLoading={false} flashcardsCount={2} cards={mockCards} />
  );

  const tiles = container.querySelectorAll('[data-testid="card-tile"]');
  expect(tiles.length).toBe(2);
  expect(tiles[0].textContent).toBe("Apple");
  expect(tiles[1].textContent).toBe("Car");
});