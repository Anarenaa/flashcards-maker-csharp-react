import { Outlet } from "react-router";
import Header from "../Shared/Header";
import "./Layout.scss";

export default function Layout({ currentUser, isTheOnlyUserMode }) {
  return (
    <div className="container">
      <Header currentUser={currentUser} isTheOnlyUserMode={isTheOnlyUserMode} />

      <main className="content-area">
        <Outlet />
      </main>
    </div>
  );
}
