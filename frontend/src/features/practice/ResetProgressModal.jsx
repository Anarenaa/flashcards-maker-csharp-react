import React from "react";
import { ModalWrapper } from "../../components/Shared/ModalWrapper";
import "./ResetProgressModal.scss"; 

export default function ResetProgressModal({ isOpen, onConfirmBatch, onConfirmAll, onClose }) {
  return (
    <ModalWrapper isOpen={isOpen} onClose={onClose} showCloseButton={true} isCentered={true} >
        <h3 className="title">Скидання прогресу</h3>
        <p className="reset-progress-question">Бажаєте скинути прогрес лише для даних карток чи для всього сету?</p>
        
        <div className="modal-actions">
          <button onClick={onConfirmBatch} className="primary-button">
            Тільки для цих карток
          </button>
          <button onClick={onConfirmAll} className="primary-button">
            Для всього сету
          </button>
        </div>
    </ModalWrapper>
  );
}