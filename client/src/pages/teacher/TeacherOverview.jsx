import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { api } from "../../lib/api";
import { PageHeader, StatCard, Card, EmptyState, Spinner } from "../../components/ui/Primitives";

export default function TeacherOverview() {
  const [data, setData] = useState(null);

  useEffect(() => {
    api.get("/api/teacher/dashboard").then(setData);
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
      <PageHeader title="Teacher dashboard" subtitle="Your classes and today's schedule." />

      <div className="grid grid-cols-2 gap-4 lg:grid-cols-3">
        <StatCard icon="🏫" label="My classes" value={data.classCount} />
        <StatCard icon="🎓" label="Students" value={data.studentCount} />
        <StatCard icon="📅" label="Classes today" value={data.todaysClasses.length} accent="text-emerald-300" />
      </div>

      <Card className="mt-6">
        <h3 className="mb-4 font-display text-sm font-semibold text-white">Today's schedule</h3>
        {data.todaysClasses.length === 0 ? (
          <EmptyState icon="📅" title="No classes today" subtitle="Enjoy the break." />
        ) : (
          <div className="space-y-2">
            {data.todaysClasses.map((c) => (
              <Link key={c.id} to={`/teacher/classes/${c.id}`} className="flex items-center justify-between rounded-xl border border-surface-border p-4 transition hover:bg-white/[0.03]">
                <div>
                  <div className="font-medium text-white">{c.name}</div>
                  <div className="text-xs text-slate-500">{c.location}</div>
                </div>
                <span className="badge bg-brand-500/15 text-brand-300">{c.shift}</span>
              </Link>
            ))}
          </div>
        )}
      </Card>

      <Card className="mt-6">
        <h3 className="mb-4 font-display text-sm font-semibold text-white">All my classes</h3>
        <div className="space-y-2">
          {data.classes.map((c) => (
            <Link key={c.id} to={`/teacher/classes/${c.id}`} className="flex items-center justify-between rounded-xl border border-surface-border p-4 transition hover:bg-white/[0.03]">
              <div className="font-medium text-white">{c.name}</div>
              <div className="text-sm text-slate-400">{c.course}</div>
            </Link>
          ))}
        </div>
      </Card>
    </div>
  );
}
