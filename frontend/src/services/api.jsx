import axios from "axios";
import toast from "react-hot-toast";

const api = axios.create({
  baseURL: "/api", // vite.config.js
  withCredentials: true, // cookies
});

// errors mark
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (
      axios.isCancel(error) ||
      error.code === "ERR_CANCELED" ||
      (error.config && error.config.skipErrorToast)
    ) {
      return Promise.reject(error);
    }

    if (
      !window.navigator.onLine ||
      error.message === "Network Error" ||
      error.code === "ERR_NETWORK"
    ) {
      error.globalMessage =
        "Здається, у вас зник інтернет. Перевірте підключення до мережі 🌐";
    }
    // response === undefined
    else if (!error.response) {
      error.globalMessage =
        "Не вдалося з'єднатися з сервером. Можливо, ведуться технічні роботи.";
    } else {
      const status = error.response.status;

      const isAuthCheck = error.config?.url?.includes("/auth/me");

      if (status === 401 && !isAuthCheck) {
        window.location.href = "/login";
      } else if (status === 403) {
        window.location.href = "/not-allowed";
      } else if (status === 429) {
        error.globalMessage =
          "Занадто багато запитів! Будь ласка, спробуйте трохи пізніше.";
      } else if (status >= 500) {
        error.globalMessage =
          "Не вдалося з'єднатися з сервером. Спробуйте пізніше.";
      }
    }

    if (error.globalMessage) {
      toast.error(error.globalMessage, {
        id: error.globalMessage, // Prevents the duplication of identical toasts
        className: "toast-error",
      });
    }

    return Promise.reject(error);
  },
);

export default api;
