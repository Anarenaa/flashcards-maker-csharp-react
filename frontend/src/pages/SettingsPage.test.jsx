import { render, fireEvent, waitFor, screen } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { http, HttpResponse } from "msw";
import { server } from "../setupTests";
import SettingsPage from "./SettingsPage";

vi.mock("react-hot-toast", () => ({
  default: { success: vi.fn(), error: vi.fn() },
}));
import toast from "react-hot-toast";

const CONFIRM_BUTTON_TEXT = "Так";
const CANCEL_BUTTON_TEXT = "Ні";

const renderPage = (props = {}) => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  });

  const defaultProps = {
    isPublic: false,
    isTheOnlyUserMode: false,
    onToggleTheOnlyUserMode: vi.fn(),
  };

  const utils = render(
    <QueryClientProvider client={queryClient}>
      <SettingsPage {...defaultProps} {...props} />
    </QueryClientProvider>,
  );

  return {
    ...utils,
    onToggleTheOnlyUserMode:
      props.onToggleTheOnlyUserMode ?? defaultProps.onToggleTheOnlyUserMode,
  };
};

beforeEach(() => {
  localStorage.clear();
  vi.clearAllMocks();
});

describe("Rendering and local state", () => {
  it("should render all three toggles and both action buttons", () => {
    const { container } = renderPage();

    expect(container.querySelector("#is-public")).toBeInTheDocument();
    expect(container.querySelector("#is-the-only-user")).toBeInTheDocument();
    expect(container.querySelector("#is-dark-theme")).toBeInTheDocument();
    expect(screen.getByText("Написати нам")).toBeInTheDocument();
    expect(screen.getByText("Видалити акаунт")).toBeInTheDocument();
  });

  it("should reflect the isPublic and isTheOnlyUserMode props on initial render", () => {
    const { container } = renderPage({
      isPublic: true,
      isTheOnlyUserMode: true,
    });

    expect(container.querySelector("#is-public")).toBeChecked();
    expect(container.querySelector("#is-the-only-user")).toBeChecked();
  });

  it("should re-sync the isPublic checkbox when the prop changes from the parent", () => {
    const { container, rerender } = renderPage({ isPublic: false });
    const queryClient = new QueryClient();

    expect(container.querySelector("#is-public")).not.toBeChecked();

    rerender(
      <QueryClientProvider client={queryClient}>
        <SettingsPage
          isPublic={true}
          isTheOnlyUserMode={false}
          onToggleTheOnlyUserMode={vi.fn()}
        />
      </QueryClientProvider>,
    );

    expect(container.querySelector("#is-public")).toBeChecked();
  });

  it("should update theme localStorage and the document attribute when toggling dark theme", () => {
    const { container } = renderPage();
    const darkThemeToggle = container.querySelector("#is-dark-theme");

    fireEvent.click(darkThemeToggle);

    expect(localStorage.getItem("themeMode")).toBe(
      darkThemeToggle.checked ? "dark" : "light",
    );
    expect(document.documentElement.getAttribute("data-theme")).toContain(
      darkThemeToggle.checked ? "dark" : "light",
    );
  });

  it("should open the EmailToSupport modal when clicking 'Написати нам' button", () => {
    renderPage();

    const supportButton = screen.getByRole("button", { name: /написати нам/i });
    fireEvent.click(supportButton);

    expect(screen.getByText("Лист до тех. підтримки")).toBeInTheDocument();
  });
});

