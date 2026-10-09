// Generic picker (used by datepicker, timepicker, etc.)
globalThis.twPicker = {
    // Marks root as already closing (or not) so this and twPicker.registerScrollReposition's
    // close-on-scroll path - two entirely independent listeners that can both decide to close the
    // same picker around the same moment (e.g. a scroll gesture that also ends in a pointerup
    // outside the panel) - never both invoke Close() for the same open. The second call, whichever
    // it is, would invoke it on a dotnetRef the first call already disposed server-side, throwing
    // "no tracked object" - checking/setting this first makes sure only one of them ever does.
    _setClosing: function (root) {
        if (!root || root.__twPickerClosing) return false;
        root.__twPickerClosing = true;
        return true;
    },

    // Calls back into .NET without letting a stale reference surface as an unhandled rejection. The
    // server may already have disposed the DotNetObjectReference (the panel closed by another path
    // while this listener was still queued), in which case invokeMethodAsync rejects asynchronously
    // and a surrounding try/catch would never see it.
    _invokeSafely: function (dotnetRef, method, ...args) {
        try {
            return Promise.resolve(dotnetRef.invokeMethodAsync(method, ...args)).catch(function (err) {
                console.debug('twPicker .NET callback skipped (reference already released)', method, err);
            });
        } catch (err) {
            console.error('twPicker .NET callback error', err);
            return Promise.resolve();
        }
    },

    // Max pointer travel (px) between down/up to still count as a tap, not a scroll/drag.
    // Reacting on pointerdown alone would close the panel as soon as a touch-scroll begins
    // outside it (breaks scrolling TwDateRangePicker's tall mobile panel), so we wait for
    // pointerup and check distance travelled instead.
    registerOutsideClick: function (root, dotnetRef) {
        if (!root) return;

        root.__twPickerClosing = false;

        const MOVE_THRESHOLD_PX = 10;
        let downTarget = null;
        let downX = 0;
        let downY = 0;

        const onPointerDown = function (e) {
            downTarget = e.target;
            downX = e.clientX ?? 0;
            downY = e.clientY ?? 0;
        };

        const onPointerUp = function (e) {
            if (downTarget === null) return;
            const target = downTarget;
            const dx = Math.abs((e.clientX ?? downX) - downX);
            const dy = Math.abs((e.clientY ?? downY) - downY);
            downTarget = null;

            if (dx > MOVE_THRESHOLD_PX || dy > MOVE_THRESHOLD_PX) return;

            try {
                if (!root.contains(target) && globalThis.twPicker._setClosing(root)) {
                    const closing = globalThis.twPicker._invokeSafely(dotnetRef, 'Close');
                    // While a panel is open everything outside it is inert, so the browser hit-tests
                    // this tap onto a non-inert ancestor and the control actually under the pointer
                    // (e.g. a dialog's Save button) never receives the click: the first tap would
                    // only close the panel. Once Close has lifted the inert state, replay the tap
                    // on whatever is really there.
                    const upX = e.clientX ?? downX;
                    const upY = e.clientY ?? downY;
                    void Promise.resolve(closing).finally(() => {
                        if (typeof document.elementFromPoint !== 'function') return;
                        const intended = document.elementFromPoint(upX, upY);
                        if (intended && intended !== target && target.isConnected && target.contains(intended)) {
                            intended.click();
                        }
                    });
                }
            } catch (err) {
                console.error('twPicker handler error', err);
            }
        };

        const onPointerCancel = function () {
            downTarget = null;
        };

        root.__twPickerPointerDown = onPointerDown;
        root.__twPickerPointerUp = onPointerUp;
        root.__twPickerPointerCancel = onPointerCancel;
        document.addEventListener('pointerdown', onPointerDown);
        document.addEventListener('pointerup', onPointerUp);
        document.addEventListener('pointercancel', onPointerCancel);
    },

    unregisterOutsideClick: function (root) {
        if (!root) return;
        const onPointerDown = root.__twPickerPointerDown;
        const onPointerUp = root.__twPickerPointerUp;
        const onPointerCancel = root.__twPickerPointerCancel;
        if (onPointerDown) document.removeEventListener('pointerdown', onPointerDown);
        if (onPointerUp) document.removeEventListener('pointerup', onPointerUp);
        if (onPointerCancel) document.removeEventListener('pointercancel', onPointerCancel);
        try {
            delete root.__twPickerPointerDown;
            delete root.__twPickerPointerUp;
            delete root.__twPickerPointerCancel;
        } catch {
            root.__twPickerPointerDown = undefined;
            root.__twPickerPointerUp = undefined;
            root.__twPickerPointerCancel = undefined;
        }
    },

    // Whether a scroll event came from the panel or something inside it. Resize events target the window,
    // which is not a Node, so they are never "within" the panel.
    _isScrollWithinPanel: function (panel, e) {
        const target = e?.target;
        return target instanceof Node && panel.contains(target);
    },

    // Gap (px) kept between a clamped panel edge and the viewport edge - mirrors the 0.5rem
    // margin already used below.
    _panelEdgeGapPx: 8,

    // Flips a just-opened popover panel away from whichever viewport edge it would overflow,
    // instead of clipping off-screen. Only overrides the panel's default "top-full left-0" style
    // on the axis that actually overflows, so panels that already fit are untouched. Re-run on
    // every reopen since available space may have changed (scroll, trigger move, resize).
    positionPanel: function (panel) {
        if (!panel) return;

        panel.style.left = '';
        panel.style.right = '';
        panel.style.top = '';
        panel.style.bottom = '';
        panel.style.marginTop = '';
        panel.style.marginBottom = '';
        panel.style.maxHeight = '';

        const rect = panel.getBoundingClientRect();
        const viewportWidth = document.documentElement.clientWidth;
        // Prefer visualViewport.height: clientHeight doesn't shrink when a mobile keyboard opens,
        // which would wrongly report room that's actually covered. Falls back where unavailable
        // (jsdom, older browsers).
        const viewportHeight = (typeof window !== 'undefined' && window.visualViewport)
            ? window.visualViewport.height
            : document.documentElement.clientHeight;

        if (rect.right > viewportWidth) {
            panel.style.left = 'auto';
            panel.style.right = '0';
        }

        // Both measured from the panel's un-flipped top edge (rect.top) so they're comparable.
        // Using rect.bottom for spaceBelow was a past bug: it grows with panel height, so a tall
        // panel could make it deeply negative and wrongly flip upward even with little room above.
        const spaceAbove = rect.top;
        const spaceBelow = viewportHeight - rect.top;

        // Only flip upward if that actually fits better, else flipping just clips the other edge.
        const flipUp = rect.bottom > viewportHeight && spaceAbove > spaceBelow;

        if (flipUp) {
            panel.style.top = 'auto';
            panel.style.bottom = '100%';
            panel.style.marginTop = '0';
            panel.style.marginBottom = '0.5rem';
        }

        // Clamp height to whichever direction was picked, since a static CSS max-height can't know
        // that in advance and mobile "100vh" doesn't shrink for the on-screen keyboard. Only
        // applied when it would actually constrain the panel.
        const availableSpace = (flipUp ? spaceAbove : spaceBelow) - globalThis.twPicker._panelEdgeGapPx;
        if (availableSpace > 0 && rect.height > availableSpace) {
            panel.style.maxHeight = availableSpace + 'px';
        }
    },

    // Same flip/clamp job as positionPanel, but for `position: fixed` panels (used by every
    // popover-style picker). Unlike absolute, fixed has no inherent relationship to its trigger,
    // so this takes the trigger ("anchor") explicitly and computes coordinates from scratch. Payoff:
    // a fixed panel isn't clipped by an ancestor's overflow (e.g. TwDialog's scrollable body).
    //
    // matchAnchorWidth (TwSelect only) sets the panel's width in px, since `w-full` would resolve
    // to 100% of the viewport instead of the trigger once the panel is fixed.
    positionPanelFixed: function (anchor, panel, matchAnchorWidth) {
        if (!anchor || !panel) return;

        panel.style.position = 'fixed';
        panel.style.left = '';
        panel.style.right = '';
        panel.style.top = '';
        panel.style.bottom = '';
        panel.style.maxHeight = '';

        const anchorRect = anchor.getBoundingClientRect();
        const viewportWidth = document.documentElement.clientWidth;
        const viewportHeight = (typeof window !== 'undefined' && window.visualViewport)
            ? window.visualViewport.height
            : document.documentElement.clientHeight;

        if (matchAnchorWidth) {
            panel.style.width = anchorRect.width + 'px';
        }

        // Place provisionally below the anchor's left edge, then measure natural size - a fixed
        // element has no rendered position to read until one is set.
        panel.style.top = anchorRect.bottom + 'px';
        panel.style.left = anchorRect.left + 'px';

        const rect = panel.getBoundingClientRect();

        if (rect.right > viewportWidth) {
            panel.style.left = 'auto';
            panel.style.right = Math.max(viewportWidth - anchorRect.right, 0) + 'px';
        }

        // Measured from the anchor's edges, not the panel's, so a tall panel can't skew which
        // direction looks like it has more room (same reasoning as positionPanel).
        const spaceAbove = anchorRect.top;
        const spaceBelow = viewportHeight - anchorRect.bottom;
        const flipUp = rect.bottom > viewportHeight && spaceAbove > spaceBelow;

        if (flipUp) {
            panel.style.top = 'auto';
            panel.style.bottom = (viewportHeight - anchorRect.top) + 'px';
        }

        const availableSpace = (flipUp ? spaceAbove : spaceBelow) - globalThis.twPicker._panelEdgeGapPx;
        if (availableSpace > 0 && rect.height > availableSpace) {
            panel.style.maxHeight = availableSpace + 'px';
        }
    },

    // Positions the panel itself (see positionPanelFixed), then keeps it correct for as long as it
    // stays open - the specifics of "correct" depend on platform, via the optional dotnetRef:
    //
    // - Desktop (dotnetRef supplied, non-touch platform): a page scroll or window resize closes the
    //   panel instead of chasing the trigger around the viewport. Scrolling the page away from the
    //   field being edited reads as the user moving on, not as "please keep the panel glued to it".
    // - Mobile, or no dotnetRef supplied: keeps repositioning the panel on every scroll/resize so it
    //   stays glued to its trigger - there, a page scroll is how touch users reveal more of a tall
    //   panel that doesn't fit the viewport (e.g. TwDateRangePicker's two-month day view), not a
    //   "moved on" signal. Also self-heals if the inline position ever gets cleared out from under
    //   it (see the MutationObserver below).
    //
    // Capture-phase on the scroll listener to catch scrolls on any scrollable ancestor, not just
    // ones that bubble to window.
    //
    // The initial positioning call happens here, as this function's own first action, rather than
    // as a separate call immediately before it: two distinct JS interop round trips left a gap
    // between "position it" and "start watching for it being lost" - a Blazor re-render patching
    // this element in that gap (it doesn't know about a style this module set outside its own
    // render tree) could wipe the position right back to the panel's unpositioned default, with
    // nothing yet watching to catch it. Doing it here too closes that gap.
    registerScrollReposition: function (anchor, panel, matchAnchorWidth, dotnetRef) {
        if (!anchor || !panel || panel.__twPickerScrollHandler) return;

        globalThis.twPicker.positionPanelFixed(anchor, panel, matchAnchorWidth);

        const closeOnScroll = !!dotnetRef && !globalThis.twDevice.prefersNativePicker();

        if (closeOnScroll) {
            // A single scroll gesture fires many scroll events in a row, not just one. Removing
            // both listeners synchronously, before calling into .NET, means only the very first of
            // those events can ever trigger a close from *this* listener - without this, every
            // event after the first but before the (also async) unregisterScrollReposition round
            // trip lands would invoke Close() again on a dotnetRef the first call has already
            // disposed server-side, throwing "no tracked object" for each one.
            //
            // _setClosing(anchor) additionally guards against registerOutsideClick's entirely
            // separate pointerup listener deciding to close the same picker around the same moment
            // (e.g. a scroll gesture that also ends in a pointerup outside the panel) - see its
            // remarks for why both listeners need to share that check.
            const handler = function (e) {
                // A scroll inside the panel itself (a long list or a tall day view) is the user working
                // with it, not the page moving away from the field, so it must neither close the panel
                // nor drop this listener.
                if (panel.isConnected && globalThis.twPicker._isScrollWithinPanel(panel, e)) return;
                document.removeEventListener('scroll', handler, true);
                window.removeEventListener('resize', handler);
                delete panel.__twPickerScrollHandler;
                // A panel that is already gone was closed by another path, which has released the
                // .NET reference, so there is nothing left to close.
                if (!panel.isConnected) return;
                if (!globalThis.twPicker._setClosing(anchor)) return;
                void globalThis.twPicker._invokeSafely(dotnetRef, 'Close');
            };
            panel.__twPickerScrollHandler = handler;
            document.addEventListener('scroll', handler, true);
            window.addEventListener('resize', handler);
            return;
        }

        const handler = function (e) {
            if (globalThis.twPicker._isScrollWithinPanel(panel, e)) return;
            globalThis.twPicker.positionPanelFixed(anchor, panel, matchAnchorWidth);
        };

        panel.__twPickerScrollHandler = handler;
        document.addEventListener('scroll', handler, true);
        window.addEventListener('resize', handler);

        const styleObserver = new MutationObserver(function () {
            if (!panel.style.position) {
                handler();
            }
        });
        styleObserver.observe(panel, { attributes: true, attributeFilter: ['style'] });
        panel.__twPickerStyleObserver = styleObserver;
    },

    unregisterScrollReposition: function (panel) {
        if (!panel?.__twPickerScrollHandler) return;

        document.removeEventListener('scroll', panel.__twPickerScrollHandler, true);
        window.removeEventListener('resize', panel.__twPickerScrollHandler);
        delete panel.__twPickerScrollHandler;

        panel.__twPickerStyleObserver?.disconnect();
        delete panel.__twPickerStyleObserver;
    }
};

