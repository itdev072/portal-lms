import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { api } from "../../lib/api";
import { PageHeader, StatCard, Card, EmptyState, Spinner, Badge } from "../../components/ui/Primitives";

const TABS = [
  { key: "InProgress", label: "In Progress" },
  { key: "Completed", label: "Completed" },
];

export default function StudentOverview() {
  const [data, setData] = useState(null);
  const [tab, setTab] = useState("InProgress");

  useEffect(() => {
    api.get("/api/student/dashboard").then(setData);
  }, []);

  if (!data) {
    return (
      <div className="flex justify-center py-20 text-brand-400">
        <Spinner className="h-8 w-8" />
      </div>
    );
  }

  const filteredCourses = data.courseProgress.filter((c) => c.status === tab);
  const nextDeadline = data.upcomingExams[0];

  return (
    <div>
      <PageHeader title="My Learning" subtitle="Your courses, progress, and what's next." />

      {/* Priority strip: next scheduled session + nearest deadline, always up top */}
      <div className="mb-6 grid grid-cols-1 gap-4 sm:grid-cols-2">
        {data.nextShift ? (
          <Card className="border-brand-500/30 bg-brand-500/5">
            <div className="flex items-start justify-between">
              <div>
                <div className="text-xs font-medium uppercase tracking-wide text-brand-300">Next class</div>
                <div className="mt-1 font-display text-lg font-semibold text-white">{data.nextShift.className}</div>
                <div className="mt-1 text-sm text-slate-400">{data.nextShift.shiftName} · {formatWhen(data.nextShift.at)}</div>
              </div>
              <span className="text-2xl">📅</span>
            </div>
          </Card>
        ) : (
          <Card>
            <EmptyState icon="📅" title="No classes scheduled" subtitle="Enroll in a course to get a schedule." />
          </Card>
        )}

        {nextDeadline ? (
          <Card className="border-rose-500/30 bg-rose-500/5">
            <div className="flex items-start justify-between">
              <div>
                <div className="text-xs font-medium uppercase tracking-wide text-rose-300">Next deadline</div>
                <div className="mt-1 font-display text-lg font-semibold text-white">{nextDeadline.title}</div>
                <div className="mt-1 text-sm text-slate-400">Due {new Date(nextDeadline.closesAt).toLocaleString()}</div>
              </div>
              <span className="text-2xl">⏰</span>
            </div>
          </Card>
        ) : (
          <Card>
            <EmptyState icon="🎉" title="Nothing due" subtitle="No exams scheduled right now." />
          </Card>
        )}
      </div>

      <div className="grid grid-cols-2 gap-4 lg:grid-cols-3">
        <StatCard icon="⚡" label="Total XP" value={data.totalXp} accent="text-amber-300" />
        <StatCard icon="📚" label="Courses enrolled" value={data.courseProgress.length} />
        <StatCard icon="📝" label="Upcoming exams" value={data.upcomingExams.length} accent="text-rose-300" />
      </div>

      <div className="mt-8 mb-4 flex items-center justify-between">
        <h2 className="font-display text-lg font-semibold text-white">My courses</h2>
        <div className="flex gap-2">
          {TABS.map((t) => (
            <button
              key={t.key}
              onClick={() => setTab(t.key)}
              className={`rounded-full px-4 py-1.5 text-sm font-medium transition ${tab === t.key ? "bg-brand-500 text-white" : "bg-white/5 text-slate-300 hover:bg-white/10"}`}
            >
              {t.label} {t.key === "InProgress" ? `(${data.courseProgress.filter((c) => c.status === "InProgress").length})` : `(${data.courseProgress.filter((c) => c.status === "Completed").length})`}
            </button>
          ))}
        </div>
      </div>

      {filteredCourses.length === 0 ? (
        <EmptyState
          icon="📚"
          title={tab === "InProgress" ? "No courses in progress" : "No completed courses yet"}
          subtitle={tab === "InProgress" ? "Enroll in a course from the catalog to get started." : "Finish all the lessons in a course to see it here."}
        />
      ) : (
        <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3">
          {filteredCourses.map((c) => (
            <Link key={c.courseId} to={`/student/courses/${c.courseId}`} className="group block">
              <Card className="h-full transition hover:-translate-y-0.5 hover:border-brand-400/40 hover:shadow-glow">
                <div
                  className="flex h-28 items-center justify-center rounded-xl text-5xl"
                  style={{ background: `linear-gradient(135deg, ${c.accentColor}33, ${c.accentColor}11)` }}
                >
                  {c.emoji}
                </div>
                <div className="mt-4">
                  <div className="flex items-center justify-between">
                    <h3 className="font-display text-sm font-semibold text-white">{c.courseName}</h3>
                    {c.status === "Completed" && <Badge tone="green">✓ Done</Badge>}
                  </div>
                  <div className="mt-2 flex items-center justify-between text-xs text-slate-400">
                    <span>{c.completed}/{c.lessonCount} lessons</span>
                    <span className="font-semibold text-brand-300">{c.percent}%</span>
                  </div>
                  <div className="mt-2 h-2 overflow-hidden rounded-full bg-white/5">
                    <div className="h-full rounded-full bg-gradient-to-r from-brand-500 to-fuchsia-500 transition-all" style={{ width: `${c.percent}%` }} />
                  </div>
                </div>
              </Card>
            </Link>
          ))}
        </div>
      )}

      <Card className="mt-8">
        <h3 className="mb-4 font-display text-sm font-semibold text-white">My schedule</h3>
        {data.schedule.length === 0 ? (
          <EmptyState icon="🗓️" title="No classes scheduled" />
        ) : (
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            {data.schedule.map((s) => (
              <div key={s.id} className="rounded-xl border border-surface-border p-4">
                <div className="font-medium text-white">{s.name}</div>
                <div className="mt-1 text-xs text-slate-400">{s.shift} · {s.days?.map((d) => d.slice(0, 3)).join(", ")}</div>
              </div>
            ))}
          </div>
        )}
      </Card>
    </div>
  );
}

function formatWhen(iso) {
  const d = new Date(iso);
  const now = new Date();
  const isToday = d.toDateString() === now.toDateString();
  const tomorrow = new Date(now); tomorrow.setDate(now.getDate() + 1);
  const isTomorrow = d.toDateString() === tomorrow.toDateString();
  const time = d.toLocaleTimeString([], { hour: "numeric", minute: "2-digit" });
  if (isToday) return `Today, ${time}`;
  if (isTomorrow) return `Tomorrow, ${time}`;
  return `${d.toLocaleDateString([], { weekday: "short", month: "short", day: "numeric" })}, ${time}`;
}
