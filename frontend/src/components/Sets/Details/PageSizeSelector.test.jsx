import { render, fireEvent } from "@testing-library/react";
import PageSizeSelector from "./PageSizeSelector";

it("should render select options, desktop buttons, and pagination counter correctly", () => {
  const pagination = {
    pageSize: 10,
    startItem: 1,
    endItem: 10,
    totalItems: 50,
  };
  const options = [10, 20, "all"];

  const { container } = render(
    <PageSizeSelector
      pagination={pagination}
      options={options}
      onChange={() => {}}
    />,
  );

  // Check mobile select
  const select = container.querySelector("select");
  expect(select.value).toBe("10");

  // Check desktop buttons (button with number 10 should be active)
  const buttons = container.querySelectorAll(".page-size-selector__option");
  expect(buttons.length).toBe(3);
  expect(buttons[0].classList.contains("active")).toBe(true);
  expect(buttons[1].classList.contains("active")).toBe(false);

  // Check pagination counter
  const counter = container.querySelector(".pagination-counter");
  expect(counter.textContent).toBe("1–10 з 50");
});

it("should call onChange with parsed number when selecting an option from mobile select", () => {
  const handleChange = vi.fn();
  const pagination = {
    pageSize: 10,
    startItem: 1,
    endItem: 10,
    totalItems: 50,
  };
  const options = [10, 20];

  const { container } = render(
    <PageSizeSelector
      pagination={pagination}
      options={options}
      onChange={handleChange}
    />,
  );

  const select = container.querySelector("select");
  fireEvent.change(select, { target: { value: "20" } });

  expect(handleChange).toHaveBeenCalledWith(20);
});

it("should call onChange with 'all' when 'all' option is selected", () => {
  const handleChange = vi.fn();
  const pagination = {
    pageSize: 10,
    startItem: 1,
    endItem: 10,
    totalItems: 50,
  };
  const options = [10, "all"];

  const { container } = render(
    <PageSizeSelector
      pagination={pagination}
      options={options}
      onChange={handleChange}
    />,
  );

  const select = container.querySelector("select");
  fireEvent.change(select, { target: { value: "all" } });

  expect(handleChange).toHaveBeenCalledWith("all");
});

it("should call onChange with option value when clicking desktop button", () => {
  const handleChange = vi.fn();
  const pagination = {
    pageSize: 10,
    startItem: 1,
    endItem: 10,
    totalItems: 50,
  };
  const options = [10, 20];

  const { container } = render(
    <PageSizeSelector
      pagination={pagination}
      options={options}
      onChange={handleChange}
    />,
  );

  const buttons = container.querySelectorAll(".page-size-selector__option");
  fireEvent.click(buttons[1]); // Click on the "20" option

  expect(handleChange).toHaveBeenCalledWith(20);
});
