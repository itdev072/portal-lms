import { useEffect, useState } from "react";
import { api } from "../../lib/api";
import { PageHeader, Table, Modal, Alert, Spinner } from "../../components/ui/Primitives";

const EMPTY = {
  levelId: "", order: 1, titleEn: "", titleKm: "", estimatedMinutes: 15, xpValue: 10,
  videoUrl: "", durationSeconds: 300, explainerMd: "", resourcesMd: "",
};

export default function AdminContent() {
  const [courses, setCourses] = useState([]);
  const [courseId, setCourseId] = useState("");
  const [lessons, setLessons] = useState(null);
  const [modal, setModal] = useState(null);
  const [error, setError] = useState("");

  useEffect(() => {
    api.get("/api/admin/courses").then((cs) => {
      setCourses(cs);
      if (cs.length) setCourseId(cs[0].id);
    });
  }, []);

  const load = (cid) => api.get(`/api/admin/courses/${cid}/lessons`).then(setLessons);
  useEffect(() => { if (courseId) load(courseId); }, [courseId]);

  const save = async (form) => {
    setError("");
    try {
      const payload = { ...form, levelId: courseId, order: Number(form.order), estimatedMinutes: Number(form.estimatedMinutes), xpValue: Number(form.xpValue), durationSeconds: Number(form.durationSeconds) };
      if (form._isNew) await api.post("/api/admin/lessons", payload);
      else await api.put(`/api/admin/lessons/${form.id}`, payload);
      setModal(null);
      load(courseId);
    } catch (err) { setError(err.message); }
  };

  const remove = async (id) => {
    if (!confirm("Delete this lesson? Any quizzes/challenges tied to it stay orphaned.")) return;
    try { await api.del(`/api/admin/lessons/${id}`); load(courseId); } catch (err) { alert(err.message); }
  };

  return (
    <div>
      <PageHeader
        title="Content"
        subtitle="Manage lessons — video, duration, written explainer, and resources — per course."
        action={<button className="btn-primary" onClick={() => setModal({ ...EMPTY, _isNew: true, order: (lessons?.length || 0) + 1 })} disabled={!courseId}>+ New lesson</button>}
      />

      <div className="mb-4 flex gap-2 overflow-x-auto">
        {courses.map((c) => (
          <button
            key={c.id}
            onClick={() => setCourseId(c.id)}
            className={`shrink-0 rounded-full px-4 py-1.5 text-sm font-medium transition ${courseId === c.id ? "bg-brand-500 text-white" : "bg-white/5 text-slate-300 hover:bg-white/10"}`}
          >
            {c.thumbnailEmoji} {c.nameEn}
          </button>
        ))}
      </div>

      {!lessons ? (
        <div className="flex justify-center py-16 text-brand-400"><Spinner className="h-7 w-7" /></div>
      ) : (
        <Table
          columns={["#", "Lesson", "Duration", "XP", "Video", ""]}
          rows={lessons}
          renderRow={(l) => (
            <>
              <td className="px-4 py-3 text-slate-400">{l.order}</td>
              <td className="px-4 py-3 font-medium text-white">{l.titleEn}</td>
              <td className="px-4 py-3 text-slate-300">{Math.round(l.durationSeconds / 60)} min video · {l.estimatedMinutes} min lesson</td>
              <td className="px-4 py-3 text-slate-300">{l.xpValue} XP</td>
              <td className="px-4 py-3">
                {l.videoUrl ? <span className="badge bg-emerald-500/15 text-emerald-300">✓ set</span> : <span className="badge bg-white/5 text-slate-400">none</span>}
              </td>
              <td className="px-4 py-3 text-right">
                <button onClick={() => setModal({ ...l, _isNew: false })} className="btn-ghost mr-2 px-3 py-1.5 text-xs">Edit</button>
                <button onClick={() => remove(l.id)} className="btn-danger px-3 py-1.5 text-xs">Delete</button>
              </td>
            </>
          )}
        />
      )}

      <Modal open={!!modal} onClose={() => setModal(null)} title={modal?._isNew ? "New lesson" : "Edit lesson"} wide>
        {modal && <LessonForm initial={modal} onSave={save} error={error} />}
      </Modal>
    </div>
  );
}

function LessonForm({ initial, onSave, error }) {
  const [form, setForm] = useState(initial);
  const set = (k) => (e) => setForm((f) => ({ ...f, [k]: e.target.value }));

  return (
    <form onSubmit={(e) => { e.preventDefault(); onSave(form); }} className="space-y-4">
      {error && <Alert>{error}</Alert>}
      <div className="grid grid-cols-[80px_1fr] gap-4">
        <div>
          <label className="label">Order</label>
          <input className="input" type="number" min="1" value={form.order} onChange={set("order")} />
        </div>
        <div>
          <label className="label">Title (English)</label>
          <input className="input" value={form.titleEn} onChange={set("titleEn")} required />
        </div>
      </div>
      <div>
        <label className="label">Title (Khmer)</label>
        <input className="input" value={form.titleKm} onChange={set("titleKm")} />
      </div>
      <div>
        <label className="label">Video URL</label>
        <input className="input" value={form.videoUrl} onChange={set("videoUrl")} placeholder="https://…/lecture.mp4" />
      </div>
      <div className="grid grid-cols-3 gap-4">
        <div>
          <label className="label">Video length (sec)</label>
          <input className="input" type="number" min="1" value={form.durationSeconds} onChange={set("durationSeconds")} />
        </div>
        <div>
          <label className="label">Est. minutes</label>
          <input className="input" type="number" min="1" value={form.estimatedMinutes} onChange={set("estimatedMinutes")} />
        </div>
        <div>
          <label className="label">XP value</label>
          <input className="input" type="number" min="0" value={form.xpValue} onChange={set("xpValue")} />
        </div>
      </div>
      <div>
        <label className="label">Explainer (shown in the Overview tab)</label>
        <textarea className="input" rows={4} value={form.explainerMd} onChange={set("explainerMd")} />
      </div>
      <div>
        <label className="label">Resources (markdown — shown in the Resources tab)</label>
        <textarea className="input" rows={3} value={form.resourcesMd} onChange={set("resourcesMd")} placeholder="- [Docs](https://…)" />
      </div>
      <button className="btn-primary w-full">Save lesson</button>
    </form>
  );
}
