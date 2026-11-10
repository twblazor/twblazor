# Localization

Every piece of text a component renders by itself (labels, placeholders, validation messages and screen reader announcements) comes from a `.resx` resource file, and dates and times are formatted for the culture your application runs under. English is built in and 23 more languages ship with the package, so there is nothing to set up: components follow `CultureInfo.CurrentUICulture`.

See the [Localization docs](https://twblazor.com/localization) for every key and option.

## Languages

| Language | Culture | Resource file | Translation |
| -------- | ------- | ------------- | ----------- |
| English | `en` | `TwBlazorStrings.resx` | Written by the maintainers |
| Chinese (Simplified, Mandarin) | `zh-Hans` | `TwBlazorStrings.zh-Hans.resx` | AI translated |
| Spanish | `es` | `TwBlazorStrings.es.resx` | AI translated |
| Hindi | `hi` | `TwBlazorStrings.hi.resx` | AI translated |
| Portuguese | `pt` | `TwBlazorStrings.pt.resx` | AI translated |
| Bengali | `bn` | `TwBlazorStrings.bn.resx` | AI translated |
| Russian | `ru` | `TwBlazorStrings.ru.resx` | AI translated |
| Japanese | `ja` | `TwBlazorStrings.ja.resx` | AI translated |
| Western Punjabi (Shahmukhi) | `pa-Arab` | `TwBlazorStrings.pa-Arab.resx` | AI translated |
| Turkish | `tr` | `TwBlazorStrings.tr.resx` | AI translated |
| Vietnamese | `vi` | `TwBlazorStrings.vi.resx` | AI translated |
| Chinese (Traditional, Cantonese) | `zh-Hant` | `TwBlazorStrings.zh-Hant.resx` | AI translated |
| Arabic, including Egyptian Arabic | `ar` | `TwBlazorStrings.ar.resx` | AI translated |
| Marathi | `mr` | `TwBlazorStrings.mr.resx` | AI translated |
| Telugu | `te` | `TwBlazorStrings.te.resx` | AI translated |
| Korean | `ko` | `TwBlazorStrings.ko.resx` | AI translated |
| Tamil | `ta` | `TwBlazorStrings.ta.resx` | AI translated |
| Urdu | `ur` | `TwBlazorStrings.ur.resx` | AI translated |
| Indonesian | `id` | `TwBlazorStrings.id.resx` | AI translated |
| German | `de` | `TwBlazorStrings.de.resx` | AI translated |
| French | `fr` | `TwBlazorStrings.fr.resx` | AI translated |
| Javanese | `jv` | `TwBlazorStrings.jv.resx` | AI translated |
| Persian | `fa` | `TwBlazorStrings.fa.resx` | AI translated |
| Italian | `it` | `TwBlazorStrings.it.resx` | AI translated |
| Polish | `pl` | `TwBlazorStrings.pl.resx` | AI translated |

A regional culture uses its language file: `es-MX` uses Spanish, `ar-EG` uses Arabic and `zh-HK` uses Traditional Chinese. Wu Chinese is written in Simplified Chinese, so it is covered by `zh-Hans`.

> [!NOTE]
> **Translations other than English were produced by AI** and have not been reviewed by native speakers, so some wording may be inaccurate or unnatural. Corrections are very welcome: edit the language file in [src/TwBlazor/Resources](src/TwBlazor/Resources) and open a pull request (see [CONTRIBUTING.md](CONTRIBUTING.md)). New languages can be added the same way.

## Wording and formats

To change the wording, add a language or set date and time formats, use `Localization` on the options you pass to `AddTwBlazor`:

```csharp
builder.Services.AddTwBlazor(options =>
{
    // Your own .resx files (TwBlazorStrings.resx, TwBlazorStrings.nl.resx, ...) are searched first.
    options.Localization.AddResource(
        new ResourceManager("MyApp.Resources.TwBlazorStrings", typeof(Program).Assembly));

    // Date and time formats default to the culture. Set only what you want to change.
    options.Localization.Formats.FirstDayOfWeek = DayOfWeek.Monday;
    options.Localization.Formats.Use12HourClock = false;
    options.Localization.Formats.DateInput = "dd/MM/yyyy";
}, Theme.CreateTheme);
```

## Right to left

Arabic, Persian, Urdu, Western Punjabi and any other culture .NET reports as right to left (such as Hebrew) mirror every component: spacing, icons and panels swap sides, and the Left and Right arrow keys swap so they keep following what is on screen.

The page has to carry the matching `dir` attribute. Wrap your layout in a `TwDirectionProvider`, which follows the current language, or set `dir` on the `html` element yourself:

```razor
<TwDirectionProvider>
    @Body
    <TwToastProvider />
    <TwDialogProvider />
</TwDirectionProvider>
```

To set the direction whatever the culture, use `options.Localization.Direction = TextDirection.RightToLeft` in `AddTwBlazor`, or pass `Direction` to a `TwDirectionProvider` to turn one part of a page.

See the [RTL Provider docs](https://twblazor.com/direction-provider) for what is mirrored and how to make your own theme and markup follow the direction.

[Back to the README](README.md)
