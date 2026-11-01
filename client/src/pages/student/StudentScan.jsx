import { useEffect, useRef, useState } from "react";
import { Html5Qrcode } from "html5-qrcode";
import { api } from "../../lib/api";
import { PageHeader, Card, Alert } from "../../components/ui/Primitives";

const SCANNER_ID = "qr-scanner-region";

export default function StudentScan() {
  const scannerRef = useRef(null);
  const [status, setStatus] = useState("idle"); // idle | scanning | checking | success | error
  const [message, setMessage] = useState("");
  const [cameraError, setCameraError] = useState("");

  useEffect(() => {
    const scanner = new Html5Qrcode(SCANNER_ID);
    scannerRef.current = scanner;

    scanner
      .start(
        { facingMode: "environment" },
        { fps: 10, qrbox: { width: 220, height: 220 } },
        onScanSuccess,
        () => {} // per-frame "nothing found" noise — ignore
      )
      .then(() => setStatus("scanning"))
      .catch((err) => setCameraError(String(err?.message || err)));

    return () => {
      // html5-qrcode's stop()/clear() throw synchronously (not a rejected promise) when
      // the scanner never reached a running state — e.g. the component unmounted before
      // the camera permission prompt resolved. Guard every step defensively.
      try {
        const maybePromise = scanner.stop();
        Promise.resolve(maybePromise)
          .catch(() => {})
          .finally(() => {
            try { scanner.clear(); } catch { /* already cleared / never started */ }
          });
      } catch {
        try { scanner.clear(); } catch { /* already cleared / never started */ }
      }
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const onScanSuccess = async (decodedText) => {
    if (status === "checking") return; // avoid double-fires while we're mid check-in
    setStatus("checking");
    setMessage("");

    const token = decodedText.trim();

    if (!navigator.geolocation) {
      setStatus("error");
      setMessage("Your device doesn't support location services, which this check-in needs.");
      return;
    }

    navigator.geolocation.getCurrentPosition(
      async (pos) => {
        try {
          const res = await api.post(`/api/attendance/check-in/${encodeURIComponent(token)}`, {
            latitude: pos.coords.latitude,
            longitude: pos.coords.longitude,
          });
          if (res.status === "Present") {
            setStatus("success");
            setMessage("You're marked present! 🎉");
          } else {
            setStatus("error");
            setMessage(describeStatus(res.status, res.distanceMeters));
          }
        } catch (err) {
          setStatus("error");
          setMessage(err.message || "Check-in failed.");
        }
      },
      () => {
        setStatus("error");
        setMessage("Location permission was denied — allow location access to check in.");
      },
      { enableHighAccuracy: true, timeout: 10000 }
    );
  };

  const scanAgain = async () => {
    setStatus("scanning");
    setMessage("");
  };

  return (
    <div>
      <PageHeader title="Scan attendance" subtitle="Point your camera at the QR code your teacher is showing." />

      <Card className="mx-auto max-w-md">
        {cameraError && <Alert>Couldn't start the camera: {cameraError}. Check your browser's camera permission for this site.</Alert>}

        <div id={SCANNER_ID} className={`overflow-hidden rounded-xl ${status === "checking" || status === "success" || status === "error" ? "hidden" : ""}`} />

        {status === "checking" && (
          <div className="flex flex-col items-center gap-3 py-10 text-slate-300">
            <div className="h-10 w-10 animate-spin rounded-full border-2 border-brand-400 border-t-transparent" />
            Checking you in…
          </div>
        )}

        {status === "success" && (
          <div className="flex flex-col items-center gap-3 py-10 animate-fadeUp">
            <div className="flex h-20 w-20 items-center justify-center rounded-full bg-emerald-500/15 text-4xl">✅</div>
            <div className="text-lg font-semibold text-emerald-300">{message}</div>
          </div>
        )}

        {status === "error" && (
          <div className="flex flex-col items-center gap-3 py-10">
            <div className="flex h-20 w-20 items-center justify-center rounded-full bg-rose-500/15 text-4xl">⚠️</div>
            <div className="text-center text-sm text-rose-300">{message}</div>
            <button onClick={scanAgain} className="btn-primary mt-2">Scan again</button>
          </div>
        )}
      </Card>
    </div>
  );
}

function describeStatus(status, distanceMeters) {
  switch (status) {
    case "OutOfRange":
      return `You're too far from the classroom to check in (${Math.round(distanceMeters)}m away). Get closer and scan again.`;
    case "Expired":
      return "This QR code has expired — ask your teacher to open a new session.";
    case "InvalidSession":
      return "That code isn't recognized — ask your teacher to open a new session.";
    default:
      return "Check-in didn't succeed — try scanning again.";
  }
}
