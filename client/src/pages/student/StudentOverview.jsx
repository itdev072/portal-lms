import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { api } from "../../lib/api";
import { PageHeader, StatCard, Card, EmptyState, Spinner } from "../../components/ui/Primitives";

export default function StudentOverview() {
  const [data, setData] = useState(null);

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

  return (
    <div>
      <PageHeader title="My dashboard" subtitle="Your progress, schedule, and what's next." />

      <div className="grid grid-cols-2 gap-4 lg:grid-cols-3">
        <StatCard icon="⚡" label="Total XP" value={data.totalXp} accent="text-amber-300" />
        <StatCard icon="📚" label="Courses enrolled" value={data.courseProgress.length} />
        <StatCard icon="📝" label="Upcoming exams" value={data.upcomingExams.length} accent="text-rose-300" />
      </div>

      <div className="mt-6 grid grid-cols-1 gap-6 lg:grid-cols-2">
        <Card>
          <h3 className="mb-4 font-display text-sm font-semibold text-white">Course progress</h3>
          {data.courseProgress.length === 0 ? (
            <EmptyState icon="📚" title="No courses yet" subtitle="Enroll in a course from the catalog to get started." />
          ) : (
            <div className="space-y-4">
              {data.courseProgress.map((c) => (
                <Link key={c.courseId} to={`/student/courses/${c.courseId}`} className="block">
                  <div className="mb-1.5 flex items-center justify-between text-sm">
                    <span className="font-medium text-white">{c.emoji} {c.courseName}</span>
                    <span className="text-slate-400">{c.completed}/{c.lessonCount} lessons</span>
                  </div>
                  <div className="h-2 overflow-hidden rounded-full bg-white/5">
                    <div className="h-full rounded-full bg-gradient-to-r from-brand-500 to-fuchsia-500 transition-all" style={{ width: `${c.percent}%` }} />
                  </div>
                </Link>
              ))}
            </div>
          )}
        </Card>

        <Card>
          <h3 className="mb-4 font-display text-sm font-semibold text-white">Upcoming deadlines</h3>
          {data.upcomingExams.length === 0 ? (
            <EmptyState icon="🎉" title="Nothing due" subtitle="No exams scheduled right now." />
          ) : (
            <div className="space-y-2">
              {data.upcomingExams.map((e) => (
                <div key={e.id} className="flex items-center justify-between rounded-xl border border-surface-border p-3">
                  <div className="text-sm font-medium text-white">{e.title}</div>
                  <div className="text-xs text-rose-300">Due {new Date(e.closesAt).toLocaleDateString()}</div>
                </div>
              ))}
            </div>
          )}
        </Card>
      </div>

      <Card className="mt-6">
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
