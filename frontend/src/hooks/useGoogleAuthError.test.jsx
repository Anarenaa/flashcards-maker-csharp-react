import { render, waitFor } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import { MemoryRouter, Routes, Route, useLocation } from "react-router";
import { useGoogleAuthError } from "./useGoogleAuthError";

// helper that helps to mock path
function TestComponent({ redirectPath, setErrors }) {
  useGoogleAuthError(redirectPath, setErrors);
  const location = useLocation();

  return (
    <div data-testid="current-path">{location.pathname + location.search}</div>
  );
}

it("should do nothing if there is no error in URL", async () => {
  const mockSetErrors = vi.fn();

  const { getByTestId } = render(
    <MemoryRouter initialEntries={["/register"]}>
      <TestComponent redirectPath="/register" setErrors={mockSetErrors} />
    </MemoryRouter>,
  );

  expect(getByTestId("current-path").textContent).toBe("/register");
  expect(mockSetErrors).not.toHaveBeenCalled();
});
it("should work correctly when used on the /login page", async () => {
  const mockSetErrors = vi.fn();

  const { getByTestId } = render(
    <MemoryRouter initialEntries={["/login?error=google_failed"]}>
      <Routes>
        <Route
          path="/login"
          element={
            <TestComponent redirectPath="/login" setErrors={mockSetErrors} />
          }
        />
      </Routes>
    </MemoryRouter>,
  );

  expect(mockSetErrors).toHaveBeenCalledWith({
    global: "Не вдалося авторизуватися через Google. Спробуйте ще раз.",
  });

  await waitFor(() => {
    expect(getByTestId("current-path").textContent).toBe("/login");
  });
});

it("should set specific message and clear URL when google_failed error is present on /register", async () => {
  const mockSetErrors = vi.fn();

  const { getByTestId } = render(
    <MemoryRouter
      initialEntries={["/register?error=google_failed&returnUrl=abc"]}
    >
      <Routes>
        <Route
          path="/register"
          element={
            <TestComponent redirectPath="/register" setErrors={mockSetErrors} />
          }
        />
      </Routes>
    </MemoryRouter>,
  );

  expect(mockSetErrors).toHaveBeenCalledWith({
    global: "Не вдалося авторизуватися через Google. Спробуйте ще раз.",
  });

  await waitFor(() => {
    expect(getByTestId("current-path").textContent).toBe(
      "/register?returnUrl=abc",
    );
  });
});

it("should set default message for any other google error", async () => {
  const mockSetErrors = vi.fn();

  const { getByTestId } = render(
    <MemoryRouter initialEntries={["/register?error=some_internal_error"]}>
      <Routes>
        <Route
          path="/register"
          element={
            <TestComponent redirectPath="/register" setErrors={mockSetErrors} />
          }
        />
      </Routes>
    </MemoryRouter>,
  );

  expect(mockSetErrors).toHaveBeenCalledWith({
    global: "Помилка сервера під час входу через Google.",
  });

  await waitFor(() => {
    expect(getByTestId("current-path").textContent).toBe("/register");
  });
});