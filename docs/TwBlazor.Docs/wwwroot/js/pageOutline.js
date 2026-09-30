// Highlights the "On this page" link for the section currently being read.
globalThis.pageOutline = {
    // Distance from the top of the viewport at which a section counts as the one being read.
    offset: 140,

    stop: null,

    observe: function (ids) {
        this.dispose();

        const offset = this.offset;

        const update = (event) => {
            const scroller = event?.target instanceof Element ? event.target : null;
            const atBottom = scroller !== null &&
                scroller.scrollTop > 0 &&
                scroller.scrollTop + scroller.clientHeight >= scroller.scrollHeight - 2;

            let active = ids[0];

            for (const id of ids) {
                const section = document.getElementById(id);

                if (section && section.getBoundingClientRect().top <= offset) {
                    active = id;
                }
            }

            if (atBottom) {
                active = ids[ids.length - 1];
            }

            for (const link of document.querySelectorAll("[data-outline-id]")) {
                if (link.dataset.outlineId === active) {
                    link.setAttribute("aria-current", "location");
                } else {
                    link.removeAttribute("aria-current");
                }
            }
        };

        // Scroll events don't bubble, and the docs content scrolls inside its own container rather
        // than the window, so listen in the capture phase on the document.
        document.addEventListener("scroll", update, { capture: true, passive: true });
        update();

        this.stop = () => document.removeEventListener("scroll", update, { capture: true });
    },

    dispose: function () {
        if (this.stop) {
            this.stop();
            this.stop = null;
        }
    }
};
