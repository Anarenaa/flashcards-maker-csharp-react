import React, { useEffect } from 'react';
import { AlertCircle, CheckCircle2, TriangleAlert, X } from 'lucide-react';
import './Alert.scss';

export default function Alert({ type, message, onClose, duration = 4000 }) {
  
  // Autoclose
  useEffect(() => {
    if (!message || !onClose) return;

    const timer = setTimeout(() => {
      onClose();
    }, duration);

    return () => clearTimeout(timer);
  }, [message, onClose, duration]);

  if (!message) return null;

  const icons = {
    success: <CheckCircle2 size={20} />,
    warning: <TriangleAlert size={20} />,
    error: <AlertCircle size={20} />
  };

  return (
    <div className={`custom-alert alert-${type}`}>
      <div className="alert-content">
        <span className="alert-icon">{icons[type]}</span>
        <span className="alert-text">{message}</span>
      </div>
      {onClose && (
        <button type="button" className="alert-close" onClick={onClose}>
          <X size={18} />
        </button>
      )}
    </div>
  );
}