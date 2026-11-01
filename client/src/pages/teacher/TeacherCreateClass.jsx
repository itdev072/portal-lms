import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { api } from "../../lib/api";
import { PageHeader, Card, Alert, Modal } from "../../components/ui/Primitives";
import MapPicker from "../../components/MapPicker";

const EMPTY_LOCATION = { name: "", latitude: 11.5564, longitude: 104.9282, radiusMeters: 100 };

export default function TeacherCreateClass() {
  const navigate = useNavigate();
  const [options, setOptions] = useState({ courses: [], shifts: [], locations: [] });
  const [form, setForm] = useState({ name: "", semesterLabel: "", levelId: "", shiftId: "", campusLocationId: "", capacity: 30 });
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);
  const [newLocation, setNewLocation] = useState(null);

  useEffect(() => {
    Promise.all([
      api.get("/api/teacher/courses"),
      api.get("/api/admin/shifts"),
      api.get("/api/admin/locations"),
    ]).then(([courses, shifts, locations]) => setOptions({ courses, shifts, locations }));
  }, []);

  const set = (k) => (e) => setForm((f) => ({ ...f, [k]: e.target.value }));

  const saveNewLocation = async () => {
    const loc = await api.post("/api/admin/locations", {
      ...newLocation,
      latitude: Number(newLocation.latitude),
      longitude: Number(newLocation.longitude),
      radiusMeters: Number(newLocation.radiusMeters),
    });
    setOptions((o) => ({ ...o, locations: [...o.locations, loc] }));
    setForm((f) => ({ ...f, campusLocationId: loc.id }));
    setNewLocation(null);
  };

  const submit = async (e) => {
    e.preventDefault();
    setError("");
    setBusy(true);
    try {
      const payload = { ...form, teacherId: "", capacity: Number(form.capacity) };
      const cls = await api.post("/api/teacher/classes", payload);
      navigate(`/teacher/classes/${cls.id}`);
    } catch (err) {
      setError(err.message);
    } finally {
      setBusy(false);
    }
  };

  return (
    <div>
      <PageHeader title="Create a class" subtitle="Schedule a new section — pick a course, a shift, and where it meets." />

      <Card className="max-w-xl">
        <form onSubmit={submit} className="space-y-4">
          {error && <Alert>{error}</Alert>}

          <div>
            <label className="label">Class name</label>
            <input className="input" value={form.name} onChange={set("name")} required placeholder="e.g. CSK1 - Evening Section B" />
          </div>

          <div>
            <label className="label">Course</label>
            <select className="input" value={form.levelId} onChange={set("levelId")} required>
              <option value="">Select a course…</option>
              {options.courses.map((c) => <option key={c.id} value={c.id}>{c.nameEn}</option>)}
            </select>
          </div>

          <div>
            <label className="label">Shift</label>
            <select className="input" value={form.shiftId} onChange={set("shiftId")} required>
              <option value="">Select a shift…</option>
              {options.shifts.map((s) => <option key={s.id} value={s.id}>{s.name}</option>)}
            </select>
          </div>

          <div>
            <label className="label">Location</label>
            <div className="flex gap-2">
              <select className="input" value={form.campusLocationId} onChange={set("campusLocationId")} required>
                <option value="">Select a location…</option>
                {options.locations.map((l) => <option key={l.id} value={l.id}>{l.name}</option>)}
              </select>
              <button type="button" className="btn-outline shrink-0" onClick={() => setNewLocation(EMPTY_LOCATION)}>
                + New
              </button>
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

          <button className="btn-primary w-full" disabled={busy}>{busy ? "Creating…" : "Create class"}</button>
        </form>
      </Card>

      <Modal open={!!newLocation} onClose={() => setNewLocation(null)} title="New location" wide>
        {newLocation && (
          <div className="space-y-4">
            <div>
              <label className="label">Location name</label>
              <input className="input" value={newLocation.name} onChange={(e) => setNewLocation((l) => ({ ...l, name: e.target.value }))} required placeholder="e.g. Room 12, Building B" />
            </div>
            <MapPicker
              lat={newLocation.latitude}
              lng={newLocation.longitude}
              radiusMeters={newLocation.radiusMeters}
              onChange={(lat, lng) => setNewLocation((l) => ({ ...l, latitude: lat, longitude: lng }))}
            />
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="label">Latitude</label>
                <input className="input" type="number" step="any" value={newLocation.latitude} onChange={(e) => setNewLocation((l) => ({ ...l, latitude: e.target.value }))} />
              </div>
              <div>
                <label className="label">Longitude</label>
                <input className="input" type="number" step="any" value={newLocation.longitude} onChange={(e) => setNewLocation((l) => ({ ...l, longitude: e.target.value }))} />
              </div>
            </div>
            <button type="button" className="btn-primary w-full" onClick={saveNewLocation} disabled={!newLocation.name}>
              Save location
            </button>
          </div>
        )}
      </Modal>
    </div>
  );
}
