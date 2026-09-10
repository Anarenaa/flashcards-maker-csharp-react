import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { createRoot } from "react-dom/client";
import { BrowserRouter } from "react-router";
import "./themes.css";
import "./index.css";
import App from "./App.jsx";

// QueryClient — це "сховище" всього кешу запитів у застосунку.
// Один інстанс на весь застосунок, створюється один раз поза компонентом.
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      // staleTime — скільки часу дані вважаються "свіжими" і НЕ
      // будуть перезапитані автоматично при повторному використанні.
      // Це наш ручний TTL, тільки вбудований і застосовується до
      // будь-якого запиту за замовчуванням.
      staleTime: 60 * 1000, // 1 хв за замовчуванням для всіх запитів
    },
  },
});

createRoot(document.getElementById("root")).render(
  <BrowserRouter>
    <QueryClientProvider client={queryClient}>
      <App />
    </QueryClientProvider>
  </BrowserRouter>,
);
