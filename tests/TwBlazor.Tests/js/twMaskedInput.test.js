// @vitest-environment jsdom

import { afterEach, describe, expect, test } from 'vitest';

// Load the script once - it assigns window.twMaskedInput on evaluation.
await import('../../../src/TwBlazor/wwwroot/js/twblazor.js');

const masked = globalThis.twMaskedInput;

const phoneMask = '(999) 999-9999';

// The <input> TwMaskedInput renders: a plain text field, with the mask kept in the element's own state.
function buildInput() {
    const el = document.createElement('input');
    el.type = 'text';
    document.body.appendChild(el);
    return el;
}

// Types into the field the way a user does: set the value, then let the input event fire.
function type(el, value) {
    el.value = value;
    el.dispatchEvent(new Event('input', { bubbles: true }));
}

describe('twMaskedInput', () => {
    afterEach(() => {
        document.body.innerHTML = '';
    });

    describe('_tokenType', () => {
        test('maps the three mask tokens to their fillable types', () => {
            expect(masked._tokenType('9')).toBe('digit');
            expect(masked._tokenType('A')).toBe('letter');
            expect(masked._tokenType('*')).toBe('alphanumeric');
        });

        test('treats every other character as a literal', () => {
            expect(masked._tokenType('-')).toBeNull();
            expect(masked._tokenType('(')).toBeNull();
            expect(masked._tokenType(' ')).toBeNull();
            expect(masked._tokenType('_')).toBeNull();
        });
    });

    describe('_matches', () => {
        test('a digit position accepts digits only', () => {
            expect(masked._matches('digit', '0')).toBe(true);
            expect(masked._matches('digit', '9')).toBe(true);
            expect(masked._matches('digit', 'a')).toBe(false);
            expect(masked._matches('digit', '-')).toBe(false);
        });

        test('a letter position accepts letters in either case', () => {
            expect(masked._matches('letter', 'A')).toBe(true);
            expect(masked._matches('letter', 'z')).toBe(true);
            expect(masked._matches('letter', '4')).toBe(false);
        });

        test('an alphanumeric position accepts letters and digits', () => {
            expect(masked._matches('alphanumeric', '7')).toBe(true);
            expect(masked._matches('alphanumeric', 'Q')).toBe(true);
            expect(masked._matches('alphanumeric', 'q')).toBe(true);
            expect(masked._matches('alphanumeric', '_')).toBe(false);
        });

        test('an unknown token type matches nothing', () => {
            expect(masked._matches(null, '1')).toBe(false);
            expect(masked._matches('literal', '1')).toBe(false);
        });
    });

    describe('_strip', () => {
        test('returns the raw characters from a fully masked value', () => {
            expect(masked._strip(phoneMask, '(123) 456-7890', '_')).toBe('1234567890');
        });

        test('ignores the placeholder and literal characters', () => {
            expect(masked._strip(phoneMask, '(12_) ___-____', '_')).toBe('12');
            expect(masked._strip(phoneMask, '(___) ___-____', '_')).toBe('');
        });

        test('works with a letter token', () => {
            expect(masked._strip('AAA-9999', 'ABC-1234', '_')).toBe('ABC1234');
            expect(masked._strip('AAA-9999', 'AB__-____', '_')).toBe('AB');
        });

        test('drops characters the token does not accept', () => {
            expect(masked._strip('9999', '12a4', '_')).toBe('124');
        });

        test('stops once the mask runs out of positions', () => {
            expect(masked._strip('999', '123456', '_')).toBe('123');
            expect(masked._strip('***', 'ab', '_')).toBe('ab');
        });
    });

    describe('_format', () => {
        test('an empty value shows the whole mask while showMask is on', () => {
            expect(masked._format(phoneMask, '', '_', true)).toBe('(___) ___-____');
        });

        test('an empty value stays empty when showMask is off', () => {
            expect(masked._format(phoneMask, '', '_', false)).toBe('');
        });

        test('fills the mask from the raw characters', () => {
            expect(masked._format(phoneMask, '1234567890', '_', true)).toBe('(123) 456-7890');
        });

        test('a partially filled value keeps the rest of the mask while showMask is on', () => {
            expect(masked._format(phoneMask, '123', '_', true)).toBe('(123) ___-____');
        });

        test('a partially filled value is truncated after the last typed character when showMask is off', () => {
            expect(masked._format(phoneMask, '123', '_', false)).toBe('(123');
            expect(masked._format('AAA-9999', 'ABC1234', '_', false)).toBe('ABC-1234');
        });

        test('honours a custom placeholder character', () => {
            expect(masked._format(phoneMask, '', '#', true)).toBe('(###) ###-####');
        });

        test('ignores raw characters past the end of the mask', () => {
            expect(masked._format('(999)', '123456', '_', true)).toBe('(123)');
            expect(masked._format('(999)', '123456', '_', false)).toBe('(123');
        });

        test('stops filling at the first raw character the token rejects', () => {
            expect(masked._format('9999', '12A4', '_', true)).toBe('12__');
            expect(masked._format('(999) 999', '12X456', '_', true)).toBe('(12_) ___');
            expect(masked._format('***', 'a-b', '_', true)).toBe('a__');
        });

        test('treats a non-ASCII digit or letter as a mismatch', () => {
            expect(masked._format('9999', '١٢٣٤', '_', true)).toBe('____');
            expect(masked._format('****', 'abä', '_', true)).toBe('ab__');
        });
    });

    describe('_caret', () => {
        test('is the index of the first token, after any leading literals', () => {
            expect(masked._caret(phoneMask, 0)).toBe(1);
            expect(masked._caret('()999', 0)).toBe(2);
        });

        test('is the index of the next unfilled token for a partly typed value', () => {
            expect(masked._caret(phoneMask, 3)).toBe(6);
            expect(masked._caret('999 999', 2)).toBe(2);
        });

        test('is the length of the mask once every position is filled', () => {
            expect(masked._caret(phoneMask, 10)).toBe(14);
            expect(masked._caret('999 999', 6)).toBe(7);
        });

        test('is the length of the mask when the value is longer than the mask', () => {
            expect(masked._caret(phoneMask, 11)).toBe(14);
            expect(masked._caret('999', 4)).toBe(3);
        });

        test('is the length of a mask that has no tokens at all', () => {
            expect(masked._caret('()-', 0)).toBe(3);
            expect(masked._caret('', 0)).toBe(0);
        });

        test('counts tokens rather than searching for the placeholder character', () => {
            expect(masked._caret('999 999', 0)).toBe(0);
            expect(masked._caret('999 999', 3)).toBe(4);
        });
    });

    describe('attach', () => {
        test('reformats the field on every input event', () => {
            const el = buildInput();
            masked.attach(el, phoneMask, '_', true);

            type(el, '1234567890');

            expect(el.value).toBe('(123) 456-7890');
        });

        test('keeps the leading literals when only part of the value is typed', () => {
            const el = buildInput();
            masked.attach(el, phoneMask, '_', true);

            type(el, '1234');

            expect(el.value).toBe('(123) 4__-____');
        });

        test('removes characters the mask does not accept', () => {
            const el = buildInput();
            masked.attach(el, 'AAA-9999', '_', true);

            type(el, 'ab1c-12x34');

            expect(el.value).toBe('abc-1234');
        });

        test('leaves the caret at the first unfilled position', () => {
            const el = buildInput();
            masked.attach(el, phoneMask, '_', true);

            type(el, '123');

            expect(el.value).toBe('(123) ___-____');
            expect(el.selectionStart).toBe(6);
            expect(el.selectionEnd).toBe(6);
        });

        test('a paste is reformatted, whether it arrives raw or already masked', () => {
            const pastedRaw = buildInput();
            masked.attach(pastedRaw, phoneMask, '_', true);
            const pastedMasked = buildInput();
            masked.attach(pastedMasked, phoneMask, '_', true);

            type(pastedRaw, '1234567890');
            type(pastedMasked, '(555) 010-9999');

            expect(pastedRaw.value).toBe('(123) 456-7890');
            expect(pastedMasked.value).toBe('(555) 010-9999');
        });

        test('backspacing the last character refills it with the placeholder and moves the caret back', () => {
            const el = buildInput();
            masked.attach(el, phoneMask, '_', true);
            type(el, '1234567890');
            expect(el.selectionStart).toBe(14);

            // The browser has already removed the character by the time the input event arrives.
            type(el, '(123) 456-789');

            expect(el.value).toBe('(123) 456-789_');
            expect(el.selectionStart).toBe(13);
            expect(el.selectionEnd).toBe(13);
        });

        test('drops a character typed into an already full field, leaving the caret at the end', () => {
            const el = buildInput();
            masked.attach(el, phoneMask, '_', true);

            type(el, '(123) 456-78905');

            expect(el.value).toBe('(123) 456-7890');
            expect(el.selectionStart).toBe(14);
        });

        test('redraws the mask when the field is emptied', () => {
            const el = buildInput();
            masked.attach(el, phoneMask, '_', true);
            type(el, '1234567890');

            type(el, '');

            expect(el.value).toBe('(___) ___-____');
            expect(el.selectionStart).toBe(1);
        });

        test('is idempotent, keeping a single listener', () => {
            const el = buildInput();
            masked.attach(el, phoneMask, '_', true);
            const handler = el.__twMaskedInput;

            masked.attach(el, phoneMask, '_', true);

            expect(el.__twMaskedInput).toBe(handler);
            type(el, '1234567890');
            expect(el.value).toBe('(123) 456-7890');
        });

        test('ignores input raised while an IME composition is in progress', () => {
            const el = buildInput();
            masked.attach(el, phoneMask, '_', true);
            el.value = '(12';
            const event = new Event('input', { bubbles: true });
            Object.defineProperty(event, 'isComposing', { value: true });

            el.dispatchEvent(event);

            expect(el.value).toBe('(12');
        });

        test('does nothing for a missing element', () => {
            expect(() => masked.attach(null, phoneMask, '_', true)).not.toThrow();
        });
    });

    describe('placeholder character that is also a literal', () => {
        test('_format still draws the literal space between filled positions', () => {
            expect(masked._format('999 999', '123456', ' ', true)).toBe('123 456');
            expect(masked._format('999 999', '', ' ', true)).toBe('       ');
        });

        test('_strip ignores every space, so nothing is regained from the padding', () => {
            expect(masked._strip('999 999', '12     ', ' ')).toBe('12');
            expect(masked._strip('999 999', '       ', ' ')).toBe('');
        });

        test('_caret counts tokens instead of searching for the placeholder', () => {
            expect(masked._caret('999 999', 3)).toBe(4);
            expect(masked._caret('999 999', 6)).toBe(7);
        });

        test('attach formats and places the caret without an indexOf collision', () => {
            const el = buildInput();
            masked.attach(el, '999 999', ' ', true);

            type(el, '123');

            expect(el.value).toBe('123    ');
            expect(el.selectionStart).toBe(4);
        });
    });

    describe('update', () => {
        test('refreshes the mask used by an already attached field', () => {
            const el = buildInput();
            masked.attach(el, '999', '_', true);

            masked.update(el, 'AAAA', '_', true);
            type(el, 'ab');

            expect(el.value).toBe('ab__');
        });

        test('refreshes showMask for an already attached field', () => {
            const el = buildInput();
            masked.attach(el, phoneMask, '_', true);
            expect(el.__twMaskedInputState.showMask).toBe(true);

            masked.update(el, phoneMask, '_', false);

            expect(el.__twMaskedInputState.showMask).toBe(false);
        });

        test('does nothing for a missing element', () => {
            expect(() => masked.update(null, phoneMask, '_', true)).not.toThrow();
        });
    });

    describe('detach', () => {
        test('removes the input listener', () => {
            const el = buildInput();
            masked.attach(el, phoneMask, '_', true);

            masked.detach(el);
            type(el, '1234567890');

            expect(el.value).toBe('1234567890');
            expect(el.__twMaskedInput).toBeUndefined();
            expect(el.__twMaskedInputState).toBeUndefined();
        });

        test('allows the field to be attached again afterwards', () => {
            const el = buildInput();
            masked.attach(el, phoneMask, '_', true);
            masked.detach(el);

            masked.attach(el, phoneMask, '_', true);
            type(el, '1234567890');

            expect(el.value).toBe('(123) 456-7890');
        });

        test('does nothing for an element that was never attached', () => {
            expect(() => masked.detach(buildInput())).not.toThrow();
            expect(() => masked.detach(null)).not.toThrow();
        });
    });
});
