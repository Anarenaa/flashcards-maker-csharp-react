import { render, fireEvent, act } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import Alert from "./Alert";

beforeEach(() => {
  vi.useFakeTimers();
});

afterEach(() => {
  vi.useRealTimers();
});

it("should render correct message and type class", () => {
  const { container } = render(
    <Alert type="success" message="Успішно збережено!" onClose={() => {}} />,
  );

  expect(container.textContent).toContain("Успішно збережено!");
  expect(container.querySelector(".custom-alert")).toHaveClass("alert-success");
});

it("should return null if message is missing", () => {
  const { container } = render(
    <Alert type="error" message="" onClose={() => {}} />,
  );
  expect(container.firstChild).toBeNull();
});

it("should call onClose when close button is clicked", () => {
  const handleClose = vi.fn();
  const { container } = render(
    <Alert type="error" message="Помилка" onClose={handleClose} />,
  );

  const closeButton = container.querySelector(".alert-close");
  fireEvent.click(closeButton);

  expect(handleClose).toHaveBeenCalledTimes(1);
});

it("should automatically call onClose after duration timeout", () => {
  const handleClose = vi.fn();

  render(
    <Alert
      type="warning"
      message="Попередження"
      onClose={handleClose}
      duration={3000}
    />,
  );

  expect(handleClose).not.toHaveBeenCalled();

  act(() => {
    vi.advanceTimersByTime(3000);
  });

  expect(handleClose).toHaveBeenCalledTimes(1);
});