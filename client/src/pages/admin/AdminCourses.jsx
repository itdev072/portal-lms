import { useEffect, useState } from "react";
import { api } from "../../lib/api";
import { PageHeader, Table, Badge, Modal, Alert } from "../../components/ui/Primitives";

const CATEGORIES = ["Programming", "WebDevelopment", "DataScience", "Design", "Business", "Language"];
const EMPTY = {
  id: "", order: 1, nameEn: "", nameKm: "", shortDescriptionEn: "", descriptionKm: "",
  category: "Programming", thumbnailEmoji: "📘", accentColor: "#6366f1", price: 0,
  isPublished: true, estimatedHours: 10, prerequisite: "",
};

export default function AdminCourses() {
  const [courses, setCourses] = useState([]);
  const [modal, setModal] = useState(null); // null | "new" | course object
  const [error, setError] = useState("");

  const load = () => api.get("/api/admin/courses").then(setCourses);
  useEffect(() => { load(); }, []);

  const openEdit = (c) => setModal(c ? { ...c } : { ...EMPTY });

  const save = async (form) => {
    setError("");
    try {
      if (form._isNew) await api.post("/api/admin/courses", form);
      else await api.put(`/api/admin/courses/${form.id}`, form);
      setModal(null);
      load();
    } catch (err) {
      setError(err.message);
    }
  };

  const remove = async (id) => {
    if (!confirm("Delete this course?")) return;
    try {
      await api.del(`/api/admin/courses/${id}`);
      load();
    } catch (err) {
      alert(err.message);
    }
  };

  return (
    <div>
      <PageHeader
        title="Courses"
        subtitle="Manage the public course catalog."
        action={<button className="btn-primary" onClick={() => setModal({ ...EMPTY, _isNew: true })}>+ New course</button>}
      />

      <Table
        columns={["Course", "Category", "Price", "Hours", "Status", ""]}
        rows={courses}
        renderRow={(c) => (
          <>
            <td className="px-4 py-3">
              <div className="flex items-center gap-2.5">
                <span className="text-lg">{c.thumbnailEmoji}</span>
                <div>
                  <div className="font-medium text-white">{c.nameEn}</div>
                  <div className="text-xs text-slate-500">{c.id}</div>
                </div>
              </div>
            </td>
            <td className="px-4 py-3 text-slate-300">{c.category}</td>
            <td className="px-4 py-3 text-slate-300">{c.price > 0 ? `$${c.price}` : "Free"}</td>
            <td className="px-4 py-3 text-slate-300">{c.estimatedHours}h</td>
            <td className="px-4 py-3">
              <Badge tone={c.isPublished ? "green" : "slate"}>{c.isPublished ? "Published" : "Draft"}</Badge>
            </td>
            <td className="px-4 py-3 text-right">
              <button onClick={() => openEdit(c)} className="btn-ghost mr-2 px-3 py-1.5 text-xs">Edit</button>
              <button onClick={() => remove(c.id)} className="btn-danger px-3 py-1.5 text-xs">Delete</button>
            </td>
          </>
        )}
      />

      <Modal open={!!modal} onClose={() => setModal(null)} title={modal?._isNew ? "New course" : "Edit course"} wide>
        {modal && <CourseForm initial={modal} onSave={save} error={error} />}
      </Modal>
    </div>
  );
}

function CourseForm({ initial, onSave, error }) {
  const [form, setForm] = useState(initial);
  const set = (k) => (e) => setForm((f) => ({ ...f, [k]: e.target.type === "checkbox" ? e.target.checked : e.target.value }));

  return (
    <form
      onSubmit={(e) => {
        e.preventDefault();
        onSave({ ...form, order: Number(form.order), price: Number(form.price), estimatedHours: Number(form.estimatedHours) });
      }}
      className="space-y-4"
    >
      {error && <Alert>{error}</Alert>}
      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="label">Course ID (short code)</label>
          <input className="input" value={form.id} onChange={set("id")} placeholder="e.g. CSK3" required disabled={!form._isNew} />
        </div>
        <div>
          <label className="label">Order</label>
          <input className="input" type="number" value={form.order} onChange={set("order")} />
        </div>
      </div>
      <div>
        <label className="label">Course name</label>
        <input className="input" value={form.nameEn} onChange={set("nameEn")} required />
      </div>
      <div>
        <label className="label">Short description</label>
        <input className="input" value={form.shortDescriptionEn} onChange={set("shortDescriptionEn")} />
      </div>
      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="label">Category</label>
          <select className="input" value={form.category} onChange={set("category")}>
            {CATEGORIES.map((c) => <option key={c} value={c}>{c}</option>)}
          </select>
        </div>
        <div>
          <label className="label">Prerequisite (course ID)</label>
          <input className="input" value={form.prerequisite || ""} onChange={set("prerequisite")} />
        </div>
      </div>
      <div className="grid grid-cols-4 gap-4">
        <div>
          <label className="label">Emoji</label>
          <input className="input text-center" value={form.thumbnailEmoji} onChange={set("thumbnailEmoji")} />
        </div>
        <div>
          <label className="label">Color</label>
          <input className="input h-[42px] p-1" type="color" value={form.accentColor} onChange={set("accentColor")} />
        </div>
        <div>
          <label className="label">Price ($)</label>
          <input className="input" type="number" min="0" value={form.price} onChange={set("price")} />
        </div>
        <div>
          <label className="label">Hours</label>
          <input className="input" type="number" min="0" value={form.estimatedHours} onChange={set("estimatedHours")} />
        </div>
      </div>
      <label className="flex items-center gap-2 text-sm text-slate-300">
        <input type="checkbox" checked={form.isPublished} onChange={set("isPublished")} className="h-4 w-4 rounded border-surface-border bg-black/20" />
        Published (visible on the public catalog)
      </label>
      <button className="btn-primary w-full">Save course</button>
    </form>
  );
}
