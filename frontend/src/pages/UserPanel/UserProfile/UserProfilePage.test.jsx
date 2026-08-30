import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { http, HttpResponse } from "msw";
import { setupServer } from "msw/node";
import UserProfilePage from "./UserProfilePage";

const server = setupServer(
  http.get("*/users/2", () => {
    return HttpResponse.json({
      id: 2,
      roles: ["User"],
      avatarUrl: null,
      userName: "jane_smith",
      email: "jane_smith@gmail.com",
      isPublic: true,
      createdAt: "2023-02-02T13:30:00",
      lastActivity: "2026-06-08T15:00:56.0733574",
      setsCount: 31,
      publicSetsCount: 24,
      collectionsCount: 1,
      flashcardsCount: 38,
      completedSets: 0,
      masteredCards: 0,
    });
  }),
  http.get("*/users/19", () => {
    return HttpResponse.json({
      id: 19,
      avatarUrl:
        "https://res.cloudinary.com/dm8319xyt/image/upload/v1787948659/avatars/6a5b391b-dbd5-4f6d-9010-1f751fd2adcf.jpg",
      userName: "mika",
      isPublic: false,
      publicSetsCount: 0,
    });
  }),
  http.get("*/users/999", () => {
    return HttpResponse.json({ message: "Not found" }, { status: 404 });
  }),
);

beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

const renderWithRouterAndQuery = (initialRoute) => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
    },
  });

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[initialRoute]}>
        <Routes>
          <Route path="/users/:id" element={<UserProfilePage />} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
};

describe("Loading and Error States", () => {
  it("should render error state when API request fails", async () => {
    renderWithRouterAndQuery("/users/999");

    expect(
      await screen.findByText("Не вдалося завантажити сторінку користувача."),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: /повернутися назад/i }),
    ).toBeInTheDocument();
  });
});

describe("Public Profile", () => {
  it("should render public user profile information and public sets link correctly", async () => {
    renderWithRouterAndQuery("/users/2");

    expect(await screen.findByText("jane_smith")).toBeInTheDocument();
    expect(screen.getByText("jane_smith@gmail.com")).toBeInTheDocument();
    expect(
      screen.getByText("Всі публічні сети цього користувача"),
    ).toBeInTheDocument();
    expect(screen.getByText("24")).toBeInTheDocument();
  });

  it("should render message when all user sets are private", async () => {
    server.use(
      http.get("**/users/20", () => {
        return HttpResponse.json({
          id: 20,
          userName: "private_sets_user",
          isPublic: true,
          setsCount: 5,
          publicSetsCount: 0,
        });
      }),
    );

    renderWithRouterAndQuery("/users/20");

    expect(
      await screen.findByText("Всі сети цього користувача є приватними"),
    ).toBeInTheDocument();
  });

  it("should not render sets link or message when user has no sets", async () => {
    server.use(
      http.get("**/users/21", () => {
        return HttpResponse.json({
          id: 21,
          userName: "no_sets_user",
          isPublic: true,
          setsCount: 0,
          publicSetsCount: 0,
        });
      }),
    );

    renderWithRouterAndQuery("/users/21");

    expect(await screen.findByText("no_sets_user")).toBeInTheDocument();

    expect(
      screen.queryByText("Всі публічні сети цього користувача"),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByText("Всі сети цього користувача є приватними"),
    ).not.toBeInTheDocument();
  });
});

describe("Private Profile", () => {
  it("should render private profile placeholder when user is not public", async () => {
    renderWithRouterAndQuery("/users/19");

    expect(
      await screen.findByText(
        "Цей профіль є приватним. Інформація про користувача схована.",
      ),
    ).toBeInTheDocument();

    expect(screen.queryByText("jane_smith@gmail.com")).not.toBeInTheDocument();
  });
  it("should render public sets link even on a private profile if public sets exist", async () => {
    server.use(
      http.get("**/users/22", () => {
        return HttpResponse.json({
          id: 22,
          userName: "private_with_public_sets",
          isPublic: false,
          setsCount: 10,
          publicSetsCount: 3,
        });
      }),
    );

    renderWithRouterAndQuery("/users/22");

    expect(
      await screen.findByText(
        "Цей профіль є приватним. Інформація про користувача схована.",
      ),
    ).toBeInTheDocument();

    expect(
      screen.getByText("Всі публічні сети цього користувача"),
    ).toBeInTheDocument();
    expect(screen.getByText("3")).toBeInTheDocument();
  });
});
