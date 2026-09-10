import { render, fireEvent, waitFor, screen } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { http, HttpResponse } from "msw";
import { server } from "../../setupTests";
import EmailToSupport from "./EmailToSupportForm";

vi.mock("react-hot-toast", () => ({
  default: { success: vi.fn(), error: vi.fn() },
}));
import toast from "react-hot-toast";

const renderForm = (props = {}) => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  });

  const defaultProps = {
    isOpen: true,
    onClose: vi.fn(),
  };

  const utils = render(
    <QueryClientProvider client={queryClient}>
      <EmailToSupport {...defaultProps} {...props} />
    </QueryClientProvider>,
  );

  return {
    ...utils,
    onClose: props.onClose ?? defaultProps.onClose,
  };
};

beforeEach(() => {
  vi.clearAllMocks();
});

describe("EmailToSupport Form - Rendering and Validation", () => {
  it("should not render anything when isOpen is false", () => {
    renderForm({ isOpen: false });
    expect(
      screen.queryByText("Лист до тех. підтримки"),
    ).not.toBeInTheDocument();
  });

  it("should render title, inputs, and submit button when open", () => {
    renderForm();

    expect(screen.getByText("Лист до тех. підтримки")).toBeInTheDocument();
    expect(screen.getByPlaceholderText("Тема")).toBeInTheDocument();
    expect(screen.getByPlaceholderText("Текст...")).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "Відправити" }),
    ).toBeInTheDocument();
  });

  it("should show validation errors when submitting an empty form", async () => {
    renderForm();

    const submitButton = screen.getByRole("button", { name: "Відправити" });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText("Введіть тему листа")).toBeInTheDocument();
      expect(screen.getByText("Введіть текст-звернення")).toBeInTheDocument();
    });
  });
});

describe("EmailToSupport Form - API interaction (MSW)", () => {
  it("should successfully send the form, call onClose, reset, and show success toast", async () => {
    server.use(
      http.post("*/api/emails/to-support", () => {
        return HttpResponse.json({}, { status: 200 });
      }),
    );

    const onClose = vi.fn();
    renderForm({ onClose });

    const subjectInput = screen.getByPlaceholderText("Тема");
    const messageInput = screen.getByPlaceholderText("Текст...");
    const submitButton = screen.getByRole("button", { name: "Відправити" });

    fireEvent.change(subjectInput, { target: { value: "Проблема з входом" } });
    fireEvent.change(messageInput, {
      target: { value: "Не можу зайти в аккаунт після оновлення." },
    });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(toast.success).toHaveBeenCalledWith("Ваш лист успішно надіслано.");
      expect(onClose).toHaveBeenCalled();
    });
  });

  it("should show error toast if request fails", async () => {
    server.use(
      http.post("*/api/emails/to-support", () => {
        return HttpResponse.json({}, { status: 500 });
      }),
    );

    renderForm();

    const subjectInput = screen.getByPlaceholderText("Тема");
    const messageInput = screen.getByPlaceholderText("Текст...");
    const submitButton = screen.getByRole("button", { name: "Відправити" });

    fireEvent.change(subjectInput, { target: { value: "Тест помилки" } });
    fireEvent.change(messageInput, {
      target: { value: "Текст тестового повідомлення." },
    });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(toast.error).toHaveBeenCalledWith("Лист не вдалося надіслати");
    });
  });
});