// code block
globalThis.twCodeBlock = {
    highlightElement: function (el) {
        hljs.highlightElement(el);
        // highlightElement bails out before adding this class when the language is unknown, which
        // would drop the theme's default code padding and background.
        el.classList.add('hljs');
    }
};

// Dialog accessibility helpers: initial focus, a Tab focus trap scoped to the dialog surface,
// background inert-ing while a dialog is open, and restoring focus to the triggering element on close.
globalThis.twDialog = {
    _focusMap: new Map(),

    _focusableSelector: 'a[href], area[href], input:not([disabled]):not([type="hidden"]), ' +
        'select:not([disabled]), textarea:not([disabled]), button:not([disabled]), ' +
        'iframe, object, embed, [contenteditable="true"], [tabindex]:not([tabindex="-1"])',

    getFocusableElements: function (container) {
        if (!container) return [];
        try {
            return Array.from(container.querySelectorAll(globalThis.twDialog._focusableSelector))
                .filter(function (el) {
                    return !el.hasAttribute('inert') && el.getClientRects().length > 0;
                });
        } catch (err) {
            console.error('twDialog.getFocusableElements error', err);
            return [];
        }
    },

    // Focuses the first focusable element in the dialog, falling back to the surface itself
    // (which carries tabindex="-1" for programmatic focus).
    focusSurface: function (surface) {
        if (!surface) return;
        const focusable = globalThis.twDialog.getFocusableElements(surface);
        if (focusable.length > 0) {
            focusable[0].focus();
        } else if (typeof surface.focus === 'function') {
            surface.focus();
        }
    },

    // Traps Tab/Shift+Tab within the dialog surface so focus can't leave it while open.
    trapFocus: function (surface) {
        if (!surface || surface.__twDialogTrapHandler) return;
        const handler = function (e) {
            if (e.key !== 'Tab') return;

            const focusable = globalThis.twDialog.getFocusableElements(surface);
            if (focusable.length === 0) {
                e.preventDefault();
                if (typeof surface.focus === 'function') surface.focus();
                return;
            }

            const first = focusable[0];
            const last = focusable.at(-1);
            const active = document.activeElement;

            if (e.shiftKey) {
                if (active === first || !surface.contains(active)) {
                    e.preventDefault();
                    last.focus();
                }
            } else if (active === last || !surface.contains(active)) {
                e.preventDefault();
                first.focus();
            }
        };
        surface.__twDialogTrapHandler = handler;
        surface.addEventListener('keydown', handler);
    },

    releaseFocusTrap: function (surface) {
        if (!surface?.__twDialogTrapHandler) return;
        surface.removeEventListener('keydown', surface.__twDialogTrapHandler);
        delete surface.__twDialogTrapHandler;
    },

    // Marks everything outside `exceptEl` inert so it can't be reached while a dialog is open.
    // Walks up from exceptEl to <body>, inert-ing siblings at every level - needed because most
    // app hosts wrap everything in one root div, so only checking body's direct children would
    // find nothing to inert. Tags what it touched so clearBackgroundInert can undo precisely that.
    setBackgroundInert: function (exceptEl) {
        if (!exceptEl || !document.body) return;
        let current = exceptEl;
        while (current && current !== document.body && current.parentElement) {
            const parent = current.parentElement;
            Array.from(parent.children).forEach(function (sibling) {
                if (sibling === current) return;
                if (sibling.hasAttribute('inert')) return;
                sibling.setAttribute('inert', '');
                sibling.dataset.twDialogInert = 'true';
            });
            current = parent;
        }
    },

    clearBackgroundInert: function () {
        if (!document.body) return;
        document.body.querySelectorAll('[data-tw-dialog-inert="true"]').forEach(function (el) {
            el.removeAttribute('inert');
            delete el.dataset.twDialogInert;
        });
    },

    // Records the focused element under an opaque token for later refocus (.NET can't hold a
    // raw DOM element reference).
    captureFocus: function () {
        const active = document.activeElement;
        if (!active || active === document.body) return null;

        const randomPart = crypto.getRandomValues(new Uint32Array(2)).join('');
        const token = 'tw-focus-' + Date.now().toString(36) + '-' + randomPart;
        active.dataset.twFocusToken = token;
        globalThis.twDialog._focusMap.set(token, active);
        return token;
    },

    restoreFocus: function (token) {
        if (!token) return;
        let el = globalThis.twDialog._focusMap.get(token);
        if (!el || !document.body.contains(el)) {
            el = document.querySelector('[data-tw-focus-token="' + token + '"]');
        }

        globalThis.twDialog._focusMap.delete(token);

        if (el) {
            delete el.dataset.twFocusToken;
            // preventScroll: true - closing on scroll (see twPicker.registerScrollReposition's
            // desktop close-on-scroll path) restores focus to a trigger the user just deliberately
            // scrolled away from; a default .focus() would scroll the page right back to it,
            // undoing the very scroll that closed the panel.
            if (typeof el.focus === 'function') el.focus({ preventScroll: true });
        }
    }
};

