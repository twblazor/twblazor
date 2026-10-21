// Direction: whether an element is laid out right to left. The nearest explicit dir attribute wins, as it
// does for the theme's rtl: styles; without one the computed direction is used.
globalThis.twDirection = {
    isRtl: function (el) {
        if (!el || typeof el.closest !== 'function') return false;
        const dir = el.closest('[dir]')?.getAttribute('dir')?.toLowerCase();
        if (dir === 'rtl' || dir === 'ltr') return dir === 'rtl';
        return typeof getComputedStyle === 'function' && getComputedStyle(el).direction === 'rtl';
    }
};

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
    // Registrations by owner key. A picker destroyed while its panel is open (the page navigated, or a
    // dialog around it closed) can no longer pass its root element, so the key is what lets its document
    // listeners and its claim on Escape be removed.
    _registrations: new Map(),

    registerOutsideClick: function (root, dotnetRef, ownerKey) {
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

        if (ownerKey) {
            globalThis.twPicker._registrations.set(ownerKey, root);
        }

        // While its panel is open the picker owns Escape, wherever focus is: in the field, in the panel,
        // on the icon button beside the field, or nowhere at all.
        globalThis.twDialog._pushEscape({
            surface: root,
            dotnetRef: dotnetRef,
            method: 'Close',
            isOpen: function () { return root.querySelector('[data-tw-popover]') !== null; }
        });
    },

    unregisterOutsideClick: function (root, ownerKey) {
        if (!root && ownerKey) {
            root = globalThis.twPicker._registrations.get(ownerKey) ?? null;
        }
        if (ownerKey) {
            globalThis.twPicker._registrations.delete(ownerKey);
        }
        if (!root) return;
        globalThis.twDialog.unregisterEscape(root);
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
    // instead of clipping off-screen. Only overrides the panel's default "top-full start-0" style
    // on the axis that actually overflows, so panels that already fit are untouched. A panel grows
    // rightwards from its trigger in a left to right layout and leftwards in a right to left one, so
    // either side can be the one that overflows. Re-run on
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
        } else if (rect.left < 0) {
            panel.style.left = '0';
            panel.style.right = 'auto';
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

    // A computed margin in pixels, or 0 when it isn't a number (e.g. "auto").
    _marginPx: function (value) {
        return Number.parseFloat(value) || 0;
    },

    // Edges of a fixed panel's containing block, in viewport coordinates. That is normally the
    // viewport itself, but an ancestor with a transform, translate, filter or similar (e.g.
    // TwSidebar's open drawer, which keeps translate-x-0) becomes the containing block instead, and
    // the panel then also moves with that ancestor's scroll position. Rather than hunting for such an
    // ancestor, the panel is pinned to each pair of edges in turn and measured where it lands.
    _fixedContainingBlock: function (panel) {
        const style = getComputedStyle(panel);
        const margin = globalThis.twPicker._marginPx;

        panel.style.left = '0px';
        panel.style.top = '0px';
        panel.style.right = 'auto';
        panel.style.bottom = 'auto';
        const start = panel.getBoundingClientRect();

        panel.style.left = 'auto';
        panel.style.top = 'auto';
        panel.style.right = '0px';
        panel.style.bottom = '0px';
        const end = panel.getBoundingClientRect();

        panel.style.right = '';
        panel.style.bottom = '';

        return {
            left: start.left - margin(style.marginLeft),
            top: start.top - margin(style.marginTop),
            right: end.right + margin(style.marginRight),
            bottom: end.bottom + margin(style.marginBottom)
        };
    },

    // Same flip/clamp job as positionPanel, but for `position: fixed` panels (used by every
    // popover-style picker). Unlike absolute, fixed has no inherent relationship to its trigger,
    // so this takes the trigger ("anchor") explicitly and computes coordinates from scratch. Payoff:
    // a fixed panel isn't clipped by an ancestor's overflow (e.g. TwDialog's scrollable body).
    // Coordinates are worked out against the viewport, then converted into the panel's actual
    // containing block (see _fixedContainingBlock) as they are applied.
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

        const block = globalThis.twPicker._fixedContainingBlock(panel);

        // Place provisionally below the anchor's starting edge (its left edge, or its right edge in a
        // right to left layout), then measure natural size - a fixed element has no rendered position
        // to read until one is set.
        const setLeft = function (x) {
            panel.style.left = (x - block.left) + 'px';
            panel.style.right = 'auto';
        };
        const setRight = function (x) {
            panel.style.left = 'auto';
            panel.style.right = (block.right - x) + 'px';
        };
        const rtl = globalThis.twDirection.isRtl(anchor);

        panel.style.top = (anchorRect.bottom - block.top) + 'px';
        panel.style.bottom = 'auto';
        if (rtl) {
            setRight(Math.min(anchorRect.right, viewportWidth));
        } else {
            setLeft(anchorRect.left);
        }

        const rect = panel.getBoundingClientRect();

        if (rtl) {
            if (rect.left < 0) setLeft(Math.max(anchorRect.left, 0));
        } else if (rect.right > viewportWidth) {
            setRight(Math.min(anchorRect.right, viewportWidth));
        }

        // Measured from the anchor's edges, not the panel's, so a tall panel can't skew which
        // direction looks like it has more room (same reasoning as positionPanel).
        const spaceAbove = anchorRect.top;
        const spaceBelow = viewportHeight - anchorRect.bottom;
        const flipUp = rect.bottom > viewportHeight && spaceAbove > spaceBelow;

        if (flipUp) {
            panel.style.top = 'auto';
            panel.style.bottom = (block.bottom - anchorRect.top) + 'px';
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
                    // tabindex="-1" (every day of a date grid but one, every tab but the selected one) can be
                    // focused by script but is skipped by Tab, so it is not an edge of the Tab order.
                    return !el.hasAttribute('inert')
                        && el.getAttribute('tabindex') !== '-1'
                        && el.getClientRects().length > 0;
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
    //
    // Controls in a container marked data-tw-inert-exempt (the toast region) are kept reachable: Tab from the
    // last control of the surface goes on to them, and from the last of them back to the surface. A toast
    // raised while a dialog is open can then be read and closed from the keyboard.
    _trapStack: [],

    _exemptSelector: '[data-tw-inert-exempt], #components-reconnect-modal, #blazor-error-ui',

    _exemptFocusable: function () {
        const found = [];
        document.querySelectorAll('[data-tw-inert-exempt]').forEach(function (container) {
            if (!container.__twTrapReturnHandler) {
                container.__twTrapReturnHandler = globalThis.twDialog._onExemptKeyDown;
                container.addEventListener('keydown', globalThis.twDialog._onExemptKeyDown);
            }
            found.push(...globalThis.twDialog.getFocusableElements(container));
        });
        return found;
    },

    // Tab out of the toast region, in either direction, returns to the trapped surface on top.
    _onExemptKeyDown: function (e) {
        if (e.key !== 'Tab') return;
        const surface = globalThis.twDialog._trapStack.at(-1);
        if (!surface?.isConnected) return;

        const exempt = globalThis.twDialog._exemptFocusable();
        const inSurface = globalThis.twDialog.getFocusableElements(surface);
        const active = document.activeElement;
        if (inSurface.length === 0) return;

        if (!e.shiftKey && active === exempt.at(-1)) {
            e.preventDefault();
            inSurface[0].focus();
        } else if (e.shiftKey && active === exempt[0]) {
            e.preventDefault();
            inSurface.at(-1).focus();
        }
    },

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

            const exempt = globalThis.twDialog._exemptFocusable();
            const first = focusable[0];
            const last = focusable.at(-1);
            const active = document.activeElement;

            if (e.shiftKey) {
                if (active === first || !surface.contains(active)) {
                    e.preventDefault();
                    (exempt.at(-1) ?? last).focus();
                }
            } else if (active === last || !surface.contains(active)) {
                e.preventDefault();
                (exempt[0] ?? first).focus();
            }
        };
        surface.__twDialogTrapHandler = handler;
        surface.addEventListener('keydown', handler);
        globalThis.twDialog._trapStack.push(surface);
    },

    releaseFocusTrap: function (surface) {
        if (!surface?.__twDialogTrapHandler) return;
        surface.removeEventListener('keydown', surface.__twDialogTrapHandler);
        delete surface.__twDialogTrapHandler;
        const stack = globalThis.twDialog._trapStack;
        const index = stack.indexOf(surface);
        if (index >= 0) stack.splice(index, 1);
    },

    // Marks everything outside `exceptEl` inert so it can't be reached while a dialog is open.
    // Walks up from exceptEl to <body>, inert-ing siblings at every level - needed because most
    // app hosts wrap everything in one root div, so only checking body's direct children would
    // find nothing to inert. Tags what it touched so clearBackgroundInert can undo precisely that.
    //
    // `owner` names whoever asked (the dialog provider, or one picker). An element stays inert until
    // every owner that marked it has cleared, so a picker closing inside a dialog can't lift the
    // dialog's own inert-ing of the page behind it. Elements marked data-tw-inert-exempt (the toast
    // container) are never made inert: a toast raised while a dialog is open must still be announced
    // and dismissable.
    _defaultInertOwner: 'default',

    _inertOwners: function (el) {
        const raw = el.dataset.twDialogInertOwners;
        return raw ? raw.split(' ').filter(Boolean) : [];
    },

    setBackgroundInert: function (exceptEl, owner) {
        if (!exceptEl || !document.body) return;
        const ownerKey = owner || globalThis.twDialog._defaultInertOwner;
        let current = exceptEl;
        while (current && current !== document.body && current.parentElement) {
            const parent = current.parentElement;
            Array.from(parent.children).forEach(function (sibling) {
                if (sibling === current) return;
                if (sibling.matches(globalThis.twDialog._exemptSelector)) return;
                const ours = sibling.dataset.twDialogInert === 'true';
                if (sibling.hasAttribute('inert') && !ours) return;
                const owners = globalThis.twDialog._inertOwners(sibling);
                if (!owners.includes(ownerKey)) owners.push(ownerKey);
                sibling.setAttribute('inert', '');
                sibling.dataset.twDialogInert = 'true';
                sibling.dataset.twDialogInertOwners = owners.join(' ');
            });
            current = parent;
        }
    },

    // Lifts the inert-ing `owner` asked for. Called with no owner it lifts everything, which is what a
    // host tearing the whole page down wants.
    clearBackgroundInert: function (owner) {
        if (!document.body) return;
        document.body.querySelectorAll('[data-tw-dialog-inert="true"]').forEach(function (el) {
            const remaining = owner
                ? globalThis.twDialog._inertOwners(el).filter(function (o) { return o !== owner; })
                : [];
            if (remaining.length > 0) {
                el.dataset.twDialogInertOwners = remaining.join(' ');
                return;
            }
            el.removeAttribute('inert');
            delete el.dataset.twDialogInert;
            delete el.dataset.twDialogInertOwners;
        });
    },

    // Escape closes the topmost dialog wherever focus happens to be. A handler on the dialog element
    // alone misses the key whenever focus is not inside it: after the focused control was removed by a
    // re-render, or while a screen reader's reading cursor is elsewhere. Listening on the document
    // makes one press enough.
    //
    // Dialogs and open popovers (picker panels, select lists) share one stack, most recently opened
    // last. Escape goes to the last entry that is still on the page and still open, so a popover opened
    // from inside a dialog takes the first press and the dialog takes the next. Which one owns the key
    // is decided by what is open, never by where focus is: with a popover open, focus can be on the icon
    // button beside the field, or lost altogether, and the press must still close the popover and not
    // the dialog around it.
    _escapeStack: [],

    _pushEscape: function (entry) {
        const stack = globalThis.twDialog._escapeStack;
        const existing = stack.findIndex(function (other) { return other.surface === entry.surface; });
        if (existing >= 0) stack.splice(existing, 1);
        stack.push(entry);
        if (stack.length === 1) {
            document.addEventListener('keydown', globalThis.twDialog._onDocumentKeyDown);
        }
    },

    // A tooltip that is showing is dismissed by Escape (its component handles that). The same press must
    // not also close the dialog or popover the tooltip's control sits in.
    _tooltipOwnsEscape: function (target) {
        const wrapper = target instanceof Element ? target.closest('[data-tw-tooltip]') : null;
        if (!wrapper) return false;
        const bubble = Array.from(wrapper.children).find(function (child) { return child.getAttribute('role') === 'tooltip'; });
        return !!bubble && getComputedStyle(bubble).display !== 'none';
    },

    _onDocumentKeyDown: function (e) {
        if (e.key !== 'Escape' || e.defaultPrevented) return;
        if (globalThis.twDialog._tooltipOwnsEscape(e.target)) return;
        const stack = globalThis.twDialog._escapeStack;

        // Entries whose element has left the page were never unregistered (their component was destroyed
        // while open), so they are dropped here.
        for (let i = stack.length - 1; i >= 0; i--) {
            if (!stack[i].surface.isConnected) stack.splice(i, 1);
        }

        for (let i = stack.length - 1; i >= 0; i--) {
            const entry = stack[i];
            if (entry.isOpen && !entry.isOpen()) continue;
            if (entry.declines?.(e)) return;
            void globalThis.twPicker._invokeSafely(entry.dotnetRef, entry.method);
            return;
        }
    },

    registerEscape: function (surface, dotnetRef) {
        if (!surface || !dotnetRef) return;
        const stack = globalThis.twDialog._escapeStack;
        if (stack.some(function (entry) { return entry.surface === surface; })) return;
        globalThis.twDialog._pushEscape({ surface: surface, dotnetRef: dotnetRef, method: 'CloseFromEscape' });
    },

    unregisterEscape: function (surface) {
        const stack = globalThis.twDialog._escapeStack;
        const index = stack.findIndex(function (entry) { return entry.surface === surface; });
        if (index < 0) return;
        stack.splice(index, 1);
        if (stack.length === 0) {
            document.removeEventListener('keydown', globalThis.twDialog._onDocumentKeyDown);
        }
    },

    // Moves focus into a just-opened popover panel: onto the element the panel marks as its starting
    // point (the selected day of a date grid, whose roving tabindex is 0), otherwise the first
    // focusable element, otherwise the panel itself.
    focusPanel: function (panel) {
        if (!panel) return;
        const preferred = panel.querySelector('[data-tw-autofocus]')
            ?? panel.querySelector('[role="grid"] [tabindex="0"]');
        if (preferred && typeof preferred.focus === 'function') {
            preferred.focus();
            return;
        }
        globalThis.twDialog.focusSurface(panel);
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

// Numeric field: the stepping itself runs in C#, this only does what C# can't do per keystroke.
// - Arrow Up/Down and Page Up/Down would move the caret or scroll the page, so their default is prevented.
// - The wheel would scroll the page while it steps a focused field, so its default is prevented too.
// - Before a step, a "change" is raised so Blazor commits any typed but uncommitted text first and the
//   step starts from what the user sees, not from the last committed value.
// - A printable key that the allowed pattern rejects is blocked. Pasted text is not, C# parses it instead.
globalThis.twNumeric = {
    _stepKeys: ['ArrowUp', 'ArrowDown', 'PageUp', 'PageDown'],

    _commit: function (el) {
        el.dispatchEvent(new Event('change', { bubbles: true }));
    },

    _canStep: function (el) {
        return !el.readOnly && !el.disabled;
    },

    attach: function (el, allowedPattern, wheelDisabled) {
        if (!el) return;
        globalThis.twNumeric.update(el, allowedPattern, wheelDisabled);
        if (el.__twNumeric) return;

        const keydown = function (e) {
            if (e.ctrlKey || e.metaKey || e.altKey) return;

            if (globalThis.twNumeric._stepKeys.includes(e.key)) {
                e.preventDefault();
                if (globalThis.twNumeric._canStep(el)) {
                    globalThis.twNumeric._commit(el);
                }
                return;
            }

            const allowed = el.__twNumericAllowed;
            if (allowed && e.key.length === 1 && !allowed.test(e.key)) {
                e.preventDefault();
            }
        };

        const wheel = function (e) {
            if (document.activeElement !== el || !globalThis.twNumeric._canStep(el) || el.__twNumericWheelDisabled) return;
            e.preventDefault();
            globalThis.twNumeric._commit(el);
        };

        // Pressing a button keeps focus in the field, so the keys and wheel carry on working after a click.
        const root = el.closest('[data-tw-numeric]');
        const mousedown = function (e) {
            if (!e.target.closest?.('[data-tw-field-button]')) return;
            e.preventDefault();
            if (document.activeElement === el) {
                globalThis.twNumeric._commit(el);
            }
        };

        el.__twNumeric = { keydown, wheel, root, mousedown };
        el.addEventListener('keydown', keydown);
        el.addEventListener('wheel', wheel, { passive: false });
        root?.addEventListener('mousedown', mousedown);
    },

    update: function (el, allowedPattern, wheelDisabled) {
        if (!el) return;
        try {
            el.__twNumericAllowed = allowedPattern ? new RegExp('^(?:' + allowedPattern + ')$', 'u') : null;
        } catch {
            el.__twNumericAllowed = null;
        }
        el.__twNumericWheelDisabled = !!wheelDisabled;
    },

    detach: function (el) {
        if (!el?.__twNumeric) return;
        el.removeEventListener('keydown', el.__twNumeric.keydown);
        el.removeEventListener('wheel', el.__twNumeric.wheel);
        el.__twNumeric.root?.removeEventListener('mousedown', el.__twNumeric.mousedown);
        delete el.__twNumeric;
        delete el.__twNumericAllowed;
        delete el.__twNumericWheelDisabled;
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

    // A date grid also uses Page Up and Page Down (previous and next month), which would otherwise
    // scroll the page behind the picker.
    _gridKeydownHandler: function (e) {
        if (globalThis.twTabs._navigationKeys.has(e.key) || e.key === 'PageUp' || e.key === 'PageDown') {
            e.preventDefault();
        }
    },

    registerKeydownGuard: function (tablist, includePagingKeys) {
        if (!tablist || tablist.__twTabsKeydownHandler) return;
        const handler = includePagingKeys ? globalThis.twTabs._gridKeydownHandler : globalThis.twTabs._tabsKeydownHandler;
        tablist.__twTabsKeydownHandler = handler;
        tablist.addEventListener('keydown', handler);
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
    },

    // The skip link's own href would be resolved against <base href>, sending the user to the site
    // root, so the link moves focus itself. The main region is focusable, so the next Tab continues
    // from the top of the page content.
    focusMain: function (el) {
        if (!el || typeof el.focus !== 'function') return;
        el.focus();
        if (typeof el.scrollIntoView === 'function') el.scrollIntoView({ block: 'start' });
    },

    focusById: function (id) {
        const el = id ? document.getElementById(id) : null;
        if (el && typeof el.focus === 'function') el.focus();
    },

    // Below the "lg" breakpoint the open sidebar is a drawer over the page, so the page content behind
    // it is made inert: it can't be tabbed into or read while the drawer covers it. On a wider
    // viewport the sidebar sits beside the content, which must stay usable. The state is re-applied
    // when the viewport crosses the breakpoint, so widening the window never leaves the content inert.
    // Returns whether the drawer is currently acting as a modal.
    //
    // `content` is everything beside the sidebar (the top bar and the page), `nav` the sidebar itself. As a
    // drawer the sidebar traps Tab and owns Escape, like any other modal surface; the top bar it covers is
    // inert along with the page, so nothing hidden behind the drawer can take focus.
    syncDrawer: function (content, nav, isOpen, dotnetRef) {
        if (!content) return false;

        content.__twSidebarOpen = !!isOpen;

        const isModal = function () {
            return content.__twSidebarOpen && globalThis.twSidebar.isMobileViewport();
        };

        const apply = function (viewportChanged) {
            const modal = isModal();
            if (modal) {
                content.setAttribute('inert', '');
                if (nav) {
                    globalThis.twDialog.trapFocus(nav);
                    // The window was narrowed (or zoomed) with the sidebar open: it has just become a
                    // drawer over the page, so focus has to come with it.
                    if (viewportChanged === true && !nav.contains(document.activeElement)) {
                        globalThis.twDialog.focusSurface(nav);
                    }
                }
            } else {
                content.removeAttribute('inert');
                if (nav) globalThis.twDialog.releaseFocusTrap(nav);
            }
            return modal;
        };

        if (!content.__twSidebarMediaHandler && typeof window.matchMedia === 'function') {
            try {
                const query = window.matchMedia('(min-width: 1024px)');
                const onChange = function () { apply(true); };
                content.__twSidebarMediaHandler = onChange;
                content.__twSidebarMediaQuery = query;
                query.addEventListener?.('change', onChange);
            } catch (err) {
                console.error('twSidebar.syncDrawer error', err);
            }
        }

        if (nav && dotnetRef && !globalThis.twDialog._escapeStack.some(function (entry) { return entry.surface === nav; })) {
            globalThis.twDialog._pushEscape({
                surface: nav,
                dotnetRef: dotnetRef,
                method: 'CloseDrawerFromEscape',
                isOpen: isModal,
                // Escape in a search field that has text clears the text. Closing the drawer is the next press.
                declines: function (e) {
                    return e.target instanceof HTMLInputElement && e.target.type === 'search' && e.target.value !== '';
                }
            });
        }

        return apply(false);
    },

    releaseDrawer: function (content, nav) {
        if (content?.__twSidebarMediaHandler) {
            content.__twSidebarMediaQuery?.removeEventListener?.('change', content.__twSidebarMediaHandler);
            delete content.__twSidebarMediaHandler;
            delete content.__twSidebarMediaQuery;
            content.removeAttribute('inert');
        }
        if (nav) {
            globalThis.twDialog.releaseFocusTrap(nav);
            globalThis.twDialog.unregisterEscape(nav);
        }
    }
};

// Roving-focus lists (TwTreeList, TwPickList): the arrow keys, Home and End move focus between the
// items, so only one item is a Tab stop. Runs client-side because a server round trip per arrow key
// would lag, and because Space and the arrows must not also scroll the page.
globalThis.twRoving = {
    _items: function (container, selector) {
        return Array.from(container.querySelectorAll(selector)).filter(function (el) {
            return el.getClientRects().length > 0 && el.getAttribute('aria-disabled') !== 'true';
        });
    },

    _mark: function (container, selector, item) {
        container.querySelectorAll(selector).forEach(function (el) {
            el.setAttribute('tabindex', el === item ? '0' : '-1');
            if (el === item) el.dataset.twRovingCurrent = 'true';
            else delete el.dataset.twRovingCurrent;
        });
    },

    // Makes sure exactly one enabled item is in the Tab order. Called after every render, since a
    // re-render resets each item's tabindex and can remove the item that held it. When the item that
    // had focus is gone (it was moved to the other list), focus goes to its neighbour instead of
    // being dropped on the page body.
    sync: function (container, selector, restoreFocus) {
        if (!container) return;
        const enabled = globalThis.twRoving._items(container, selector);
        const active = document.activeElement;
        const current = (enabled.includes(active) ? active : null)
            ?? enabled.find(function (el) { return el.dataset.twRovingCurrent === 'true'; })
            ?? enabled[0]
            ?? null;
        globalThis.twRoving._mark(container, selector, current);
        if (restoreFocus && current && (active === document.body || active === null)) {
            current.focus();
        }
    },

    attach: function (container, selector, mode) {
        if (!container) return;
        if (container.__twRovingHandler) {
            globalThis.twRoving.sync(container, selector, false);
            return;
        }

        const handler = function (e) {
            const item = e.target instanceof Element ? e.target.closest(selector) : null;
            // Keys pressed in a control nested inside an item (a tree row's checkbox) belong to it.
            if (!item || item !== e.target || !container.contains(item)) return;

            const items = globalThis.twRoving._items(container, selector);
            const index = items.indexOf(item);
            let target = null;

            switch (e.key) {
                case 'ArrowDown':
                    target = items[Math.min(index + 1, items.length - 1)];
                    break;
                case 'ArrowUp':
                    target = items[Math.max(index - 1, 0)];
                    break;
                case 'Home':
                    target = items[0];
                    break;
                case 'End':
                    target = items.at(-1);
                    break;
                case 'ArrowRight':
                case 'ArrowLeft': {
                    if (mode !== 'tree') return;
                    const expanded = item.getAttribute('aria-expanded');
                    // In a right to left tree the children sit to the left, so the keys swap roles.
                    const inwardKey = globalThis.twDirection.isRtl(container) ? 'ArrowLeft' : 'ArrowRight';
                    // Expanding and collapsing is done by the component itself; this only moves focus.
                    if (e.key === inwardKey) {
                        if (expanded === 'true') target = items[index + 1] ?? null;
                    } else if (expanded !== 'true') {
                        target = item.parentElement?.closest(selector) ?? null;
                    }
                    break;
                }
                case ' ':
                    // Blazor handles the activation; this only stops the page scrolling.
                    e.preventDefault();
                    return;
                default:
                    return;
            }

            e.preventDefault();
            if (target && target !== item) {
                globalThis.twRoving._mark(container, selector, target);
                target.focus();
            }
        };

        const onFocusIn = function (e) {
            const item = e.target instanceof Element ? e.target.closest(selector) : null;
            if (!item || item !== e.target) return;
            globalThis.twRoving._mark(container, selector, item);
        };

        container.__twRovingHandler = handler;
        container.__twRovingFocusIn = onFocusIn;
        container.addEventListener('keydown', handler);
        container.addEventListener('focusin', onFocusIn);
        globalThis.twRoving.sync(container, selector, false);
    },

    detach: function (container) {
        if (!container?.__twRovingHandler) return;
        container.removeEventListener('keydown', container.__twRovingHandler);
        container.removeEventListener('focusin', container.__twRovingFocusIn);
        delete container.__twRovingHandler;
        delete container.__twRovingFocusIn;
    }
};

// Carousel: Left and Right change slide, but not while the user is typing or operating a control
// inside a slide that uses those keys itself.
globalThis.twCarousel = {
    _arrowKeyOwners: 'input, textarea, select, [contenteditable="true"], [role="slider"], [role="tab"], ' +
        '[role="listbox"], [role="grid"], [role="tree"], [role="spinbutton"], [role="combobox"]',

    _ownsArrowKeys: function (target) {
        if (!(target instanceof Element)) return false;
        return target.closest(globalThis.twCarousel._arrowKeyOwners) !== null;
    },

    attach: function (el, dotnetRef) {
        if (!el || el.__twCarouselHandler) return;
        const handler = function (e) {
            if (e.key !== 'ArrowLeft' && e.key !== 'ArrowRight') return;
            if (e.defaultPrevented || globalThis.twCarousel._ownsArrowKeys(e.target)) return;
            e.preventDefault();
            // The previous button sits on the right in a right to left layout, so the keys follow it there.
            const previousKey = globalThis.twDirection.isRtl(el) ? 'ArrowRight' : 'ArrowLeft';
            void globalThis.twPicker._invokeSafely(dotnetRef, e.key === previousKey ? 'PreviousSlideFromKey' : 'NextSlideFromKey');
        };
        el.__twCarouselHandler = handler;
        el.addEventListener('keydown', handler);
    },

    detach: function (el) {
        if (!el?.__twCarouselHandler) return;
        el.removeEventListener('keydown', el.__twCarouselHandler);
        delete el.__twCarouselHandler;
    },

    prefersReducedMotion: function () {
        try {
            return window.matchMedia('(prefers-reduced-motion: reduce)').matches;
        } catch {
            return false;
        }
    }
};

// Select: keyboard navigation and highlighting for the desktop single-select listbox. Focus stays on
// the listbox itself and the highlighted option is exposed through aria-activedescendant (the ARIA
// listbox pattern), so this runs entirely client-side - a server round trip per arrow key would lag -
// and only calls back into .NET (SelectOption) once an option is committed.
globalThis.twSelect = {
    _typeaheadResetMs: 500,

    // Deselects every option of a native <select multiple>. Removing an option's selected attribute does not
    // deselect an option the user picked, so clearing has to set the property.
    deselectAll: function (select) {
        if (!select?.options) return;
        Array.from(select.options).forEach(function (option) { option.selected = false; });
    },

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
                case 'Tab':
                    // Tab closes the list and puts focus back on the field, so the next Tab moves on.
                    // The focus trap would otherwise swallow the key and leave the user stuck here.
                    e.stopPropagation();
                    void globalThis.twPicker._invokeSafely(dotnetRef, 'Close');
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

// Autocomplete: keyboard navigation for the suggestions list. Unlike twSelect, focus stays in the text
// field so the user can keep typing, and the highlighted suggestion is exposed through the field's
// aria-activedescendant (the APG combobox pattern). The list is found through the field's aria-controls,
// which is only set while it is shown. Runs client-side so an arrow key never waits on a server round trip.
globalThis.twAutocomplete = {
    _listbox: function (input) {
        const id = input.getAttribute('aria-controls');
        return id ? document.getElementById(id) : null;
    },

    _options: function (listbox) {
        return listbox ? Array.from(listbox.querySelectorAll('[role="option"]')) : [];
    },

    _setActive: function (input, listbox, option) {
        globalThis.twAutocomplete._options(listbox).forEach(function (o) { delete o.dataset.active; });
        if (!option) {
            input.removeAttribute('aria-activedescendant');
            return;
        }
        option.dataset.active = 'true';
        input.setAttribute('aria-activedescendant', option.id);
        option.scrollIntoView?.({ block: 'nearest' });
    },

    // The field's list, its options and the index of the highlighted one (-1 when none is).
    _state: function (input) {
        const listbox = globalThis.twAutocomplete._listbox(input);
        const all = globalThis.twAutocomplete._options(listbox);
        const activeId = input.getAttribute('aria-activedescendant');
        return { listbox, all, current: all.findIndex(function (o) { return o.id === activeId; }) };
    },

    _keyHandlers: {
        ArrowDown: function (input, dotnetRef, e, state) {
            e.preventDefault();
            if (state.all.length === 0) {
                // Alt + Arrow Down opens the list without moving into it.
                void globalThis.twPicker._invokeSafely(dotnetRef, 'OpenFromKeyboard', e.altKey ? null : 'first');
            } else if (!e.altKey) {
                globalThis.twAutocomplete._setActive(input, state.listbox, state.all[(state.current + 1) % state.all.length]);
            }
        },

        ArrowUp: function (input, dotnetRef, e, state) {
            e.preventDefault();
            if (state.all.length === 0) {
                void globalThis.twPicker._invokeSafely(dotnetRef, 'OpenFromKeyboard', 'last');
                return;
            }
            const previous = state.current <= 0 ? state.all.length - 1 : state.current - 1;
            globalThis.twAutocomplete._setActive(input, state.listbox, state.all[previous]);
        },

        Enter: function (input, dotnetRef, e, state) {
            // With nothing highlighted Enter keeps its default, so it still submits a form.
            if (state.current < 0) return;
            e.preventDefault();
            void globalThis.twPicker._invokeSafely(dotnetRef, 'SelectOption', Number.parseInt(state.all[state.current].dataset.value, 10));
        }
    },

    attach: function (input, dotnetRef) {
        if (!input || input.__twAutocomplete) return;

        const keydown = function (e) {
            if (e.ctrlKey || e.metaKey || input.readOnly || input.disabled) return;
            const handler = Object.hasOwn(globalThis.twAutocomplete._keyHandlers, e.key) ? globalThis.twAutocomplete._keyHandlers[e.key] : null;
            handler?.(input, dotnetRef, e, globalThis.twAutocomplete._state(input));
        };

        input.__twAutocomplete = { keydown };
        input.addEventListener('keydown', keydown);
    },

    // Called after each render while the list is open, and once after it closes. Options that were filtered
    // out take the highlight with them, and `highlight` ('first', 'last' or 'reset') moves it on purpose.
    refresh: function (input, highlight) {
        if (!input) return;
        const self = globalThis.twAutocomplete;
        const listbox = self._listbox(input);
        if (!listbox) {
            input.removeAttribute('aria-activedescendant');
            return;
        }

        if (!listbox.__twAutocompletePointer) {
            listbox.__twAutocompletePointer = true;
            listbox.addEventListener('pointermove', function (e) {
                const option = e.target.closest?.('[role="option"]');
                if (option && option.dataset.active !== 'true') {
                    self._setActive(input, listbox, option);
                }
            });
        }

        const all = self._options(listbox);
        if (highlight === 'first') {
            self._setActive(input, listbox, all[0]);
        } else if (highlight === 'last') {
            self._setActive(input, listbox, all.at(-1));
        } else if (highlight === 'reset') {
            self._setActive(input, listbox, null);
        } else {
            const activeId = input.getAttribute('aria-activedescendant');
            self._setActive(input, listbox, all.find(function (o) { return o.id === activeId; }) ?? null);
        }
    },

    detach: function (input) {
        if (!input?.__twAutocomplete) return;
        input.removeEventListener('keydown', input.__twAutocomplete.keydown);
        delete input.__twAutocomplete;
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
// A bubble is absolutely positioned inside its wrapper, so a scrolling ancestor such as a table's scroll area would
// cut it off. While a bubble inside one is showing it is fixed to the viewport instead, next to its control.
globalThis.twTooltip = {
    _wrapperSelector: '[data-tw-tooltip]',
    _openAttribute: 'data-tw-tooltip-open',
    _edgeMargin: 8,
    _gap: 8,
    _registered: false,
    _floating: new Set(),
    _savedStyles: new WeakMap(),

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

    // Whether an element makes itself the containing block of fixed descendants, so a fixed bubble below it is
    // placed relative to it rather than the viewport and is still clipped by whatever clips it.
    _trapsFixed: function (style) {
        const set = function (value) { return !!value && value !== 'none'; };
        return set(style.transform) || set(style.translate) || set(style.scale) || set(style.rotate)
            || set(style.perspective) || set(style.filter) || set(style.backdropFilter)
            || /paint|layout|strict|content/.test(style.contain || '')
            || /transform|perspective|filter/.test(style.willChange || '');
    },

    // Whether an ancestor between the wrapper and the page clips its overflow, which would cut the bubble off, in a
    // way a fixed bubble can escape. A transformed wrapper or ancestor met first would hold a fixed bubble inside the
    // same clip, so the bubble is then left where it is.
    _isClipped: function (wrapper) {
        const tooltip = globalThis.twTooltip;
        for (let el = wrapper; el && el !== document.body && el !== document.documentElement; el = el.parentElement) {
            const style = getComputedStyle(el);
            if (tooltip._trapsFixed(style)) return false;
            if (el === wrapper) continue;
            const clips = [style.overflow, style.overflowX, style.overflowY].some(function (value) { return value && value !== 'visible'; });
            if (clips) return true;
        }
        return false;
    },

    _reset: function (wrapper) {
        const tooltip = globalThis.twTooltip;
        tooltip._floating.delete(wrapper);
        const { bubble, arrow } = tooltip._parts(wrapper);
        if (bubble) {
            // Puts back the component's own inline style (such as a chart's anchored left and bottom) that floating replaced.
            if (tooltip._savedStyles.has(bubble)) {
                const saved = tooltip._savedStyles.get(bubble);
                tooltip._savedStyles.delete(bubble);
                if (saved === null) {
                    bubble.removeAttribute('style');
                } else {
                    bubble.setAttribute('style', saved);
                }
            } else {
                bubble.style.marginLeft = '';
            }
        }
        if (arrow) arrow.style.marginLeft = '';
    },

    // Fixes a showing bubble to the viewport on its placement's side of the wrapper, kept on screen.
    _float: function (wrapper) {
        const tooltip = globalThis.twTooltip;
        const { bubble, arrow } = tooltip._parts(wrapper);
        if (!bubble) return false;

        if (!tooltip._savedStyles.has(bubble)) {
            tooltip._savedStyles.set(bubble, bubble.getAttribute('style'));
        }
        bubble.style.position = 'fixed';
        bubble.style.inset = '0 auto auto 0';
        bubble.style.margin = '0';
        bubble.style.translate = 'none';
        bubble.style.transform = 'none';
        if (arrow) arrow.style.marginLeft = '';

        const size = bubble.getBoundingClientRect();
        if (size.width === 0 && size.height === 0) {
            tooltip._reset(wrapper);
            return false;
        }

        const rect = wrapper.getBoundingClientRect();
        const gap = tooltip._gap;
        const rtl = globalThis.twDirection.isRtl(wrapper);
        const placement = (bubble.dataset.twPlacement || 'top').toLowerCase();
        const centreX = rect.left + (rect.width - size.width) / 2;
        const centreY = rect.top + (rect.height - size.height) / 2;
        const before = rect.left - size.width - gap;
        const after = rect.right + gap;

        let left = centreX;
        let top = rect.top - size.height - gap;
        if (placement === 'bottom') {
            top = rect.bottom + gap;
        } else if (placement === 'left') {
            left = rtl ? after : before;
            top = centreY;
        } else if (placement === 'right') {
            left = rtl ? before : after;
            top = centreY;
        }

        const margin = tooltip._edgeMargin;
        const viewportWidth = document.documentElement.clientWidth;
        const fitted = viewportWidth > 0 ? Math.max(margin, Math.min(left, viewportWidth - margin - size.width)) : left;
        // Pinned at 0,0 the bubble sits on its containing block's corner, which is the viewport's unless an ancestor
        // outside the clip is transformed, so this offset keeps it on its control either way.
        bubble.style.left = (fitted - size.left) + 'px';
        bubble.style.top = (top - size.top) + 'px';
        if (arrow && fitted !== left && (placement === 'top' || placement === 'bottom')) {
            arrow.style.marginLeft = (left - fitted) + 'px';
        }
        tooltip._floating.add(wrapper);
        return true;
    },

    // Called when a wrapper may have started showing its bubble. The hover or focus styles may not apply until the
    // next frame, so a bubble that is not showing yet is tried once more then.
    _show: function (wrapper) {
        const tooltip = globalThis.twTooltip;
        if (!tooltip._isClipped(wrapper)) return;
        if (tooltip._float(wrapper)) return;
        const schedule = globalThis.requestAnimationFrame ?? function (fn) { return setTimeout(fn, 0); };
        schedule(function () {
            if (wrapper.isConnected) tooltip._float(wrapper);
        });
    },

    open: function (wrapper) {
        wrapper.setAttribute(globalThis.twTooltip._openAttribute, '');
        if (globalThis.twTooltip._isClipped(wrapper)) {
            globalThis.twTooltip._float(wrapper);
        } else {
            globalThis.twTooltip._fit(wrapper);
        }
    },

    close: function (wrapper) {
        wrapper.removeAttribute(globalThis.twTooltip._openAttribute);
        globalThis.twTooltip._reset(wrapper);
    },

    _wrapperOf: function (target) {
        return target?.closest?.(globalThis.twTooltip._wrapperSelector) ?? null;
    },

    _onEnter: function (e) {
        const tooltip = globalThis.twTooltip;
        const wrapper = tooltip._wrapperOf(e.target);
        if (wrapper && !(e.relatedTarget instanceof Node && wrapper.contains(e.relatedTarget))) {
            tooltip._show(wrapper);
        }
    },

    _onLeave: function (e) {
        const tooltip = globalThis.twTooltip;
        const wrapper = tooltip._wrapperOf(e.target);
        if (!wrapper || !tooltip._floating.has(wrapper)) return;
        if (e.relatedTarget instanceof Node && wrapper.contains(e.relatedTarget)) return;
        if (wrapper.hasAttribute(tooltip._openAttribute)) return;
        tooltip._reset(wrapper);
    },

    // A fixed bubble does not move with its control, so it follows it on any scroll or resize.
    _onViewportChange: function () {
        globalThis.twTooltip._floating.forEach(function (wrapper) {
            if (wrapper.isConnected) {
                globalThis.twTooltip._float(wrapper);
            } else {
                globalThis.twTooltip._floating.delete(wrapper);
            }
        });
    },

    _onPointerDown: function (e) {
        if (e.pointerType === 'mouse') return;

        const tooltip = globalThis.twTooltip;
        const wrapper = e.target?.closest?.(tooltip._wrapperSelector) ?? null;
        const opened = document.querySelector(`[${tooltip._openAttribute}]`);

        if (opened) tooltip.close(opened);
        if (wrapper && wrapper !== opened) tooltip.open(wrapper);
    },

    // A tooltip wrapped around a control that is already focusable (a button, a link) must not add a
    // second Tab stop, and its text has to describe that control, not the wrapper. Returns whether such
    // a control was found, so the component can drop the wrapper's own tabindex.
    describeControl: function (wrapper, tooltipId) {
        if (!wrapper || !tooltipId) return false;
        const { bubble } = globalThis.twTooltip._parts(wrapper);
        const control = globalThis.twDialog.getFocusableElements(wrapper).find(function (el) {
            return el !== wrapper && !bubble?.contains(el);
        });
        if (!control) return false;

        const ids = (control.getAttribute('aria-describedby') ?? '').split(' ').filter(Boolean);
        if (!ids.includes(tooltipId)) {
            ids.push(tooltipId);
            control.setAttribute('aria-describedby', ids.join(' '));
        }
        return true;
    },

    register: function () {
        if (globalThis.twTooltip._registered || typeof document === 'undefined') return;
        const tooltip = globalThis.twTooltip;
        document.addEventListener('pointerdown', tooltip._onPointerDown);
        document.addEventListener('pointerover', tooltip._onEnter);
        document.addEventListener('focusin', tooltip._onEnter);
        document.addEventListener('pointerout', tooltip._onLeave);
        document.addEventListener('focusout', tooltip._onLeave);
        document.addEventListener('scroll', tooltip._onViewportChange, true);
        globalThis.addEventListener?.('resize', tooltip._onViewportChange);
        tooltip._registered = true;
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

// Checkbox: the "mixed" state only exists as a DOM property, which markup can't set.
globalThis.twCheckbox = {
    setIndeterminate: function (el, value) {
        if (el) el.indeterminate = !!value;
    }
};

// Focus hand-off for controls that remove themselves (a dismissed alert, a removed file chip). Without
// it focus falls back to the page body and the keyboard user starts again from the top.
globalThis.twFocus = {
    // Moves focus to the nearest focusable element after `el`, or failing that the nearest before it.
    moveToNeighbour: function (el) {
        if (!el) return;
        const all = globalThis.twDialog.getFocusableElements(document.body).filter(function (candidate) {
            return !el.contains(candidate);
        });
        const following = all.find(function (candidate) {
            return (el.compareDocumentPosition(candidate) & Node.DOCUMENT_POSITION_FOLLOWING) !== 0;
        });
        const target = following ?? all.at(-1);
        if (target) target.focus();
    },

    // Focuses the first element matching `selector`, making it programmatically focusable first if it
    // is not (a heading). Used after a client-side navigation, where the new page's heading is both where
    // the keyboard should continue from and what a screen reader should read out. Returns whether an
    // element was found.
    focusBySelector: function (selector) {
        const el = selector ? document.querySelector(selector) : null;
        if (!el || typeof el.focus !== 'function') return false;
        if (!el.hasAttribute('tabindex')) el.setAttribute('tabindex', '-1');
        el.focus();
        return true;
    },

    focusById: function (id) {
        const el = id ? document.getElementById(id) : null;
        if (el && typeof el.focus === 'function') el.focus();
    }
};

// Read-only slider: stops the keys that would change a native range input's value, without touching Tab.
globalThis.twSliderLock = {
    _keys: ['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight', 'Home', 'End', 'PageUp', 'PageDown'],

    _handler: function (e) {
        if (globalThis.twSliderLock._keys.includes(e.key)) e.preventDefault();
    },

    set: function (el, locked) {
        if (!el) return;
        if (locked && !el.__twSliderLocked) {
            el.addEventListener('keydown', globalThis.twSliderLock._handler);
            el.__twSliderLocked = true;
        } else if (!locked && el.__twSliderLocked) {
            el.removeEventListener('keydown', globalThis.twSliderLock._handler);
            el.__twSliderLocked = false;
        }
    }
};

// Scroll containers (a wide table, a long code sample) have to be focusable for the keyboard to scroll
// them, but only while they actually overflow. A container whose content fits would otherwise be a Tab
// stop that does nothing. The markup ships focusable and named, so it works without script; this takes
// the Tab stop, role and name away while there is nothing to scroll and puts them back when there is.
globalThis.twScrollRegion = {
    _observers: new WeakMap(),

    _overflows: function (el) {
        return el.scrollWidth > el.clientWidth + 1 || el.scrollHeight > el.clientHeight + 1;
    },

    _sync: function (el) {
        if (globalThis.twScrollRegion._overflows(el)) {
            el.setAttribute('tabindex', '0');
            el.setAttribute('role', 'group');
            if (el.dataset.twScrollLabel) el.setAttribute('aria-label', el.dataset.twScrollLabel);
        } else {
            el.removeAttribute('tabindex');
            el.removeAttribute('role');
            el.removeAttribute('aria-label');
        }
    },

    observe: function (el) {
        if (!el || globalThis.twScrollRegion._observers.has(el)) return;
        el.dataset.twScrollLabel = el.getAttribute('aria-label') ?? '';
        globalThis.twScrollRegion._sync(el);
        if (typeof ResizeObserver === 'undefined') return;
        const observer = new ResizeObserver(function () { globalThis.twScrollRegion._sync(el); });
        observer.observe(el);
        // The content can outgrow the container without the container itself resizing.
        Array.from(el.children).forEach(function (child) { observer.observe(child); });
        globalThis.twScrollRegion._observers.set(el, observer);
    },

    unobserve: function (el) {
        const observer = el ? globalThis.twScrollRegion._observers.get(el) : null;
        if (!observer) return;
        observer.disconnect();
        globalThis.twScrollRegion._observers.delete(el);
    }
};

// Table column resizing: a focusable separator on the border at the end of each resizable column's header. The
// separators live in a layer after the table rather than inside the header cells, so their names do not become part
// of the column headings. They are rebuilt from the <col> elements, so they always follow the columns' order.
globalThis.twTable = {
    _keyStep: 16,
    _largeKeyStep: 64,

    _layer: function (wrapper) {
        return wrapper ? wrapper.querySelector(':scope > [data-tw-resize-layer]') : null;
    },

    // The table's <col> elements with the index of the first column each covers.
    _columns: function (wrapper) {
        const table = wrapper?.querySelector(':scope > table');
        if (!table) return [];
        let index = 0;
        return Array.from(table.querySelectorAll(':scope > colgroup > col')).map(function (col) {
            const entry = { col: col, index: index };
            index += Math.max(1, col.span || 1);
            return entry;
        });
    },

    // The header cell in the first header row that starts at a column index, counting colspans.
    _headerCell: function (wrapper, columnIndex) {
        const row = wrapper.querySelector(':scope > table > thead > tr');
        if (!row) return null;
        let index = 0;
        for (const cell of row.children) {
            if (index === columnIndex) return cell;
            index += Math.max(1, cell.colSpan || 1);
            if (index > columnIndex) return null;
        }
        return null;
    },

    _format: function (format, value) {
        return (format || '{0}').replace('{0}', value);
    },

    // Creates, removes and positions the separators to match the current columns.
    layoutResize: function (wrapper) {
        const layer = globalThis.twTable._layer(wrapper);
        if (!layer) return;

        const resizable = globalThis.twTable._columns(wrapper).filter(function (entry) {
            return entry.col.dataset.twResizable === 'true' && entry.col.id;
        });
        const existing = new Map(Array.from(layer.children).map(function (handle) { return [handle.dataset.twResizeKey, handle]; }));
        const wrapperRect = wrapper.getBoundingClientRect();
        const rtl = globalThis.twDirection.isRtl(wrapper);
        let order = 0;
        if (wrapper.__twTableResize?.resized) globalThis.twTable._fitTable(wrapper);

        resizable.forEach(function (entry) {
            const key = entry.col.id;
            let handle = existing.get(key);
            existing.delete(key);
            if (!handle) {
                handle = document.createElement('div');
                handle.dataset.twResizeKey = key;
                handle.setAttribute('role', 'separator');
                handle.setAttribute('aria-orientation', 'vertical');
                handle.setAttribute('tabindex', '0');
            }
            handle.className = layer.dataset.handleClass || '';
            // Keeps the separators in column order, which is their Tab order. A separator already in place is left
            // alone: moving the one being dragged would drop its pointer capture.
            const place = layer.children[order++] ?? null;
            if (place === null) {
                layer.append(handle);
            } else if (place !== handle) {
                place.before(handle);
            }

            const label = entry.col.dataset.twLabel || String(entry.index + 1);
            handle.setAttribute('aria-label', globalThis.twTable._format(layer.dataset.labelFormat, label));
            handle.setAttribute('aria-valuemin', entry.col.dataset.twMin || '48');
            handle.setAttribute('aria-valuemax', entry.col.dataset.twMax || '2000');

            const cell = globalThis.twTable._headerCell(wrapper, entry.index);
            if (!cell) {
                handle.hidden = true;
                return;
            }

            handle.hidden = false;
            const rect = cell.getBoundingClientRect();
            const edge = rtl ? rect.left : rect.right;
            handle.style.left = (edge - wrapperRect.left + wrapper.scrollLeft) + 'px';
            handle.style.top = (rect.top - wrapperRect.top + wrapper.scrollTop) + 'px';
            handle.style.height = rect.height + 'px';

            const width = Math.round(rect.width);
            handle.setAttribute('aria-valuenow', String(width));
            handle.setAttribute('aria-valuetext', globalThis.twTable._format(layer.dataset.valueFormat, width));
        });

        existing.forEach(function (handle) { handle.remove(); });
    },

    _table: function (wrapper) {
        return wrapper?.querySelector(':scope > table') ?? null;
    },

    // A full width table shares any spare width between its columns, so the other columns would grow or shrink as
    // one is resized and the border would drift away from the pointer. Once resized, the table is exactly as wide
    // as its columns.
    _fitTable: function (wrapper) {
        const table = globalThis.twTable._table(wrapper);
        if (!table) return;
        let total = 0;
        const sized = globalThis.twTable._columns(wrapper).every(function (entry) {
            const width = Number.parseFloat(entry.col.style.width);
            if (!entry.col.style.width.endsWith('px') || Number.isNaN(width)) return false;
            total += width;
            return true;
        });
        const fitted = sized && total > 0;
        table.style.width = fitted ? total + 'px' : '';
        if (wrapper.__twTableResize) wrapper.__twTableResize.resized = fitted;
    },

    // Gives every column its current width in pixels, so resizing one does not reflow the others, and returns them.
    _freezeWidths: function (wrapper) {
        const widths = {};
        const measured = globalThis.twTable._columns(wrapper).map(function (entry) {
            const cell = globalThis.twTable._headerCell(wrapper, entry.index);
            return { entry: entry, width: cell ? Math.round(cell.getBoundingClientRect().width) : null };
        });
        measured.forEach(function (item) {
            if (item.width === null || !item.entry.col.id) return;
            item.entry.col.style.width = item.width + 'px';
            widths[item.entry.col.id] = item.width;
        });
        globalThis.twTable._fitTable(wrapper);
        return widths;
    },

    _resize: function (wrapper, handle, width) {
        const col = globalThis.twTable._columns(wrapper).find(function (entry) { return entry.col.id === handle.dataset.twResizeKey; })?.col;
        if (!col) return null;
        const min = Number(col.dataset.twMin || 48);
        const max = Number(col.dataset.twMax || 2000);
        const clamped = Math.min(max, Math.max(min, Math.round(width)));
        col.style.width = clamped + 'px';
        globalThis.twTable._fitTable(wrapper);
        return clamped;
    },

    _commit: function (wrapper, widths) {
        globalThis.twTable.layoutResize(wrapper);
        const dotnetRef = wrapper.__twTableResize?.dotnetRef;
        if (dotnetRef) {
            void globalThis.twPicker._invokeSafely(dotnetRef, 'CommitColumnWidths', widths);
        }
    },

    attachResize: function (wrapper, dotnetRef) {
        if (!wrapper || wrapper.__twTableResize) return;

        const handleFor = function (e) {
            const handle = e.target?.closest?.('[data-tw-resize-key]') ?? null;
            return handle && wrapper.contains(handle) ? handle : null;
        };

        const onPointerDown = function (e) {
            const handle = handleFor(e);
            if (!handle || e.button !== 0) return;
            e.preventDefault();

            const widths = globalThis.twTable._freezeWidths(wrapper);
            const key = handle.dataset.twResizeKey;
            const startX = e.clientX;
            const startWidth = widths[key] ?? 0;
            const pointerId = e.pointerId;
            // The separator sits on the column's end border, so dragging towards the end widens it either way round.
            const towardsEnd = globalThis.twDirection.isRtl(wrapper) ? -1 : 1;
            try {
                handle.setPointerCapture?.(pointerId);
            } catch {
                // The pointer is already gone; the document listeners below still end the drag.
            }

            // Pointer events can arrive many times a frame, so the column is resized once per frame at the latest position.
            const schedule = globalThis.requestAnimationFrame ?? function (fn) { return setTimeout(fn, 0); };
            const cancel = globalThis.cancelAnimationFrame ?? clearTimeout;
            let latestX = startX;
            let frame = 0;
            const apply = function () {
                frame = 0;
                const width = globalThis.twTable._resize(wrapper, handle, startWidth + (latestX - startX) * towardsEnd);
                if (width !== null) {
                    widths[key] = width;
                    globalThis.twTable.layoutResize(wrapper);
                }
            };

            const root = document.documentElement;
            const previousCursor = root.style.cursor;
            const previousSelect = root.style.userSelect;
            root.style.cursor = 'col-resize';
            root.style.userSelect = 'none';
            handle.dataset.twResizing = '';

            const onMove = function (ev) {
                if (ev.pointerId !== pointerId) return;
                latestX = ev.clientX;
                if (!frame) frame = schedule(apply);
            };
            // Listening on the document, not the separator, still ends the drag if pointer capture is lost.
            const onEnd = function (ev) {
                if (ev.pointerId !== undefined && ev.pointerId !== pointerId) return;
                document.removeEventListener('pointermove', onMove);
                document.removeEventListener('pointerup', onEnd);
                document.removeEventListener('pointercancel', onEnd);
                if (frame) {
                    cancel(frame);
                    frame = 0;
                }
                if (ev.type !== 'pointercancel' && ev.clientX !== undefined) latestX = ev.clientX;
                apply();
                root.style.cursor = previousCursor;
                root.style.userSelect = previousSelect;
                delete handle.dataset.twResizing;
                if (handle.hasPointerCapture?.(pointerId)) handle.releasePointerCapture(pointerId);
                globalThis.twTable._commit(wrapper, widths);
            };

            document.addEventListener('pointermove', onMove);
            document.addEventListener('pointerup', onEnd);
            document.addEventListener('pointercancel', onEnd);
        };

        const onKeyDown = function (e) {
            const handle = handleFor(e);
            if (!handle || (e.key !== 'ArrowLeft' && e.key !== 'ArrowRight')) return;
            e.preventDefault();

            const widths = globalThis.twTable._freezeWidths(wrapper);
            const key = handle.dataset.twResizeKey;
            const step = e.shiftKey ? globalThis.twTable._largeKeyStep : globalThis.twTable._keyStep;
            // Arrow Right widens in a left to right layout and Arrow Left in a right to left one: the key always
            // moves the border the way it points.
            const widens = (e.key === 'ArrowRight') !== globalThis.twDirection.isRtl(wrapper);
            const width = globalThis.twTable._resize(wrapper, handle, (widths[key] ?? 0) + (widens ? step : -step));
            if (width === null) return;
            widths[key] = width;
            globalThis.twTable._commit(wrapper, widths);
        };

        let frame = 0;
        const relayout = function () {
            if (frame) return;
            const schedule = globalThis.requestAnimationFrame ?? function (fn) { return setTimeout(fn, 0); };
            frame = schedule(function () {
                frame = 0;
                globalThis.twTable.layoutResize(wrapper);
            });
        };

        let observer = null;
        if (typeof ResizeObserver !== 'undefined') {
            observer = new ResizeObserver(relayout);
            observer.observe(wrapper);
            const table = wrapper.querySelector(':scope > table');
            if (table) observer.observe(table);
        }

        wrapper.addEventListener('pointerdown', onPointerDown);
        wrapper.addEventListener('keydown', onKeyDown);
        wrapper.__twTableResize = { dotnetRef: dotnetRef, onPointerDown: onPointerDown, onKeyDown: onKeyDown, observer: observer };
        globalThis.twTable.layoutResize(wrapper);
    },

    detachResize: function (wrapper) {
        const state = wrapper?.__twTableResize;
        if (!state) return;
        wrapper.removeEventListener('pointerdown', state.onPointerDown);
        wrapper.removeEventListener('keydown', state.onKeyDown);
        state.observer?.disconnect();
        delete wrapper.__twTableResize;
        const layer = globalThis.twTable._layer(wrapper);
        if (layer) layer.replaceChildren();
    }
};

// Menu (TwMenu): positions the open menu against its button and moves focus between its items, following the ARIA
// menu button pattern. Escape and clicks outside are handled by twPicker.registerOutsideClick and the shared Escape
// stack; choosing an item is a click on its <button>, handled in .NET.
globalThis.twMenu = {
    _itemSelector: '[role="menuitem"], [role="menuitemcheckbox"], [role="menuitemradio"]',

    _items: function (menu) {
        return Array.from(menu.querySelectorAll(globalThis.twMenu._itemSelector)).filter(function (item) {
            return item.getAttribute('aria-disabled') !== 'true' && !item.hidden;
        });
    },

    _text: function (item) {
        return (item.textContent || '').trim().toLowerCase();
    },

    // Moves focus to the next item whose text starts with the typed character, after the focused one and wrapping.
    _typeahead: function (items, current, character) {
        const start = items.indexOf(current);
        for (let offset = 1; offset <= items.length; offset++) {
            const item = items[(start + offset) % items.length];
            if (globalThis.twMenu._text(item).startsWith(character)) return item;
        }
        return null;
    },

    focus: function (el) {
        if (!el) return;
        const target = el.matches?.('button, [tabindex]') ? el : el.querySelector('button, [tabindex]');
        target?.focus();
    },

    open: function (menu, anchor, dotnetRef, focusWhich) {
        if (!menu) return;

        // Fixed positioning keeps the menu clear of a scrolling container such as a table. On desktop a page scroll
        // closes it; on touch devices it follows its button instead.
        if (anchor) {
            globalThis.twPicker.registerScrollReposition(anchor, menu, false, dotnetRef);
        }

        if (!menu.__twMenuKeyDown) {
            const onKeyDown = function (e) {
                const items = globalThis.twMenu._items(menu);
                if (items.length === 0) return;
                const current = items.indexOf(document.activeElement);
                let target = null;

                switch (e.key) {
                    case 'ArrowDown':
                        target = items[(current + 1) % items.length];
                        break;
                    case 'ArrowUp':
                        target = items[(current - 1 + items.length) % items.length];
                        break;
                    case 'Home':
                        target = items[0];
                        break;
                    case 'End':
                        target = items.at(-1);
                        break;
                    case 'Tab':
                        // Tab leaves the menu: close it and let the browser carry focus on.
                        if (dotnetRef) void globalThis.twPicker._invokeSafely(dotnetRef, 'CloseFromTab');
                        return;
                    case ' ':
                        // A menu item is a <button>, so Space clicks it; this only stops the page scrolling.
                        return;
                    default:
                        if (e.key.length === 1 && !e.ctrlKey && !e.metaKey && !e.altKey && e.key.trim() !== '') {
                            target = globalThis.twMenu._typeahead(items, items[current] ?? null, e.key.toLowerCase());
                            if (!target) return;
                        } else {
                            return;
                        }
                }

                e.preventDefault();
                target?.focus();
            };
            menu.addEventListener('keydown', onKeyDown);
            menu.__twMenuKeyDown = onKeyDown;
        }

        const items = globalThis.twMenu._items(menu);
        const first = focusWhich === 'last' ? items.at(-1) : items[0];
        (first ?? menu).focus();
    },

    close: function (menu) {
        if (!menu) return;
        globalThis.twPicker.unregisterScrollReposition(menu);
        if (menu.__twMenuKeyDown) {
            menu.removeEventListener('keydown', menu.__twMenuKeyDown);
            delete menu.__twMenuKeyDown;
        }
    }
};

// Popover: a floating panel anchored to its trigger. The panel is fixed, so a scrolling or clipping container
// cannot cut it off. It opens on the requested side, flips to the opposite side when that has more room, and is
// kept inside the viewport along the other axis.
globalThis.twPopover = {
    _clamp: function (value, min, max) {
        return max < min ? min : Math.min(Math.max(value, min), max);
    },

    _viewport: function () {
        const height = (typeof window !== 'undefined' && window.visualViewport)
            ? window.visualViewport.height
            : document.documentElement.clientHeight;
        return { width: document.documentElement.clientWidth, height: height };
    },

    // Places the panel against the anchor and returns the side it ended up on. Start and end follow the layout
    // direction, so a right to left page opens "start" on the right.
    position: function (anchor, panel, placement, align, offset) {
        if (!anchor || !panel) return null;

        const gap = globalThis.twPicker._panelEdgeGapPx;
        const space = Number(offset) || 0;
        const viewport = globalThis.twPopover._viewport();
        const rtl = globalThis.twDirection.isRtl(anchor);

        panel.style.position = 'fixed';
        panel.style.maxHeight = '';
        panel.style.maxWidth = Math.max(viewport.width - 2 * gap, 0) + 'px';
        const block = globalThis.twPicker._fixedContainingBlock(panel);
        panel.style.left = '0px';
        panel.style.top = '0px';
        panel.style.right = 'auto';
        panel.style.bottom = 'auto';

        const a = anchor.getBoundingClientRect();
        const size = panel.getBoundingClientRect();
        const width = size.width;
        const height = size.height;

        let side = placement || 'bottom';
        if (side === 'start') side = rtl ? 'right' : 'left';
        else if (side === 'end') side = rtl ? 'left' : 'right';

        const room = {
            top: a.top - space,
            bottom: viewport.height - a.bottom - space,
            left: a.left - space,
            right: viewport.width - a.right - space
        };
        const opposite = { top: 'bottom', bottom: 'top', left: 'right', right: 'left' };
        const vertical = side === 'top' || side === 'bottom';
        const needed = vertical ? height : width;
        if (room[side] < needed && room[opposite[side]] > room[side]) side = opposite[side];

        let x;
        let y;
        if (side === 'top' || side === 'bottom') {
            const startX = rtl ? a.right - width : a.left;
            const endX = rtl ? a.left : a.right - width;
            if (align === 'center') x = a.left + (a.width - width) / 2;
            else if (align === 'end') x = endX;
            else x = startX;
            x = globalThis.twPopover._clamp(x, gap, viewport.width - width - gap);

            const available = room[side] - gap;
            let shown = height;
            if (available > 0 && height > available) {
                panel.style.maxHeight = available + 'px';
                shown = available;
            }
            y = side === 'bottom' ? a.bottom + space : a.top - space - shown;
        } else {
            x = side === 'right' ? a.right + space : a.left - space - width;
            if (align === 'center') y = a.top + (a.height - height) / 2;
            else if (align === 'end') y = a.bottom - height;
            else y = a.top;
            y = globalThis.twPopover._clamp(y, gap, viewport.height - height - gap);
        }

        panel.style.left = (x - block.left) + 'px';
        panel.style.top = (y - block.top) + 'px';
        panel.dataset.twSide = side;
        return side;
    },

    open: function (root, anchor, panel, dotnetRef, options) {
        if (!root || !anchor || !panel) return;
        const settings = options || {};
        const place = function () {
            globalThis.twPopover.position(anchor, panel, settings.placement, settings.align, settings.offset);
        };
        place();

        // The panel follows its trigger as the page scrolls or resizes. A scroll inside the panel is left alone.
        const reposition = function (e) {
            if (e && e.target instanceof Node && panel.contains(e.target)) return;
            if (panel.isConnected) place();
        };
        document.addEventListener('scroll', reposition, true);
        window.addEventListener('resize', reposition);
        panel.__twPopoverReposition = reposition;

        if (settings.closeOnOutsideClick !== false) {
            globalThis.twPicker.registerOutsideClick(root, dotnetRef, null);
        }

        // Escape returns focus to the trigger, so it has its own entry rather than the outside click's.
        globalThis.twDialog._pushEscape({
            surface: root,
            dotnetRef: dotnetRef,
            method: 'CloseFromEscape',
            isOpen: function () { return panel.isConnected; }
        });

        // Tabbing out of the popover closes it. A click on nothing focusable leaves relatedTarget empty, and is
        // handled as an outside click instead.
        const onFocusOut = function (e) {
            const next = e.relatedTarget;
            if (next instanceof Node && !root.contains(next) && panel.isConnected) {
                void globalThis.twPicker._invokeSafely(dotnetRef, 'CloseFromFocusOut');
            }
        };
        root.addEventListener('focusout', onFocusOut);
        root.__twPopoverFocusOut = onFocusOut;

        if (settings.autoFocus !== false) {
            globalThis.twDialog.focusPanel(panel);
        }
    },

    close: function (root, panel) {
        if (panel?.__twPopoverReposition) {
            document.removeEventListener('scroll', panel.__twPopoverReposition, true);
            window.removeEventListener('resize', panel.__twPopoverReposition);
            delete panel.__twPopoverReposition;
        }
        if (!root) return;
        globalThis.twPicker.unregisterOutsideClick(root, null);
        globalThis.twDialog.unregisterEscape(root);
        if (root.__twPopoverFocusOut) {
            root.removeEventListener('focusout', root.__twPopoverFocusOut);
            delete root.__twPopoverFocusOut;
        }
    }
};

// On this page: marks the link to the section currently being read with aria-current="location".
globalThis.twOnThisPage = {
    _stops: new Map(),

    observe: function (key, nav, ids, offset) {
        globalThis.twOnThisPage.dispose(key);
        if (!nav || !ids || ids.length === 0) return;

        const update = function (event) {
            const scroller = event?.target instanceof Element ? event.target : null;
            const atBottom = scroller !== null &&
                scroller.scrollTop > 0 &&
                scroller.scrollTop + scroller.clientHeight >= scroller.scrollHeight - 2;

            let active = ids[0];

            ids.forEach(function (id) {
                const section = document.getElementById(id);
                if (section && section.getBoundingClientRect().top <= offset) {
                    active = id;
                }
            });

            // The last section can be too short to ever reach the offset line.
            if (atBottom) {
                active = ids[ids.length - 1];
            }

            nav.querySelectorAll('[data-tw-section-id]').forEach(function (link) {
                if (link.dataset.twSectionId === active) {
                    link.setAttribute('aria-current', 'location');
                } else {
                    link.removeAttribute('aria-current');
                }
            });
        };

        // Scroll events don't bubble, and the content may scroll inside its own container rather than the
        // window, so listen in the capture phase on the document.
        document.addEventListener('scroll', update, { capture: true, passive: true });
        update();

        globalThis.twOnThisPage._stops.set(key, function () {
            document.removeEventListener('scroll', update, { capture: true });
        });
    },

    dispose: function (key) {
        const stop = globalThis.twOnThisPage._stops.get(key);
        if (!stop) return;
        stop();
        globalThis.twOnThisPage._stops.delete(key);
    }
};

globalThis.twDataTable = {
    loadState: function (key) {
        try {
            return globalThis.localStorage.getItem(key);
        } catch {
            // Storage is blocked (private browsing, a sandboxed frame); the table starts from its parameters.
            return null;
        }
    },

    saveState: function (key, json) {
        try {
            globalThis.localStorage.setItem(key, json);
        } catch {
            // Storage is blocked or full; the table still works, it just will not be restored next time.
        }
    },

    focus: function (id) {
        const el = document.getElementById(id);
        if (el) {
            el.focus();
        }
    },

    // Column filter: a dialog positioned under its filter button. A press that starts and ends outside both closes it,
    // Escape closes it through the shared Escape stack, and Tab stays inside it while it is open.
    openFilter: function (panel, anchorId, dotnetRef) {
        if (!panel || panel.__twDataTableFilter) return;

        const anchor = document.getElementById(anchorId);
        if (anchor) {
            globalThis.twPicker.registerScrollReposition(anchor, panel, false, dotnetRef);
        }

        const inside = function (target) {
            return target instanceof Node && (panel.contains(target) || (anchor?.contains(target) ?? false));
        };
        let downTarget = null;
        const onPointerDown = function (e) { downTarget = e.target; };
        const onPointerUp = function (e) {
            const target = downTarget;
            downTarget = null;
            if (target && !inside(target) && !inside(e.target)) {
                void globalThis.twPicker._invokeSafely(dotnetRef, 'Close');
            }
        };

        document.addEventListener('pointerdown', onPointerDown);
        document.addEventListener('pointerup', onPointerUp);
        panel.__twDataTableFilter = { onPointerDown: onPointerDown, onPointerUp: onPointerUp };
        globalThis.twDialog._pushEscape({ surface: panel, dotnetRef: dotnetRef, method: 'Close' });
        globalThis.twDialog.trapFocus(panel);
        globalThis.twDialog.focusPanel(panel);
    },

    closeFilter: function (panel) {
        const state = panel?.__twDataTableFilter;
        if (!state) return;
        document.removeEventListener('pointerdown', state.onPointerDown);
        document.removeEventListener('pointerup', state.onPointerUp);
        delete panel.__twDataTableFilter;
        globalThis.twPicker.unregisterScrollReposition(panel);
        globalThis.twDialog.unregisterEscape(panel);
        globalThis.twDialog.releaseFocusTrap(panel);
    },

    download: async function (fileName, contentType, streamReference) {
        const buffer = await streamReference.arrayBuffer();
        const url = URL.createObjectURL(new Blob([buffer], { type: contentType }));
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        link.style.display = 'none';
        document.body.appendChild(link);
        link.click();
        link.remove();
        // Revoking straight away can cancel the download in some browsers, so wait a turn.
        setTimeout(function () { URL.revokeObjectURL(url); }, 0);
    }
};
