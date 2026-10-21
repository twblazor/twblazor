// @vitest-environment jsdom

import { afterEach, describe, expect, test } from 'vitest';

await import('../../../src/TwBlazor/wwwroot/js/twblazor.js');

function buildForm(html) {
    const form = document.createElement('form');
    form.id = 'form';
    form.innerHTML = html;
    document.body.appendChild(form);
    // jsdom has no layout, so the visibility filter needs every element to report a box.
    form.querySelectorAll('*').forEach(function (el) { el.getClientRects = () => [{}]; });
    return form;
}

describe('twForm', () => {
    afterEach(() => {
        document.body.innerHTML = '';
    });

    test('focusFirstInvalid focuses the first field marked invalid', () => {
        buildForm(`
            <input id="name">
            <input id="email" aria-invalid="true">
            <input id="age" aria-invalid="true">`);

        expect(globalThis.twForm.focusFirstInvalid('form')).toBe(true);
        expect(document.activeElement.id).toBe('email');
    });

    test('focusFirstInvalid skips disabled fields', () => {
        buildForm(`
            <input id="locked" aria-invalid="true" disabled>
            <input id="email" aria-invalid="true">`);

        globalThis.twForm.focusFirstInvalid('form');
        expect(document.activeElement.id).toBe('email');
    });

    test('focusFirstInvalid reports when there is nothing to focus', () => {
        buildForm('<input id="name">');

        expect(globalThis.twForm.focusFirstInvalid('form')).toBe(false);
        expect(globalThis.twForm.focusFirstInvalid('missing')).toBe(false);
        expect(globalThis.twForm.focusFirstInvalid(null)).toBe(false);
    });

    test('focusFirstInvalid moves into an invalid radio group', () => {
        buildForm(`
            <fieldset id="room" aria-invalid="true">
                <input type="radio" id="single" name="room">
                <input type="radio" id="double" name="room">
            </fieldset>`);

        globalThis.twForm.focusFirstInvalid('form');
        expect(document.activeElement.id).toBe('single');
    });

    test('focusField focuses a control directly', () => {
        buildForm('<input id="name">');

        expect(globalThis.twForm.focusField('name')).toBe(true);
        expect(document.activeElement.id).toBe('name');
    });

    test('focusField prefers the checked option of a group', () => {
        buildForm(`
            <fieldset id="room">
                <input type="radio" id="single" name="room">
                <input type="radio" id="double" name="room" checked>
            </fieldset>`);

        globalThis.twForm.focusField('room');
        expect(document.activeElement.id).toBe('double');
    });

    test('focusField reports a missing or empty target', () => {
        buildForm('<fieldset id="empty"></fieldset>');

        expect(globalThis.twForm.focusField('missing')).toBe(false);
        expect(globalThis.twForm.focusField('empty')).toBe(false);
    });
});
