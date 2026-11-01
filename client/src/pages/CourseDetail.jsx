import { useEffect, useState } from "react";
import { Link, useParams, useNavigate } from "react-router-dom";
import { api } from "../lib/api";
import { useAuth } from "../context/AuthContext";
import { Spinner, Alert } from "../components/ui/Primitives";

export default function CourseDetail() {
  const { id } = useParams();
  const { user } = useAuth();
  const navigate = useNavigate();
  const [data, setData] = useState(null);
  const [enrollMsg, setEnrollMsg] = useState("");
  const [busy, setBusy] = useState(false);

  useEffect(() => {
    api.get(`/api/courses/${id}`).then(setData).catch(() => setData(null));
  }, [id]);

  if (!data) {
    return (
      <div className="flex min-h-screen items-center justify-center text-brand-400">
        <Spinner className="h-8 w-8" />
      </div>
    );
  }

  const { course, lessons } = data;

  const handleEnroll = async () => {
    if (!user) return navigate("/login", { state: { from: `/courses/${id}` } });
    setBusy(true);
    setEnrollMsg("");
    try {
      const res = await api.post(`/api/student/enroll/${id}`);
      setEnrollMsg(res.message);
    } catch (err) {
      setEnrollMsg(err.message);
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="min-h-screen bg-surface">
      <header className="border-b border-surface-border bg-surface-card/40 px-4 py-4 sm:px-6 lg:px-8">
        <Link to="/" className="text-sm text-slate-400 hover:text-white">
          ← Back to courses
        </Link>
      </header>

      <div
        className="border-b border-surface-border px-4 py-14 sm:px-6 lg:px-8"
        style={{ background: `linear-gradient(135deg, ${course.accentColor}22, transparent)` }}
      >
        <div className="mx-auto max-w-4xl">
          <div className="text-6xl">{course.thumbnailEmoji}</div>
          <h1 className="mt-4 font-display text-3xl font-bold text-white sm:text-4xl">{course.nameEn}</h1>
          <p className="mt-3 max-w-2xl text-slate-400">{course.shortDescriptionEn}</p>
          <div className="mt-5 flex flex-wrap items-center gap-4 text-sm text-slate-400">
            <span>★ {course.ratingAverage} ({course.ratingCount} ratings)</span>
            <span>·</span>
            <span>{course.enrolledCount} students enrolled</span>
            <span>·</span>
            <span>{course.estimatedHours}h</span>
            <span>·</span>
            <span>{course.lessonCount} lessons</span>
          </div>
        </div>
      </div>

      <div className="mx-auto grid max-w-4xl grid-cols-1 gap-8 px-4 py-10 sm:px-6 lg:grid-cols-[1fr_280px] lg:px-8">
        <div>
          <h2 className="font-display text-lg font-semibold text-white">Lessons</h2>
          <div className="mt-4 space-y-2">
            {lessons.map((l, i) => (
              <div key={l.id} className="card flex items-center justify-between p-4">
                <div className="flex items-center gap-3">
                  <span className="flex h-8 w-8 items-center justify-center rounded-lg bg-white/5 text-xs font-semibold text-slate-400">
                    {i + 1}
                  </span>
                  <div>
                    <div className="text-sm font-medium text-white">{l.titleEn}</div>
                    <div className="text-xs text-slate-500">{l.estimatedMinutes} min · {l.xpValue} XP</div>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>

        <div className="card h-fit p-5">
          <div className="font-display text-2xl font-bold text-white">{course.price > 0 ? `$${course.price}` : "Free"}</div>
          <button onClick={handleEnroll} disabled={busy} className="btn-primary mt-4 w-full">
            {busy ? "Enrolling…" : "Enroll now"}
          </button>
          {enrollMsg && (
            <div className="mt-3">
              <Alert tone="green">{enrollMsg}</Alert>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
