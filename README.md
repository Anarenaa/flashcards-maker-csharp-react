## Сценарій інтеграції
**Назва**: Автоматизоване створення навчального контексту на основі ШІ та зовнішніх сервісів.

**Проблема користувача**: Користувач хоче швидко створити набір карток для вивчення нової теми. 

**Вирішення проблеми (2 варіанти)**: 
- Користувач створює сет за допомогою ШІ, вказавши чіткий опис сету, який хоче отримати
- Користувач створює сет самостійно, використовуючи підказки для означення або перекладу термінів

## Використані зовнішні API
| API | BaseUrl | Endpoint | Отримані дані | Кешування |
|---|---|---|---|---|
| Gemini | `https://generativelanguage.googleapis.com/v1beta/`| `interactions` | згенеровані картки, fallback-підказка | ні, 24 год |
| Wikipedia | `https://{lang}.wikipedia.org/`| `api/rest_v1/page/summary/{Uri.EscapeDataString(term)}` | підказка - означення слова | 24 год |
| MyMemory | `https://api.mymemory.translated.net/`| `get?q={Uri.EscapeDataString(term)}&langpair={fromLang}\|{toLang}` | підказка - переклад слова | 24 год |

## Fallback-стратегії
- Wikipedia / MyMemory недоступний → генеруємо підказку в Gemini
- Gemini недоступний → відображаємо загальну підказку "Підказка недоступна, спробуйте пізніше"

- Gemini недоступний для генерації сету → виводиться помилка про недоступність такої опції із закликом спробувати пізніше 

## Власний Web API
- Controller: `SetsApiController`
- Swagger: `/swagger`
- Тест: `GET /api/sets`

## Як запустити
1. Скопіювати `appsettings.json` -> `appsettings.Development.json`
2. Додати API ключі
3. Зробити `Update-Database`
4. `dotnet run`