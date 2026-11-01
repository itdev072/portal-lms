# Portal — Learning Management System

A role-based LMS (Admin / Teacher / Student) with a public course marketplace, built as
an **ASP.NET Core 10 Web API** backend and a **React + Tailwind CSS** single-page
front end, served as one container / one deploy.

Live at: https://portal-lms.onrender.com

## Quick start (local development)

Two terminals:

```bash
# Terminal 1 — API (http://localhost:5000)
cd src/Portal.Api
dotnet run

# Terminal 2 — React dev server (http://localhost:5173, proxies /api to the API)
cd client
npm install
npm run dev
```

Open `http://localhost:5173`. On first run the API seeds demo data automatically.

### Demo accounts (all use password `Passw0rd!`)

| Role    | Email                        | What you can try |
|---------|-------------------------------|-------------------|
| Student | student@portal.local          | Browse courses, enroll, dashboard, progress, lessons, quizzes, coding challenges |
| Teacher | teacher@portal.local          | My classes, roster, manual + QR attendance |
| Admin   | admin@portal.local             | Overview analytics, Courses/Classes/Shifts/Locations/Users CRUD |

New registrations (`/register`) are always created as **Student** — Teacher and Admin
accounts are provisioned by an Admin, matching the product spec.

## Building for production

The `Dockerfile` at the repo root builds both halves into one image: it builds the
React app first (`npm run build`), then builds the API with the React build copied
into its `wwwroot/`, so the API serves the SPA itself — one container, one Render
service, no CORS configuration needed. This is exactly what Render builds on every
push to `main`.

## Architecture

```
src/Portal.Api/
  Controllers/    REST API controllers (JSON) — Auth, Courses, Classes, Shifts,
                   Locations, Users, Attendance, Dashboard, Student
  Data/            JsonCollection<T>, AppData, SeedData — file-backed persistence
  Models/Domain/    ApplicationUser, Level (Course), ClassSection, Shift, Attendance,
                     Exam, Progress, etc.
  Models/Dto/       API request/response shapes
  Services/         GeoFenceService, AttendanceService, GradingService
  Program.cs        Cookie auth (401/403 for an API, not redirects), static SPA hosting

client/
  src/
    pages/
      Landing, CourseDetail, Login, Register        (public)
      admin/     Overview, Courses, Classes, Shifts, Locations, Users (CRUD)
      teacher/   Overview, Classes, Class attendance (manual + QR)
      student/   Overview (progress/deadlines/schedule), Course learning interface
    components/   DashboardLayout (role sidebar), reusable UI primitives
    context/      AuthContext (cookie session, same-origin fetch)
```

## Why there's no database (and how to add one later)

The sandbox this was originally built in blocks NuGet (`api.nuget.org`), which rules
out EF Core, SQLite, and ASP.NET Core Identity's UI package — anything that isn't
part of the ASP.NET Core **shared framework**. So this deliberately uses only what
ships in the shared framework plus the .NET base class library:

- **Persistence**: a small JSON-file-backed store (`Data/JsonCollection.cs` +
  `Data/AppData.cs`) — one `App_Data/*.json` file per entity collection, thread-safe,
  `System.Text.Json` only. Behind a plain interface (`Find`, `Where`, `Upsert`,
  `Delete`, `All`), so swapping in EF Core + Postgres later means rewriting
  `AppData`'s internals, not touching any controller.
- **Auth**: ASP.NET Core's cookie authentication + `PasswordHasher<T>` — both part of
  the shared framework.

**Known limitation on Render's free tier**: the filesystem isn't persistent across
restarts/redeploys, so data added after a deploy (new users, enrollments, grades,
attendance) resets on the next deploy. Render's free PostgreSQL is the natural next
step — swap `AppData` for an EF Core–backed implementation once real, persistent
multi-user data matters more than "it just runs with zero setup."

## What's implemented (Phase 1)

- **Public landing page** — hero, searchable/filterable course catalog, course detail
  page with lessons list and a one-click Enroll.
- **Auth & roles** — cookie-based login/register, role-gated dashboards.
- **Admin** — overview analytics (students/teachers/classes/courses/revenue +
  attendance trend and enrollment-by-course charts), full CRUD for Courses, Classes,
  Shifts, Campus Locations, and Users.
- **Teacher** — assigned classes, student roster, **dual attendance**: manual
  present/absent/late/excused marking, or opening a QR check-in window (geofenced —
  see `GeoFenceService`, haversine distance against a saved campus location radius).
- **Student** — personalized dashboard (XP, course progress bars, upcoming exam
  deadlines, weekly schedule), and a learning interface per course (lesson content,
  multiple-choice/true-false quizzes, "predict the output" coding challenges, all
  auto-graded).

## Deferred to Phase 2 (flagged, not silently skipped)

- **Google Maps integration** for picking/visualizing campus locations (needs a
  Google Maps API key, which has to come from whoever owns the Google Cloud billing
  account — coordinates can be entered directly as numbers for now, under Admin →
  Locations).
- **Real scannable QR codes** — the teacher's QR check-in currently renders a
  stylized placeholder grid (the token/session logic behind it is fully real); swap
  in a QR-rendering library once dependencies are unblocked for the target
  environment.
- A swap from the JSON file store to a real database before any production,
  always-on, multi-teacher rollout.
- Midterm exam creation UI for teachers (the API/grading already supports it,
  carried over from the original build — just needs a React screen).
