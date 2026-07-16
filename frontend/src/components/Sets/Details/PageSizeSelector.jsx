import "./PageSizeSelector.scss";

export default function PageSizeSelector({ pagination, options, onChange }) {
  return (
    <div className="page-size-selector">
      <span className="page-size-selector__label">Показувати:</span>

      <select 
        className="page-size-selector__mobile sort-select" 
        value={pagination.pageSize} 
        onChange={(e) => onChange(e.target.value === "all" ? "all" : Number(e.target.value))}
      >
        {options.map((opt) => (
          <option key={opt} value={opt}>{opt === "all" ? "Усі" : opt}</option>
        ))}
      </select>
      <div className="page-size-selector__desktop">
        {options.map((opt) => (
          <button
            key={opt}
            type="button"
            className={`page-size-selector__option ${pagination.pageSize === opt ? "active" : ""}`}
            onClick={() => onChange(opt)}
          >
            {opt === "all" ? "Усі" : opt}
          </button>
        ))}
      </div>
      
      <div className="pagination-counter">
        {`${pagination.startItem}–${pagination.endItem} з ${pagination.totalItems}`}
      </div>
    </div>
  );
}
