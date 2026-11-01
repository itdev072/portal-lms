import { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import { api } from "../../lib/api";
import { Spinner, Badge } from "../../components/ui/Primitives";

export default function StudentCourse() {
  const { id } = useParams();
  const [data, setData] = useState(null);
  const [activeLessonId, setActiveLessonId] = useState(null);

  useEffect(() => {
    api.get(`/api/courses/${id}`).then((d) => {
      setData(d);
      setActiveLessonId(d.lessons[0]?.id);
    });
  }, [id]);

  if (!data) {
    return (
      <div className="flex justify-center py-20 text-brand-400">
        <Spinner className="h-8 w-8" />
      </div>
    );
  }

  return (
    <div>
      <Link to="/student" className="text-sm text-slate-400 hover:text-white">← Back to dashboard</Link>
      <h1 className="mt-3 font-display text-2xl font-bold text-white">{data.course.thumbnailEmoji} {data.course.nameEn}</h1>

      <div className="mt-6 grid grid-cols-1 gap-6 lg:grid-cols-[280px_1fr]">
        <div className="card h-fit p-2">
          {data.lessons.map((l, i) => (
            <button
              key={l.id}
              onClick={() => setActiveLessonId(l.id)}
              className={`flex w-full items-center gap-3 rounded-xl p-3 text-left text-sm transition ${
                activeLessonId === l.id ? "bg-brand-500/15 text-brand-300" : "text-slate-300 hover:bg-white/5"
              }`}
            >
              <span className="flex h-7 w-7 shrink-0 items-center justify-center rounded-lg bg-white/5 text-xs font-semibold">{i + 1}</span>
              <span className="truncate">{l.titleEn}</span>
            </button>
          ))}
        </div>

        {activeLessonId && <LessonViewer courseId={id} lessonId={activeLessonId} />}
      </div>
    </div>
  );
}

function LessonViewer({ courseId, lessonId }) {
  const [lesson, setLesson] = useState(null);
  const [tab, setTab] = useState("learn");

  useEffect(() => {
    setLesson(null);
    api.get(`/api/courses/${courseId}/lessons/${lessonId}`).then(setLesson);
  }, [courseId, lessonId]);

  if (!lesson) {
    return (
      <div className="card flex justify-center p-10 text-brand-400">
        <Spinner className="h-6 w-6" />
      </div>
    );
  }

  const tabs = [
    { key: "learn", label: "📖 Learn" },
    ...(lesson.quizzes.length ? [{ key: "quiz", label: "🧠 Quiz" }] : []),
    ...(lesson.challenges.length ? [{ key: "challenge", label: "💻 Challenge" }] : []),
  ];

  return (
    <div className="card">
      <div className="border-b border-surface-border p-5">
        <h2 className="font-display text-lg font-semibold text-white">{lesson.lesson.titleEn}</h2>
        <div className="mt-1 text-xs text-slate-500">{lesson.lesson.estimatedMinutes} min · {lesson.lesson.xpValue} XP</div>
        <div className="mt-4 flex gap-2">
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
      </div>
      <div className="p-5">
        {tab === "learn" && <LearnTab lesson={lesson.lesson} />}
        {tab === "quiz" && <QuizTab quizzes={lesson.quizzes} />}
        {tab === "challenge" && <ChallengeTab challenges={lesson.challenges} />}
      </div>
    </div>
  );
}

function LearnTab({ lesson }) {
  return (
    <div>
      <p className="whitespace-pre-wrap text-sm leading-relaxed text-slate-300">{lesson.explainerMd}</p>
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
