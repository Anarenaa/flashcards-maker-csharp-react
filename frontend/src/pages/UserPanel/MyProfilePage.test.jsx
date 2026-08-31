import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { http, HttpResponse } from "msw";
import { setupServer } from "msw/node";
import MyProfilePage from "./MyProfilePage";

vi.mock("react-hot-toast", () => ({
  default: {
    success: vi.fn(),
    error: vi.fn(),
  },
}));

const server = setupServer(
  http.put("*/users/me", async ({ request }) => {
    const formData = await request.formData();
    const userName = formData.get("UserName");

    if (userName === "existing_user") {
      return HttpResponse.json(
        { message: "Користувач з таким ім'ям вже існує" },
        { status: 400 },
      );
    }

    return HttpResponse.json({ success: true });
  }),
);

beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

const mockUser = {
  userName: "test_user",
  email: "test@example.com",
  createdAt: "2023-01-01T00:00:00",
  avatarUrl: null,
};

const renderWithQuery = (ui) => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });

  return render(
    <QueryClientProvider client={queryClient}>{ui}</QueryClientProvider>,
  );
};

describe("MyProfilePage - Render and Actions", () => {
  it("should render user profile information correctly", () => {
    renderWithQuery(<MyProfilePage currentUser={mockUser} />);

    expect(screen.getByText("test_user")).toBeInTheDocument();
    expect(screen.getByText("test@example.com")).toBeInTheDocument();
    expect(
      screen.queryByRole("button", { name: /зберегти зміни/i }),
    ).not.toBeInTheDocument();
  });

  it("should enable name editing and show save button when username changes", () => {
    renderWithQuery(<MyProfilePage currentUser={mockUser} />);

    const userNameSpan = screen.getByText("test_user");
    fireEvent.click(userNameSpan);

    const input = screen.getByRole("textbox");
    expect(input).toBeInTheDocument();
    expect(input).toHaveValue("test_user");

    fireEvent.change(input, { target: { value: "new_username" } });

    expect(
      screen.getByRole("button", { name: /зберегти зміни/i }),
    ).toBeInTheDocument();
  });

  it("should remove avatar preview and show save button when remove avatar is clicked", () => {
    const userWithAvatar = {
      ...mockUser,
      avatarUrl: "https://example.com/avatar.jpg",
    };
    renderWithQuery(<MyProfilePage currentUser={userWithAvatar} />);

    const removeBtn = screen.getByTitle("Видалити зображення");
    expect(removeBtn).toBeInTheDocument();

    fireEvent.click(removeBtn);

    expect(screen.queryByAltText("Avatar")).not.toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: /зберегти зміни/i }),
    ).toBeInTheDocument();
  });

  it("should submit form and trigger update mutation successfully", async () => {
    renderWithQuery(<MyProfilePage currentUser={mockUser} />);

    fireEvent.click(screen.getByText("test_user"));
    const input = screen.getByRole("textbox");

    fireEvent.change(input, { target: { value: "updated_user" } });

    const saveBtn = screen.getByRole("button", { name: /зберегти зміни/i });
    fireEvent.click(saveBtn);

    await waitFor(() => {
      expect(
        screen.queryByRole("button", { name: /зберегти зміни/i }),
      ).not.toBeInTheDocument();
    });
  });
});

describe("MyProfilePage - Validation and Errors", () => {
  it("should show Zod validation error when username contains invalid characters", async () => {
    renderWithQuery(<MyProfilePage currentUser={mockUser} />);

    fireEvent.click(screen.getByText("test_user"));
    const input = screen.getByRole("textbox");

    fireEvent.change(input, { target: { value: "invalid name!" } });

    await waitFor(() => {
      expect(
        screen.getByText(
          "Ім'я користувача може містити лише латинські літери, цифри, крапки та підкреслення",
        ),
      ).toBeInTheDocument();
    });
  });

  it("should handle server error when updating profile", async () => {
    renderWithQuery(<MyProfilePage currentUser={mockUser} />);

    fireEvent.click(screen.getByText("test_user"));
    const input = screen.getByRole("textbox");

    fireEvent.change(input, { target: { value: "existing_user" } });

    const saveBtn = screen.getByRole("button", { name: /зберегти зміни/i });
    fireEvent.click(saveBtn);

    await waitFor(() => {
      expect(
        screen.getByRole("button", { name: /зберегти зміни/i }),
      ).toBeInTheDocument();
    });
  });
});
