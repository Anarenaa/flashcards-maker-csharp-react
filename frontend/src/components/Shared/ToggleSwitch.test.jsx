import { render, fireEvent, screen } from "@testing-library/react";
import ToggleSwitch from "./ToggleSwitch";

it("should render with basic props (id and checkbox)", () => {
  const { container } = render(<ToggleSwitch id="test-toggle" />);

  const checkbox = container.querySelector("#test-toggle");
  expect(checkbox).toBeInTheDocument();
  expect(checkbox).toHaveAttribute("type", "checkbox");
  expect(container.querySelector(".toggle-slider")).toBeInTheDocument();
});

it("should correctly set the checked state", () => {
  const { container } = render(
    <ToggleSwitch id="checked-toggle" checked={true} />,
  );
  const checkbox = container.querySelector("#checked-toggle");

  expect(checkbox).toBeChecked();
});

it("should call the onChange handler when clicked", () => {
  const handleChange = vi.fn();
  const { container } = render(
    <ToggleSwitch id="click-toggle" onChange={handleChange} />,
  );
  const checkbox = container.querySelector("#click-toggle");

  fireEvent.click(checkbox);
  expect(handleChange).toHaveBeenCalledTimes(1);
});

it("should render label on the left when labelPosition='left'", () => {
  render(
    <ToggleSwitch id="left-label" label="Test label" labelPosition="left" />,
  );

  const labelElement = screen.getByText("Test label");
  expect(labelElement).toBeInTheDocument();
  expect(labelElement).toHaveClass("toggle-label");
});

it("should render label on the right by default", () => {
  render(<ToggleSwitch id="right-label" label="Right label" />);

  const labelElement = screen.getByText("Right label");
  expect(labelElement).toBeInTheDocument();
  expect(labelElement).toHaveClass("toggle-label");
});

it("should apply a custom class to the wrapper", () => {
  const { container } = render(
    <ToggleSwitch id="custom-class" className="my-custom-wrapper" />,
  );

  const wrapper = container.querySelector(".toggle-switch");
  expect(wrapper).toHaveClass("my-custom-wrapper");
});

it("should successfully support registerProps from React Hook Form", () => {
  const mockRegister = {
    name: "public",
    onChange: vi.fn(),
    onBlur: vi.fn(),
  };

  const { container } = render(
    <ToggleSwitch id="hook-form-toggle" registerProps={mockRegister} />,
  );
  const checkbox = container.querySelector("#hook-form-toggle");

  expect(checkbox).toHaveAttribute("name", "public");

  fireEvent.click(checkbox);
  expect(mockRegister.onChange).toHaveBeenCalled();
});
