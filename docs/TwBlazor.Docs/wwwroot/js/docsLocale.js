// Keeps the docs site's language and direction choices from the navbar switcher.
// The language is kept in two places: the ASP.NET Core culture cookie, which the Server host reads on each
// request, and localStorage, which the WebAssembly host reads in Program.cs before the app starts.
globalThis.docsLocale = {
    cultureKey: "docs-culture",
    cultureCookie: ".AspNetCore.Culture",
    directionKey: "docs-direction",

    read: function (key) {
        try {
            return localStorage.getItem(key);
        } catch {
            return null;
        }
    },

    write: function (key, value) {
        try {
            if (value) {
                localStorage.setItem(key, value);
            } else {
                localStorage.removeItem(key);
            }
        } catch {
            // Storage is blocked; the choice lasts until the page is reloaded.
        }
    },

    getCulture: function () {
        return this.read(this.cultureKey);
    },

    // Saves the culture (or clears it when null) and reloads, since Blazor only picks up a culture at start.
    setCulture: function (code) {
        this.write(this.cultureKey, code);

        if (code) {
            const value = encodeURIComponent(`c=${code}|uic=${code}`);
            document.cookie = `${this.cultureCookie}=${value}; path=/; max-age=31536000; samesite=lax`;
        } else {
            document.cookie = `${this.cultureCookie}=; path=/; max-age=0; samesite=lax`;
        }

        globalThis.location.reload();
    },

    // Marks the page with the culture the WebAssembly host started in, as the Server host does in App.razor.
    applyToDocument: function (lang, dir) {
        document.documentElement.lang = lang;
        document.documentElement.dir = dir;
    },

    // The direction picked in the switcher: "ltr", "rtl", or null to follow the language. It is kept in memory
    // only, so it survives moving between pages (which swaps the layout) but a fresh load follows the language.
    direction: null,

    getDirection: function () {
        return this.direction;
    },

    setDirection: function (direction) {
        this.direction = direction;
    }
};

// Earlier versions saved the direction in localStorage, which made it stick across loads. Clear it.
docsLocale.write(docsLocale.directionKey, null);
