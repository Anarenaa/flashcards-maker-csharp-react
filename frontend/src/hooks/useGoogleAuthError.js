import { useEffect } from "react";
import { useSearchParams, useNavigate } from "react-router";

export function useGoogleAuthError(redirectPath) {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();

  return useEffect(() => {
    const googleError = searchParams.get("error");
    if (!googleError) return;

    let message = "Помилка сервера під час входу через Google.";
    if (googleError === "google_failed") {
      message = "Не вдалося авторизуватися через Google. Спробуйте ще раз.";
    }

    const newParams = new URLSearchParams(searchParams);
    newParams.delete("error");
    const searchStr = newParams.toString();
    
    navigate(`${redirectPath}${searchStr ? `?${searchStr}` : ""}`, { replace: true });

    return { global: message };
  }, [searchParams, navigate, redirectPath]);
}