export function Card({ className = "", children, ...rest }) {
  return (
    <div className={`card p-5 ${className}`} {...rest}>
      {children}
    </div>
  );
}

export function StatCard({ icon, label, value, accent = "text-brand-300" }) {
  return (
    <Card className="flex items-center gap-4 animate-fadeUp">
      <div className={`flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-white/5 text-xl ${accent}`}>
        {icon}
      </div>
      <div>
        <div className="text-2xl font-bold font-display tracking-tight">{value}</div>
        <div className="text-xs text-slate-400">{label}</div>
      </div>
    </Card>
  );
}

export function Badge({ children, tone = "slate" }) {
  const tones = {
    slate: "bg-white/5 text-slate-300",
    green: "bg-emerald-500/15 text-emerald-300",
    red: "bg-rose-500/15 text-rose-300",
    amber: "bg-amber-500/15 text-amber-300",
    brand: "bg-brand-500/15 text-brand-300",
  };
  return <span className={`badge ${tones[tone] || tones.slate}`}>{children}</span>;
}

export function Spinner({ className = "" }) {
  return (
    <svg className={`animate-spin ${className}`} viewBox="0 0 24 24" fill="none">
      <circle className="opacity-20" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="3" />
      <path d="M22 12a10 10 0 0 0-10-10" stroke="currentColor" strokeWidth="3" strokeLinecap="round" />
    </svg>
  );
}

export function PageHeader({ title, subtitle, action }) {
  return (
    <div className="mb-6 flex flex-wrap items-start justify-between gap-4">
      <div>
        <h1 className="font-display text-2xl font-bold tracking-tight text-white">{title}</h1>
        {subtitle && <p className="mt-1 text-sm text-slate-400">{subtitle}</p>}
      </div>
      {action}
    </div>
  );
}

export function EmptyState({ icon = "📭", title, subtitle }) {
  return (
    <div className="flex flex-col items-center justify-center rounded-2xl border border-dashed border-surface-border py-16 text-center">
      <div className="mb-3 text-4xl">{icon}</div>
      <div className="font-medium text-slate-300">{title}</div>
      {subtitle && <div className="mt-1 text-sm text-slate-500">{subtitle}</div>}
    </div>
  );
}

export function Modal({ open, onClose, title, children, wide = false }) {
  if (!open) return null;
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div className="absolute inset-0 bg-black/60 backdrop-blur-sm animate-fadeUp" onClick={onClose} />
      <div className={`relative z-10 w-full ${wide ? "max-w-2xl" : "max-w-md"} rounded-2xl border border-surface-border bg-surface-card p-6 shadow-2xl animate-fadeUp max-h-[85vh] overflow-y-auto`}>
        <div className="mb-4 flex items-center justify-between">
          <h3 className="font-display text-lg font-semibold text-white">{title}</h3>
          <button onClick={onClose} className="rounded-lg p-1.5 text-slate-400 hover:bg-white/10 hover:text-white">
            ✕
          </button>
        </div>
        {children}
      </div>
    </div>
  );
}

export function Alert({ children, tone = "red" }) {
  const tones = {
    red: "bg-rose-500/10 border-rose-500/30 text-rose-300",
    green: "bg-emerald-500/10 border-emerald-500/30 text-emerald-300",
  };
  return <div className={`rounded-xl border px-4 py-3 text-sm ${tones[tone]}`}>{children}</div>;
}

export function Table({ columns, rows, keyField = "id", renderRow }) {
  return (
    <div className="overflow-x-auto rounded-2xl border border-surface-border">
      <table className="w-full text-left text-sm">
        <thead>
          <tr className="border-b border-surface-border bg-white/[0.02] text-xs uppercase tracking-wide text-slate-400">
            {columns.map((c) => (
              <th key={c} className="px-4 py-3 font-medium">
                {c}
              </th>
            ))}
          </tr>
        </thead>
        <tbody className="divide-y divide-surface-border">
          {rows.length === 0 ? (
            <tr>
              <td colSpan={columns.length} className="px-4 py-10 text-center text-slate-500">
                No records yet.
              </td>
            </tr>
          ) : (
            rows.map((row) => <tr key={row[keyField]} className="transition hover:bg-white/[0.02]">{renderRow(row)}</tr>)
          )}
        </tbody>
      </table>
    </div>
  );
}
