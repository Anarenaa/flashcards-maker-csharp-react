import { useParams } from "react-router";
import { useUrlPagination } from "../../../hooks/useUrlPagination";
import SetDetailsLayout from "../../../components/Layouts/SetDetailsLayout";
import { useHandleBackLink } from "../../../utils/useHandleBackLink";

export default function SetDetailsPage() {
  const { id } = useParams();
  const { page, pageSize, setPaginationParams } = useUrlPagination();

  const handleBack = useHandleBackLink("/main");

  return (
    <SetDetailsLayout
      endpoint="/sets"
      setId={id}
      initialPage={page}
      initialPageSize={pageSize}
      onParamsChange={setPaginationParams}
      isMine={false}
      onBack={handleBack}
      backLabel="Назад"
      onPracticeLink={`/sets/${id}/practice`}
    />
  );
}
