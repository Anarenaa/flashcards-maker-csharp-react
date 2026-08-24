import { render, fireEvent } from "@testing-library/react";
import ResetProgressModal from "./ResetProgressModal";

describe("ResetProgressModal", () => {
  it("should render modal title, question, and action buttons correctly when isOpen is true", () => {
    const { container } = render(
      <ResetProgressModal
        isOpen={true}
        onConfirmBatch={() => {}}
        onConfirmAll={() => {}}
        onClose={() => {}}
      />,
    );

    expect(container.querySelector(".title").textContent).toBe(
      "Скидання прогресу",
    );
    expect(
      container.querySelector(".reset-progress-question").textContent,
    ).toBe("Бажаєте скинути прогрес лише для даних карток чи для всього сету?");

    const buttons = container.querySelectorAll("button");

    const batchBtn = Array.from(buttons).find((b) =>
      b.textContent.includes("Тільки для цих карток"),
    );
    const allBtn = Array.from(buttons).find((b) =>
      b.textContent.includes("Для всього сету"),
    );

    expect(batchBtn).toBeDefined();
    expect(allBtn).toBeDefined();
  });

  it("should not render anything when isOpen is false", () => {
    const { container } = render(
      <ResetProgressModal
        isOpen={false}
        onConfirmBatch={() => {}}
        onConfirmAll={() => {}}
        onClose={() => {}}
      />,
    );

    expect(container.firstChild).toBeNull();
  });

  it("should call onConfirmBatch when clicking batch reset button", () => {
    const handleConfirmBatch = vi.fn();
    const { container } = render(
      <ResetProgressModal
        isOpen={true}
        onConfirmBatch={handleConfirmBatch}
        onConfirmAll={() => {}}
        onClose={() => {}}
      />,
    );

    const buttons = container.querySelectorAll("button");
    const batchBtn = Array.from(buttons).find((b) =>
      b.textContent.includes("Тільки для цих карток"),
    );

    fireEvent.click(batchBtn);

    expect(handleConfirmBatch).toHaveBeenCalledTimes(1);
  });

  it("should call onConfirmAll when clicking all set reset button", () => {
    const handleConfirmAll = vi.fn();
    const { container } = render(
      <ResetProgressModal
        isOpen={true}
        onConfirmBatch={() => {}}
        onConfirmAll={handleConfirmAll}
        onClose={() => {}}
      />,
    );

    const buttons = container.querySelectorAll("button");
    const allBtn = Array.from(buttons).find((b) =>
      b.textContent.includes("Для всього сету"),
    );

    fireEvent.click(allBtn);

    expect(handleConfirmAll).toHaveBeenCalledTimes(1);
  });
});
