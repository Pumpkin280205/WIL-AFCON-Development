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

// ---- Feedback: success / error toasts ----

function hideToast(toast) {
    if (!toast || toast.classList.contains("is-hiding")) {
        return;
    }

    toast.classList.add("is-hiding");
    setTimeout(function () { toast.remove(); }, 200);
}

document.addEventListener("click", function (event) {
    var button = event.target.closest("[data-action='dismiss-toast']");
    if (button) {
        hideToast(button.closest(".toast-message"));
    }
});

// Success messages fade out after a few seconds; errors stay until dismissed.
// The timer pauses while the message is hovered or focused so it can be read.
document.querySelectorAll(".toast-message[data-auto-hide='true']").forEach(function (toast) {
    var timer;

    function start() { timer = setTimeout(function () { hideToast(toast); }, 6000); }
    function stop() { clearTimeout(timer); }

    toast.addEventListener("mouseenter", stop);
    toast.addEventListener("mouseleave", start);
    toast.addEventListener("focusin", stop);
    toast.addEventListener("focusout", start);

    start();
});

// Escape closes any open message
document.addEventListener("keydown", function (event) {
    if (event.key === "Escape") {
        document.querySelectorAll(".toast-message").forEach(hideToast);
    }
});

// ---- Feedback: loading state on submitted forms ----
// Shows a spinner on the clicked button and blocks double submits, so a
// slow request cannot create the same record twice. Only POST forms that
// passed validation; add data-no-loading to a form to opt out.

document.addEventListener("submit", function (event) {
    var form = event.target;

    if (event.defaultPrevented ||
        form.method.toLowerCase() !== "post" ||
        form.hasAttribute("data-no-loading")) {
        return;
    }

    if (form.dataset.submitting === "true") {
        event.preventDefault();
        return;
    }

    form.dataset.submitting = "true";
    form.setAttribute("aria-busy", "true");

    var submitter = event.submitter ||
        form.querySelector("button[type='submit'], button:not([type]), input[type='submit']");

    if (submitter && submitter.tagName === "BUTTON") {
        submitter.classList.add("is-loading");
        submitter.insertAdjacentHTML("afterbegin", "<span class='btn-spinner' aria-hidden='true'></span>");
    }

    // Disable after the browser has read the form, so the clicked button's
    // formaction (Accept / Reject) still applies
    setTimeout(function () {
        form.querySelectorAll("button[type='submit'], button:not([type]), input[type='submit']")
            .forEach(function (button) {
                if (!button.disabled) {
                    button.disabled = true;
                    button.dataset.loadingDisabled = "true";
                }
            });
    }, 0);
});

// Undo the loading state if the page is restored from the back/forward cache
window.addEventListener("pageshow", function (event) {
    if (!event.persisted) {
        return;
    }

    document.querySelectorAll("form[data-submitting='true']").forEach(function (form) {
        delete form.dataset.submitting;
        form.removeAttribute("aria-busy");
        form.querySelectorAll(".btn-spinner").forEach(function (spinner) { spinner.remove(); });
        form.querySelectorAll(".is-loading").forEach(function (button) {
            button.classList.remove("is-loading");
        });
        // Only re-enable buttons this script disabled (some start disabled on purpose)
        form.querySelectorAll("[data-loading-disabled='true']").forEach(function (button) {
            button.disabled = false;
            delete button.dataset.loadingDisabled;
        });
    });
});
