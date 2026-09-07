import "./ConfirmModal.scss";

export default function ConfirmModal({ onConfirm, onCancel }) {
  return (
    <div className="modal-backdrop position-top">
      <div className="confirm-modal">
        <p>Ви точно бажаєте видалити цей елемент?</p>
        <div className="confirm-actions">
          <button onClick={onConfirm}>Так</button>
          <button onClick={onCancel}>Ні</button>
        </div>
      </div>
    </div>
  );
}
