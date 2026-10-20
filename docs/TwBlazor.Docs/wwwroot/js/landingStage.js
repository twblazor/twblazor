// Starts the landing page's "live build" panels when they scroll into view.
// A panel is any element with data-stage. Until it is marked .is-live its lines and components wait, hidden.
// Without this script, or with reduced motion, nothing is hidden and every panel shows its finished state.
globalThis.landingStage = {
    observer: null,

    observe: function (root) {
        root.querySelectorAll("[data-stage]:not([data-stage-observed])").forEach((stage) => {
            stage.setAttribute("data-stage-observed", "");
            this.observer.observe(stage);
        });
    },

    init: function () {
        if (!("IntersectionObserver" in globalThis) ||
            globalThis.matchMedia("(prefers-reduced-motion: reduce)").matches) {
            return;
        }

        document.documentElement.classList.add("stage-js");

        this.observer = new IntersectionObserver((entries) => {
            entries.forEach((entry) => {
                if (entry.isIntersecting) {
                    entry.target.classList.add("is-live");
                    this.observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.35 });

        this.observe(document);

        // Blazor adds panels after this script runs: on navigation to the page and when a panel is replayed.
        new MutationObserver(() => this.observe(document))
            .observe(document.body, { childList: true, subtree: true });
    }
};

landingStage.init();
