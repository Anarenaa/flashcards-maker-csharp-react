import { render, fireEvent, screen } from "@testing-library/react";
import ActionsDropdown from "./ActionsDropdown";

it("should render trigger button and keep menu closed initially", () => {
  const { container } = render(<ActionsDropdown onEdit={() => {}} onDelete={() => {}} />);
  
  expect(container.querySelector(".actions-trigger-btn")).not.toBeNull();
  expect(container.querySelector(".actions-menu")).toBeNull();
});

it("should toggle menu open and closed when clicking the trigger button", () => {
  const { container } = render(<ActionsDropdown onEdit={() => {}} onDelete={() => {}} />);
  const triggerBtn = container.querySelector(".actions-trigger-btn");

  fireEvent.click(triggerBtn);
  expect(container.querySelector(".actions-menu")).not.toBeNull();

  fireEvent.click(triggerBtn);
  expect(container.querySelector(".actions-menu")).toBeNull();
});

it("should call onEdit and close menu when clicking edit option", () => {
  const handleEdit = vi.fn();
  const { container } = render(<ActionsDropdown onEdit={handleEdit} onDelete={() => {}} />);

  fireEvent.click(container.querySelector(".actions-trigger-btn"));
  const editBtn = screen.getByText(/Редагувати/i);
  
  fireEvent.click(editBtn);

  expect(handleEdit).toHaveBeenCalledTimes(1);
  expect(container.querySelector(".actions-menu")).toBeNull();
});

it("should call onDelete and close menu when clicking delete option", () => {
  const handleDelete = vi.fn();
  const { container } = render(<ActionsDropdown onEdit={() => {}} onDelete={handleDelete} />);

  fireEvent.click(container.querySelector(".actions-trigger-btn"));
  const deleteBtn = screen.getByText(/Видалити/i);
  
  fireEvent.click(deleteBtn);

  expect(handleDelete).toHaveBeenCalledTimes(1);
  expect(container.querySelector(".actions-menu")).toBeNull();
});

it("should close menu when clicking outside of component", () => {
  const { container } = render(
    <div>
      <div data-testid="outside">Outside</div>
      <ActionsDropdown onEdit={() => {}} onDelete={() => {}} />
    </div>
  );

  fireEvent.click(container.querySelector(".actions-trigger-btn"));
  expect(container.querySelector(".actions-menu")).not.toBeNull();

  fireEvent.mouseDown(screen.getByTestId("outside"));
  expect(container.querySelector(".actions-menu")).toBeNull();
});