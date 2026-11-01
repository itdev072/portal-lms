import { useEffect, useRef, useState, useCallback } from "react";
import { useParams, Link } from "react-router-dom";
import { api } from "../../lib/api";
import { Spinner, Badge, Card, EmptyState } from "../../components/ui/Primitives";

export default function StudentCourse() {
  const { id } = useParams();
  const [data, setData] = useState(null);
  const [activeLessonId, setActiveLessonId] = useState(null);
  const [completedIds, setCompletedIds] = useState(new Set());
  const [sidebarOpen, setSidebarOpen] = useState(true);

  const loadProgress = useCallback(() => {
    api.get(`/api/student/courses/${id}/progress`).then((r) => setCompletedIds(new Set(r.completedLessonIds)));
  }, [id]);

  useEffect(() => {
    api.get(`/api/courses/${id}`).then((d) => {
      setData(d);
      setActiveLessonId(d.lessons[0]?.id);
    });
    loadProgress();
  }, [id, loadProgress]);

  if (!data) {
    return (
      <div className="flex justify-center py-20 text-brand-400">
        <Spinner className="h-8 w-8" />
      </div>
    );
  }

  return (
    <div>
      <Link to="/student" className="text-sm text-slate-400 hover:text-white">← Back to My Learning</Link>
      <div className="mt-3 flex items-center justify-between">
        <h1 className="font-display text-2xl font-bold text-white">{data.course.thumbnailEmoji} {data.course.nameEn}</h1>
        <button onClick={() => setSidebarOpen((v) => !v)} className="btn-ghost hidden px-3 py-1.5 text-xs lg:inline-flex">
          {sidebarOpen ? "Hide curriculum ▸" : "◂ Show curriculum"}
        </button>
      </div>

      <div className={`mt-6 grid grid-cols-1 gap-6 ${sidebarOpen ? "lg:grid-cols-[1fr_300px]" : ""}`}>
        {activeLessonId && (
          <CoursePlayer
            courseId={id}
            lessonId={activeLessonId}
            isCompleted={completedIds.has(activeLessonId)}
            onCompleted={loadProgress}
          />
        )}

        {sidebarOpen && (
          <Curriculum
            lessons={data.lessons}
            activeLessonId={activeLessonId}
            completedIds={completedIds}
            onSelect={setActiveLessonId}
          />
        )}
      </div>
    </div>
  );
}

function Curriculum({ lessons, activeLessonId, completedIds, onSelect }) {
  return (
    <Card className="h-fit p-2 lg:order-2">
      <div className="px-3 py-2 text-xs font-semibold uppercase tracking-wide text-slate-500">Curriculum</div>
      {lessons.map((l, i) => {
        const done = completedIds.has(l.id);
        const active = activeLessonId === l.id;
        return (
          <button
            key={l.id}
            onClick={() => onSelect(l.id)}
            className={`flex w-full items-center gap-3 rounded-xl p-3 text-left text-sm transition ${
              active ? "bg-brand-500/15 text-brand-300" : "text-slate-300 hover:bg-white/5"
            }`}
          >
            <span className={`flex h-7 w-7 shrink-0 items-center justify-center rounded-lg text-xs font-semibold ${done ? "bg-emerald-500/20 text-emerald-300" : "bg-white/5"}`}>
              {done ? "✓" : i + 1}
            </span>
            <span className="min-w-0 flex-1">
              <div className="truncate">{l.titleEn}</div>
              <div className="text-[11px] text-slate-500">{formatDuration(l.durationSeconds || l.estimatedMinutes * 60)}</div>
            </span>
          </button>
        );
      })}
    </Card>
  );
}

function formatDuration(totalSeconds) {
  const m = Math.floor(totalSeconds / 60);
  const s = Math.round(totalSeconds % 60);
  return `${m}:${s.toString().padStart(2, "0")}`;
}

