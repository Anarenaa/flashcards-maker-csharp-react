import { X } from "lucide-react";
import "./ModalWrapper.scss";

export const ModalWrapper = ({
  isOpen,
  onClose,
  children,
  showCloseButton,
  isCentered = false,
  size = "md", // "sm" | "md" | "lg"
}) => {
  if (!isOpen) return null;

  return (
    <div className="modal-backdrop">
      <div
        className={`modal-content modal-content--${size} ${isCentered ? "centered" : ""}`}
      >
        {showCloseButton && (
          <button className="close-btn" onClick={onClose}>
            <X size={20} />
          </button>
        )}

        {children}
      </div>
    </div>
  );
};