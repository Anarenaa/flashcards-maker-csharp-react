import "./StatsGrid.scss";

export default function StatsGrid({ user, columnCount = 2 }) {
  return (
    <div className="stats-grid" style={{gridTemplateColumns: `repeat(${columnCount}, 1fr)`}}>
      <div className="stat-card">
        <span className="label">Кількість сетів</span>
        <div className="stat-number">{user?.setsCount}</div>
      </div>

      <div className="stat-card">
        <span className="label">Кількість карток</span>
        <div className="stat-number">{user?.flashcardsCount}</div>
      </div>

      <div className="stat-card">
        <span className="label">Вивчено cетів</span>
        <div className="stat-number">{user?.completedSets}</div>
      </div>

      <div className="stat-card">
        <span className="label">Вивчено карток</span>
        <div className="stat-number">{user?.masteredCards}</div>
      </div>
    </div>
  );
}
