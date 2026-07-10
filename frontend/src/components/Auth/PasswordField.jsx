import { useState } from "react";
import { Eye, EyeOff } from "lucide-react";
import "./PasswordField.scss";

export default function PasswordField({
  placeholder,
  inputClassName,
  name,
  register
}) {
  const [showPassword, setShowPassword] = useState(false);

  return (
    <div className="password-container">
      <input
        type={showPassword ? "text" : "password"}
        placeholder={placeholder}
        className={inputClassName} 
        // name, value and onChange into register
        {...(register ? register(name) : {})} 
      />

      <span
        className="toggle-password"
        onClick={() => setShowPassword(!showPassword)}
      >
        {showPassword ? <EyeOff size={20} /> : <Eye size={20} />}
      </span>
    </div>
  );
}