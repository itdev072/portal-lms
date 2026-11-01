import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { api } from "../lib/api";
import { useAuth } from "../context/AuthContext";
import { Spinner } from "../components/ui/Primitives";

const CATEGORIES = [
  { key: "", label: "All" },
  { key: "Programming", label: "Programming" },
  { key: "WebDevelopment", label: "Web Development" },
  { key: "DataScience", label: "Data Science" },
];

export default function Landing() {
  const { user } = useAuth();
  const [courses, setCourses] = useState([]);
  const [loading, setLoading] = useState(true);
  const [q, setQ] = useState("");
  const [category, setCategory] = useState("");

  useEffect(() => {
    setLoading(true);
    const params = new URLSearchParams();
    if (q) params.set("q", q);
    if (category) params.set("category", category);
    api
      .get(`/api/courses?${params.toString()}`)
      .then(setCourses)
      .catch(() => setCourses([]))
      .finally(() => setLoading(false));
  }, [q, category]);

  return (
    <div className="min-h-screen bg-surface">
      <TopNav user={user} />
      <Hero q={q} setQ={setQ} />

      <section className="mx-auto max-w-7xl px-4 py-14 sm:px-6 lg:px-8">
        <div className="mb-8 flex flex-wrap items-center justify-between gap-4">
          <div>
            <h2 className="font-display text-2xl font-bold text-white">Featured courses</h2>
            <p className="mt-1 text-sm text-slate-400">Hands-on, project-based, taught live by real instructors.</p>
          </div>
          <div className="flex flex-wrap gap-2">
            {CATEGORIES.map((c) => (
              <button
                key={c.key}
                onClick={() => setCategory(c.key)}
                className={`rounded-full px-4 py-1.5 text-sm font-medium transition ${
                  category === c.key ? "bg-brand-500 text-white shadow-glow" : "bg-white/5 text-slate-300 hover:bg-white/10"
                }`}
              >
                {c.label}
              </button>
            ))}
          </div>
        </div>

        {loading ? (
          <div className="flex justify-center py-20 text-brand-400">
            <Spinner className="h-8 w-8" />
          </div>
        ) : courses.length === 0 ? (
          <div className="rounded-2xl border border-dashed border-surface-border py-20 text-center text-slate-500">
            No courses match your search.
          </div>
        ) : (
          <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {courses.map((c, i) => (
              <CourseCard key={c.id} course={c} delay={i * 0.05} />
            ))}
          </div>
        )}
      </section>

      <LearningPaths />
      <Footer />
    </div>
  );
}

function TopNav({ user }) {
  return (
    <header className="sticky top-0 z-40 border-b border-surface-border/80 bg-surface/80 backdrop-blur-md">
      <div className="mx-auto flex max-w-7xl items-center justify-between px-4 py-3.5 sm:px-6 lg:px-8">
        <div className="flex items-center gap-2.5">
          <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-gradient-to-br from-brand-400 to-brand-600 font-display text-lg font-bold text-white shadow-glow">
            P
          </div>
          <span className="font-display text-xl font-bold tracking-tight text-white">Portal</span>
        </div>
        <div className="flex items-center gap-3">
          {user ? (
            <Link to={`/${user.role.toLowerCase()}`} className="btn-primary">
              Go to dashboard
            </Link>
          ) : (
            <>
              <Link to="/login" className="btn-ghost">
                Log in
              </Link>
              <Link to="/register" className="btn-primary">
                Register
              </Link>
            </>
          )}
        </div>
      </div>
    </header>
  );
}

