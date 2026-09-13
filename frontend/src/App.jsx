import { useState, useEffect } from "react";
import { Route, Routes, useNavigate } from "react-router";
import { Toaster } from "react-hot-toast";
import { useQuery } from "@tanstack/react-query";
import api from "./services/api";
import Loader from "./components/Shared/Loader";
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
import PracticeTestPage from "./pages/UserPanel/Practice/PracticeTestPage";
import UserProfilePage from "./pages/UserPanel/UserProfile/UserProfilePage";
import UserSetsPage from "./pages/UserPanel/UserProfile/UserSetsPage";

function App() {
  const [isAuth, setIsAuth] = useState(null);
  const [isTheOnlyUserMode, setIsTheOnlyUserMode] = useState(() => {
    const saved = localStorage.getItem("isTheOnlyUserMode");
     if (saved === null) {
      localStorage.setItem("isTheOnlyUserMode", "false");
      return false;
    }
    return saved === "true";
  });
  const navigate = useNavigate();

  const handleToggleTheOnlyUserMode = (newValue) => {
    setIsTheOnlyUserMode(newValue);
    localStorage.setItem("isTheOnlyUserMode", newValue ? "true" : "false");
  };

  const { data: user, isLoading: isUserLoading } = useQuery({
    queryKey: ["my-profile"],
    queryFn: () => api.get("users/me").then((res) => res.data),
    staleTime: Infinity,
    enabled: !!isAuth,
  });

  useEffect(() => {
    const checkUser = async () => {
      try {
        const res = await api.get("/auth/me", { skipErrorToast: true });
        setIsAuth(res.data);

        // redirect authenticated users from the root path to /main so the browser URL matches the page
        if (window.location.pathname === "/") {
          navigate("/main", { replace: true });
        }
      } catch (error) {
        setIsAuth(false);
      }
    };

    checkUser();
  }, []);

  if (isAuth === null || (isAuth === true && isUserLoading)) {
    return (
      <Loader
        loadingText="Завантаження додатка..."
        scale={1.2}
        fullHeight={true}
      />
    );
  }

  return (
    <>
      <Toaster position="top-right" reverseOrder={false} />
      <Routes>
        <Route path="/" element={isAuth ? <MainPage /> : <HomePage />} />
        <Route path="register" element={<RegisterPage />} />
        <Route path="login" element={<LoginPage />} />
        <Route path="verify-email" element={<VerifyEmailPage />} />
        <Route path="email-sent" element={<EmailSentPage />} />
        <Route path="change-password" element={<ChangePassword />} />

        <Route
          element={
            <Layout currentUser={user} isTheOnlyUserMode={isTheOnlyUserMode} />
          }
        >
          <Route path="main" element={<MainPage />} />
          <Route path="my-sets" element={<MySetsPage isTheOnlyUserMode={isTheOnlyUserMode} />} />
          {
            //<Route path="my-collections" element={<MyCollectionsPage />} />
          }
          <Route
            path="my-profile"
            element={<MyProfilePage currentUser={user} />}
          />
          <Route
            path="settings"
            element={<SettingsPage isPublic={user?.isPublic} isTheOnlyUserMode={isTheOnlyUserMode} onToggleTheOnlyUserMode={handleToggleTheOnlyUserMode} />}
          />
        </Route>

        <Route path="sets/:id">
          <Route index element={<SetDetailsPage />} />
          <Route path="practice">
            <Route index element={<PracticeMapPage />} />
            <Route path="test" element={<PracticeTestPage />} />
          </Route>
        </Route>

        <Route path="my-sets/:id">
          <Route index element={<MySetDetailsPage />} />
          <Route path="practice">
            <Route index element={<PracticeMapPage />} />
            <Route path="test" element={<PracticeTestPage />} />
          </Route>
        </Route>

        <Route path="users">
          <Route path=":id" element={<UserProfilePage />} />
          <Route path=":id/sets" element={<UserSetsPage />} />
        </Route>
      </Routes>
    </>
  );
}

export default App;