// Custom role="slider" elements (e.g. TwColorPicker's strips) need Arrow/Home/End to change value
// without triggering native scroll. A blanket @onkeydown:preventDefault in Razor would also block
// Tab, trapping focus. This listener only preventDefaults the specific keys the slider handles,
// and doesn't stop propagation, so Blazor's own keydown handling still runs normally.
globalThis.twSlider = {
    _scrollKeys: ['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight', 'Home', 'End'],

    _scrollKeyHandler: function (e) {
        if (globalThis.twSlider._scrollKeys.includes(e.key)) {
            e.preventDefault();
        }
    },

    preventScrollKeys: function (el) {
        if (!el || el.__twSliderScrollHandler) return;
        el.__twSliderScrollHandler = globalThis.twSlider._scrollKeyHandler;
        el.addEventListener('keydown', globalThis.twSlider._scrollKeyHandler);
    }
};

// Tabs: prevents native scroll for the WAI-ARIA APG tablist navigation keys (arrows, Home, End)
// without ever preventDefault-ing Tab, whose default action (moving focus out) must stay intact.
// Runs alongside Blazor's own keydown binding (which does the actual tab-switching in C#); a
// static `@onkeydown:preventDefault="true"` can't express this selectivity since it'd hit every key.
globalThis.twTabs = {
    _navigationKeys: new Set(['ArrowRight', 'ArrowLeft', 'ArrowUp', 'ArrowDown', 'Home', 'End']),

    _tabsKeydownHandler: function (e) {
        if (globalThis.twTabs._navigationKeys.has(e.key)) {
            e.preventDefault();
        }
    },

    registerKeydownGuard: function (tablist) {
        if (!tablist || tablist.__twTabsKeydownHandler) return;
        tablist.__twTabsKeydownHandler = globalThis.twTabs._tabsKeydownHandler;
        tablist.addEventListener('keydown', globalThis.twTabs._tabsKeydownHandler);
    },

    unregisterKeydownGuard: function (tablist) {
        if (!tablist?.__twTabsKeydownHandler) return;
        tablist.removeEventListener('keydown', tablist.__twTabsKeydownHandler);
        delete tablist.__twTabsKeydownHandler;
    }
};

