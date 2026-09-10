import "./ToggleSwitch.scss";

export default function ToggleSwitch({
  id,
  checked,
  onChange,
  registerProps,
  label,
  labelPosition = "right",
  className = "",
}) {
  return (
    <label className={`toggle-switch ${className}`} htmlFor={id}>
      {label && labelPosition === "left" && (
        <span className="toggle-label">{label}</span>
      )}
      <input
        type="checkbox"
        id={id}
        checked={checked}
        onChange={onChange}
        {...registerProps}
      />
      <span className="toggle-slider"></span>
      {label && labelPosition === "right" && (
        <span className="toggle-label">{label}</span>
      )}
    </label>
  );
}
