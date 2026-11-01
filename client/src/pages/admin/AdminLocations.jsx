import { useEffect, useState } from "react";
import { api } from "../../lib/api";
import { PageHeader, Table, Modal, Alert } from "../../components/ui/Primitives";

const EMPTY = { name: "", latitude: 11.5564, longitude: 104.9282, radiusMeters: 100 };

export default function AdminLocations() {
  const [locations, setLocations] = useState([]);
  const [modal, setModal] = useState(null);
  const [error, setError] = useState("");

  const load = () => api.get("/api/admin/locations").then(setLocations);
  useEffect(() => { load(); }, []);

  const save = async (form) => {
    setError("");
    try {
      const payload = { ...form, latitude: Number(form.latitude), longitude: Number(form.longitude), radiusMeters: Number(form.radiusMeters) };
      if (form._isNew) await api.post("/api/admin/locations", payload);
      else await api.put(`/api/admin/locations/${form.id}`, payload);
      setModal(null);
      load();
    } catch (err) { setError(err.message); }
  };

  const remove = async (id) => {
    if (!confirm("Delete this location?")) return;
    try { await api.del(`/api/admin/locations/${id}`); load(); } catch (err) { alert(err.message); }
  };

  return (
    <div>
      <PageHeader
        title="Campus locations"
        subtitle="Saved classroom / building locations used for QR + GPS attendance verification."
        action={<button className="btn-primary" onClick={() => setModal({ ...EMPTY, _isNew: true })}>+ New location</button>}
      />
      <div className="mb-4 rounded-xl border border-amber-500/20 bg-amber-500/5 px-4 py-3 text-sm text-amber-300">
        📍 These coordinates power the geofenced attendance check. A visual map picker (Google Maps) is coming in the next phase — for now, enter coordinates directly.
      </div>
      <Table
        columns={["Name", "Latitude", "Longitude", "Radius", ""]}
        rows={locations}
        renderRow={(l) => (
          <>
            <td className="px-4 py-3 font-medium text-white">{l.name}</td>
            <td className="px-4 py-3 text-slate-300">{l.latitude}</td>
            <td className="px-4 py-3 text-slate-300">{l.longitude}</td>
            <td className="px-4 py-3 text-slate-300">{l.radiusMeters}m</td>
            <td className="px-4 py-3 text-right">
              <button onClick={() => setModal({ ...l })} className="btn-ghost mr-2 px-3 py-1.5 text-xs">Edit</button>
              <button onClick={() => remove(l.id)} className="btn-danger px-3 py-1.5 text-xs">Delete</button>
            </td>
          </>
        )}
      />
      <Modal open={!!modal} onClose={() => setModal(null)} title={modal?._isNew ? "New location" : "Edit location"}>
        {modal && <LocationForm initial={modal} onSave={save} error={error} />}
      </Modal>
    </div>
  );
}

function LocationForm({ initial, onSave, error }) {
  const [form, setForm] = useState(initial);
  const set = (k) => (e) => setForm((f) => ({ ...f, [k]: e.target.value }));
  return (
    <form onSubmit={(e) => { e.preventDefault(); onSave(form); }} className="space-y-4">
      {error && <Alert>{error}</Alert>}
      <div>
        <label className="label">Location name</label>
        <input className="input" value={form.name} onChange={set("name")} required placeholder="e.g. Main Campus - Building A" />
      </div>
      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="label">Latitude</label>
          <input className="input" type="number" step="any" value={form.latitude} onChange={set("latitude")} required />
        </div>
        <div>
          <label className="label">Longitude</label>
          <input className="input" type="number" step="any" value={form.longitude} onChange={set("longitude")} required />
        </div>
      </div>
      <div>
        <label className="label">Allowed radius (meters)</label>
        <input className="input" type="number" min="10" value={form.radiusMeters} onChange={set("radiusMeters")} />
      </div>
      <button className="btn-primary w-full">Save location</button>
    </form>
  );
}
