// @vitest-environment jsdom

import { afterEach, describe, expect, test, vi } from 'vitest';

// Load the script once - it assigns window.twTagInput on evaluation.
await import('../../../src/TwBlazor/wwwroot/js/twblazor.js');

const tagInput = globalThis.twTagInput;

// A plain text field, as TwTagInput renders: the listeners hang off the element itself.
function buildInput() {
    const el = document.createElement('input');
    el.type = 'text';
    document.body.appendChild(el);
    return el;
}

function keydown(target, keyName, props = {}) {
    const event = new KeyboardEvent('keydown', { key: keyName, bubbles: true, cancelable: true });
    for (const [name, value] of Object.entries(props)) {
        Object.defineProperty(event, name, { value });
    }
    target.dispatchEvent(event);
    return event;
}

function paste(target, text) {
    const event = new Event('paste', { bubbles: true, cancelable: true });
    Object.defineProperty(event, 'clipboardData', { value: { getData: () => text } });
    target.dispatchEvent(event);
    return event;
}

function dotnetRef() {
    return { invokeMethodAsync: vi.fn() };
}

describe('twTagInput', () => {
    afterEach(() => {
        vi.restoreAllMocks();
        document.body.innerHTML = '';
    });

    describe('_splits', () => {
        test('splits on separators and on line breaks', () => {
            expect(tagInput._splits('a, b', ',')).toBe(true);
            expect(tagInput._splits('a; b', ';')).toBe(true);
            expect(tagInput._splits('a\nb', '')).toBe(true);
            expect(tagInput._splits('a\tb', '')).toBe(true);
            expect(tagInput._splits('a\rb', '')).toBe(true);
            expect(tagInput._splits('plain', ',')).toBe(false);
        });
    });

    describe('attach', () => {
        test('Enter commits the field value', () => {
            const el = buildInput();
            const ref = dotnetRef();
            tagInput.attach(el, ref, ',');
            el.value = 'Blazor';

            const event = keydown(el, 'Enter');

            expect(event.defaultPrevented).toBe(true);
            expect(ref.invokeMethodAsync).toHaveBeenCalledWith('CommitText', 'Blazor');
        });

        test('a separator key commits the text without typing the character', () => {
            const el = buildInput();
            const ref = dotnetRef();
            tagInput.attach(el, ref, ',');
            el.value = 'red';

            const event = keydown(el, ',');

            expect(event.defaultPrevented).toBe(true);
            expect(ref.invokeMethodAsync).toHaveBeenCalledWith('CommitText', 'red');
        });

        test('Enter in an empty field keeps the form submit', () => {
            const el = buildInput();
            const ref = dotnetRef();
            tagInput.attach(el, ref, ',');
            el.value = '   ';

            const event = keydown(el, 'Enter');

            expect(event.defaultPrevented).toBe(false);
            expect(ref.invokeMethodAsync).not.toHaveBeenCalled();
        });

        test('an already-handled key is left alone', () => {
            const el = buildInput();
            const ref = dotnetRef();
            tagInput.attach(el, ref, ',');
            el.value = 'Blazor';

            // twAutocomplete is attached first and, for a highlighted suggestion, prevents the default,
            // so the tag input must hand the key off instead of committing it again.
            const event = new KeyboardEvent('keydown', { key: 'Enter', bubbles: true, cancelable: true });
            event.preventDefault();
            el.dispatchEvent(event);

            expect(ref.invokeMethodAsync).not.toHaveBeenCalled();
        });

        test('ignores keys during IME composition', () => {
            const el = buildInput();
            const ref = dotnetRef();
            tagInput.attach(el, ref, ',');
            el.value = 'Blazor';

            const composing = keydown(el, 'Enter', { isComposing: true });
            const processing = keydown(el, 'Enter', { keyCode: 229 });

            expect(composing.defaultPrevented).toBe(false);
            expect(processing.defaultPrevented).toBe(false);
            expect(ref.invokeMethodAsync).not.toHaveBeenCalled();
        });

        test('leaves read-only and disabled fields alone', () => {
            const readOnly = buildInput();
            readOnly.readOnly = true;
            const readOnlyRef = dotnetRef();
            tagInput.attach(readOnly, readOnlyRef, ',');
            readOnly.value = 'Blazor';

            const disabled = buildInput();
            disabled.disabled = true;
            const disabledRef = dotnetRef();
            tagInput.attach(disabled, disabledRef, ',');
            disabled.value = 'Blazor';

            const readOnlyEvent = keydown(readOnly, 'Enter');
            const disabledEvent = keydown(disabled, 'Enter');

            expect(readOnlyEvent.defaultPrevented).toBe(false);
            expect(disabledEvent.defaultPrevented).toBe(false);
            expect(readOnlyRef.invokeMethodAsync).not.toHaveBeenCalled();
            expect(disabledRef.invokeMethodAsync).not.toHaveBeenCalled();
        });

        test('pasting text that splits is handed to .NET as one combined value', () => {
            const el = buildInput();
            const ref = dotnetRef();
            tagInput.attach(el, ref, ',');
            el.value = 'pre';
            el.selectionStart = 3;
            el.selectionEnd = 3;

            const event = paste(el, 'a,b');

            expect(event.defaultPrevented).toBe(true);
            expect(ref.invokeMethodAsync).toHaveBeenCalledWith('PasteText', 'prea,b');
        });

        test('a paste without a separator is left to the browser', () => {
            const el = buildInput();
            const ref = dotnetRef();
            tagInput.attach(el, ref, ',');

            const event = paste(el, 'plain');

            expect(event.defaultPrevented).toBe(false);
            expect(ref.invokeMethodAsync).not.toHaveBeenCalled();
        });

        test('attach is idempotent, keeping a single pair of listeners', () => {
            const el = buildInput();
            const ref = dotnetRef();
            const add = vi.spyOn(el, 'addEventListener');
            tagInput.attach(el, ref, ',');
            const handler = el.__twTagInput;

            tagInput.attach(el, ref, ',');

            expect(el.__twTagInput).toBe(handler);
            expect(add).toHaveBeenCalledTimes(2); // keydown + paste

            el.value = 'Blazor';
            keydown(el, 'Enter');
            expect(ref.invokeMethodAsync).toHaveBeenCalledTimes(1);
        });

        test('does nothing for a missing element', () => {
            expect(() => tagInput.attach(null, dotnetRef(), ',')).not.toThrow();
        });
    });

    describe('update', () => {
        test('replaces the separators used by an already attached field', () => {
            const el = buildInput();
            const ref = dotnetRef();
            tagInput.attach(el, ref, ',');
            el.value = 'red';

            tagInput.update(el, ';');

            const comma = keydown(el, ',');
            expect(comma.defaultPrevented).toBe(false);

            const semicolon = keydown(el, ';');
            expect(semicolon.defaultPrevented).toBe(true);
            expect(ref.invokeMethodAsync).toHaveBeenCalledWith('CommitText', 'red');
        });

        test('does nothing for a field that was never attached', () => {
            const el = buildInput();
            expect(() => tagInput.update(el, ',')).not.toThrow();
            expect(() => tagInput.update(null, ',')).not.toThrow();
        });
    });

    describe('detach', () => {
        test('removes the listeners', () => {
            const el = buildInput();
            const ref = dotnetRef();
            tagInput.attach(el, ref, ',');
            el.value = 'Blazor';

            tagInput.detach(el);
            const event = keydown(el, 'Enter');

            expect(event.defaultPrevented).toBe(false);
            expect(ref.invokeMethodAsync).not.toHaveBeenCalled();
            expect(el.__twTagInput).toBeUndefined();
        });

        test('does nothing for an element that was never attached', () => {
            expect(() => tagInput.detach(buildInput())).not.toThrow();
            expect(() => tagInput.detach(null)).not.toThrow();
        });
    });
});
