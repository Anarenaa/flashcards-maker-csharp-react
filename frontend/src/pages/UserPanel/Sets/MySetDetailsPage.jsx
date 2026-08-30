import { useParams } from "react-router";
import { useUrlPagination } from "../../../hooks/useUrlPagination";
import { useHandleBackLink } from "../../../utils/useHandleBackLink";
import SetDetailsLayout from "../../../components/Layouts/SetDetailsLayout";

export default function MySetDetailsPage() {
  const { id } = useParams();
  const { page, pageSize, setPaginationParams } = useUrlPagination();

  const handleBack = useHandleBackLink("/my-sets");

  return (
    <SetDetailsLayout
      endpoint="/my-sets"
      setId={id}
      initialPage={page}
      initialPageSize={pageSize}
      onParamsChange={setPaginationParams}
      isMine={true}
      onBack={handleBack}
      backLabel="Назад до моїх сетів"
      onPracticeLink={`/my-sets/${id}/practice`}
    />
  );
}