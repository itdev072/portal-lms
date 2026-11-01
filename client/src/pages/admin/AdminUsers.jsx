import { useEffect, useState } from "react";
import { api } from "../../lib/api";
import { PageHeader, Table, Badge, Modal, Alert } from "../../components/ui/Primitives";

const ROLES = ["Student", "Teacher", "Admin"];

export default function AdminUsers() {
  const [users, setUsers] = useState([]);
  const [filter, setFilter] = useState("");
  const [modal, setModal] = useState(null);
  const [error, setError] = useState("");

  const load = () => api.get("/api/admin/users").then(setUsers);
  useEffect(() => { load(); }, []);

  const filtered = filter ? users.filter((u) => u.role === filter) : users;

  const save = async (form) => {
    setError("");
    try {
      if (form._isNew) {
        await api.post("/api/admin/users", { displayName: form.displayName, email: form.email, password: form.password, role: form.role, profilePictureDataUrl: form.profilePictureDataUrl || null });
      } else {
        await api.put(`/api/admin/users/${form.id}`, { displayName: form.displayName, role: form.role, isActive: form.isActive, profilePictureDataUrl: form.profilePictureDataUrl ?? null });
      }
      setModal(null);
      load();
    } catch (err) { setError(err.message); }
  };

  const deactivate = async (id) => {
    if (!confirm("Deactivate this user?")) return;
    await api.del(`/api/admin/users/${id}`);
    load();
  };

  return (
    <div>
      <PageHeader
        title="Users"
        subtitle="Manage teachers, students, and admins."
        action={<button className="btn-primary" onClick={() => setModal({ displayName: "", email: "", password: "", role: "Teacher", _isNew: true })}>+ New user</button>}
      />

      <div className="mb-4 flex gap-2">
        {["", ...ROLES].map((r) => (
          <button
            key={r || "all"}
            onClick={() => setFilter(r)}
            className={`rounded-full px-4 py-1.5 text-sm font-medium transition ${filter === r ? "bg-brand-500 text-white" : "bg-white/5 text-slate-300 hover:bg-white/10"}`}
          >
            {r || "All"}
          </button>
        ))}
      </div>

      <Table
        columns={["Name", "Email", "Role", "Status", ""]}
        rows={filtered}
        renderRow={(u) => (
          <>
            <td className="px-4 py-3">
              <div className="flex items-center gap-3">
                <Avatar name={u.displayName} src={u.profilePictureDataUrl} />
                <span className="font-medium text-white">{u.displayName}</span>
              </div>
            </td>
            <td className="px-4 py-3 text-slate-300">{u.email}</td>
            <td className="px-4 py-3"><Badge tone="brand">{u.role}</Badge></td>
            <td className="px-4 py-3"><Badge tone={u.isActive ? "green" : "red"}>{u.isActive ? "Active" : "Inactive"}</Badge></td>
            <td className="px-4 py-3 text-right">
              <button onClick={() => setModal({ ...u })} className="btn-ghost mr-2 px-3 py-1.5 text-xs">Edit</button>
              {u.isActive && <button onClick={() => deactivate(u.id)} className="btn-danger px-3 py-1.5 text-xs">Deactivate</button>}
            </td>
          </>
        )}
      />

      <Modal open={!!modal} onClose={() => setModal(null)} title={modal?._isNew ? "New user" : "Edit user"}>
        {modal && <UserForm initial={modal} onSave={save} error={error} />}
      </Modal>
    </div>
  );
}

export function Avatar({ name, src, size = 36 }) {
  if (src) {
    return <img src={src} alt={name} className="shrink-0 rounded-full object-cover" style={{ width: size, height: size }} />;
  }
  return (
    <div
      className="flex shrink-0 items-center justify-center rounded-full bg-brand-500/20 text-sm font-semibold text-brand-300"
      style={{ width: size, height: size }}
    >
      {name?.[0]?.toUpperCase() || "?"}
    </div>
  );
}

function fileToDataUrl(file) {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => resolve(reader.result);
    reader.onerror = reject;
    reader.readAsDataURL(file);
  });
}

function UserForm({ initial, onSave, error }) {
  const [form, setForm] = useState({ isActive: true, ...initial });
  const set = (k) => (e) => setForm((f) => ({ ...f, [k]: e.target.type === "checkbox" ? e.target.checked : e.target.value }));

  const onPhoto = async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;
    if (file.size > 1_500_000) {
      alert("Please choose an image under 1.5MB — profile pictures are stored inline.");
      return;
    }
    const dataUrl = await fileToDataUrl(file);
    setForm((f) => ({ ...f, profilePictureDataUrl: dataUrl }));
  };

  return (
    <form onSubmit={(e) => { e.preventDefault(); onSave(form); }} className="space-y-4">
      {error && <Alert>{error}</Alert>}

      <div className="flex items-center gap-4">
        <Avatar name={form.displayName} src={form.profilePictureDataUrl} size={56} />
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
        <label className="label">Full name</label>
        <input className="input" value={form.displayName} onChange={set("displayName")} required />
      </div>
      <div>
        <label className="label">Email</label>
        <input className="input" type="email" value={form.email} onChange={set("email")} required disabled={!form._isNew} />
      </div>
      {form._isNew && (
        <div>
          <label className="label">Temporary password</label>
          <input className="input" type="password" minLength={8} value={form.password} onChange={set("password")} required />
        </div>
      )}
      <div>
        <label className="label">Role</label>
        <select className="input" value={form.role} onChange={set("role")}>
          {ROLES.map((r) => <option key={r} value={r}>{r}</option>)}
        </select>
      </div>
      {!form._isNew && (
        <label className="flex items-center gap-2 text-sm text-slate-300">
          <input type="checkbox" checked={form.isActive} onChange={set("isActive")} className="h-4 w-4 rounded border-surface-border bg-black/20" />
          Active
        </label>
      )}
      <button className="btn-primary w-full">Save user</button>
    </form>
  );
}