// Calendar: scrolls the Day/Week time grid to a starting position (e.g. 8am) once on mount.
// Expressed as a fraction of scrollHeight, not a fixed offset, so it stays correct regardless of
// slot height (TwCalendarTheme.SlotRow can be overridden) or root font size.
globalThis.twCalendar = {
    scrollToFraction: function (container, fraction) {
        if (!container) return;
        container.scrollTop = container.scrollHeight * fraction;
    }
};

// Device detection (used by pickers to defer to the platform's native input UI on mobile).
globalThis.twDevice = {
    getPlatform: function () {
        const nav = typeof navigator !== 'undefined' ? navigator : {};
        const userAgent = nav.userAgent || '';
        const platform = nav.platform || '';
        const maxTouchPoints = nav.maxTouchPoints || 0;

        // iPadOS 13+ reports a desktop Mac UA, so touch-capable "MacIntel" is treated as iOS.
        const isIPadOS = platform === 'MacIntel' && maxTouchPoints > 1;

        if (isIPadOS || /iPhone|iPad|iPod/.test(userAgent)) {
            return 'ios';
        }

        if (/Android/.test(userAgent)) {
            return 'android';
        }

        return 'other';
    },

    prefersNativePicker: function () {
        const platform = globalThis.twDevice.getPlatform();
        return platform === 'ios' || platform === 'android';
    }
};

