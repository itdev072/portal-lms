import { NavLink, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { useState } from "react";

export default function DashboardLayout({ navItems, roleLabel, children }) {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [mobileOpen, setMobileOpen] = useState(false);

  const handleLogout = async () => {
    await logout();
    navigate("/");
  };

  const Nav = (
    <nav className="flex flex-1 flex-col gap-1">
      {navItems.map((item) => (
        <NavLink
          key={item.to}
          to={item.to}
          end={item.end}
          onClick={() => setMobileOpen(false)}
          className={({ isActive }) => `nav-link ${isActive ? "nav-link-active" : ""}`}
        >
          <span className="text-base">{item.icon}</span>
          {item.label}
        </NavLink>
      ))}
    </nav>
  );

  return (
    <div className="flex min-h-screen bg-surface">
      {/* Desktop sidebar */}
      <aside className="hidden w-64 shrink-0 flex-col border-r border-surface-border bg-surface-card/40 p-4 lg:flex">
        <Brand />
        <div className="mt-6 flex-1">{Nav}</div>
        <UserFooter user={user} roleLabel={roleLabel} onLogout={handleLogout} />
      </aside>

      {/* Mobile top bar + drawer */}
      <div className="flex flex-1 flex-col">
        <header className="flex items-center justify-between border-b border-surface-border bg-surface-card/40 px-4 py-3 lg:hidden">
          <Brand compact />
          <button onClick={() => setMobileOpen(true)} className="rounded-lg p-2 text-slate-300 hover:bg-white/10">
            <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <path d="M4 6h16M4 12h16M4 18h16" strokeLinecap="round" />
            </svg>
          </button>
        </header>

        {mobileOpen && (
          <div className="fixed inset-0 z-50 lg:hidden">
            <div className="absolute inset-0 bg-black/60" onClick={() => setMobileOpen(false)} />
            <div className="relative z-10 flex h-full w-72 flex-col bg-surface-card p-4">
              <div className="mb-6 flex items-center justify-between">
                <Brand />
                <button onClick={() => setMobileOpen(false)} className="rounded-lg p-1.5 text-slate-400 hover:bg-white/10">✕</button>
              </div>
              {Nav}
              <UserFooter user={user} roleLabel={roleLabel} onLogout={handleLogout} />
            </div>
          </div>
        )}

        <main className="flex-1 p-4 sm:p-6 lg:p-8">{children}</main>
      </div>
    </div>
  );
}

function Brand({ compact = false }) {
  return (
    <div className="flex items-center gap-2.5">
      <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-gradient-to-br from-brand-400 to-brand-600 font-display text-lg font-bold text-white shadow-glow">
        P
      </div>
      {!compact && <span className="font-display text-lg font-bold text-white">Portal</span>}
    </div>
  );
}

function UserFooter({ user, roleLabel, onLogout }) {
  return (
    <div className="mt-4 border-t border-surface-border pt-4">
      <div className="flex items-center gap-3 rounded-xl p-2">
        <div className="flex h-9 w-9 items-center justify-center rounded-full bg-brand-500/20 text-sm font-semibold text-brand-300">
          {user?.displayName?.[0]?.toUpperCase() || "?"}
        </div>
        <div className="min-w-0 flex-1">
          <div className="truncate text-sm font-medium text-slate-100">{user?.displayName}</div>
          <div className="text-xs text-slate-500">{roleLabel}</div>
        </div>
      </div>
      <button onClick={onLogout} className="nav-link mt-1 w-full text-left text-rose-400 hover:bg-rose-500/10 hover:text-rose-300">
        <span>🚪</span> Log out
      </button>
    </div>
  );
}
