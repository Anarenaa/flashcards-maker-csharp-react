import { render, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import PasswordField from "./PasswordField";

const mockRegister = (name, handleChange = () => {}) => () => ({
  name,
  onChange: handleChange,
});

it("should render input with correct placeholder, name, and class", () => {
  const { container } = render(
    <PasswordField
      placeholder="Введіть пароль"
      name="password"
      inputClassName="custom-input-class"
      register={mockRegister("password")}
    />
  );

  const input = container.querySelector("input");

  expect(input).not.toBeNull();
  expect(input.getAttribute("placeholder")).toBe("Введіть пароль");
  expect(input.getAttribute("name")).toBe("password");
  expect(input.getAttribute("type")).toBe("password");
  expect(input).toHaveClass("custom-input-class");
});

it("should toggle input type between 'password' and 'text' on icon click", () => {
  const { container } = render(
    <PasswordField
      placeholder="Password"
      name="password"
      register={mockRegister("password")}
    />
  );

  const input = container.querySelector("input");
  const toggleButton = container.querySelector(".toggle-password");

  expect(input.getAttribute("type")).toBe("password");

  fireEvent.click(toggleButton);
  expect(input.getAttribute("type")).toBe("text");

  fireEvent.click(toggleButton);
  expect(input.getAttribute("type")).toBe("password");
});

it("should call onChange handler when typing", () => {
  const handleChange = vi.fn(); 

  const { container } = render(
    <PasswordField
      placeholder="Password"
      name="password"
      register={mockRegister("password", handleChange)}
    />
  );

  const input = container.querySelector("input");

  fireEvent.change(input, { target: { value: "my-secret-pass" } });

  expect(handleChange).toHaveBeenCalled();
});