function CoursePlayer({ courseId, lessonId, isCompleted, onCompleted }) {
  const [lesson, setLesson] = useState(null);
  const [tab, setTab] = useState("overview");
  const [notes, setNotes] = useState([]);
  const [noteText, setNoteText] = useState("");
  const [questions, setQuestions] = useState([]);
  const videoRef = useRef(null);
  const markedDoneRef = useRef(false);

  useEffect(() => {
    setLesson(null);
    setTab("overview");
    markedDoneRef.current = false;
    api.get(`/api/courses/${courseId}/lessons/${lessonId}`).then(setLesson);
    api.get(`/api/lessons/${lessonId}/notes`).then(setNotes);
    api.get(`/api/lessons/${lessonId}/questions`).then(setQuestions);
  }, [courseId, lessonId]);

  const handleEnded = async () => {
    if (markedDoneRef.current) return;
    markedDoneRef.current = true;
    try {
      await api.post(`/api/lessons/${lessonId}/complete`);
      onCompleted();
    } catch {
      // best-effort — not worth surfacing an error for an auto-complete ping
    }
  };

  const saveNote = async () => {
    if (!noteText.trim() || !videoRef.current) return;
    const timestampSeconds = videoRef.current.currentTime;
    const note = await api.post(`/api/lessons/${lessonId}/notes`, { timestampSeconds, text: noteText.trim() });
    setNotes((n) => [...n, note].sort((a, b) => a.timestampSeconds - b.timestampSeconds));
    setNoteText("");
  };

  const seekTo = (seconds) => {
    if (videoRef.current) {
      videoRef.current.currentTime = seconds;
      videoRef.current.play().catch(() => {});
    }
  };

  if (!lesson) {
    return (
      <div className="card flex justify-center p-10 text-brand-400 lg:order-1">
        <Spinner className="h-6 w-6" />
      </div>
    );
  }

  const tabs = [
    { key: "overview", label: "📖 Overview" },
    { key: "qa", label: "💬 Q&A" },
    { key: "resources", label: "📎 Resources" },
    ...(lesson.quizzes.length ? [{ key: "quiz", label: "🧠 Quiz" }] : []),
    ...(lesson.challenges.length ? [{ key: "challenge", label: "💻 Challenge" }] : []),
  ];

  return (
    <div className="lg:order-1">
      <div className="card overflow-hidden">
        <div className="relative bg-black">
          <video
            ref={videoRef}
            key={lesson.lesson.id}
            src={lesson.lesson.videoUrl}
            controls
            className="aspect-video w-full"
            onEnded={handleEnded}
          />
          {isCompleted && (
            <div className="absolute right-3 top-3">
              <Badge tone="green">✓ Completed</Badge>
            </div>
          )}
        </div>

        <div className="border-b border-surface-border p-5">
          <h2 className="font-display text-lg font-semibold text-white">{lesson.lesson.titleEn}</h2>
          <div className="mt-1 text-xs text-slate-500">{lesson.lesson.estimatedMinutes} min · {lesson.lesson.xpValue} XP</div>
        </div>

        {/* Timestamped notes — tied directly to the video above */}
        <div className="border-b border-surface-border p-5">
          <h3 className="mb-3 text-sm font-semibold text-white">My notes</h3>
          <div className="flex gap-2">
            <input
              className="input"
              placeholder="Add a note at the current timestamp…"
              value={noteText}
              onChange={(e) => setNoteText(e.target.value)}
              onKeyDown={(e) => e.key === "Enter" && saveNote()}
            />
            <button onClick={saveNote} className="btn-primary shrink-0 px-4 text-xs">Save</button>
          </div>
          {notes.length > 0 && (
            <div className="mt-3 space-y-1.5">
              {notes.map((n) => (
                <button
                  key={n.id}
                  onClick={() => seekTo(n.timestampSeconds)}
                  className="flex w-full items-start gap-2 rounded-lg px-2 py-1.5 text-left text-sm text-slate-300 transition hover:bg-white/5"
                >
                  <span className="mt-0.5 shrink-0 rounded bg-brand-500/15 px-1.5 py-0.5 text-[11px] font-mono text-brand-300">
                    {formatDuration(n.timestampSeconds)}
                  </span>
                  <span className="truncate">{n.text}</span>
                </button>
              ))}
            </div>
          )}
        </div>

        <div className="p-5">
          <div className="mb-4 flex flex-wrap gap-2">
            {tabs.map((t) => (
              <button
                key={t.key}
                onClick={() => setTab(t.key)}
                className={`rounded-lg px-3 py-1.5 text-xs font-medium transition ${tab === t.key ? "bg-brand-500 text-white" : "bg-white/5 text-slate-400 hover:bg-white/10"}`}
              >
                {t.label}
              </button>
            ))}
          </div>

          {tab === "overview" && <OverviewTab lesson={lesson.lesson} />}
          {tab === "qa" && <QaTab lessonId={lessonId} questions={questions} setQuestions={setQuestions} />}
          {tab === "resources" && <ResourcesTab lesson={lesson.lesson} />}
          {tab === "quiz" && <QuizTab quizzes={lesson.quizzes} />}
          {tab === "challenge" && <ChallengeTab challenges={lesson.challenges} />}
        </div>
      </div>
    </div>
  );
}

