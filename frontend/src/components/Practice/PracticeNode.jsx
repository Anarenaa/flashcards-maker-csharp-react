import React from "react";
import "./PracticeNode.scss";

export default function PracticeNode({
  modeId,
  unlocked,
  nextWillBeUnlocked,
  isLast,
  getModeName,
  setId,
  isMySet,
  isReversed,
  navigate,
}) {
  const handleNodeClick = () => {
    const basePath = isMySet ? `/my-sets/${setId}/test` : `/sets/${setId}/test`;
    navigate(`${basePath}?mode=${modeId}&isReversed=${isReversed}`);
  };

  return (
    <div className="node">
      {unlocked ? (
        <button
          onClick={handleNodeClick}
          className="circle unlocked"
        >
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
