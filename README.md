# WiseUpDude

An intelligent learning and quiz platform that allows users to create and take quizzes from various content sources.

* **Purpose:** WiseUpDude is an “intelligent learning and quiz platform” that lets people create and take quizzes from a variety of content sources.
* **Authentication:** The app isn’t messing around on the security front. It uses ASP.NET Identity with Entity Framework Core for user management and supports external login via Google and Facebook. Roles are seeded for Admins, Free/Paid/Enterprise subscribers, and cookies are configured for persistent logins.
* **Architecture:** It’s a multi‑project .NET solution. There’s a server project (WiseUpDude), a WebAssembly client (WiseUpDude.Client), data and model libraries, services, and unit tests. The server uses Razor Components with interactive server and WebAssembly render modes.
* **Tech Stack:** ASP.NET Core with EF Core (SQL Server) and Identity for auth, Serilog and Application Insights for logging, Azure Functions for background tasks (resource creation), and a cache layer. The client is built with Blazor WebAssembly and uses dependency injection.
* **AI Integration:** This isn’t just another CRUD app; it plugs into multiple AI services. It creates chat clients for Azure OpenAI, GitHub’s GPT‑4o (via Azure), Perplexity and Gemini APIs. These services are registered in the DI container, so the app can generate quiz questions or learning resources using different LLMs.
* **Quizzes & Topics:** Quizzes have a `Type` (“Topic” or “Prompt”), optional `Prompt`, optional `TopicId`, `Description`, and a `Difficulty` level. Each quiz question stores the question text, options (for multiple choice), correct answer, explanation, difficulty, and even context snippets/citations for “learn mode”.
* **Learn Mode:** A unique “learn mode” flag is stored with each user quiz. When enabled, the app can present extra context snippets and citations for answers and track whether the user is in practice or exam mode.
* **Data Access & Services:** The `QuizRepository` and `UserQuizRepository` handle CRUD for quizzes, map between EF entities and DTOs, and support asynchronous operations. They include helpers to get quizzes by topic, recent user quizzes, update quiz names and learn mode, and compute scores by comparing user answers to correct answers.
* **External Auth Setup:** The README provides step‑by‑step instructions for setting up Facebook and Google OAuth, including how to configure redirect URIs and store secrets in user‑secrets or environment variables.

In short, **WiseUpDude** is a robust, AI‑enhanced quiz platform built in .NET. It supports secure user management, dynamic quiz creation (by topic or custom prompt), social logins, and a learn mode with citations. You’ve got all the pieces wired up for a full‑stack, AI‑powered learning site — not too shabby for a “retired” developer!
