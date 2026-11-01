import { useEffect, useState } from "react";
import { api } from "../../lib/api";
import { PageHeader, Card, Alert, Spinner } from "../../components/ui/Primitives";

export default function AdminSettings() {
  const [form, setForm] = useState(null);
  const [error, setError] = useState("");
  const [saved, setSaved] = useState(false);
  const [busy, setBusy] = useState(false);

  useEffect(() => {
    api.get("/api/admin/settings").then(setForm);
  }, []);

  const set = (k) => (e) => setForm((f) => ({ ...f, [k]: e.target.type === "checkbox" ? e.target.checked : e.target.value }));

  const save = async (e) => {
    e.preventDefault();
    setError("");
    setSaved(false);
    setBusy(true);
    try {
      const payload = { ...form, defaultAttendanceRadiusMeters: Number(form.defaultAttendanceRadiusMeters) };
      const updated = await api.put("/api/admin/settings", payload);
      setForm(updated);
      setSaved(true);
    } catch (err) {
      setError(err.message);
    } finally {
      setBusy(false);
    }
  };

  if (!form) {
    return (
      <div className="flex justify-center py-20 text-brand-400">
        <Spinner className="h-8 w-8" />
      </div>
    );
  }

  return (
    <div>
      <PageHeader title="Settings" subtitle="Platform-wide configuration." />
      <Card className="max-w-xl">
        <form onSubmit={save} className="space-y-4">
          {error && <Alert>{error}</Alert>}
          {saved && <Alert tone="green">Settings saved.</Alert>}

          <div>
            <label className="label">Site name</label>
            <input className="input" value={form.siteName} onChange={set("siteName")} required />
          </div>
          <div>
            <label className="label">Support email</label>
            <input className="input" type="email" value={form.supportEmail} onChange={set("supportEmail")} required />
          </div>
          <div>
            <label className="label">Default attendance radius (meters)</label>
            <input className="input" type="number" min="10" value={form.defaultAttendanceRadiusMeters} onChange={set("defaultAttendanceRadiusMeters")} />
            <p className="mt-1 text-xs text-slate-500">Used as the default when a new campus location is created.</p>
          </div>
          <label className="flex items-center gap-2 text-sm text-slate-300">
            <input type="checkbox" checked={form.allowPublicRegistration} onChange={set("allowPublicRegistration")} className="h-4 w-4 rounded border-surface-border bg-black/20" />
            Allow public self-registration (new sign-ups become Students)
          </label>

          <button className="btn-primary" disabled={busy}>{busy ? "Saving…" : "Save settings"}</button>
        </form>
      </Card>
    </div>
  );
}
