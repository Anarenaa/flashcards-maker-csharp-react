import api from "./api";

// Категорії редагує адмін — можуть змінюватись, тому кешуємо
// не назавжди, а на обмежений час (TTL). Після спливу TTL
// наступний виклик getCategories() зробить свіжий запит.
const CATEGORIES_TTL = 5 * 60 * 1000; // 5 хвилин

let categoriesCache = { promise: null, timestamp: 0 };

export function getCategories() {
  const isStale = Date.now() - categoriesCache.timestamp > CATEGORIES_TTL;

  if (!categoriesCache.promise || isStale) {
    categoriesCache = {
      promise: api.get("/categories").then((res) => res.data),
      timestamp: Date.now(),
    };
  }

  return categoriesCache.promise;
}

// Скидає кеш вручну — наприклад, одразу після того, як адмін
// додав/змінив категорію в тій самій сесії.
export function invalidateCategories() {
  categoriesCache = { promise: null, timestamp: 0 };
}

// Типи сетів приходять з бекенду і стабільні — кешуємо
// на весь час життя SPA-сесії, без TTL.
let typesPromise = null;

export function getTypes() {
  if (!typesPromise) {
    typesPromise = api.get("/sets/types").then((res) => res.data);
  }
  return typesPromise;
}