function OverviewTab({ lesson }) {
  return (
    <div>
      <p className="whitespace-pre-wrap text-sm leading-relaxed text-slate-300">{lesson.explainerMd || "No written overview for this lesson yet."}</p>
      {lesson.keyTerms?.length > 0 && (
        <div className="mt-5 flex flex-wrap gap-2">
          {lesson.keyTerms.map((t, i) => (
            <Badge key={i} tone="brand">{t.en} — {t.km}</Badge>
          ))}
        </div>
      )}
      {lesson.codeExamples?.map((ex, i) => (
        <div key={i} className="mt-4">
          <div className="mb-1 text-xs font-medium text-slate-500">{ex.caption}</div>
          <pre className="overflow-x-auto rounded-xl bg-black/40 p-4 text-sm text-emerald-300"><code>{ex.code}</code></pre>
        </div>
      ))}
    </div>
  );
}

function ResourcesTab({ lesson }) {
  if (!lesson.resourcesMd) {
    return <EmptyState icon="📎" title="No resources linked" subtitle="Your teacher hasn't added extra resources for this lesson." />;
  }
  // Minimal markdown: render "- [label](url)" lines as links, everything else as text.
  const lines = lesson.resourcesMd.split("\n").filter(Boolean);
  return (
    <ul className="space-y-2 text-sm">
      {lines.map((line, i) => {
        const match = line.match(/\[(.+?)\]\((.+?)\)/);
        if (match) {
          return (
            <li key={i}>
              <a href={match[2]} target="_blank" rel="noreferrer" className="text-brand-300 hover:underline">
                🔗 {match[1]}
              </a>
            </li>
          );
        }
        return <li key={i} className="text-slate-300">{line.replace(/^-\s*/, "")}</li>;
      })}
    </ul>
  );
}

function QaTab({ lessonId, questions, setQuestions }) {
  const [text, setText] = useState("");
  const topLevel = questions.filter((q) => !q.parentId);
  const repliesFor = (id) => questions.filter((q) => q.parentId === id);

  const ask = async () => {
    if (!text.trim()) return;
    const q = await api.post(`/api/lessons/${lessonId}/questions`, { text: text.trim(), parentId: null });
    setQuestions((qs) => [...qs, q]);
    setText("");
  };

  return (
    <div>
      <div className="flex gap-2">
        <input className="input" placeholder="Ask a question about this lesson…" value={text} onChange={(e) => setText(e.target.value)} onKeyDown={(e) => e.key === "Enter" && ask()} />
        <button onClick={ask} className="btn-primary shrink-0 px-4 text-xs">Post</button>
      </div>

      <div className="mt-4 space-y-4">
        {topLevel.length === 0 ? (
          <EmptyState icon="💬" title="No questions yet" subtitle="Be the first to ask about this lesson." />
        ) : (
          topLevel.map((q) => (
            <div key={q.id} className="rounded-xl border border-surface-border p-3">
              <div className="flex items-center gap-2 text-sm font-medium text-white">{q.authorName}</div>
              <p className="mt-1 text-sm text-slate-300">{q.text}</p>
              {repliesFor(q.id).map((r) => (
                <div key={r.id} className="mt-2 ml-4 border-l border-surface-border pl-3">
                  <div className="text-xs font-medium text-brand-300">{r.authorName}</div>
                  <p className="mt-0.5 text-sm text-slate-300">{r.text}</p>
                </div>
              ))}
            </div>
          ))
        )}
      </div>
    </div>
  );
}