// Sidebar: viewport check for whether the mobile drawer's focus trap and background inert-ing
// should be armed. Matches Tailwind's "lg" breakpoint (1024px) - below it the sidebar is a modal
// drawer; above it, a persistent panel beside usable page content, where trapping focus is wrong.
globalThis.twSidebar = {
    isMobileViewport: function () {
        try {
            return !window.matchMedia('(min-width: 1024px)').matches;
        } catch (err) {
            console.error('twSidebar.isMobileViewport error', err);
            return true;
        }
    },

    scrollToTop: function (el) {
        if (el) {
            el.scrollTop = 0;
        }
    }
};

// Select: keyboard navigation and highlighting for the desktop single-select listbox. Focus stays on
// the listbox itself and the highlighted option is exposed through aria-activedescendant (the ARIA
// listbox pattern), so this runs entirely client-side - a server round trip per arrow key would lag -
// and only calls back into .NET (SelectOption) once an option is committed.
globalThis.twSelect = {
    _typeaheadResetMs: 500,

    attachListbox: function (listbox, dotnetRef, selectedOptionId, commitMethod) {
        if (!listbox || listbox.__twSelectAttached) return;
        listbox.__twSelectAttached = true;

        const options = function () {
            return Array.from(listbox.querySelectorAll('[role="option"]'));
        };

        const setActive = function (option) {
            options().forEach(function (o) { delete o.dataset.active; });
            if (!option) {
                listbox.removeAttribute('aria-activedescendant');
                return;
            }
            option.dataset.active = 'true';
            listbox.setAttribute('aria-activedescendant', option.id);
            option.scrollIntoView({ block: 'nearest' });
        };

        const activeIndex = function () {
            return options().findIndex(function (o) { return o.dataset.active === 'true'; });
        };

        const commit = function (option) {
            if (!option) return;
            void globalThis.twPicker._invokeSafely(dotnetRef, commitMethod || 'SelectOption', Number.parseInt(option.dataset.value, 10));
        };

        let typeahead = '';
        let typeaheadTimer;

        listbox.addEventListener('keydown', function (e) {
            const all = options();
            if (all.length === 0) return;
            const current = activeIndex();

            switch (e.key) {
                case 'ArrowDown':
                    setActive(all[Math.min(current + 1, all.length - 1)]);
                    break;
                case 'ArrowUp':
                    setActive(all[Math.max(current - 1, 0)]);
                    break;
                case 'Home':
                    setActive(all[0]);
                    break;
                case 'End':
                    setActive(all.at(-1));
                    break;
                case 'Enter':
                case ' ':
                    commit(all[current]);
                    break;
                default: {
                    if (e.key.length !== 1 || e.ctrlKey || e.metaKey || e.altKey) return;
                    typeahead += e.key.toLowerCase();
                    clearTimeout(typeaheadTimer);
                    typeaheadTimer = setTimeout(function () { typeahead = ''; }, globalThis.twSelect._typeaheadResetMs);
                    // Searching from just after the current option lets repeated presses of one letter cycle.
                    const ordered = all.slice(current + 1).concat(all.slice(0, current + 1));
                    const match = ordered.find(function (o) {
                        return o.textContent.trim().toLowerCase().startsWith(typeahead);
                    });
                    if (match) setActive(match);
                    return;
                }
            }

            e.preventDefault();
        });

        listbox.addEventListener('pointermove', function (e) {
            const option = e.target.closest('[role="option"]');
            if (option && option.dataset.active !== 'true') {
                setActive(option);
            }
        });

        const initial = (selectedOptionId && document.getElementById(selectedOptionId)) || options()[0];
        setActive(initial);
        listbox.focus({ preventScroll: true });
    }
};

