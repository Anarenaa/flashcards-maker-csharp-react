import { useNavigate } from 'react-router';
import './PracticeNode.scss';

export default function PracticeNode({
  modeId,
  unlocked,
  nextWillBeUnlocked,
  isLast,
  getModeName,
  setId,
  isMySet,
  isReversed,
  page,
  pageSize,
}) {
  const navigate = useNavigate();

  const handleNodeClick = () => {
    const basePath = isMySet
      ? `/my-sets/${setId}/practice/test`
      : `/sets/${setId}/practice/test`;
    const params = new URLSearchParams({
      page,
      pageSize,
      mode: modeId,
      isReversed,
    });
    navigate(`${basePath}?${params.toString()}`);
  };

  return (
    <div className="node">
      {unlocked ? (
        <button onClick={handleNodeClick} className="circle unlocked">
          {getModeName(modeId)}
        </button>
      ) : (
        <div className="circle locked">
          {getModeName(modeId)}
          <div className="locked-label">🔒 Заблоковано</div>
        </div>
      )}
      {!isLast && (
        <div className={`line ${nextWillBeUnlocked ? "active" : ""}`}></div>
      )}
    </div>
  );
}
