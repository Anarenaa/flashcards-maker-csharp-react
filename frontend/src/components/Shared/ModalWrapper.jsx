import { X } from "lucide-react";
import "./ModalWrapper.scss";

export const ModalWrapper = ({
  isOpen,
  onClose,
  children,
  showCloseButton,
  isCentered = false
}) => {
  if (!isOpen) return null;

  return (
    <div className="modal-backdrop">
      <div className={`modal-content ${isCentered && "centered"}`}>
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
