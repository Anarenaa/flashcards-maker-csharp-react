import SetsPageLayout from "../../components/Layouts/SetsPageLayout";
import "./MySetsPage.scss";

export default function MySetsPage() {
  return (
    <SetsPageLayout
      endpoint="/my-sets"
      isMine={true}
      loadingText="Шукаємо твої сети..."
      extraCard={
        <div className="set-card add-new-card">
            <span>+</span>
            Створити новий сет
        </div>
      }
    />
  );
}