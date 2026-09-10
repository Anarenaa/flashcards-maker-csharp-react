import "./ConfirmModal.scss";

export default function ConfirmModal({ text = "Ви точно бажаєте видалити цей елемент?", onConfirm, onCancel }) {
  return (
    <div className="modal-backdrop position-top">
      <div className="confirm-modal">
        <p>{text}</p>
        <div className="confirm-actions">
          <button onClick={onConfirm}>Так</button>
          <button onClick={onCancel}>Ні</button>
        </div>
      </div>
    </div>
  );
}
