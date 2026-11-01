import { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import { api } from "../../lib/api";
import { PageHeader, Card, Badge, Spinner } from "../../components/ui/Primitives";

const STATUS_OPTIONS = ["Present", "Absent", "Late", "Excused"];
const STATUS_TONE = { Present: "green", Late: "amber", Absent: "red", Excused: "slate" };

export default function TeacherClassDetail() {
  const { id } = useParams();
  const [roster, setRoster] = useState(null);
  const [session, setSession] = useState(null);
  const [busy, setBusy] = useState("");

  const load = () => api.get(`/api/teacher/classes/${id}/attendance`).then(setRoster);
  useEffect(() => { load(); }, [id]);

  const mark = async (studentId, status) => {
    setBusy(studentId);
    try {
      await api.post(`/api/teacher/classes/${id}/attendance`, { studentId, status, date: null });
      await load();
    } finally {
      setBusy("");
    }
  };

  const openQr = async () => {
    const s = await api.post("/api/teacher/attendance/sessions", { classSectionId: id, validMinutes: 10 });
    setSession(s);
  };

  if (!roster) {
    return (
      <div className="flex justify-center py-20 text-brand-400">
        <Spinner className="h-8 w-8" />
      </div>
    );
  }

  return (
    <div>
      <Link to="/teacher/classes" className="text-sm text-slate-400 hover:text-white">← Back to classes</Link>
      <PageHeader
        title="Attendance — today"
        subtitle="Mark manually, or open a QR check-in window for students to scan."
        action={
          <button onClick={openQr} className="btn-primary">
            📱 Open QR check-in
          </button>
        }
      />

      {session && (
        <Card className="mb-6 flex flex-wrap items-center justify-between gap-4 border-brand-500/30 bg-brand-500/5">
          <div>
            <div className="text-sm font-medium text-brand-300">QR session open</div>
            <div className="text-xs text-slate-400">
              Token <code className="text-slate-300">{session.token}</code> · expires {new Date(session.expiresAt).toLocaleTimeString()}
            </div>
            <div className="mt-1 text-xs text-slate-500">
              Students scan this from their Portal account at: {session.checkInPath}
            </div>
          </div>
          <div className="rounded-xl bg-white p-3">
            <QrPlaceholder value={session.token} />
          </div>
        </Card>
      )}

      <Card className="p-0">
        <div className="divide-y divide-surface-border">
          {roster.map((r) => (
            <div key={r.studentId} className="flex flex-wrap items-center justify-between gap-3 p-4">
              <div className="flex items-center gap-3">
                <div className="flex h-9 w-9 items-center justify-center rounded-full bg-brand-500/15 text-sm font-semibold text-brand-300">
                  {r.studentName[0]?.toUpperCase()}
                </div>
                <div>
                  <div className="font-medium text-white">{r.studentName}</div>
                  {r.status && (
                    <div className="text-xs text-slate-500">
                      {r.source === "Qr" ? "Checked in via QR" : "Marked manually"}
                    </div>
                  )}
                </div>
              </div>
              <div className="flex items-center gap-2">
                {r.status && <Badge tone={STATUS_TONE[r.status] || "slate"}>{r.status}</Badge>}
                <div className="flex gap-1">
                  {STATUS_OPTIONS.map((s) => (
                    <button
                      key={s}
                      disabled={busy === r.studentId}
                      onClick={() => mark(r.studentId, s)}
                      className={`rounded-lg px-2.5 py-1.5 text-xs font-medium transition ${
                        r.status === s ? "bg-brand-500 text-white" : "bg-white/5 text-slate-400 hover:bg-white/10"
                      }`}
                    >
                      {s}
                    </button>
                  ))}
                </div>
              </div>
            </div>
          ))}
        </div>
      </Card>
    </div>
  );
}

// A lightweight, dependency-free QR placeholder rendered as a stylized grid — swapped
// for a real scannable code once a QR rendering library is wired in (phase 2).
function QrPlaceholder({ value }) {
  const cells = Array.from(value).slice(0, 64);
  return (
    <div className="grid grid-cols-8 gap-0.5" title={value}>
      {cells.map((ch, i) => (
        <div key={i} className={`h-2.5 w-2.5 ${ch.charCodeAt(0) % 2 === 0 ? "bg-black" : "bg-white"}`} />
      ))}
    </div>
  );
}
