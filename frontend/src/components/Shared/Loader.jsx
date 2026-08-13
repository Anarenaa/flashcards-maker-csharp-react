import { Loader2 } from "lucide-react";
import "./Loader.scss";

export default function Loader({ loadingText = "Завантаження...", scale = 1.0, fullHeight = false }) {
  return (
    <div className={`loading-container ${fullHeight ? "full-height" : ""}`}>
      <Loader2 className="spinner-icon" size={34 * scale} />
      <p className="loading-text" style={{fontSize: 1.1 * scale + 'rem'}}>{loadingText}</p>
    </div>
  );
}