// Color picker: touch events. Blazor's TouchEventArgs only gives viewport-relative clientX/clientY
// (unlike MouseEventArgs' element-relative offsetX/offsetY), so these helpers translate a touch
// point the same way the mouse handlers already do via e.OffsetX/e.OffsetY.
globalThis.twColorPicker = {
    relativePosition: function (el, clientX, clientY) {
        if (!el) return [0, 0];
        const rect = el.getBoundingClientRect();
        return [clientX - rect.left, clientY - rect.top];
    },
    // Measures actual rendered size so drag math uses real layout instead of a hardcoded guess -
    // stays correct across responsive breakpoints, zoom, or an overridden dialog width.
    getSize: function (el) {
        if (!el) return [0, 0];
        const rect = el.getBoundingClientRect();
        return [rect.width, rect.height];
    },
    // Feature-detects the EyeDropper API (Chromium-only) so the pick-from-screen button can be
    // omitted where unsupported instead of showing one that throws on click.
    supportsEyeDropper: function () {
        return typeof EyeDropper !== 'undefined';
    },
    // Opens the native eyedropper, resolving to the picked hex color or null if unavailable or
    // cancelled (Escape/click-away raises AbortError, a normal cancellation here).
    openEyeDropper: async function () {
        if (typeof EyeDropper === 'undefined') return null;
        try {
            const result = await new EyeDropper().open();
            return result.sRGBHex;
        } catch (err) {
            // AbortError = user cancelled, not worth logging; anything else is unexpected.
            if (err?.name !== 'AbortError') {
                console.error('twColorPicker.openEyeDropper error', err);
            }
            return null;
        }
    }
};

