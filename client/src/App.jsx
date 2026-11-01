import { Routes, Route, Navigate, Outlet } from "react-router-dom";
import { useAuth } from "./context/AuthContext";
import { Spinner } from "./components/ui/Primitives";
import DashboardLayout from "./components/DashboardLayout";

import Landing from "./pages/Landing";
import CourseDetail from "./pages/CourseDetail";
import Login from "./pages/Login";
import Register from "./pages/Register";

import AdminOverview from "./pages/admin/AdminOverview";
import AdminCourses from "./pages/admin/AdminCourses";
import AdminClasses from "./pages/admin/AdminClasses";
import AdminShifts from "./pages/admin/AdminShifts";
import AdminLocations from "./pages/admin/AdminLocations";
import AdminUsers from "./pages/admin/AdminUsers";
import AdminContent from "./pages/admin/AdminContent";
import AdminSettings from "./pages/admin/AdminSettings";

import TeacherOverview from "./pages/teacher/TeacherOverview";
import TeacherClasses from "./pages/teacher/TeacherClasses";
import TeacherClassDetail from "./pages/teacher/TeacherClassDetail";
import TeacherCreateClass from "./pages/teacher/TeacherCreateClass";

import StudentOverview from "./pages/student/StudentOverview";
import StudentCourse from "./pages/student/StudentCourse";
import StudentScan from "./pages/student/StudentScan";

import MyAccount from "./pages/MyAccount";

const ADMIN_NAV = [
  { to: "/admin", end: true, icon: "📊", label: "Overview" },
  { to: "/admin/courses", icon: "📚", label: "Courses" },
  { to: "/admin/content", icon: "🎬", label: "Content" },
  { to: "/admin/classes", icon: "🏫", label: "Classes" },
  { to: "/admin/shifts", icon: "⏰", label: "Shifts" },
  { to: "/admin/locations", icon: "📍", label: "Locations" },
  { to: "/admin/users", icon: "👥", label: "Users" },
  { to: "/admin/settings", icon: "⚙️", label: "Settings" },
  { to: "/admin/account", icon: "🙍", label: "My account" },
];
const TEACHER_NAV = [
  { to: "/teacher", end: true, icon: "📊", label: "Overview" },
  { to: "/teacher/classes", icon: "🏫", label: "My classes" },
  { to: "/teacher/classes/new", icon: "➕", label: "Create class" },
  { to: "/teacher/account", icon: "🙍", label: "My account" },
];
const STUDENT_NAV = [
  { to: "/student", end: true, icon: "📊", label: "My Learning" },
  { to: "/student/scan", icon: "📷", label: "Scan attendance" },
  { to: "/", icon: "🔎", label: "Browse courses" },
  { to: "/student/account", icon: "🙍", label: "My account" },
];

function Protected({ roles }) {
  const { user, loading } = useAuth();
  if (loading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-surface text-brand-400">
        <Spinner className="h-8 w-8" />
      </div>
    );
  }
  if (!user) return <Navigate to="/login" replace />;
  if (roles && !roles.includes(user.role)) return <Navigate to={`/${user.role.toLowerCase()}`} replace />;
  return <Outlet />;
}

function RoleShell({ navItems, roleLabel }) {
  return (
    <DashboardLayout navItems={navItems} roleLabel={roleLabel}>
      <Outlet />
    </DashboardLayout>
  );
}

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<Landing />} />
      <Route path="/courses/:id" element={<CourseDetail />} />
      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />

      <Route element={<Protected roles={["Admin"]} />}>
        <Route element={<RoleShell navItems={ADMIN_NAV} roleLabel="Administrator" />}>
          <Route path="/admin" element={<AdminOverview />} />
          <Route path="/admin/courses" element={<AdminCourses />} />
          <Route path="/admin/content" element={<AdminContent />} />
          <Route path="/admin/classes" element={<AdminClasses />} />
          <Route path="/admin/shifts" element={<AdminShifts />} />
          <Route path="/admin/locations" element={<AdminLocations />} />
          <Route path="/admin/users" element={<AdminUsers />} />
          <Route path="/admin/settings" element={<AdminSettings />} />
          <Route path="/admin/account" element={<MyAccount />} />
        </Route>
      </Route>

      <Route element={<Protected roles={["Teacher"]} />}>
        <Route element={<RoleShell navItems={TEACHER_NAV} roleLabel="Teacher" />}>
          <Route path="/teacher" element={<TeacherOverview />} />
          <Route path="/teacher/classes" element={<TeacherClasses />} />
          <Route path="/teacher/classes/new" element={<TeacherCreateClass />} />
          <Route path="/teacher/classes/:id" element={<TeacherClassDetail />} />
          <Route path="/teacher/account" element={<MyAccount />} />
        </Route>
      </Route>

      <Route element={<Protected roles={["Student"]} />}>
        <Route element={<RoleShell navItems={STUDENT_NAV} roleLabel="Student" />}>
          <Route path="/student" element={<StudentOverview />} />
          <Route path="/student/courses/:id" element={<StudentCourse />} />
          <Route path="/student/scan" element={<StudentScan />} />
          <Route path="/student/account" element={<MyAccount />} />
        </Route>
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
