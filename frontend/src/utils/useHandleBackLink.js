import { useNavigate } from "react-router";

export function useHandleBackLink(fallbackUrl) {
  const navigate = useNavigate();

  return () => {
    navigate(fallbackUrl, { replace: true });
  };
}