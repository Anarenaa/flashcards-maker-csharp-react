import { useState, useEffect } from "react";
import { Route, Routes, useNavigate } from "react-router";
import api from "./services/api";
import Layout from "./components/Layouts/Layout";
import HomePage from "./pages/HomePage";
import RegisterPage from "./pages/Auth/RegisterPage";
import LoginPage from "./pages/Auth/LoginPage";
import MainPage from "./pages/UserPanel/MainPage";
import MySetsPage from "./pages/UserPanel/MySetsPage";
import MyCollectionsPage from "./pages/UserPanel/MyCollectionsPage";
import MyProfilePage from "./pages/UserPanel/MyProfilePage";
import SettingsPage from "./pages/SettingsPage";
import VerifyEmailPage from "./pages/Auth/ResetPassword/VerifyEmailPage";
import EmailSentPage from "./pages/Auth/ResetPassword/EmailSentPage";
import ChangePassword from "./pages/Auth/ResetPassword/ChangePassword";
import SetDetailsPage from "./pages/UserPanel/Sets/SetDetailsPage";
import MySetDetailsPage from "./pages/UserPanel/Sets/MySetDetailsPage";
import PracticeMapPage from "./pages/UserPanel/Practice/PracticeMapPage";

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
      <Route path="/" element={isAuth ? <MainPage /> : <HomePage />} />
      <Route path="register" element={<RegisterPage />} />
      <Route path="login" element={<LoginPage />} />
      <Route path="verify-email" element={<VerifyEmailPage />} />
      <Route path="email-sent" element={<EmailSentPage />} />
      <Route path="change-password" element={<ChangePassword />} />

      <Route element={<Layout currentUser={currentUser} />}>
        <Route path="main" element={<MainPage />} />
        <Route path="my-sets" element={<MySetsPage />} />
        <Route path="my-collections" element={<MyCollectionsPage />} />
        <Route path="my-profile" element={<MyProfilePage />} />
        <Route path="settings" element={<SettingsPage />} />
      </Route>

      <Route path="sets/:id">
        <Route index element={<SetDetailsPage />} />
        <Route path="practice" element={<PracticeMapPage />} />
      </Route>

      <Route path="my-sets/:id">
        <Route index element={<MySetDetailsPage />} />
        <Route path="practice" element={<PracticeMapPage />} />
      </Route>
    </Routes>
  );
}

export default App;
