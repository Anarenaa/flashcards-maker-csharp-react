import { useState, useEffect } from "react";
import { Route, Routes, useNavigate } from "react-router";
import api from "./services/api";
import HomePage from "./pages/HomePage";
import RegisterPage from "./pages/Auth/RegisterPage";
import LoginPage from "./pages/Auth/LoginPage";
import MainPage from "./pages/MainPage";
import MySetsPage from "./pages/MySetsPage";
import MyCollectionsPage from "./pages/MyCollectionsPage";
import MyProfilePage from "./pages/MyProfilePage";
import SettingsPage from "./pages/SettingsPage";

function App() {
  const [isAuth, setIsAuth] = useState(null);
  const [currentUser, setCurrentUser] = useState(null);
  const navigate = useNavigate();

  useEffect(() => {
    const checkUser = async () => {
      try {
        const res = await api.get("/auth/me");

        setCurrentUser(res.data); // { id, role, username, avatarUrl, email }
        setIsAuth(true);

        if (window.location.pathname === "/") {
          navigate("/main", { replace: true });
        }
      } catch (error) {
        setCurrentUser(null);
        setIsAuth(false);
      }
    };

    checkUser();
  }, []);
  
  if (isAuth === null) {
    //заглушка, змінити на красивий компонент прогрузки
    return (
      <div className="flex h-screen w-screen items-center justify-center bg-zinc-950 text-white">
        <div className="animate-pulse text-xl">Завантаження додатка...</div>
      </div>
    );
  }
  
  return (
    <Routes>
      <Route path="/" element={<HomePage />} />
      <Route path="register" element={<RegisterPage />} />
      <Route path="login" element={<LoginPage />} />

      <Route path="main" element={<MainPage currentUser={currentUser} />} />
      <Route path="my-sets" element={<MySetsPage currentUser={currentUser} />} />
      <Route path="my-collections" element={<MyCollectionsPage currentUser={currentUser} />} />
      <Route path="my-profile" element={<MyProfilePage currentUser={currentUser} />} />
      <Route path="settings" element={<SettingsPage currentUser={currentUser} />} />
    </Routes>
  );
}

export default App;