describe("Solo-user mode toggle", () => {
  it("should open a confirmation modal when enabling solo-user mode, without calling the callback yet", () => {
    const onToggleTheOnlyUserMode = vi.fn();
    const { container } = renderPage({
      isTheOnlyUserMode: false,
      onToggleTheOnlyUserMode,
    });

    fireEvent.click(container.querySelector("#is-the-only-user"));

    expect(onToggleTheOnlyUserMode).not.toHaveBeenCalled();
  });

  it("should call onToggleTheOnlyUserMode(false) immediately when disabling, without a confirmation modal", () => {
    const onToggleTheOnlyUserMode = vi.fn();
    const { container } = renderPage({
      isTheOnlyUserMode: true,
      onToggleTheOnlyUserMode,
    });

    fireEvent.click(container.querySelector("#is-the-only-user"));

    expect(onToggleTheOnlyUserMode).toHaveBeenCalledWith(false);
  });

  it("should close the confirmation modal without calling the callback when cancelled", () => {
    const onToggleTheOnlyUserMode = vi.fn();
    const { container } = renderPage({
      isTheOnlyUserMode: false,
      onToggleTheOnlyUserMode,
    });

    fireEvent.click(container.querySelector("#is-the-only-user"));
    fireEvent.click(screen.getByText(CANCEL_BUTTON_TEXT));

    expect(onToggleTheOnlyUserMode).not.toHaveBeenCalled();
  });
});

describe("API interaction (MSW)", () => {
  it("should call switch-my-publicity and show a success toast when toggling isPublic", async () => {
    server.use(
      http.post("*/api/users/switch-my-publicity", () => {
        return HttpResponse.json({}, { status: 200 });
      }),
    );

    const { container } = renderPage({ isPublic: false });
    fireEvent.click(container.querySelector("#is-public"));

    await waitFor(() => {
      expect(toast.success).toHaveBeenCalledWith("Публічність змінена успішно");
    });
  });

  it("should confirm solo-user mode, call make-my-sets-private, and invoke the callback on success", async () => {
    server.use(
      http.post("*/api/users/make-my-sets-private", () => {
        return HttpResponse.json({}, { status: 200 });
      }),
    );

    const onToggleTheOnlyUserMode = vi.fn();
    const { container } = renderPage({
      isTheOnlyUserMode: false,
      onToggleTheOnlyUserMode,
    });

    fireEvent.click(container.querySelector("#is-the-only-user"));
    fireEvent.click(screen.getByText(CONFIRM_BUTTON_TEXT));

    await waitFor(() => {
      expect(onToggleTheOnlyUserMode).toHaveBeenCalledWith(true);
      expect(toast.success).toHaveBeenCalledWith(
        "Режим єдиного користувача активовано",
      );
    });
  });

  it("should show an error toast and NOT call the callback if make-my-sets-private fails", async () => {
    server.use(
      http.post("*/api/users/make-my-sets-private", () => {
        return HttpResponse.json({}, { status: 500 });
      }),
    );

    const onToggleTheOnlyUserMode = vi.fn();
    const { container } = renderPage({
      isTheOnlyUserMode: false,
      onToggleTheOnlyUserMode,
    });

    fireEvent.click(container.querySelector("#is-the-only-user"));
    fireEvent.click(screen.getByText(CONFIRM_BUTTON_TEXT));

    await waitFor(() => {
      expect(toast.error).toHaveBeenCalledWith(
        "Не вдалося активувати режим єдиного користувача",
      );
      expect(onToggleTheOnlyUserMode).not.toHaveBeenCalled();
    });
  });

  it("should successfully trigger account deletion API request", async () => {
    let deleteCalled = false;
    server.use(
      http.delete("*/api/users/me", () => {
        deleteCalled = true;
        return HttpResponse.json({}, { status: 200 });
      }),
    );

    renderPage();

    fireEvent.click(screen.getByText("Видалити акаунт"));
    fireEvent.click(screen.getByText(CONFIRM_BUTTON_TEXT));

    await waitFor(() => {
      expect(deleteCalled).toBe(true);
    });
  });

  it("should show an error toast if account deletion fails", async () => {
    server.use(
      http.delete("*/api/users/me", () => {
        return HttpResponse.json({}, { status: 500 });
      }),
    );

    renderPage();

    fireEvent.click(screen.getByText("Видалити акаунт"));
    fireEvent.click(screen.getByText(CONFIRM_BUTTON_TEXT));

    await waitFor(() => {
      expect(toast.error).toHaveBeenCalledWith("Акаунт видалити не вдалося.");
    });
  });
});
