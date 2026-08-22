import { Minus, Plus } from "lucide-react";
import "./Stepper.scss";

export default function Stepper({ value, onChange, min = 1, max = 100 }) {
  const handleInputChange = (e) => {
    const val = parseInt(e.target.value, 10);

    // allow writing
    if (isNaN(val)) {
      onChange("");
      return;
    }

    if (val >= min && val <= max) {
      onChange(val);
    }
  };

  return (
    <div className="stepper">
      <button
        type="button"
        onClick={() => value > min && onChange(Number(value) - 1)}
        className="decrement-btn"
      >
        <Minus size={22} className="icon" />
      </button>
      <input
        type="number"
        value={value}
        onChange={handleInputChange}
        onBlur={() => !value && onChange(min)}
      />
      <button
        type="button"
        onClick={() => value < max && onChange(Number(value) + 1)}
        className="increment-btn"
      >
        <Plus size={22} className="icon" />
      </button>
    </div>
  );
}
