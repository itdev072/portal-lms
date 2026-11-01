import { useEffect, useState } from "react";
import { api } from "../../lib/api";
import { PageHeader, Table, Modal, Alert } from "../../components/ui/Primitives";

const DAYS = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
const EMPTY = { name: "", startTime: "08:00:00", endTime: "10:00:00", days: [] };

export default function AdminShifts() {
  const [shifts, setShifts] = useState([]);
  const [modal, setModal] = useState(null);
  const [error, setError] = useState("");

  const load = () => api.get("/api/admin/shifts").then(setShifts);
  useEffect(() => { load(); }, []);

  const save = async (form) => {
    setError("");
    try {
      if (form._isNew) await api.post("/api/admin/shifts", form);
      else await api.put(`/api/admin/shifts/${form.id}`, form);
      setModal(null);
      load();
    } catch (err) { setError(err.message); }
  };

  const remove = async (id) => {
    if (!confirm("Delete this shift?")) return;
    try { await api.del(`/api/admin/shifts/${id}`); load(); } catch (err) { alert(err.message); }
  };

  return (
    <div>
      <PageHeader
        title="Shifts"
        subtitle="Recurring time slots classes can be scheduled on."
        action={<button className="btn-primary" onClick={() => setModal({ ...EMPTY, _isNew: true })}>+ New shift</button>}
      />
      <Table
        columns={["Name", "Time", "Days", ""]}
        rows={shifts}
        renderRow={(s) => (
          <>
            <td className="px-4 py-3 font-medium text-white">{s.name}</td>
            <td className="px-4 py-3 text-slate-300">{s.startTime?.slice(0, 5)} – {s.endTime?.slice(0, 5)}</td>
            <td className="px-4 py-3 text-slate-300">{s.days.map((d) => DAYS[d]?.slice(0, 3)).join(", ")}</td>
            <td className="px-4 py-3 text-right">
              <button onClick={() => setModal({ ...s })} className="btn-ghost mr-2 px-3 py-1.5 text-xs">Edit</button>
              <button onClick={() => remove(s.id)} className="btn-danger px-3 py-1.5 text-xs">Delete</button>
            </td>
          </>
        )}
      />
      <Modal open={!!modal} onClose={() => setModal(null)} title={modal?._isNew ? "New shift" : "Edit shift"}>
        {modal && <ShiftForm initial={modal} onSave={save} error={error} />}
      </Modal>
    </div>
  );
}

function ShiftForm({ initial, onSave, error }) {
  const [form, setForm] = useState(initial);
  const toggleDay = (i) =>
    setForm((f) => ({ ...f, days: f.days.includes(i) ? f.days.filter((d) => d !== i) : [...f.days, i] }));

  return (
    <form onSubmit={(e) => { e.preventDefault(); onSave(form); }} className="space-y-4">
      {error && <Alert>{error}</Alert>}
      <div>
        <label className="label">Shift name</label>
        <input className="input" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} required />
      </div>
      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="label">Start time</label>
          <input className="input" type="time" value={form.startTime?.slice(0, 5)} onChange={(e) => setForm({ ...form, startTime: e.target.value + ":00" })} />
        </div>
        <div>
          <label className="label">End time</label>
          <input className="input" type="time" value={form.endTime?.slice(0, 5)} onChange={(e) => setForm({ ...form, endTime: e.target.value + ":00" })} />
        </div>
      </div>
      <div>
        <label className="label">Days</label>
        <div className="flex flex-wrap gap-2">
          {DAYS.map((d, i) => (
            <button
              type="button"
              key={d}
              onClick={() => toggleDay(i)}
              className={`rounded-lg px-3 py-1.5 text-xs font-medium transition ${
                form.days.includes(i) ? "bg-brand-500 text-white" : "bg-white/5 text-slate-400 hover:bg-white/10"
              }`}
            >
              {d.slice(0, 3)}
            </button>
          ))}
        </div>
      </div>
      <button className="btn-primary w-full">Save shift</button>
    </form>
  );
}