function Hero({ q, setQ }) {
  return (
    <section className="relative overflow-hidden border-b border-surface-border">
      <div className="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_at_top,_rgba(99,102,241,0.18),_transparent_60%)]" />
      <div className="relative mx-auto max-w-5xl px-4 py-24 text-center sm:px-6 lg:px-8">
        <div className="mb-5 inline-flex items-center gap-2 rounded-full border border-surface-border bg-white/5 px-4 py-1.5 text-xs font-medium text-slate-300 animate-fadeUp">
          🎓 Live classes · QR attendance · Real instructors
        </div>
        <h1 className="font-display text-4xl font-extrabold tracking-tight text-white sm:text-6xl animate-fadeUp">
          Learn skills that
          <span className="bg-gradient-to-r from-brand-400 to-fuchsia-400 bg-clip-text text-transparent"> actually ship.</span>
        </h1>
        <p className="mx-auto mt-5 max-w-2xl text-lg text-slate-400 animate-fadeUp">
          Project-based courses in programming and web development, with live classes,
          hands-on coding challenges, and progress tracking built for serious students.
        </p>
        <div className="mx-auto mt-8 flex max-w-xl items-center gap-2 rounded-2xl border border-surface-border bg-surface-card p-2 shadow-2xl animate-fadeUp">
          <svg className="ml-2 h-5 w-5 shrink-0 text-slate-500" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <circle cx="11" cy="11" r="7" />
            <path d="m21 21-4.3-4.3" strokeLinecap="round" />
          </svg>
          <input
            value={q}
            onChange={(e) => setQ(e.target.value)}
            placeholder="Search for courses — C#, web development, data..."
            className="flex-1 bg-transparent px-1 py-2 text-sm text-slate-100 placeholder-slate-500 outline-none"
          />
        </div>
        <div className="mt-8 flex flex-wrap items-center justify-center gap-x-8 gap-y-3 text-sm text-slate-500 animate-fadeUp">
          <Stat value="4" label="courses live" />
          <Stat value="6" label="students enrolled" />
          <Stat value="2" label="instructors" />
        </div>
      </div>
    </section>
  );
}

function Stat({ value, label }) {
  return (
    <div className="flex items-center gap-2">
      <span className="font-display text-lg font-bold text-white">{value}</span>
      <span>{label}</span>
    </div>
  );
}

function CourseCard({ course, delay }) {
  return (
    <Link
      to={`/courses/${course.id}`}
      style={{ animationDelay: `${delay}s` }}
      className="group card animate-fadeUp overflow-hidden p-0 transition-all hover:-translate-y-1 hover:shadow-glow"
    >
      <div
        className="flex h-32 items-center justify-center text-5xl"
        style={{ background: `linear-gradient(135deg, ${course.accentColor}33, ${course.accentColor}0d)` }}
      >
        <span className="transition-transform group-hover:scale-110">{course.thumbnailEmoji}</span>
      </div>
      <div className="p-5">
        <div className="mb-1.5 flex items-center justify-between">
          <span className="text-xs font-medium uppercase tracking-wide text-brand-300">{course.category}</span>
          <span className="text-xs text-slate-500">{course.estimatedHours}h</span>
        </div>
        <h3 className="font-display text-base font-semibold text-white group-hover:text-brand-300">{course.nameEn}</h3>
        <p className="mt-1.5 line-clamp-2 text-sm text-slate-400">{course.shortDescriptionEn}</p>
        <div className="mt-4 flex items-center justify-between">
          <div className="flex items-center gap-1 text-sm text-amber-400">
            ★ <span className="font-medium">{course.ratingAverage}</span>
            <span className="text-slate-500">({course.ratingCount})</span>
          </div>
          <span className="font-display text-sm font-bold text-white">{course.price > 0 ? `$${course.price}` : "Free"}</span>
        </div>
      </div>
    </Link>
  );
}

function LearningPaths() {
  const paths = [
    { emoji: "🧩", title: "C# Fundamentals", desc: "CSK1 → CSK2 — syntax through control flow." },
    { emoji: "🌐", title: "Web Developer Track", desc: "React components, state, and shipping UIs." },
    { emoji: "📊", title: "Data Foundations", desc: "Structures, SQL, and reading real datasets." },
  ];
  return (
    <section className="border-t border-surface-border bg-white/[0.015] py-14">
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <h2 className="font-display text-2xl font-bold text-white">Learning paths</h2>
        <p className="mt-1 text-sm text-slate-400">Structured sequences to take you from zero to job-ready.</p>
        <div className="mt-8 grid grid-cols-1 gap-5 sm:grid-cols-3">
          {paths.map((p) => (
            <div key={p.title} className="card p-5">
              <div className="text-3xl">{p.emoji}</div>
              <div className="mt-3 font-display font-semibold text-white">{p.title}</div>
              <div className="mt-1 text-sm text-slate-400">{p.desc}</div>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}

function Footer() {
  return (
    <footer className="border-t border-surface-border py-8">
      <div className="mx-auto max-w-7xl px-4 text-center text-sm text-slate-500 sm:px-6 lg:px-8">
        © {new Date().getFullYear()} Portal — Learning Management System
      </div>
    </footer>
  );
}
