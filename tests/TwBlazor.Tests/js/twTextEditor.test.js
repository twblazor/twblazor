// @vitest-environment jsdom

import { afterEach, beforeEach, describe, expect, test, vi } from 'vitest';

await import('../../../src/TwBlazor/wwwroot/js/twblazor.js');

const editor = globalThis.twTextEditor;

// The editing area TwTextEditor renders, with a toolbar holding a Tab stop. jsdom has no editing commands, so
// document.execCommand is a mock each test can inspect.
function build({ html = '', editable = true, toolbar = true } = {}) {
    const root = document.createElement('div');
    root.innerHTML = `
        ${toolbar ? '<div id="ed-toolbar" role="toolbar"><button id="bold" tabindex="0">B</button><button id="italic" tabindex="-1">I</button></div>' : ''}
        <div id="ed" role="textbox" contenteditable="${editable ? 'true' : 'false'}"></div>
        <input id="url" />`;
    document.body.appendChild(root);
    const area = root.querySelector('#ed');
    area.innerHTML = html;
    const dotnetRef = { invokeMethodAsync: vi.fn(() => Promise.resolve()) };
    return { area, dotnetRef, bold: root.querySelector('#bold'), url: root.querySelector('#url') };
}

function select(node, start, end = start) {
    const range = document.createRange();
    range.setStart(node, start);
    range.setEnd(node, end);
    const selection = document.getSelection();
    selection.removeAllRanges();
    selection.addRange(range);
    return range;
}

function keydown(target, key, props = {}) {
    const event = new KeyboardEvent('keydown', { key, bubbles: true, cancelable: true, ...props });
    target.dispatchEvent(event);
    return event;
}

function calls(dotnetRef, method) {
    return dotnetRef.invokeMethodAsync.mock.calls.filter(c => c[0] === method);
}