function QuizTab({ quizzes }) {
  return (
    <div className="space-y-6">
      {quizzes.map((q) => (
        <QuizQuestion key={q.id} quiz={q} />
      ))}
    </div>
  );
}

function QuizQuestion({ quiz }) {
  const [selected, setSelected] = useState(null);
  const [result, setResult] = useState(null);
  const [busy, setBusy] = useState(false);

  const submit = async () => {
    if (selected === null) return;
    setBusy(true);
    try {
      const res = await api.post(`/api/student/quiz/${quiz.id}/submit`, selected);
      setResult(res);
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="rounded-xl border border-surface-border p-4">
      <p className="text-sm font-medium text-white">{quiz.questionKm}</p>
      <div className="mt-3 space-y-2">
        {quiz.options.map((opt, i) => (
          <button
            key={i}
            onClick={() => setSelected(i)}
            disabled={!!result}
            className={`flex w-full items-center rounded-lg border px-3 py-2 text-left text-sm transition ${
              selected === i ? "border-brand-400 bg-brand-500/10 text-brand-200" : "border-surface-border text-slate-300 hover:bg-white/5"
            } ${result && i === result.correctIndex ? "border-emerald-500 bg-emerald-500/10 text-emerald-300" : ""}`}
          >
            {opt}
          </button>
        ))}
      </div>
      {!result ? (
        <button onClick={submit} disabled={selected === null || busy} className="btn-primary mt-3 px-4 py-2 text-xs">
          Submit answer
        </button>
      ) : (
        <div className={`mt-3 rounded-lg p-3 text-sm ${result.isCorrect ? "bg-emerald-500/10 text-emerald-300" : "bg-rose-500/10 text-rose-300"}`}>
          {result.isCorrect ? `Correct! +${result.xpAwarded} XP` : "Not quite."} {result.explanationKm}
        </div>
      )}
    </div>
  );
}

function ChallengeTab({ challenges }) {
  return (
    <div className="space-y-6">
      {challenges.map((c) => (
        <ChallengeCard key={c.id} challenge={c} />
      ))}
    </div>
  );
}

function ChallengeCard({ challenge }) {
  const [code, setCode] = useState(challenge.starterCode);
  const [output, setOutput] = useState("");
  const [result, setResult] = useState(null);
  const [busy, setBusy] = useState(false);

  const submit = async () => {
    setBusy(true);
    try {
      const res = await api.post(`/api/student/challenge/${challenge.id}/submit`, output);
      setResult(res);
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="rounded-xl border border-surface-border p-4">
      <p className="text-sm font-medium text-white">{challenge.promptKm}</p>
      <pre className="mt-3 overflow-x-auto rounded-xl bg-black/40 p-4 text-sm text-slate-300"><code>{code}</code></pre>
      <label className="label mt-3">Your predicted output</label>
      <input className="input" value={output} onChange={(e) => setOutput(e.target.value)} placeholder="Type the exact console output…" />
      <button onClick={submit} disabled={busy} className="btn-primary mt-3 px-4 py-2 text-xs">
        Check output
      </button>
      {result && (
        <div className={`mt-3 rounded-lg p-3 text-sm ${result.allPassed ? "bg-emerald-500/10 text-emerald-300" : "bg-rose-500/10 text-rose-300"}`}>
          {result.allPassed ? "All test cases passed! 🎉" : "Not quite right — check your output and try again."}
        </div>
      )}
    </div>
  );
}
