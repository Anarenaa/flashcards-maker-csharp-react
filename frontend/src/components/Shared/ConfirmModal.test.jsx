import { render, fireEvent, screen } from "@testing-library/react";
import ConfirmModal from "./ConfirmModal";

it("should render confirmation text and action buttons", () => {
  render(<ConfirmModal isOpen={true} onConfirm={() => {}} onCancel={() => {}} />);

  expect(screen.getByText(/Ви точно бажаєте видалити цей елемент/i)).not.toBeNull();
  expect(screen.getByText("Так")).not.toBeNull();
  expect(screen.getByText("Ні")).not.toBeNull();
});

it("should call onConfirm when clicking 'Так' button", () => {
  const handleConfirm = vi.fn();
  render(<ConfirmModal isOpen={true} onConfirm={handleConfirm} onCancel={() => {}} />);

  fireEvent.click(screen.getByText("Так"));

  expect(handleConfirm).toHaveBeenCalledTimes(1);
});

it("should call onCancel when clicking 'No' button", () => {
  const handleCancel = vi.fn();
  render(<ConfirmModal isOpen={true} onConfirm={() => {}} onCancel={handleCancel} />);

  fireEvent.click(screen.getByText("Ні"));

  expect(handleCancel).toHaveBeenCalledTimes(1);
});