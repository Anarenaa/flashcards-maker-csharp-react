import { useParams } from "react-router";
import SetDetailsLayout from "../../../components/Layouts/SetDetailsLayout";

export default function SetDetailsPage() {
  const { id } = useParams();
  return (
    <SetDetailsLayout
      endpoint="/sets"
      setId={id}
      isMine={false}
      backHref="/main"
      backLabel="Назад до головної"
      onPracticeLink={`/sets/${id}/practice`}
    />
  );
}