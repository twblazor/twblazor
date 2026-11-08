// @vitest-environment jsdom

import { afterEach, describe, expect, test, vi } from 'vitest';

await import('../../../src/TwBlazor/wwwroot/js/twblazor.js');

function mockDownload() {
    URL.createObjectURL = vi.fn(() => 'blob:file');
    URL.revokeObjectURL = vi.fn();
    const clicked = [];
    vi.spyOn(HTMLAnchorElement.prototype, 'click').mockImplementation(function () {
        clicked.push({ href: this.href, download: this.download, attached: document.body.contains(this) });
    });
    return clicked;
}

function stream(bytes) {
    return { arrayBuffer: vi.fn(async () => new Uint8Array(bytes).buffer) };
}

describe('twDownload', () => {
    afterEach(() => {
        vi.restoreAllMocks();
        vi.useRealTimers();
        document.body.innerHTML = '';
    });

    test('saves the streamed bytes as a file with its media type, then frees the object URL', async () => {
        vi.useFakeTimers();
        const clicked = mockDownload();

        await globalThis.twDownload.save('notes.docx', 'application/vnd.openxmlformats-officedocument.wordprocessingml.document', stream([1, 2, 3, 4]));

        expect(clicked).toEqual([{ href: 'blob:file', download: 'notes.docx', attached: true }]);
        expect(document.querySelector('a')).toBeNull();
        const blob = URL.createObjectURL.mock.calls[0][0];
        expect(blob.type).toBe('application/vnd.openxmlformats-officedocument.wordprocessingml.document');
        expect(blob.size).toBe(4);

        vi.runAllTimers();
        expect(URL.revokeObjectURL).toHaveBeenCalledWith('blob:file');
    });

    test('the data table\'s download saves through it', async () => {
        const save = vi.spyOn(globalThis.twDownload, 'save').mockResolvedValue(undefined);
        const reference = stream([1]);

        await globalThis.twDataTable.download('orders.csv', 'text/csv', reference);

        expect(save).toHaveBeenCalledWith('orders.csv', 'text/csv', reference);
    });
});
