# Portal — C# Learning Management System

A full role-based web app (Student / Teacher / Admin) for the "Software Project
Development" C# course: lessons, quizzes, coding challenges, midterm exams, and
QR-code attendance that's verified against a campus location server-side.

Built with **ASP.NET Core 10 (MVC)**. The UI is a dark navy theme with a
left sidebar (collapsible on mobile) and a split-screen login/register
screen, matching a reference lecturer-portal style — all in
`wwwroot/css/site.css` and `Views/Shared/_Layout.cshtml`, layered on top of
Bootstrap 5.3's built-in dark mode (`data-bs-theme="dark"`).

## Quick start

```bash
cd src/Portal.Web
dotnet run
```

Then open the URL it prints (defaults to something like `http://localhost:5000`
or `http://localhost:5219` if you set `--urls`). On first run it seeds demo data
automatically — you'll see this in the console:

```
Seeded demo accounts (password: Passw0rd!): admin@portal.local, teacher@portal.local, student@portal.local
```

### Demo accounts (all use password `Passw0rd!`)

| Role    | Email                   | What you can try |
|---------|-------------------------|-------------------|
| Student | student@portal.local    | Dashboard, lessons, quizzes, coding challenges, midterm exam, scanning a QR to check in |
| Teacher | teacher@portal.local    | Roster, content management, creating an attendance session (QR code), creating/publishing an exam |
| Admin   | admin@portal.local      | User management, level management |

New registrations (`/Account/Register`) are always created as **Student** —
Teacher and Admin accounts are provisioned by an Admin, matching the product
spec.

## What's implemented (v1 / P0)

- **Auth & roles** — cookie-based login, registration, role-gated areas for
  Student / Teacher / Admin.
- **Leveled content** — CSK1/CSK2 seeded (mapped to the course's chapters),
  each with lessons (Khmer + English explainer, key terms, code examples),
  multiple-choice/true-false quizzes, and "predict the output" coding
  challenges. A lesson completes once its quizzes are correct and its
  challenge passes; a level completes once all its lessons do, and XP
  accumulates on the student dashboard.
- **Midterm exams** — a separate assessment type from lesson quizzes: teacher
  builds a question bank, sets a time window (opens/closes) and a time limit,
  publishes it, students get exactly one attempt, auto-graded on submit, with
  a results roster for the teacher.
- **QR + geofenced attendance** — a teacher opens an attendance session for a
  class + a configured campus location; students scan the QR (rendered
  client-side, in-browser — no third-party QR API call) which takes them to a
  check-in page that asks their phone for its GPS position. The **server**
  computes the distance to the campus location (haversine) and decides
  Present vs. Out of range — the QR code and the token inside it never mark
  anyone present by themselves. See `Services/AttendanceService.cs` and
  `Services/GeoFenceService.cs`.
- **Admin** — user creation, role changes, activate/deactivate, level CRUD.
- **Khmer script rendering** — Noto Sans Khmer is vendored locally (not
  loaded from Google Fonts) under `wwwroot/lib/fonts/noto-sans-khmer/`, so
  Khmer text shapes correctly regardless of the viewer's OS fonts or network
  restrictions.

## Why there's no database (and how to add one later)

This sandbox's network allowlist blocks NuGet (`api.nuget.org`), which rules
out EF Core, SQLite, ASP.NET Core Identity's UI package, and the QRCoder
package — anything that isn't part of the ASP.NET Core **shared framework**.
So this v1 deliberately uses only what ships in the shared framework plus the
.NET base class library:

- **Persistence**: a small JSON-file-backed store (`Data/JsonCollection.cs` +
  `Data/AppData.cs`) — one `App_Data/*.json` file per entity collection,
  thread-safe, using only `System.Text.Json`. It's behind a plain interface
  (`Find`, `Where`, `Upsert`, `Delete`, `All`), so swapping in EF Core +
  SQLite/Postgres later means rewriting `AppData`'s internals, not touching
  any controller.
- **Auth**: ASP.NET Core's cookie authentication + `PasswordHasher<T>` for
  password hashing — both part of the shared framework, no NuGet package
  needed, no `UserManager`/`IdentityDbContext`.
- **QR codes**: rendered in the browser via a vendored, self-contained bundle
  of the `qrcode` npm package (`wwwroot/lib/qrcode/qrcode.min.js`, MIT
  licensed) — the server only ever generates a token, never an image. This
  also means an attendance session's token isn't sent to any third-party QR
  service.

None of this is a limitation of ASP.NET Core itself — it's purely a
workaround for this sandbox's NuGet restriction. On a normal machine with
NuGet access, swap `AppData` for an `IDbContext`-backed implementation
whenever real concurrent usage / multiple servers / backups matter more than
"it just runs with zero setup."

## Project layout

```
src/Portal.Web/
  Controllers/        Account, Student, Teacher, Admin, Attendance, Home
  Models/Domain/       Users, content (Level/Lesson/QuizItem/CodingChallenge),
                        progress, exams, attendance
  Models/ViewModels/    Per-page view models (+ flattened create/edit forms)
  Services/             GeoFenceService, GradingService, AttendanceService
  Data/                 JsonCollection<T>, AppData, SeedData
  Views/                Razor views, organized by controller
  wwwroot/              Bootstrap (template default), vendored QR JS, vendored
                        Noto Sans Khmer font, site.css, attendance JS
  App_Data/             JSON data files (created/seeded on first run — safe
                        to delete to reset to the seed data)
```

## Not yet built (flagged, not silently skipped)

These were out of scope for "build it for now" but are natural next steps and
are called out in the product spec as P1/P2:

- Bulk content import (lessons are currently created one at a time in the
  teacher UI; fine for a pilot, slow for 28 chapters' worth of content).
- Real code execution for coding challenges (v1 grades by matching the
  student's typed output against the expected output — "predict the
  output" — rather than compiling and running their C#).
- CSK7 (Web App Development) / CSK8 (Cloud & Deployment) content — these
  levels exist in the content master prompt's design but have no seeded
  lessons yet.
- Self-service password reset, email verification.
- A swap from the JSON file store to a real database (see above) before any
  production/multi-teacher rollout.

## Testing notes

End-to-end tested via automated requests and browser screenshots: student
login → lesson → quiz grading → coding-challenge grading → XP/progress
accumulation; teacher login → attendance session creation → QR render →
student check-in from on-campus coordinates (Present) and off-campus
coordinates (Out of range, correct distance reported) → teacher sees both
records; midterm exam creation → question authoring → publish → student
takes it once → auto-graded → teacher sees the score; admin user creation →
role change → deactivate/reactivate; admin level create/edit/delete.
