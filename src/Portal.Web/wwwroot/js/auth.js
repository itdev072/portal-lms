// Password show/hide toggle for the auth screens. Pure vanilla JS, no deps.
document.querySelectorAll(".password-toggle").forEach(function (btn) {
    btn.addEventListener("click", function () {
        var input = document.getElementById(btn.dataset.target);
        if (!input) return;
        var showing = input.type === "text";
        input.type = showing ? "password" : "text";
        btn.querySelector(".icon-eye").classList.toggle("d-none", !showing);
        btn.querySelector(".icon-eye-off").classList.toggle("d-none", showing);
    });
});
