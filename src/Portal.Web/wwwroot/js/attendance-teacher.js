// Renders the attendance QR code client-side (the server never generates an image file
// for it — see README "Why no QRCoder NuGet package"). The canvas just encodes the
// check-in URL for the current session token; the server decides present/out-of-range
// when the student's phone later posts its own GPS location to that URL.
(function () {
    const canvas = document.getElementById("attendance-qr");
    if (!canvas) return;

    const url = canvas.getAttribute("data-checkin-url");
    if (!url || typeof QRCode === "undefined") return;

    QRCode.toCanvas(canvas, url, { width: 260, margin: 2 }, function (error) {
        if (error) {
            console.error("QR render failed:", error);
            const fallback = document.getElementById("attendance-qr-fallback");
            if (fallback) fallback.style.display = "block";
        }
    });

    // Live-ish refresh of the check-in list without a full page reload.
    const list = document.getElementById("attendance-record-list");
    if (list) {
        setInterval(() => {
            window.location.reload();
        }, 15000);
    }
})();