// Skeleton: measures a TwSkeleton's hidden ChildContent so C# can generate placeholder blocks
// shaped like it, instead of one generic box. A ResizeObserver re-measures on any layout change
// (render, breakpoints, image load, resize) and reports via dotnetRef; no separate "measure once"
// entry point is needed since ResizeObserver already fires immediately on observe().
globalThis.twSkeleton = {
    _observers: new WeakMap(),

    // Elements with no visible element children are "leaves" that get a placeholder box; elements
    // with visible children are structural wrappers (e.g. TwCard's container div) and are walked
    // into instead, so the skeleton follows real content down to its actual text/image/icon boxes.
    _isVisible: function (el, rect) {
        const style = getComputedStyle(el);
        if (style.display === 'none' || style.visibility === 'collapse') return false;
        rect = rect || el.getBoundingClientRect();
        return rect.width > 0 && rect.height > 0;
    },

    _hasVisibleElementChildren: function (el) {
        // Wrapped because Array.some's callback also passes (index, array), which would leak into
        // _isVisible's optional "rect" parameter if passed directly.
        return Array.from(el.children).some(function (child) {
            return globalThis.twSkeleton._isVisible(child);
        });
    },

    // "Round" means every corner's border-radius covers at least half the shorter side - catches
    // both a circular avatar and a "rounded-full" badge without special-casing classes or tags.
    _isRound: function (el, rect) {
        const style = getComputedStyle(el);
        const radii = [
            style.borderTopLeftRadius,
            style.borderTopRightRadius,
            style.borderBottomLeftRadius,
            style.borderBottomRightRadius
        ];
        const minSide = Math.min(rect.width, rect.height);
        return radii.every(function (radius) {
            return Number.parseFloat(radius) >= (minSide / 2) - 1;
        });
    },

    // One skeleton bar per wrapped visual line, not one box per paragraph - Range.getClientRects()
    // gives exactly that, one rect per line a text node wraps onto.
    _textLineRects: function (el, containerRect) {
        const rects = [];
        const walker = document.createTreeWalker(el, NodeFilter.SHOW_TEXT, {
            acceptNode: function (node) {
                return node.textContent.trim().length > 0 ? NodeFilter.FILTER_ACCEPT : NodeFilter.FILTER_REJECT;
            }
        });

        let node;
        while ((node = walker.nextNode())) {
            const range = document.createRange();
            range.selectNodeContents(node);
            const clientRects = range.getClientRects();
            for (const lineRect of clientRects) {
                if (lineRect.width <= 0 || lineRect.height <= 0) continue;
                rects.push({
                    top: lineRect.top - containerRect.top,
                    left: lineRect.left - containerRect.left,
                    width: lineRect.width,
                    height: lineRect.height,
                    shape: 'text'
                });
            }
        }

        return rects;
    },

    _walk: function (el, containerRect, out) {
        const rect = el.getBoundingClientRect();
        if (!globalThis.twSkeleton._isVisible(el, rect)) return;

        if (globalThis.twSkeleton._hasVisibleElementChildren(el)) {
            Array.from(el.children).forEach(function (child) {
                globalThis.twSkeleton._walk(child, containerRect, out);
            });
            return;
        }

        // A round leaf (e.g. an avatar with initials) is always one circle, even with text content -
        // splitting "JD" into its own text line would misrepresent it.
        if (globalThis.twSkeleton._isRound(el, rect)) {
            out.push({
                top: rect.top - containerRect.top,
                left: rect.left - containerRect.left,
                width: rect.width,
                height: rect.height,
                shape: 'circle'
            });
            return;
        }

        const hasText = el.textContent && el.textContent.trim().length > 0;
        if (hasText) {
            const lines = globalThis.twSkeleton._textLineRects(el, containerRect);
            if (lines.length > 0) {
                out.push(...lines);
                return;
            }
        }

        out.push({
            top: rect.top - containerRect.top,
            left: rect.left - containerRect.left,
            width: rect.width,
            height: rect.height,
            shape: 'rect'
        });
    },

    _measure: function (container) {
        const containerRect = container.getBoundingClientRect();
        const out = [];
        Array.from(container.children).forEach(function (child) {
            globalThis.twSkeleton._walk(child, containerRect, out);
        });
        return out;
    },

    observe: function (container, dotnetRef) {
        if (!container || globalThis.twSkeleton._observers.has(container)) return;

        const emit = function () {
            try {
                dotnetRef.invokeMethodAsync('OnRectsMeasured', globalThis.twSkeleton._measure(container));
            } catch (err) {
                console.error('twSkeleton emit error', err);
            }
        };

        const observer = new ResizeObserver(emit);
        observer.observe(container);
        globalThis.twSkeleton._observers.set(container, observer);
        emit();
    },

    unobserve: function (container) {
        if (!container) return;
        const observer = globalThis.twSkeleton._observers.get(container);
        if (observer) {
            observer.disconnect();
            globalThis.twSkeleton._observers.delete(container);
        }
    }
};

