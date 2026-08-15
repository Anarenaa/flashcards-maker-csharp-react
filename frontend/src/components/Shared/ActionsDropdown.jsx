import { useState, useRef, useEffect } from "react";
import { MoreVertical, Edit, Trash2 } from "lucide-react";
import "./ActionsDropdown.scss";

export default function ActionsDropdown({ onEdit, onDelete }) {
  const [isOpen, setIsOpen] = useState(false);
  const menuRef = useRef(null);

  // Закриваємо меню при кліку за його межами
  useEffect(() => {
    function handleClickOutside(event) {
      if (menuRef.current && !menuRef.current.contains(event.target)) {
        setIsOpen(false);
      }
    }
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  return (
    <div className="actions-dropdown" ref={menuRef}>
      <button 
        className="actions-trigger-btn icon" 
        onClick={() => setIsOpen((prev) => !prev)}
      >
        <MoreVertical size={18} />
      </button>

      {isOpen && (
        <div className="actions-menu">
          <button 
            className="action-item" 
            onClick={() => { setIsOpen(false); onEdit(); }}
          >
            <Edit size={14} /> Редагувати
          </button>
          <button 
            className="action-item delete" 
            onClick={() => { setIsOpen(false); onDelete(); }}
          >
            <Trash2 size={14} /> Видалити
          </button>
        </div>
      )}
    </div>
  );
}