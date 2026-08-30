import { useNavigate } from "react-router";

export function useHandleBackLink(fallbackUrl) {
  const navigate = useNavigate();

  return () => {
    if (window.history.state && window.history.state.idx > 0) {
      navigate(-1);
    } else {
      navigate(fallbackUrl);
    }
  };
}