// Tooltips: a touch screen has no hover and a tap does not give keyboard focus, so the hover and focus
// styles never reveal a bubble there. One document listener opens the tooltip under a touch or pen tap by
// marking its wrapper (the theme reveals the bubble for a marked wrapper) and closes it on the next tap
// anywhere, so at most one is open. Mouse input is left to the hover styles.
globalThis.twTooltip = {
    _wrapperSelector: '[data-tw-tooltip]',
    _openAttribute: 'data-tw-tooltip-open',
    _edgeMargin: 8,
    _registered: false,

    _parts: function (wrapper) {
        const bubble = Array.from(wrapper.children).find(child => child.getAttribute('role') === 'tooltip') ?? null;
        const arrow = bubble ? Array.from(bubble.children).find(child => child.getAttribute('aria-hidden') === 'true') ?? null : null;
        return { bubble, arrow };
    },

    // A bubble is centered on its control, so one near the edge of the screen would be cut off and make the
    // page scroll sideways. Slides it back inside, and the arrow the other way so it still points at the control.
    _fit: function (wrapper) {
        const { bubble, arrow } = globalThis.twTooltip._parts(wrapper);
        if (!bubble) return;

        const margin = globalThis.twTooltip._edgeMargin;
        const rect = bubble.getBoundingClientRect();
        const viewportWidth = document.documentElement.clientWidth;

        let shift = 0;
        if (rect.right > viewportWidth - margin) {
            shift = viewportWidth - margin - rect.right;
        }
        if (rect.left + shift < margin) {
            shift = margin - rect.left;
        }
        if (shift === 0) return;

        bubble.style.marginLeft = `${shift}px`;
        if (arrow) arrow.style.marginLeft = `${-shift}px`;
    },

    open: function (wrapper) {
        wrapper.setAttribute(globalThis.twTooltip._openAttribute, '');
        globalThis.twTooltip._fit(wrapper);
    },

    close: function (wrapper) {
        wrapper.removeAttribute(globalThis.twTooltip._openAttribute);
        const { bubble, arrow } = globalThis.twTooltip._parts(wrapper);
        if (bubble) bubble.style.marginLeft = '';
        if (arrow) arrow.style.marginLeft = '';
    },

    _onPointerDown: function (e) {
        if (e.pointerType === 'mouse') return;

        const tooltip = globalThis.twTooltip;
        const wrapper = e.target?.closest?.(tooltip._wrapperSelector) ?? null;
        const opened = document.querySelector(`[${tooltip._openAttribute}]`);

        if (opened) tooltip.close(opened);
        if (wrapper && wrapper !== opened) tooltip.open(wrapper);
    },

    register: function () {
        if (globalThis.twTooltip._registered || typeof document === 'undefined') return;
        document.addEventListener('pointerdown', globalThis.twTooltip._onPointerDown);
        globalThis.twTooltip._registered = true;
    }
};

globalThis.twTooltip.register();

// Avatar: reports whether a picture has already failed to load. An image that fails before Blazor attaches
// its error handler never raises that event again, so TwAvatar asks once after rendering. decode() is used
// instead of naturalWidth because a vector image without its own dimensions can report a width of zero.
globalThis.twAvatar = {
    isBroken: async function (img) {
        if (!img?.complete) return false;
        if (typeof img.decode !== 'function') return img.naturalWidth === 0;

        try {
            await img.decode();
            return false;
        } catch {
            return true;
        }
    }
};
