import { render, screen, fireEvent } from "@testing-library/react";
import PaginationFooter from "./PaginationFooter";

const basePagination = {
  currentPage: 2,
  pageSize: 20,
  totalItems: 100,
  hasPrev: true,
  hasNext: true,
};

it("does not render when totalItems <= pageSize", () => {
  const { container } = render(
    <PaginationFooter
      pagination={{ ...basePagination, totalItems: 15 }}
      onPageChange={vi.fn()}
    />,
  );

  expect(container.firstChild).toBeNull();
});

it("shows the current page number", () => {
  render(
    <PaginationFooter pagination={basePagination} onPageChange={vi.fn()} />,
  );
  expect(screen.getByText("Сторінка 2")).toBeInTheDocument();
});

it("disables the 'prev' button when hasPrev is false", () => {
  render(
    <PaginationFooter
      pagination={{ ...basePagination, hasPrev: false }}
      onPageChange={vi.fn()}
    />,
  );
  const buttons = screen.getAllByRole("button");
  expect(buttons[0]).toBeDisabled();
  expect(buttons[1]).not.toBeDisabled();
});

it("calls onPageChange with the correct direction", () => {
  const onPageChange = vi.fn();
  render(
    <PaginationFooter
      pagination={basePagination}
      onPageChange={onPageChange}
    />,
  );

  const buttons = screen.getAllByRole("button");
  fireEvent.click(buttons[0]);
  expect(onPageChange).toHaveBeenCalledWith("prev");

  fireEvent.click(buttons[1]);
  expect(onPageChange).toHaveBeenCalledWith("next");
});
