# Flashcards Maker

Flashcards Maker is a full-stack learning platform for creating, organizing, and practicing digital flashcards. Users can build private or public study sets, enrich cards with generated definitions and translations, listen to pronunciations, and track their learning progress.

The application is built with **.NET 9 / ASP.NET Core Web API** and **React with Vite**.

## Features

### Authentication and accounts

- User registration, login, logout, and current-user sessions
- JWT authentication stored and read through an authentication cookie
- Google OAuth login
- Email verification and password changes
- User profile management, avatar upload, and account deletion
- Public/private profile and set visibility controls

### Flashcard authoring

- Create, edit, delete, and browse flashcard sets
- Public and private sets
- Flashcards with terms, definitions, examples, and additional learning context
- Categories and set-category management
- Batch flashcard creation
- Search, filtering, sorting, pagination, and set type selection
- AI-assisted set generation from a topic or description
- AI, Wikipedia, and translation hints while creating cards

### Practice and accessibility

- Practice sessions with answer checking and results
- Progress tracking and batch progress updates
- Reset progress for a set or a group of cards
- Practice limits and learning statistics
- Text-to-speech pronunciation through ElevenLabs
- Light and dark theme switching persisted in the browser
- Responsive interface with an on-screen keyboard for writing practice

## Technology stack

### Backend

- **C# and .NET 9**
- **ASP.NET Core Web API** with controller-based routing
- **Entity Framework Core 9** for data access and migrations
- **ASP.NET Core Identity** for users and roles
- **JWT Bearer authentication** and **Google OAuth 2.0**
- **Swagger / OpenAPI** for development API documentation
- **Repository and service layers** with a unit-of-work abstraction
- **Polly-based HTTP resilience** with retries, timeouts, circuit breakers, and jitter
- **Memory cache** for external-service results
- **Cloudinary** for image storage
- **SMTP/Gmail** for application email
- **HtmlSanitizer** for sanitizing user-provided HTML content

### Frontend

- **React 19** for the user interface
- **Vite** for development and production builds
- **React Router** for client-side routing
- **TanStack React Query** for server-state fetching, caching, and mutations
- **Axios** for API communication
- **React Hook Form** and **Zod** for form state and validation
- **@hookform/resolvers** for connecting Zod schemas to React Hook Form
- **Lucide React** for interface icons
- **React Hot Toast** for notifications
- **react-simple-keyboard** and **simple-keyboard-layouts** for writing practice
- **SCSS/Sass** for component and page styling
- **Vitest**, **Testing Library**, **jest-dom**, and **MSW** for frontend tests and API mocking
- **ESLint** for static analysis

## External integrations

| Service            | Purpose                                        |
| ------------------ | ---------------------------------------------- |
| Google Gemini      | AI-generated flashcard sets and fallback hints |
| Wikipedia REST API | Definitions and term summaries                 |
| MyMemory           | Word and phrase translations                   |
| ElevenLabs         | Text-to-speech pronunciation                   |
| Google OAuth       | Social login                                   |
| Gmail SMTP         | Verification and application email             |
| Cloudinary         | User avatar and image storage                  |

External HTTP clients use configured timeouts, retries, and circuit breakers. Dictionary and hint responses can be cached for a limited period to reduce repeated requests.

## Repository structure

```text
.
├── backend/
│   ├── Core/          # Domain models, DTOs, contexts, and migrations
│   ├── Repositories/  # Repository and unit-of-work implementations
│   ├── Services/      # Business logic and external integrations
│   ├── WebAPI/        # ASP.NET Core controllers and application startup
│   └── FlashcardsMaker.sln
└── frontend/
    ├── src/components/  # Reusable UI components
    ├── src/features/    # Feature-specific forms and workflows
    ├── src/pages/       # Route-level pages
    ├── src/services/    # API client and service functions
    ├── src/hooks/       # React Query and reusable hooks
    └── vite.config.js
```

## Requirements

- .NET SDK 9.0 or later
- Node.js 20 or later and npm
- PostgreSQL or SQL Server
- A local HTTPS development certificate trusted by ASP.NET Core
- Optional credentials for Google, Gemini, Cloudinary, Gmail SMTP, and ElevenLabs

## Configuration

The backend reads configuration from `backend/WebAPI/appsettings.json` and `appsettings.Development.json`.

Create a local development configuration and provide values for:

- `DatabaseProvider`: `PostgreSql` or `SqlServer`
- `ConnectionStrings:PostgreSqlConnection` or `ConnectionStrings:SqlServerConnection`
- `FrontendUrl`
- `Jwt:Key`, `Jwt:Issuer`, and `Jwt:Audience`
- `Authentication:Google:ClientId` and `Authentication:Google:ClientSecret`
- `Cloudinary:*`
- `EmailSettings:*`
- `Gemini:ApiKey`
- `ElevenLabs:ApiKey`

