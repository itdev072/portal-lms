import { useEffect, useState } from "react";
import { api } from "../../lib/api";
import { PageHeader, Table, Modal, Alert } from "../../components/ui/Primitives";

const EMPTY = { name: "", teacherId: "", semesterLabel: "", levelId: "", shiftId: "", campusLocationId: "", capacity: 30 };

export default function AdminClasses() {
  const [classes, setClasses] = useState([]);
  const [options, setOptions] = useState({ courses: [], teachers: [], shifts: [], locations: [] });
  const [modal, setModal] = useState(null);
  const [error, setError] = useState("");

  const load = () => api.get("/api/admin/classes").then(setClasses);

  useEffect(() => {
    load();
    Promise.all([
      api.get("/api/admin/courses"),
      api.get("/api/teachers"),
      api.get("/api/admin/shifts"),
      api.get("/api/admin/locations"),
    ]).then(([courses, teachers, shifts, locations]) => setOptions({ courses, teachers, shifts, locations }));
  }, []);

  const save = async (form) => {
    setError("");
    try {
      const payload = { ...form, capacity: Number(form.capacity) };
      if (form._isNew) await api.post("/api/admin/classes", payload);
      else await api.put(`/api/admin/classes/${form.id}`, payload);
      setModal(null);
      load();
    } catch (err) { setError(err.message); }
  };

  const remove = async (id) => {
    if (!confirm("Delete this class? Enrollments will be removed too.")) return;
    try { await api.del(`/api/admin/classes/${id}`); load(); } catch (err) { alert(err.message); }
  };

  return (
    <div>
      <PageHeader
        title="Classes"
        subtitle="Scheduled class sections — course + teacher + shift + location."
        action={<button className="btn-primary" onClick={() => setModal({ ...EMPTY, _isNew: true })}>+ New class</button>}
      />
      <Table
        columns={["Class", "Course", "Teacher", "Shift", "Location", "Enrolled", ""]}
        rows={classes}
        renderRow={(c) => (
          <>
            <td className="px-4 py-3 font-medium text-white">{c.name}</td>
            <td className="px-4 py-3 text-slate-300">{c.courseName}</td>
            <td className="px-4 py-3 text-slate-300">{c.teacherName}</td>
            <td className="px-4 py-3 text-slate-300">{c.shiftName}</td>
            <td className="px-4 py-3 text-slate-300">{c.campusLocationName}</td>
            <td className="px-4 py-3 text-slate-300">{c.enrolledCount}/{c.capacity}</td>
            <td className="px-4 py-3 text-right">
              <button onClick={() => setModal({ ...c, _isNew: false })} className="btn-ghost mr-2 px-3 py-1.5 text-xs">Edit</button>
              <button onClick={() => remove(c.id)} className="btn-danger px-3 py-1.5 text-xs">Delete</button>
            </td>
          </>
        )}
      />
      <Modal open={!!modal} onClose={() => setModal(null)} title={modal?._isNew ? "New class" : "Edit class"}>
        {modal && <ClassForm initial={modal} options={options} onSave={save} error={error} />}
      </Modal>
    </div>
  );
}

function ClassForm({ initial, options, onSave, error }) {
  const [form, setForm] = useState(initial);
  const set = (k) => (e) => setForm((f) => ({ ...f, [k]: e.target.value }));

  return (
    <form onSubmit={(e) => { e.preventDefault(); onSave(form); }} className="space-y-4">
      {error && <Alert>{error}</Alert>}
      <div>
        <label className="label">Class name</label>
        <input className="input" value={form.name} onChange={set("name")} required placeholder="e.g. SPD Year 2 - Section A" />
      </div>
      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="label">Course</label>
          <select className="input" value={form.levelId} onChange={set("levelId")} required>
            <option value="">Select a course…</option>
            {options.courses.map((c) => <option key={c.id} value={c.id}>{c.nameEn}</option>)}
          </select>
        </div>
        <div>
          <label className="label">Teacher</label>
          <select className="input" value={form.teacherId} onChange={set("teacherId")} required>
            <option value="">Select a teacher…</option>
            {options.teachers.map((t) => <option key={t.id} value={t.id}>{t.displayName}</option>)}
          </select>
        </div>
      </div>
      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="label">Shift</label>
          <select className="input" value={form.shiftId} onChange={set("shiftId")} required>
            <option value="">Select a shift…</option>
            {options.shifts.map((s) => <option key={s.id} value={s.id}>{s.name}</option>)}
          </select>
        </div>
        <div>
          <label className="label">Location</label>
          <select className="input" value={form.campusLocationId} onChange={set("campusLocationId")} required>
            <option value="">Select a location…</option>
            {options.locations.map((l) => <option key={l.id} value={l.id}>{l.name}</option>)}
          </select>
        </div>
      </div>
      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="label">Semester label</label>
          <input className="input" value={form.semesterLabel} onChange={set("semesterLabel")} placeholder="e.g. Semester 1" />
        </div>
        <div>
          <label className="label">Capacity</label>
          <input className="input" type="number" min="1" value={form.capacity} onChange={set("capacity")} />
        </div>
      </div>
      <button className="btn-primary w-full">Save class</button>
    </form>
  );
}
