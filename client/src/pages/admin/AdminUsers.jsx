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
        await api.post("/api/admin/users", { displayName: form.displayName, email: form.email, password: form.password, role: form.role });
      } else {
        await api.put(`/api/admin/users/${form.id}`, { displayName: form.displayName, role: form.role, isActive: form.isActive });
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
            <td className="px-4 py-3 font-medium text-white">{u.displayName}</td>
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

function UserForm({ initial, onSave, error }) {
  const [form, setForm] = useState({ isActive: true, ...initial });
  const set = (k) => (e) => setForm((f) => ({ ...f, [k]: e.target.type === "checkbox" ? e.target.checked : e.target.value }));

  return (
    <form onSubmit={(e) => { e.preventDefault(); onSave(form); }} className="space-y-4">
      {error && <Alert>{error}</Alert>}
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
