import axios from 'axios';

const api = axios.create({
  baseURL: '/api', // vite.config.js
  withCredentials: true, // cookies
});

// errors mark
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (!window.navigator.onLine || error.message === 'Network Error' || error.code === 'ERR_NETWORK') {
      error.globalMessage = "Здається, у вас зник інтернет. Перевірте підключення до мережі 🌐";
    }
    // response === undefined
    else if (!error.response) {
        error.globalMessage = "Не вдалося з'єднатися з сервером. Можливо, ведуться технічні роботи.";
    } else {
      const status = error.response.status;

      const isAuthCheck = error.config?.url?.includes('/auth/me');

      if (status === 401 && !isAuthCheck) {
        window.location.href = '/login';
      } else if (status === 404) {
        window.location.href = '/not-found'
      } else if (status === 429) {
        error.globalMessage = "Занадто багато запитів! Будь ласка, зачекайте хвилину перед наступною спробою.";
      } else if (status >= 500) {
        error.globalMessage = "Не вдалося з'єднатися з сервером. Спробуйте пізніше.";
      }
    }
    
    return Promise.reject(error);
  }
);

export default api;