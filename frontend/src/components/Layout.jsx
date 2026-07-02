import { Outlet } from "react-router";
import Header from "./Header";
import './Layout.scss';

export default function Layout({ currentUser }) {
  return (
    <div className="container">
      <Header currentUser={currentUser} />

      <main className="content-area">
        <Outlet />
      </main>
    </div>
  );
}