Do not commit API keys, OAuth secrets, SMTP passwords, database passwords, or production connection strings. Prefer environment variables or a local secrets manager. Any credentials that have previously been committed should be revoked and replaced.

## Running locally

### 1. Clone the repository

```bash
git clone <repository-url>
cd flashcards-maker-csharp-react
```

### 2. Configure the backend

Create or update `backend/WebAPI/appsettings.Development.json` with local, non-production values. Select the database provider in the same file.

For PostgreSQL:

```json
{
  "DatabaseProvider": "PostgreSql",
  "ConnectionStrings": {
    "PostgreSqlConnection": "Host=localhost;Port=5432;Database=flashcards;Username=postgres;Password=change-me"
  }
}
```

For SQL Server:

```json
{
  "DatabaseProvider": "SqlServer",
  "ConnectionStrings": {
    "SqlServerConnection": "Server=localhost;Database=FlashcardsMaker;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

### 3. Apply database migrations

From the `backend` directory, run the command for the selected provider:

```bash
cd backend
dotnet restore FlashcardsMaker.sln
dotnet ef database update --project Core --startup-project WebAPI --context PostgreSqlDataContext
```

For SQL Server, use:

```bash
dotnet ef database update --project Core --startup-project WebAPI --context SqlServerDataContext
```

If Entity Framework CLI is not installed, install it once with:

```bash
dotnet tool install --global dotnet-ef
```

### 4. Start the API

```bash
cd backend/WebAPI
dotnet run
```

The local API is configured for HTTPS. Swagger is available in development at:

```text
https://localhost:7273/swagger
```

The exact port is shown by `dotnet run` and may differ between launch profiles.

### 5. Start the React application

Open a second terminal:

```bash
cd frontend
npm install
npm run dev
```

Vite proxies `/api` requests to `https://localhost:7273` by default. If the backend uses another port, update `frontend/vite.config.js`.

The frontend is normally available at `http://localhost:5173`.

## Useful commands

### Backend

```bash
dotnet build backend/FlashcardsMaker.sln
dotnet run --project backend/WebAPI/WebAPI.csproj
```

### Frontend

```bash
cd frontend
npm run dev       # Start Vite development server
npm run build     # Create a production build
npm run preview   # Preview the production build locally
npm run lint      # Run ESLint
npm test          # Run Vitest in watch mode
npm test -- --run # Run the test suite once
```

The frontend test setup uses JSDOM and MSW to mock API requests. An HTML test report is generated under `frontend/html-report/` by the configured Vitest reporter.

## API overview

The API is organized around resource-based controllers under `/api`:

- `/api/auth` - registration, login, Google login, verification, logout, and password changes
- `/api/users` - profile and visibility management
- `/api/sets` - public set browsing, set details, types, and progress
- `/api/my-sets` - authenticated set creation, editing, deletion, categories, and AI generation
- `/api/sets/{setId}/flashcards` - flashcard CRUD, batch creation, and hints
- `/api/sets/{setId}/flashcards/{flashcardId}/contexts` - flashcard context CRUD
- `/api/practice` - sessions, answer checks, results, limits, and progress operations
- `/api/categories` - available categories
- `/api/tts` - text-to-speech generation
- `/api/emails` - support email submission

For the complete request and response schemas, run the backend in Development and open Swagger.

## Architecture notes

The backend separates responsibilities into four projects:

1. `Core` contains models, DTOs, database contexts, and migrations.
2. `Repositories` handles persistence and unit-of-work coordination.
3. `Services` contains application logic and external API integrations.
4. `WebAPI` exposes HTTP endpoints, authentication, Swagger, and dependency injection configuration.

The frontend uses route-level pages, reusable components, feature modules, custom hooks, and a shared Axios client. React Query is used for asynchronous server state, while local component state handles form and interaction state.

## Production checklist

- Replace every development secret and rotate credentials that may have been exposed.
- Use a managed PostgreSQL or SQL Server instance with encrypted connections.
- Set a strong, unique JWT signing key outside source control.
- Configure the production frontend URL and OAuth callback URLs.
- Configure HTTPS and trusted certificates at the reverse proxy or hosting platform.
- Restrict CORS and allowed hosts to the production domains.
- Store Cloudinary, SMTP, Gemini, and ElevenLabs credentials in deployment secrets.
- Apply migrations as part of the deployment process.
- Disable development Swagger exposure if the production API should not publish its schema.
- Build the frontend with `npm run build` and serve the generated `frontend/dist` assets from a static host or reverse proxy.

## License

No license has been specified for this repository yet. Add a license file before distributing or reusing the project publicly.
