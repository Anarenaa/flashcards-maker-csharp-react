import { useParams } from "react-router";
import { useUrlPagination } from "../../../hooks/useUrlPagination";
import SetDetailsLayout from "../../../components/Layouts/SetDetailsLayout";

export default function SetDetailsPage() {
  const { id } = useParams();
  const { page, pageSize, setPaginationParams } = useUrlPagination();

  return (
    <SetDetailsLayout
      endpoint="/sets"
      setId={id}
      initialPage={page}
      initialPageSize={pageSize}
      onParamsChange={setPaginationParams}
      isMine={false}
      backHref="/main"
      backLabel="Назад до головної"
      onPracticeLink={`/sets/${id}/practice`}
    />
  );
}
