// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Print buttons (kept out of inline onclick handlers so the
// Content-Security-Policy can block all inline script).
document.addEventListener("click", function (event) {
    if (event.target.closest("[data-action='print']")) {
        window.print();
    }
});
