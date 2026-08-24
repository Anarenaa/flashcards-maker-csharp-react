import { render, fireEvent } from "@testing-library/react";
import { ModalWrapper } from "./ModalWrapper";

it("should not render anything when isOpen is false", () => {
  const { container } = render(
    <ModalWrapper isOpen={false} onClose={() => {}}>
      <p>Modal Content</p>
    </ModalWrapper>,
  );

  expect(container.firstChild).toBeNull();
});

it("should render content, correct size class, and close button when isOpen is true", () => {
  const handleClose = vi.fn();
  const { container } = render(
    <ModalWrapper
      isOpen={true}
      onClose={handleClose}
      showCloseButton={true}
      size="sm"
      isCentered={true}
    >
      <p>Modal Content</p>
    </ModalWrapper>,
  );

  expect(container.textContent).toContain("Modal Content");
  expect(container.querySelector(".modal-content--sm")).not.toBeNull();
  expect(container.querySelector(".centered")).not.toBeNull();

  const closeBtn = container.querySelector(".close-btn");
  expect(closeBtn).not.toBeNull();

  fireEvent.click(closeBtn);
  expect(handleClose).toHaveBeenCalledTimes(1);
});