describe('twTextEditor', () => {
    beforeEach(() => {
        document.execCommand = vi.fn(() => true);
    });

    afterEach(() => {
        document.querySelectorAll('[role="textbox"]').forEach(el => editor.detach(el));
        delete document.execCommand;
        delete document.queryCommandState;
        vi.useRealTimers();
        vi.restoreAllMocks();
        document.body.innerHTML = '';
    });

    describe('safeHref', () => {
        test('keeps web, email, phone and relative addresses', () => {
            expect(editor.safeHref('https://example.com')).toBe('https://example.com');
            expect(editor.safeHref('http://example.com')).toBe('http://example.com');
            expect(editor.safeHref('mailto:ada@example.com')).toBe('mailto:ada@example.com');
            expect(editor.safeHref('tel:+441234')).toBe('tel:+441234');
            expect(editor.safeHref('/docs')).toBe('/docs');
            expect(editor.safeHref('#top')).toBe('#top');
        });

        test('refuses script and data addresses, even when disguised', () => {
            expect(editor.safeHref('javascript:alert(1)')).toBeNull();
            expect(editor.safeHref('JavaScript:alert(1)')).toBeNull();
            expect(editor.safeHref('java\nscript:alert(1)')).toBeNull();
            expect(editor.safeHref(' \u0001javascript:alert(1)')).toBeNull();
            expect(editor.safeHref('data:text/html,<script>')).toBeNull();
            expect(editor.safeHref('vbscript:x')).toBeNull();
        });

        test('refuses empty and missing addresses', () => {
            expect(editor.safeHref('')).toBeNull();
            expect(editor.safeHref('   ')).toBeNull();
            expect(editor.safeHref(null)).toBeNull();
        });
    });

    describe('sanitize', () => {
        test('keeps the tags the editor produces', () => {
            const html = '<h2>Title</h2><p><strong>a</strong> <em>b</em> <u>c</u> <s>d</s></p><ul><li>one</li></ul><ol><li>two</li></ol><blockquote>q</blockquote><pre><code>x</code></pre>';
            expect(editor.sanitize(html)).toBe(html);
        });

        test('renames the browser tags to the ones the editor writes out', () => {
            expect(editor.sanitize('<p><b>a</b><i>b</i><strike>c</strike><del>d</del></p>')).toBe('<p><strong>a</strong><em>b</em><s>c</s><s>d</s></p>');
            expect(editor.sanitize('<h5>small</h5>')).toBe('<h3>small</h3>');
        });

        test('drops scripts, styles, images and other embedded content with what is inside them', () => {
            const html = '<p>safe</p><script>alert(1)</script><style>p{}</style><img src=x onerror="alert(1)"><iframe src="x"></iframe><svg><script>alert(1)</script></svg>';
            expect(editor.sanitize(html)).toBe('<p>safe</p>');
        });

        test('removes every attribute except a safe link address', () => {
            const html = '<p class="x" style="color:red" onclick="alert(1)">a <a href="https://example.com" target="_blank" onclick="x">link</a></p>';
            expect(editor.sanitize(html)).toBe('<p>a <a href="https://example.com">link</a></p>');
        });

        test('unwraps a link with an unsafe address, keeping its text', () => {
            expect(editor.sanitize('<p><a href="javascript:alert(1)">click</a></p>')).toBe('<p>click</p>');
        });

        test('unwraps unknown tags, keeping their text', () => {
            expect(editor.sanitize('<p><span style="font-weight:bold">a</span><font>b</font></p>')).toBe('<p>ab</p>');
        });

        test('turns a top level div into a paragraph, but unwraps one inside a list item', () => {
            expect(editor.sanitize('<div>one</div><div>two</div>')).toBe('<p>one</p><p>two</p>');
            expect(editor.sanitize('<ul><li><div>item</div></li></ul>')).toBe('<ul><li>item</li></ul>');
        });

        test('wraps loose text at the top level in a paragraph', () => {
            expect(editor.sanitize('hello <strong>world</strong><p>next</p>')).toBe('<p>hello <strong>world</strong></p><p>next</p>');
        });

        test('returns an empty string for nothing', () => {
            expect(editor.sanitize('')).toBe('');
            expect(editor.sanitize(null)).toBe('');
        });
    });

    describe('reading the content', () => {
        test('getHtml returns sanitized HTML, or nothing when there is no text', () => {
            const { area } = build({ html: '<p><b>Hi</b></p>' });
            expect(editor.getHtml(area)).toBe('<p><strong>Hi</strong></p>');
            area.innerHTML = '<p><br></p>';
            expect(editor.getHtml(area)).toBe('');
        });

        test('getText puts a line break between paragraphs, list items and line breaks', () => {
            const { area } = build({ html: '<h1>Title</h1><p>one<br>two</p><ul><li>a</li><li>b</li></ul>' });
            expect(editor.getText(area)).toBe('Title\none\ntwo\na\nb');
        });

        test('getLength counts the characters without line breaks', () => {
            const { area } = build({ html: '<p>ab</p><p>cd</p>' });
            expect(editor.getLength(area)).toBe(4);
        });

        test('return empty values without an element', () => {
            expect(editor.getHtml(null)).toBe('');
            expect(editor.getText(null)).toBe('');
            expect(editor.getLength(null)).toBe(0);
        });
    });

    describe('setHtml', () => {
        test('writes sanitized HTML and returns it', () => {
            const { area } = build();
            const result = editor.setHtml(area, '<p>Hi<script>x</script></p>');
            expect(area.innerHTML).toBe('<p>Hi</p>');
            expect(result).toBe('<p>Hi</p>');
        });

        test('marks the area as empty so its placeholder shows', () => {
            const { area } = build();
            editor.setHtml(area, '');
            expect(area.hasAttribute('data-empty')).toBe(true);
            editor.setHtml(area, '<p>text</p>');
            expect(area.hasAttribute('data-empty')).toBe(false);
        });

        test('does not show the placeholder over an empty list', () => {
            const { area } = build();
            editor.setHtml(area, '<ul><li></li></ul>');
            expect(area.hasAttribute('data-empty')).toBe(false);
        });

        test('does not report the new content back to .NET', () => {
            const { area, dotnetRef } = build();
            editor.attach(area, dotnetRef, { debounce: 0 });
            editor.setHtml(area, '<p>from code</p>');
            area.dispatchEvent(new Event('input'));
            expect(calls(dotnetRef, 'NotifyInput')).toHaveLength(0);
        });
    });

    describe('getState', () => {
        test('reports the marks, paragraph style and position at the caret', () => {
            const { area } = build({ html: '<h2>ab<strong>cd</strong></h2>' });
            select(area.querySelector('strong').firstChild, 1);
            const state = editor.getState(area);
            expect(state.formats).toEqual(['Bold', 'Heading2']);
            expect(state.index).toBe(3);
            expect(state.length).toBe(0);
            expect(state.link).toBeNull();
        });

        test('reports lists, quotes and links', () => {
            const { area } = build({ html: '<ol><li><a href="/x">item</a></li></ol><blockquote><em>q</em></blockquote>' });
            select(area.querySelector('a').firstChild, 2);
            expect(editor.getState(area).formats).toEqual(['Paragraph', 'NumberedList', 'Link']);
            expect(editor.getState(area).link).toBe('/x');

            select(area.querySelector('em').firstChild, 0, 1);
            const quote = editor.getState(area);
            expect(quote.formats).toEqual(['Italic', 'Blockquote']);
            expect(quote.length).toBe(1);
        });

        test('prefers the browser\'s own state, which knows about bold turned on at an empty caret', () => {
            const { area } = build({ html: '<p>plain</p>' });
            document.queryCommandState = vi.fn(command => command === 'bold');
            select(area.querySelector('p').firstChild, 2);
            expect(editor.getState(area).formats).toEqual(['Bold', 'Paragraph']);
        });

        test('returns null when the selection is outside the area', () => {
            const { area, url } = build({ html: '<p>text</p>' });
            url.value = 'x';
            select(document.body, 0);
            expect(editor.getState(area)).toBeNull();
        });
    });

    describe('format', () => {
        test('runs the editing command for each mark and list', () => {
            const { area, dotnetRef } = build({ html: '<p>text</p>' });
            editor.attach(area, dotnetRef, {});
            select(area.querySelector('p').firstChild, 0, 4);

            for (const [name, command] of [['Bold', 'bold'], ['Italic', 'italic'], ['Underline', 'underline'], ['Strikethrough', 'strikeThrough'], ['BulletList', 'insertUnorderedList'], ['NumberedList', 'insertOrderedList']]) {
                editor.format(area, name);
                expect(document.execCommand).toHaveBeenLastCalledWith(command, false, undefined);
            }
        });

        test('formats paragraphs as a heading, and turns a quote or code block back into a paragraph', () => {
            const { area, dotnetRef } = build({ html: '<p>text</p><blockquote>quote</blockquote>' });
            editor.attach(area, dotnetRef, {});

            select(area.querySelector('p').firstChild, 1);
            editor.format(area, 'Heading1');
            expect(document.execCommand).toHaveBeenLastCalledWith('formatBlock', false, '<h1>');
            editor.format(area, 'CodeBlock');
            expect(document.execCommand).toHaveBeenLastCalledWith('formatBlock', false, '<pre>');

            select(area.querySelector('blockquote').firstChild, 1);
            editor.format(area, 'Blockquote');
            expect(document.execCommand).toHaveBeenLastCalledWith('formatBlock', false, '<p>');
        });

        test('clear formatting also removes links', () => {
            const { area, dotnetRef } = build({ html: '<p>text</p>' });
            editor.attach(area, dotnetRef, {});
            select(area.querySelector('p').firstChild, 0, 2);
            editor.format(area, 'ClearFormatting');
            expect(document.execCommand.mock.calls.map(c => c[0])).toEqual(expect.arrayContaining(['removeFormat', 'unlink']));
        });

        test('runs undo and redo', () => {
            const { area, dotnetRef } = build({ html: '<p>text</p>' });
            editor.attach(area, dotnetRef, {});
            editor.format(area, 'Undo');
            expect(document.execCommand).toHaveBeenLastCalledWith('undo', false, undefined);
            editor.format(area, 'Redo');
            expect(document.execCommand).toHaveBeenLastCalledWith('redo', false, undefined);
        });

        test('ignores unknown names, and a read-only area', () => {
            const { area, dotnetRef } = build({ html: '<p>text</p>' });
            editor.attach(area, dotnetRef, {});
            editor.format(area, 'Nonsense');
            area.setAttribute('contenteditable', 'false');
            editor.format(area, 'Bold');
            expect(document.execCommand).not.toHaveBeenCalledWith('bold', false, undefined);
        });

        test('reports the change to .NET at once', () => {
            const { area, dotnetRef } = build({ html: '<p>text</p>' });
            editor.attach(area, dotnetRef, { debounce: 1000 });
            select(area.querySelector('p').firstChild, 0, 4);
            document.execCommand = vi.fn(() => {
                area.innerHTML = '<p><b>text</b></p>';
                return true;
            });
            editor.format(area, 'Bold');
            expect(calls(dotnetRef, 'NotifyInput')).toEqual([['NotifyInput', '<p><strong>text</strong></p>', 'text', 4]]);
        });

        test('waits for an open picker to release focus, then applies the format with focus in the text', async () => {
            const { area, dotnetRef } = build({ html: '<p>text</p>' });
            editor.attach(area, dotnetRef, {});
            select(area.querySelector('p').firstChild, 1);
            document.dispatchEvent(new Event('selectionchange'));

            // A picker's panel keeps focus inside itself until it closes, so focusing the area does nothing at first.
            const panel = document.createElement('div');
            panel.tabIndex = 0;
            document.body.appendChild(panel);
            panel.focus();
            const focus = area.focus.bind(area);
            area.focus = () => {};

            editor.format(area, 'Heading1');
            expect(document.execCommand).not.toHaveBeenCalledWith('formatBlock', false, '<h1>');

            // The picker closes and is removed, so the area can take focus again.
            panel.remove();
            area.focus = focus;
            await new Promise(resolve => setTimeout(resolve, 100));

            expect(document.execCommand).toHaveBeenCalledWith('formatBlock', false, '<h1>');
            expect(document.activeElement).toBe(area);
        });

        test('gives up waiting once the area is removed', async () => {
            const { area, dotnetRef } = build({ html: '<p>text</p>' });
            editor.attach(area, dotnetRef, {});
            area.focus = () => {};
            editor.format(area, 'Bold');
            area.remove();
            await new Promise(resolve => setTimeout(resolve, 100));
            expect(document.execCommand).not.toHaveBeenCalledWith('bold', false, undefined);
        });

        test('puts focus back on the toolbar control that ran it', () => {
            const { area, dotnetRef, bold } = build({ html: '<p>text</p>' });
            editor.attach(area, dotnetRef, { toolbarId: 'ed-toolbar' });
            bold.focus();
            editor.format(area, 'Bold');
            expect(document.activeElement).toBe(bold);
        });

        test('restores the selection the area had before focus left it', () => {
            const { area, dotnetRef, url } = build({ html: '<p>hello</p>' });
            editor.attach(area, dotnetRef, {});
            area.focus();
            select(area.querySelector('p').firstChild, 1, 3);
            document.dispatchEvent(new Event('selectionchange'));

            url.focus();
            select(url, 0);
            let selected = '';
            document.execCommand = vi.fn(() => {
                selected = document.getSelection().toString();
                return true;
            });
            editor.format(area, 'Bold');
            expect(selected).toBe('el');
        });
    });

    describe('links', () => {
        test('links the selected text', () => {
            const { area, dotnetRef } = build({ html: '<p>text</p>' });
            editor.attach(area, dotnetRef, {});
            select(area.querySelector('p').firstChild, 0, 4);
            editor.link(area, 'https://example.com');
            expect(document.execCommand).toHaveBeenCalledWith('createLink', false, 'https://example.com');
        });

        test('wraps the selection itself when the browser has no createLink command', () => {
            const { area, dotnetRef } = build({ html: '<p>text</p>' });
            editor.attach(area, dotnetRef, {});
            document.execCommand = vi.fn(() => false);
            select(area.querySelector('p').firstChild, 0, 4);
            editor.link(area, 'https://example.com');
            expect(area.innerHTML).toBe('<p><a href="https://example.com">text</a></p>');
        });

        test('inserts the address as a link when nothing is selected', () => {
            const { area, dotnetRef } = build({ html: '<p>go </p>' });
            editor.attach(area, dotnetRef, {});
            document.execCommand = vi.fn(() => false);
            select(area.querySelector('p').firstChild, 3);
            editor.link(area, 'https://example.com');
            expect(area.innerHTML).toBe('<p>go <a href="https://example.com">https://example.com</a></p>');
        });

        test('changes the address of the link the caret is in', () => {
            const { area, dotnetRef } = build({ html: '<p><a href="https://old.example">site</a></p>' });
            editor.attach(area, dotnetRef, {});
            select(area.querySelector('a').firstChild, 2);
            editor.link(area, 'https://new.example');
            expect(area.querySelector('a').getAttribute('href')).toBe('https://new.example');
        });

        test('refuses an unsafe address', () => {
            const { area, dotnetRef } = build({ html: '<p>text</p>' });
            editor.attach(area, dotnetRef, {});
            select(area.querySelector('p').firstChild, 0, 4);
            editor.link(area, 'javascript:alert(1)');
            expect(document.execCommand).not.toHaveBeenCalled();
        });

        test('unlink removes the link the caret is in, keeping its text', () => {
            const { area, dotnetRef } = build({ html: '<p><a href="/x">site</a> after</p>' });
            editor.attach(area, dotnetRef, {});
            document.execCommand = vi.fn(() => false);
            select(area.querySelector('a').firstChild, 1);
            editor.unlink(area);
            expect(area.innerHTML).toBe('<p>site after</p>');
        });
    });

    describe('attach', () => {
        test('reports typing once it pauses for the debounce interval', () => {
            vi.useFakeTimers();
            const { area, dotnetRef } = build();
            editor.attach(area, dotnetRef, { debounce: 250 });
            area.innerHTML = '<p>Hi</p>';
            area.dispatchEvent(new Event('input'));
            expect(calls(dotnetRef, 'NotifyInput')).toHaveLength(0);
            vi.advanceTimersByTime(250);
            expect(calls(dotnetRef, 'NotifyInput')).toEqual([['NotifyInput', '<p>Hi</p>', 'Hi', 2]]);
        });

        test('reports every keystroke when the debounce interval is 0', () => {
            const { area, dotnetRef } = build();
            editor.attach(area, dotnetRef, { debounce: 0 });
            area.innerHTML = '<p>H</p>';
            area.dispatchEvent(new Event('input'));
            expect(calls(dotnetRef, 'NotifyInput')).toHaveLength(1);
        });

        test('reports a waiting change when focus leaves, then the blur', () => {
            vi.useFakeTimers();
            const { area, dotnetRef } = build();
            editor.attach(area, dotnetRef, { debounce: 5000 });
            area.innerHTML = '<p>Hi</p>';
            area.dispatchEvent(new Event('input'));
            area.dispatchEvent(new Event('blur'));
            expect(dotnetRef.invokeMethodAsync.mock.calls.map(c => c[0])).toEqual(['NotifyInput', 'NotifyBlur']);
        });

        test('flush reports a waiting change straight away', () => {
            vi.useFakeTimers();
            const { area, dotnetRef } = build();
            editor.attach(area, dotnetRef, { debounce: 5000 });
            area.innerHTML = '<p>Hi</p>';
            area.dispatchEvent(new Event('input'));
            editor.flush(area);
            expect(calls(dotnetRef, 'NotifyInput')).toHaveLength(1);
        });

        test('does not report content that did not change', () => {
            const { area, dotnetRef } = build({ html: '<p>same</p>' });
            editor.attach(area, dotnetRef, { debounce: 0 });
            area.dispatchEvent(new Event('input'));
            expect(calls(dotnetRef, 'NotifyInput')).toHaveLength(0);
        });

        test('makes new paragraphs <p> elements and reports focus', () => {
            const { area, dotnetRef } = build();
            editor.attach(area, dotnetRef, {});
            area.dispatchEvent(new Event('focus'));
            expect(document.execCommand).toHaveBeenCalledWith('defaultParagraphSeparator', false, 'p');
            expect(document.execCommand).toHaveBeenCalledWith('styleWithCSS', false, false);
            expect(calls(dotnetRef, 'NotifyFocus')).toHaveLength(1);
        });

        test('reports the formats when the selection moves inside the area, once per change', () => {
            const { area, dotnetRef } = build({ html: '<p><strong>ab</strong></p>' });
            editor.attach(area, dotnetRef, {});
            select(area.querySelector('strong').firstChild, 1);
            document.dispatchEvent(new Event('selectionchange'));
            document.dispatchEvent(new Event('selectionchange'));
            const reported = calls(dotnetRef, 'NotifyState');
            expect(reported).toHaveLength(1);
            expect(reported[0][1].formats).toEqual(['Bold', 'Paragraph']);
        });

        test('ignores selection changes outside the area', () => {
            const { area, dotnetRef } = build({ html: '<p>ab</p>' });
            editor.attach(area, dotnetRef, {});
            select(document.body, 0);
            document.dispatchEvent(new Event('selectionchange'));
            expect(calls(dotnetRef, 'NotifyState')).toHaveLength(0);
        });

        test('attaches only once', () => {
            const { area, dotnetRef } = build();
            editor.attach(area, dotnetRef, {});
            const state = area.__twTextEditor;
            editor.attach(area, { invokeMethodAsync: vi.fn() }, {});
            expect(area.__twTextEditor).toBe(state);
        });

        test('update changes the toolbar and debounce interval', () => {
            const { area, dotnetRef } = build();
            editor.attach(area, dotnetRef, { debounce: 100 });
            editor.update(area, { toolbarId: 'ed-toolbar', debounce: 0 });
            expect(area.__twTextEditor.toolbarId).toBe('ed-toolbar');
            expect(area.__twTextEditor.debounce).toBe(0);
        });

        test('detach removes the listeners', () => {
            const { area, dotnetRef } = build();
            editor.attach(area, dotnetRef, { debounce: 0 });
            editor.detach(area);
            area.innerHTML = '<p>Hi</p>';
            area.dispatchEvent(new Event('input'));
            area.dispatchEvent(new Event('focus'));
            expect(dotnetRef.invokeMethodAsync).not.toHaveBeenCalled();
            expect(area.__twTextEditor).toBeUndefined();
        });
    });

    describe('shortcuts', () => {
        test.each([
            [{ key: 'b', ctrlKey: true }, 'bold'],
            [{ key: 'i', metaKey: true }, 'italic'],
            [{ key: 'U', ctrlKey: true }, 'underline'],
            [{ key: 'X', code: 'KeyX', ctrlKey: true, shiftKey: true }, 'strikeThrough'],
            [{ key: '&', code: 'Digit7', ctrlKey: true, shiftKey: true }, 'insertOrderedList'],
            [{ key: '*', code: 'Digit8', ctrlKey: true, shiftKey: true }, 'insertUnorderedList'],
            [{ key: '\\', ctrlKey: true }, 'removeFormat'],
        ])('%o runs %s', (props, command) => {
            const { area, dotnetRef } = build({ html: '<p>text</p>' });
            editor.attach(area, dotnetRef, {});
            const event = keydown(area, props.key, props);
            expect(event.defaultPrevented).toBe(true);
            expect(document.execCommand).toHaveBeenCalledWith(command, false, undefined);
        });

        test('Ctrl + Alt + digit sets a heading or a paragraph', () => {
            const { area, dotnetRef } = build({ html: '<p>text</p>' });
            editor.attach(area, dotnetRef, {});
            select(area.querySelector('p').firstChild, 1);
            keydown(area, '2', { code: 'Digit2', ctrlKey: true, altKey: true });
            expect(document.execCommand).toHaveBeenLastCalledWith('formatBlock', false, '<h2>');
            keydown(area, '0', { code: 'Digit0', ctrlKey: true, altKey: true });
            expect(document.execCommand).toHaveBeenLastCalledWith('formatBlock', false, '<p>');
        });

        test('Ctrl + K asks .NET to open the link bar', () => {
            const { area, dotnetRef } = build({ html: '<p>text</p>' });
            editor.attach(area, dotnetRef, {});
            const event = keydown(area, 'k', { ctrlKey: true });
            expect(event.defaultPrevented).toBe(true);
            expect(calls(dotnetRef, 'OpenLinkPanel')).toHaveLength(1);
        });

        test('Alt + F10 moves focus to the toolbar\'s Tab stop', () => {
            const { area, dotnetRef, bold } = build({ html: '<p>text</p>' });
            editor.attach(area, dotnetRef, { toolbarId: 'ed-toolbar' });
            const event = keydown(area, 'F10', { altKey: true });
            expect(event.defaultPrevented).toBe(true);
            expect(document.activeElement).toBe(bold);
        });

        test('Alt + F10 does nothing without a toolbar', () => {
            const { area, dotnetRef } = build({ toolbar: false });
            editor.attach(area, dotnetRef, {});
            expect(keydown(area, 'F10', { altKey: true }).defaultPrevented).toBe(false);
        });

        test('leaves other keys, keys during IME composition and a read-only area alone', () => {
            const { area, dotnetRef } = build({ html: '<p>text</p>' });
            editor.attach(area, dotnetRef, {});
            expect(keydown(area, 'a').defaultPrevented).toBe(false);
            expect(keydown(area, 'Tab').defaultPrevented).toBe(false);
            expect(keydown(area, 'b', { ctrlKey: true, isComposing: true }).defaultPrevented).toBe(false);
            area.setAttribute('contenteditable', 'false');
            expect(keydown(area, 'b', { ctrlKey: true }).defaultPrevented).toBe(false);
            expect(document.execCommand).not.toHaveBeenCalled();
        });
    });

    describe('paste and drop', () => {
        function paste(target, data) {
            const event = new Event('paste', { bubbles: true, cancelable: true });
            Object.defineProperty(event, 'clipboardData', { value: { getData: type => data[type] ?? '' } });
            target.dispatchEvent(event);
            return event;
        }

        test('inserts pasted HTML sanitized', () => {
            const { area, dotnetRef } = build();
            editor.attach(area, dotnetRef, {});
            const event = paste(area, { 'text/html': '<p style="x">a<script>b</script></p>', 'text/plain': 'a' });
            expect(event.defaultPrevented).toBe(true);
            expect(document.execCommand).toHaveBeenCalledWith('insertHTML', false, '<p>a</p>');
        });

        test('inserts plain text when there is no HTML', () => {
            const { area, dotnetRef } = build();
            editor.attach(area, dotnetRef, {});
            paste(area, { 'text/plain': 'just text' });
            expect(document.execCommand).toHaveBeenCalledWith('insertText', false, 'just text');
        });

        test('leaves a paste into a read-only area to the browser', () => {
            const { area, dotnetRef } = build({ editable: false });
            editor.attach(area, dotnetRef, {});
            expect(paste(area, { 'text/plain': 'x' }).defaultPrevented).toBe(false);
        });

        test('inserts dropped content as plain text', () => {
            const { area, dotnetRef } = build({ html: '<p>text</p>' });
            editor.attach(area, dotnetRef, {});
            const event = new Event('drop', { bubbles: true, cancelable: true });
            Object.defineProperty(event, 'dataTransfer', { value: { getData: type => (type === 'text/plain' ? 'dropped' : '<img src=x onerror=alert(1)>') } });
            area.dispatchEvent(event);
            expect(event.defaultPrevented).toBe(true);
            expect(document.execCommand).toHaveBeenCalledWith('insertText', false, 'dropped');
            expect(document.execCommand).not.toHaveBeenCalledWith('insertHTML', expect.anything(), expect.anything());
        });
    });

    describe('bindLinkInput', () => {
        test('focuses the field and selects its text', () => {
            const { url, dotnetRef } = build();
            url.value = 'https://example.com';
            editor.bindLinkInput('url', dotnetRef);
            expect(document.activeElement).toBe(url);
            expect(url.selectionStart).toBe(0);
            expect(url.selectionEnd).toBe(url.value.length);
        });

        test('Enter applies the link without submitting a form, and Escape cancels', () => {
            const { url, dotnetRef } = build();
            editor.bindLinkInput('url', dotnetRef);
            editor.bindLinkInput('url', dotnetRef);
            url.value = 'example.com';

            expect(keydown(url, 'Enter').defaultPrevented).toBe(true);
            expect(calls(dotnetRef, 'ApplyLink')).toEqual([['ApplyLink', 'example.com']]);

            expect(keydown(url, 'Escape').defaultPrevented).toBe(true);
            expect(calls(dotnetRef, 'CloseLinkPanel')).toHaveLength(1);
        });

        test('does nothing for a missing field', () => {
            expect(() => editor.bindLinkInput('missing', { invokeMethodAsync: vi.fn() })).not.toThrow();
        });
    });

    test('focus restores the caret to the end when the area never had a selection', () => {
        const { area, dotnetRef } = build({ html: '<p>text</p>' });
        editor.attach(area, dotnetRef, {});
        editor.focus(area);
        expect(document.activeElement).toBe(area);
        const range = document.getSelection().getRangeAt(0);
        expect(range.collapsed).toBe(true);
        expect(area.contains(range.startContainer)).toBe(true);
    });
});
