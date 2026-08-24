import { render, fireEvent, screen } from "@testing-library/react";
import Stepper from "./Stepper";

it("should render current value and buttons correctly", () => {
  const { container } = render(<Stepper value={5} onChange={() => {}} />);
  
  const input = container.querySelector('input[type="number"]');
  expect(input.value).toBe("5");
  expect(container.querySelector(".decrement-btn")).not.toBeNull();
  expect(container.querySelector(".increment-btn")).not.toBeNull();
});

it("should call onChange with incremented value when clicking plus button", () => {
  const handleChange = vi.fn();
  const { container } = render(<Stepper value={5} onChange={handleChange} min={1} max={10} />);

  fireEvent.click(container.querySelector(".increment-btn"));

  expect(handleChange).toHaveBeenCalledWith(6);
});

it("should call onChange with decremented value when clicking minus button", () => {
  const handleChange = vi.fn();
  const { container } = render(<Stepper value={5} onChange={handleChange} min={1} max={10} />);

  fireEvent.click(container.querySelector(".decrement-btn"));

  expect(handleChange).toHaveBeenCalledWith(4);
});

it("should not decrement below min value", () => {
  const handleChange = vi.fn();
  const { container } = render(<Stepper value={1} onChange={handleChange} min={1} max={10} />);

  fireEvent.click(container.querySelector(".decrement-btn"));

  expect(handleChange).not.toHaveBeenCalled();
});

it("should handle manual typing within allowed range", () => {
  const handleChange = vi.fn();
  const { container } = render(<Stepper value={5} onChange={handleChange} min={1} max={10} />);

  const input = container.querySelector('input[type="number"]');
  fireEvent.change(input, { target: { value: "8" } });

  expect(handleChange).toHaveBeenCalledWith(8);
});

it("should reset to min on blur if input is empty", () => {
  const handleChange = vi.fn();
  const { container } = render(<Stepper value={""} onChange={handleChange} min={1} max={10} />);

  const input = container.querySelector('input[type="number"]');
  fireEvent.blur(input);

  expect(handleChange).toHaveBeenCalledWith(1);
});