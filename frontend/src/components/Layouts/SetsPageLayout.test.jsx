import { fireEvent, waitFor, screen } from "@testing-library/react";
import { http, HttpResponse } from "msw";
import { server, renderWithProviders } from "../../setupTests";
import SetsPageLayout from "./SetsPageLayout";

vi.mock("../Sets/SetCard", () => ({
  default: ({ set }) => <div data-testid="set-card">{set.name}</div>,
}));

const mockSetsResponse = (overrides = {}) => ({
  items: [{ id: 1, name: "German A1" }],
  currentPage: 1,
  totalItems: 1,
  startItem: 1,
  endItem: 1,
  hasPreviousPage: false,
  hasNextPage: false,
  ...overrides,
});

function setupDefaultHandlers(endpoint = "/sets") {
  server.use(
    http.get("*/api/categories", () => HttpResponse.json([{ id: 1, name: "Household" }])),
    http.get("*/api/sets/types", () => HttpResponse.json([{ id: 0, name: "Words" }])),
    http.get(`*/api${endpoint}`, () => HttpResponse.json(mockSetsResponse()))
  );
}

describe("SetsPageLayout — Main page (isMine=false)", () => {
  beforeEach(() => setupDefaultHandlers("/sets"));

  it("loads and displays the list of sets", async () => {
    renderWithProviders(
      <SetsPageLayout endpoint="/sets" isMine={false} loadingText="Loading sets..." />
    );

    expect(screen.getByText("Loading sets...")).toBeInTheDocument();

    await waitFor(() => {
      expect(screen.getByText("German A1")).toBeInTheDocument();
    });
  });

  it("does not fire an extra request while typing without submit", async () => {
    let searchRequests = 0;
    server.use(
      http.get("*/api/sets", ({ request }) => {
        const url = new URL(request.url);
        if (url.searchParams.get("searchText")) searchRequests += 1;
        return HttpResponse.json(mockSetsResponse());
      })
    );

    renderWithProviders(
      <SetsPageLayout endpoint="/sets" isMine={false} loadingText="Loading sets..." />
    );
    await waitFor(() => screen.getByText("German A1"));

    const input = screen.getByPlaceholderText("Шукати сети...");
    fireEvent.change(input, { target: { value: "German" } });

    await new Promise((r) => setTimeout(r, 50));
    expect(searchRequests).toBe(0);
  });

  it("fires exactly one request for the whole word on 'Search' click", async () => {
    let capturedSearchTexts = [];
    server.use(
      http.get("*/api/sets", ({ request }) => {
        const url = new URL(request.url);
        const searchText = url.searchParams.get("searchText");
        if (searchText) capturedSearchTexts.push(searchText);
        return HttpResponse.json(mockSetsResponse());
      })
    );

    renderWithProviders(
      <SetsPageLayout endpoint="/sets" isMine={false} loadingText="Loading sets..." />
    );
    await waitFor(() => screen.getByText("German A1"));

    fireEvent.change(screen.getByPlaceholderText("Шукати сети..."), {
      target: { value: "German" },
    });
    fireEvent.click(screen.getByText("Шукати"));

    await waitFor(() => expect(capturedSearchTexts).toContain("German"));
    expect(capturedSearchTexts).toHaveLength(1);
  });

  it("clicking a progress tab triggers a new request immediately", async () => {
    let progressRequests = [];
    server.use(
      http.get("*/api/sets", ({ request }) => {
        const url = new URL(request.url);
        const progress = url.searchParams.get("progress");
        if (progress) progressRequests.push(progress);
        return HttpResponse.json(mockSetsResponse());
      })
    );

    renderWithProviders(
      <SetsPageLayout endpoint="/sets" isMine={false} loadingText="Loading sets..." />
    );
    await waitFor(() => screen.getByText("German A1"));

    fireEvent.click(screen.getByText("Вивчені"));

    await waitFor(() => expect(progressRequests).toContain("completed"));
  });

  it("shows an empty-state message when the list is empty", async () => {
    server.use(
      http.get("*/api/sets", () => HttpResponse.json(mockSetsResponse({ items: [], totalItems: 0 })))
    );

    renderWithProviders(
      <SetsPageLayout endpoint="/sets" isMine={false} loadingText="Loading sets..." emptyText="Nothing found" />
    );

    await waitFor(() => {
      expect(screen.getByText("Nothing found")).toBeInTheDocument();
    });
  });

  it("shows the pagination footer only when totalItems > pageSize", async () => {
    server.use(
      http.get("*/api/sets", () =>
        HttpResponse.json(mockSetsResponse({ totalItems: 100, hasNextPage: true }))
      )
    );

    renderWithProviders(
      <SetsPageLayout endpoint="/sets" isMine={false} loadingText="Loading sets..." />
    );

    await waitFor(() => {
      expect(screen.getByText("Сторінка 1")).toBeInTheDocument();
    });
  });
});

describe("SetsPageLayout — My sets (isMine=true)", () => {
  beforeEach(() => setupDefaultHandlers("/my-sets"));

  it("new user with no filters and no sets sees only the '+' card, no empty message", async () => {
    server.use(
      http.get("*/api/my-sets", () =>
        HttpResponse.json(mockSetsResponse({ items: [], totalItems: 0 }))
      )
    );

    renderWithProviders(
      <SetsPageLayout
        endpoint="/my-sets"
        isMine={true}
        loadingText="Loading your sets..."
        emptyText="Nothing found"
        extraCard={<div data-testid="add-card">+</div>}
      />
    );

    await waitFor(() => {
      expect(screen.getByTestId("add-card")).toBeInTheDocument();
    });
    expect(screen.queryByText("Nothing found")).not.toBeInTheDocument();
  });

  it("the same user searching and getting 0 results now sees the empty message", async () => {
    server.use(
      http.get("*/api/my-sets", ({ request }) => {
        const url = new URL(request.url);
        const hasSearch = !!url.searchParams.get("searchText");
        return HttpResponse.json(
          mockSetsResponse(hasSearch ? { items: [], totalItems: 0 } : {})
        );
      })
    );

    renderWithProviders(
      <SetsPageLayout
        endpoint="/my-sets"
        isMine={true}
        loadingText="Loading your sets..."
        emptyText="Nothing found"
        extraCard={<div data-testid="add-card">+</div>}
      />
    );

    await waitFor(() => screen.getByText("German A1"));

    fireEvent.change(screen.getByPlaceholderText("Шукати сети..."), {
      target: { value: "nonexistent" },
    });
    fireEvent.click(screen.getByText("Шукати"));

    await waitFor(() => {
      expect(screen.getByText("Nothing found")).toBeInTheDocument();
    });
  });

  it("the '+' card only shows on the 'all sets' tab (progress === '')", async () => {
    renderWithProviders(
      <SetsPageLayout
        endpoint="/my-sets"
        isMine={true}
        loadingText="Loading your sets..."
        extraCard={<div data-testid="add-card">+</div>}
      />
    );
    await waitFor(() => screen.getByText("German A1"));
    expect(screen.getByTestId("add-card")).toBeInTheDocument();

    fireEvent.click(screen.getByText("Вивчені"));

    await waitFor(() => {
      expect(screen.queryByTestId("add-card")).not.toBeInTheDocument();
    });
  });
});