import { useEffect } from "react";
import { useSearchParams, useNavigate } from "react-router";

export function useGoogleAuthError(redirectPath, setErrors) {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();

  useEffect(() => {
    const googleError = searchParams.get("error");
    if (!googleError) return;

    let message = "Помилка сервера під час входу через Google.";
    if (googleError === "google_failed") {
      message = "Не вдалося авторизуватися через Google. Спробуйте ще раз.";
    }

    if (setErrors) {
      setErrors({ global: message });
    }

    const newParams = new URLSearchParams(searchParams);
    newParams.delete("error");
    const searchStr = newParams.toString();
    
    navigate(`${redirectPath}${searchStr ? `?${searchStr}` : ""}`, { replace: true });
    
  }, [searchParams, navigate, redirectPath, setErrors]);
}