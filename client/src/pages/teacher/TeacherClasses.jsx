import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { api } from "../../lib/api";
import { PageHeader, Table } from "../../components/ui/Primitives";

export default function TeacherClasses() {
  const [classes, setClasses] = useState([]);

  useEffect(() => {
    api.get("/api/teacher/classes").then(setClasses);
  }, []);

  return (
    <div>
      <PageHeader title="My classes" subtitle="Manage your rosters and attendance." />
      <Table
        columns={["Class", "Course", "Shift", "Location", "Enrolled", ""]}
        rows={classes}
        renderRow={(c) => (
          <>
            <td className="px-4 py-3 font-medium text-white">{c.name}</td>
            <td className="px-4 py-3 text-slate-300">{c.courseName}</td>
            <td className="px-4 py-3 text-slate-300">{c.shiftName}</td>
            <td className="px-4 py-3 text-slate-300">{c.campusLocationName}</td>
            <td className="px-4 py-3 text-slate-300">{c.enrolledCount}/{c.capacity}</td>
            <td className="px-4 py-3 text-right">
              <Link to={`/teacher/classes/${c.id}`} className="btn-outline px-3 py-1.5 text-xs">Open</Link>
            </td>
          </>
        )}
      />
    </div>
  );
}
