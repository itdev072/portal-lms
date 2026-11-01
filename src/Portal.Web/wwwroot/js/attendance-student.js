// Asks the student's own phone for its current GPS location (never trusts a hidden
// form field for this) and submits it to the server, which is the only place the
// present/out-of-range decision is actually made (GeoFenceService + AttendanceService).
(function () {
    const btn = document.getElementById("checkin-button");
    const status = document.getElementById("checkin-status");
    const form = document.getElementById("checkin-form");
    const latInput = document.getElementById("checkin-lat");
    const lngInput = document.getElementById("checkin-lng");

    if (!btn || !form) return;

    btn.addEventListener("click", function () {
        if (!("geolocation" in navigator)) {
            status.textContent = "This browser can't share location. Try a phone with GPS enabled.";
            return;
        }

        btn.disabled = true;
        status.textContent = "Getting your location...";

        navigator.geolocation.getCurrentPosition(
            function (position) {
                latInput.value = position.coords.latitude;
                lngInput.value = position.coords.longitude;
                status.textContent = "Location found, checking in...";
                form.submit();
            },
            function (error) {
                btn.disabled = false;
                status.textContent = "Couldn't get your location (" + error.message + "). Location must be allowed to check in.";
            },
            { enableHighAccuracy: true, timeout: 15000 }
        );
    });
})();
