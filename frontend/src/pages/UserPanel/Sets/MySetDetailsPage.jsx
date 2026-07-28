import { useParams } from "react-router";
import SetDetailsLayout from "../../../components/Layouts/SetDetailsLayout";

export default function MySetDetailsPage() {
  const { id } = useParams();
  return (
    <SetDetailsLayout
      endpoint="/my-sets"
      setId={id}
      isMine={true}
      backHref="/my-sets"
      backLabel="Назад до моїх сетів"
      onPracticeLink={`/my-sets/${id}/practice`}
    />
  );
}