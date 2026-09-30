/*
 * docfx "modern" template hook. The template does a dynamic
 * `import('./main.js')` and reads the default export, so everything the site
 * customises has to hang off this object.
 */
export default {
    // Follow the visitor's OS setting, matching twblazor.com.
    defaultTheme: 'auto',

    // Rendered into the navbar as Bootstrap Icons.
    iconLinks: [
        {
            icon: 'book',
            href: 'https://twblazor.com/',
            title: 'Component documentation'
        },
        {
            icon: 'github',
            href: 'https://github.com/twblazor/twblazor',
            title: 'GitHub'
        }
    ]
};
