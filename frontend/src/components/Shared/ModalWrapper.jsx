import { X } from "lucide-react";
import "./ModalWrapper.scss";

export const ModalWrapper = ({
  isOpen,
  onClose,
  children,
  showCloseButton,
}) => {
  if (!isOpen) return null;

  return (
    <div className="modal-backdrop">
      <div className="modal-content">
        {showCloseButton && (
          <button className="close-btn" onClick={onClose}>
            <X size={18} />
          </button>
        )}

        {children}
      </div>
    </div>
  );
};
