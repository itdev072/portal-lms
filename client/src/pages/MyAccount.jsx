import { useEffect, useState } from "react";
import { api } from "../lib/api";
import { PageHeader, Card, Alert, Spinner } from "../components/ui/Primitives";
import { Avatar } from "./admin/AdminUsers";

function fileToDataUrl(file) {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => resolve(reader.result);
    reader.onerror = reject;
    reader.readAsDataURL(file);
  });
}

export default function MyAccount() {
  const [form, setForm] = useState(null);
  const [error, setError] = useState("");
  const [saved, setSaved] = useState(false);
  const [busy, setBusy] = useState(false);

  useEffect(() => {
    api.get("/api/account").then(setForm);
  }, []);

  if (!form) {
    return (
      <div className="flex justify-center py-20 text-brand-400">
        <Spinner className="h-8 w-8" />
      </div>
    );
  }

  const setPref = (k) => (e) =>
    setForm((f) => ({ ...f, preferences: { ...f.preferences, [k]: e.target.type === "checkbox" ? e.target.checked : e.target.value } }));

  const onPhoto = async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;
    if (file.size > 1_500_000) {
      alert("Please choose an image under 1.5MB.");
      return;
    }
    const dataUrl = await fileToDataUrl(file);
    setForm((f) => ({ ...f, profilePictureDataUrl: dataUrl }));
  };

  const save = async (e) => {
    e.preventDefault();
    setError("");
    setSaved(false);
    setBusy(true);
    try {
      const updated = await api.put("/api/account", {
        displayName: form.displayName,
        profilePictureDataUrl: form.profilePictureDataUrl ?? null,
        preferences: form.preferences,
      });
      setForm(updated);
      setSaved(true);
    } catch (err) {
      setError(err.message);
    } finally {
      setBusy(false);
    }
  };

  return (
    <div>
      <PageHeader title="My account" subtitle="Your profile picture and preferences." />
      <Card className="max-w-xl">
        <form onSubmit={save} className="space-y-5">
          {error && <Alert>{error}</Alert>}
          {saved && <Alert tone="green">Saved.</Alert>}

          <div className="flex items-center gap-4">
            <Avatar name={form.displayName} src={form.profilePictureDataUrl} size={64} />
            <div>
              <label className="btn-ghost cursor-pointer px-3 py-1.5 text-xs">
                Upload photo
                <input type="file" accept="image/*" className="hidden" onChange={onPhoto} />
              </label>
              {form.profilePictureDataUrl && (
                <button type="button" onClick={() => setForm((f) => ({ ...f, profilePictureDataUrl: "" }))} className="ml-2 text-xs text-slate-500 hover:text-rose-300">
                  Remove
                </button>
              )}
            </div>
          </div>

          <div>
            <label className="label">Display name</label>
            <input className="input" value={form.displayName} onChange={(e) => setForm((f) => ({ ...f, displayName: e.target.value }))} required />
          </div>

          <div>
            <label className="label">Email</label>
            <input className="input" value={form.email} disabled />
          </div>

          <div className="border-t border-surface-border pt-4">
            <h3 className="mb-3 font-display text-sm font-semibold text-white">Preferences</h3>

            <label className="mb-3 flex items-center gap-2 text-sm text-slate-300">
              <input type="checkbox" checked={form.preferences.emailNotifications} onChange={setPref("emailNotifications")} className="h-4 w-4 rounded border-surface-border bg-black/20" />
              Email notifications
            </label>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="label">Theme</label>
                <select className="input" value={form.preferences.theme} onChange={setPref("theme")}>
                  <option value="dark">Dark</option>
                  <option value="light">Light</option>
                </select>
              </div>
              <div>
                <label className="label">Language</label>
                <select className="input" value={form.preferences.language} onChange={setPref("language")}>
                  <option value="en">English</option>
                  <option value="km">ភាសាខ្មែរ (Khmer)</option>
                </select>
              </div>
            </div>
          </div>

          <button className="btn-primary w-full" disabled={busy}>{busy ? "Saving…" : "Save changes"}</button>
        </form>
      </Card>
    </div>
  );
}
