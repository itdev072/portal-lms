import { useEffect, useState } from "react";
import { api } from "../../lib/api";
import { PageHeader, StatCard, Card, Spinner } from "../../components/ui/Primitives";
import { AreaChart, Area, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid, BarChart, Bar } from "recharts";

export default function AdminOverview() {
  const [stats, setStats] = useState(null);

  useEffect(() => {
    api.get("/api/admin/dashboard").then(setStats);
  }, []);

  if (!stats) {
    return (
      <div className="flex justify-center py-20 text-brand-400">
        <Spinner className="h-8 w-8" />
      </div>
    );
  }

  const trend = stats.attendanceTrend.map((d) => ({ ...d, label: d.date.slice(5) }));

  return (
    <div>
      <PageHeader title="Admin overview" subtitle="Platform-wide activity and enrollment." />

      <div className="grid grid-cols-2 gap-4 lg:grid-cols-5">
        <StatCard icon="🎓" label="Students" value={stats.studentCount} />
        <StatCard icon="🧑‍🏫" label="Teachers" value={stats.teacherCount} />
        <StatCard icon="🏫" label="Classes" value={stats.classCount} />
        <StatCard icon="📚" label="Courses" value={stats.courseCount} />
        <StatCard icon="💵" label="Revenue" value={`$${stats.revenue}`} accent="text-emerald-300" />
      </div>

      <div className="mt-6 grid grid-cols-1 gap-6 lg:grid-cols-2">
        <Card>
          <h3 className="mb-4 font-display text-sm font-semibold text-white">Attendance, last 7 days</h3>
          <ResponsiveContainer width="100%" height={220}>
            <AreaChart data={trend}>
              <defs>
                <linearGradient id="present" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="#6366f1" stopOpacity={0.4} />
                  <stop offset="95%" stopColor="#6366f1" stopOpacity={0} />
                </linearGradient>
              </defs>
              <CartesianGrid strokeDasharray="3 3" stroke="#232a3d" vertical={false} />
              <XAxis dataKey="label" stroke="#64748b" fontSize={12} tickLine={false} axisLine={false} />
              <YAxis stroke="#64748b" fontSize={12} tickLine={false} axisLine={false} allowDecimals={false} />
              <Tooltip contentStyle={{ background: "#141927", border: "1px solid #232a3d", borderRadius: 12, fontSize: 13 }} />
              <Area type="monotone" dataKey="present" stroke="#818cf8" fill="url(#present)" strokeWidth={2} name="Present" />
            </AreaChart>
          </ResponsiveContainer>
        </Card>

        <Card>
          <h3 className="mb-4 font-display text-sm font-semibold text-white">Enrollments by course</h3>
          <ResponsiveContainer width="100%" height={220}>
            <BarChart data={stats.enrollmentsByCourse}>
              <CartesianGrid strokeDasharray="3 3" stroke="#232a3d" vertical={false} />
              <XAxis dataKey="nameEn" stroke="#64748b" fontSize={11} tickLine={false} axisLine={false} interval={0} angle={-10} textAnchor="end" height={50} />
              <YAxis stroke="#64748b" fontSize={12} tickLine={false} axisLine={false} allowDecimals={false} />
              <Tooltip contentStyle={{ background: "#141927", border: "1px solid #232a3d", borderRadius: 12, fontSize: 13 }} />
              <Bar dataKey="enrolled" fill="#8b5cf6" radius={[6, 6, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </Card>
      </div>
    </div>
  );
}
