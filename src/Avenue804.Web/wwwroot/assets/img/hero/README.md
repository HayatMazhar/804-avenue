# Hero images

The home page hero background lives in this folder. Files referenced by
`_HomeSections.cshtml`:

| File                          | Used as                                          |
| ----------------------------- | ------------------------------------------------ |
| `home-hero.webp` (1×, ~1800w) | `<source srcset>` for modern browsers.           |
| `home-hero@2x.webp`           | Retina (`(min-resolution: 2dppx)`) variant.      |

Neither WebP is committed to the repo — they're authored by the marketing
team, exported as `quality=80` WebP (≤ 250 KB each) and dropped into this
folder. While they're missing, the Razor markup falls back to a curated
Unsplash construction photo (the historical production hero):

```
https://images.unsplash.com/photo-1504307651254-35680f356dfd?w=1800&q=80
```

Once the WebP files are present, the markup will emit:

```html
<picture>
  <source type="image/webp"
          srcset="/assets/img/hero/home-hero.webp 1x,
                  /assets/img/hero/home-hero@2x.webp 2x">
  <img src="https://images.unsplash.com/..." alt="..." />
</picture>
```

A future enhancement (tracked in the audit plan) is to honour a SiteSetting
`home.hero.image_url` so admins can override the path from the UI without a
deploy.
