import { useState, useEffect } from "react";
import { Route, Routes } from "react-router";
import api from "./services/api";
import HomePage from "./pages/HomePage";
import MainPage from "./pages/MainPage";
import RegisterPage from "./pages/Auth/RegisterPage";
import LoginPage from "./pages/Auth/LoginPage";

function App() {
  const [isAuth, setIsAuth] = useState(null);

  useEffect(() => {
    api.get("/auth/me")
      .then(() => setIsAuth(true))
      .catch(() => setIsAuth(false));
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
      <Route path="/" element={ isAuth ? <MainPage /> : <HomePage />} />
      <Route path="register" element={<RegisterPage />} />
      <Route path="login" element={<LoginPage />} />
    </Routes>
  );
}

export default App;
