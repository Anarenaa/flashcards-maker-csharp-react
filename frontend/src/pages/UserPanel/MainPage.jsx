import SetsPageLayout from "../../components/Layouts/SetsPageLayout";
import "./MainPage.scss";

export default function MainPage() {
  return (
    <SetsPageLayout
      endpoint="/sets"
      isMine={false}
      loadingText="Шукаємо сети..."
    />
  );
}
