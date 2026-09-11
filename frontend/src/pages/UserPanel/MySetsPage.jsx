import { useState } from "react";
import SetsPageLayout from "../../components/Layouts/SetsPageLayout";
import "./MySetsPage.scss";
import SetForm from "../../features/sets/SetForm";

export default function MySetsPage({ isTheOnlyUserMode }) {
  const [isFormOpen, setIsFormOpen] = useState(false);
  return (
    <>
      <SetsPageLayout
        endpoint="/my-sets"
        isMine={true}
        isInfinity={true}
        isTheOnlyUserMode={isTheOnlyUserMode}
        loadingText="Завантажуємо твої сети..."
        extraCard={
          <button
            className="set-card add-new-card"
            onClick={() => setIsFormOpen(true)}
          >
            <span>+</span>
            Створити новий сет
          </button>
        }
      />
      {isFormOpen && <SetForm isOpen={isFormOpen} onClose={() => setIsFormOpen(false)} isTheOnlyUserMode={isTheOnlyUserMode} />}
    </>
  );
}
