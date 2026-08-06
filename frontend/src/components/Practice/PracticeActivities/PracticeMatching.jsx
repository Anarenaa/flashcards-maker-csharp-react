import React, { useState, useMemo, useEffect } from "react";
import "./PracticeMatching.scss";

export default function PracticeMatching({
  batch,
  shuffle,
  onComplete,
  onSaveStep,
}) {
  const [sT, setST] = useState(null);
  const [sD, setSD] = useState(null);
  const [matches, setMatches] = useState(0);
  const [itemStatuses, setItemStatuses] = useState({});
  const [hiddenItems, setHiddenItems] = useState(new Set());

  useEffect(() => {
    setST(null);
    setSD(null);
    setMatches(0);
    setItemStatuses({});
    setHiddenItems(new Set());
  }, [batch]); // clean states after switching branch

  const terms = useMemo(() =>
    shuffle(batch.map((c) => ({ id: c.id, text: c.term, type: "t" }))),
    [batch]
  );

  const defs = useMemo(() =>
    shuffle(batch.map((d) => ({ id: d.id, text: d.definition, type: "d" }))),
    [batch]
  );

  const handleItemClick = (item, elementKey) => {
    if (hiddenItems.has(item.id) || itemStatuses[elementKey] === "correct")
      return;

    const nextST = item.type === "t" ? { ...item, key: elementKey } : sT;
    const nextSD = item.type === "d" ? { ...item, key: elementKey } : sD;

    if (item.type === "t") setST({ ...item, key: elementKey });
    else setSD({ ...item, key: elementKey });

    if (nextST && nextSD) {
      if (nextST.id === nextSD.id) {
        setItemStatuses((prev) => ({
          ...prev,
          [nextST.key]: "correct",
          [nextSD.key]: "correct",
        }));
        onSaveStep(nextST.id, true, 3);
        const newMatches = matches + 1;
        setMatches(newMatches);
        setST(null);
        setSD(null);

        setTimeout(() => {
          setHiddenItems((prev) => new Set([...prev, nextST.id]));
          if (newMatches === batch.length) onComplete();
        }, 300);
      } else {
        setItemStatuses((prev) => ({
          ...prev,
          [nextST.key]: "wrong",
          [nextSD.key]: "wrong",
        }));
        setST(null);
        setSD(null);

        setTimeout(() => {
          setItemStatuses((prev) => {
            const copy = { ...prev };
            delete copy[nextST.key];
            delete copy[nextSD.key];
            return copy;
          });
        }, 400);
      }
    }
  };

  return (
    <div className="matching-wrapper">
      <div className="matching-column">
        {terms.map((t, idx) => {
          const key = `t_${idx}`;
          const isSelected = sT?.key === key;
          const status = itemStatuses[key];
          const isHidden = hiddenItems.has(t.id);

          return (
            <div
              key={key}
              className={`matching-item ${isSelected ? "selected" : ""} ${status === "correct" ? "correct-flash" : ""} ${status === "wrong" ? "wrong-flash" : ""} ${isHidden ? "matched-hidden" : ""}`}
              onClick={() => handleItemClick(t, key)}
            >
              {t.text}
            </div>
          );
        })}
      </div>
      <div className="matching-column">
        {defs.map((d, idx) => {
          const key = `d_${idx}`;
          const isSelected = sD?.key === key;
          const status = itemStatuses[key];
          const isHidden = hiddenItems.has(d.id);

          return (
            <div
              key={key}
              className={`matching-item ${isSelected ? "selected" : ""} ${status === "correct" ? "correct-flash" : ""} ${status === "wrong" ? "wrong-flash" : ""} ${isHidden ? "matched-hidden" : ""}`}
              onClick={() => handleItemClick(d, key)}
            >
              {d.text}
            </div>
          );
        })}
      </div>
    </div>
  );
}