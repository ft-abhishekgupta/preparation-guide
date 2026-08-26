"""Turn the exported HTML notes into a browsable GitHub Pages site.

The `.html` files in this repo are standalone exports (Markdown Preview
Enhanced / crossnote). This script post-processes them in place so they behave
like a documentation site:

  * real <title> tags taken from each page's <h1>
  * a sticky top bar with home link, section dropdown and breadcrumb
  * a footer with previous / section index / next links
  * readable max-width layout
  * dead relative links (targets that do not exist) are unlinked
  * generates index.html (site home, copy of README.html), 404.html, .nojekyll

Everything injected is wrapped in `site-chrome` markers and stripped before it
is re-inserted, so the script is idempotent and safe to re-run after
re-exporting the notes from Markdown.

Usage:  python tools/build_site.py
"""

from __future__ import annotations

import html
import os
import re
import shutil
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
REPO_URL = "https://github.com/ft-abhishekgupta/preparation-guide"
BASE_PATH = "/" + REPO_URL.rsplit("/", 1)[-1] + "/"
SITE_NAME = "Interview Prep Notebook"

START = "<!-- site-chrome:start -->"
END = "<!-- site-chrome:end -->"
CHROME_BLOCK = re.compile(r"\s*" + re.escape(START) + r".*?" + re.escape(END) + r"\s*", re.DOTALL)
HEAD_CLOSE_RE = re.compile(r"\s*</head>", re.IGNORECASE)
BODY_CLOSE_RE = re.compile(r"\s*</body>", re.IGNORECASE)

H1_RE = re.compile(r"<h1\b[^>]*>(.*?)</h1>", re.DOTALL | re.IGNORECASE)
TITLE_RE = re.compile(r"<title>.*?</title>", re.DOTALL | re.IGNORECASE)
BODY_OPEN_RE = re.compile(r"<body\b[^>]*>\s*", re.IGNORECASE)
TAG_RE = re.compile(r"<[^>]+>")
ANCHOR_RE = re.compile(r'<a\b([^>]*?)href="([^"]*)"([^>]*)>(.*?)</a>', re.DOTALL | re.IGNORECASE)
NUM_PREFIX_RE = re.compile(r"^\d+[.)]?\s+")


def text_of(fragment: str) -> str:
    return html.unescape(TAG_RE.sub("", fragment)).strip()


def page_heading(source: str) -> str | None:
    match = H1_RE.search(source)
    return text_of(match.group(1)) if match else None


class Page:
    def __init__(self, path: Path):
        self.path = path
        self.rel = path.relative_to(ROOT).as_posix()
        self.source = path.read_text(encoding="utf-8")
        heading = page_heading(self.source) or path.stem
        self.title = NUM_PREFIX_RE.sub("", heading).strip() or heading
        self.section: "Section | None" = None

    def href_from(self, other: "Page") -> str:
        depth = len(Path(other.rel).parts) - 1
        return ("../" * depth) + self.rel


class Section:
    def __init__(self, directory: Path):
        self.directory = directory
        self.index: Page = Page(directory / "00-README.html")
        self.index.section = self
        self.title = self.index.title
        self.pages: list[Page] = [self.index]

    def add(self, page: Page) -> None:
        page.section = self
        self.pages.append(page)


def collect() -> tuple[Page, list[Section]]:
    home = Page(ROOT / "README.html")
    sections: list[Section] = []

    for directory in sorted(p for p in ROOT.iterdir() if p.is_dir()):
        if directory.name.startswith(".") or directory.name == "tools":
            continue
        if not (directory / "00-README.html").exists():
            continue
        section = Section(directory)
        for file in sorted(directory.glob("*.html")):
            if file.name != "00-README.html":
                section.add(Page(file))
        sections.append(section)

    return home, sections


def relative_prefix(page: Page) -> str:
    return "../" * (len(Path(page.rel).parts) - 1)


def styles() -> str:
    return f"""{START}
<style>
  .site-bar, .site-footer {{
    font-family: Inter, -apple-system, BlinkMacSystemFont, "Segoe UI", Helvetica, Arial, sans-serif;
    --site-fg: #d7dae0;
    --site-muted: #9ca3af;
    --site-link: #7dcfff;
    --site-hover: #ff79c6;
    --site-border: #30364d;
  }}
  .site-bar {{
    position: sticky; top: 0; z-index: 50;
    display: flex; flex-wrap: wrap; align-items: center; gap: 14px;
    margin: 0; padding: 10px clamp(16px, 4vw, 40px);
    background: #1a1d2b; color: var(--site-fg);
    border-bottom: 1px solid var(--site-border);
    font-size: 14px; line-height: 1.4;
  }}
  .site-bar a {{ color: var(--site-fg); text-decoration: none; }}
  .site-bar a:hover {{ color: var(--site-hover); }}
  .site-bar__brand {{ font-weight: 600; color: #7aa2f7 !important; }}
  .site-bar__crumb {{ color: var(--site-muted); }}
  .site-bar__crumb a {{ color: var(--site-link) !important; }}
  .site-bar__spacer {{ margin-left: auto; color: var(--site-muted) !important; }}
  .site-menu {{ position: relative; }}
  .site-menu > summary {{
    cursor: pointer; list-style: none; padding: 3px 12px; border-radius: 6px;
    border: 1px solid var(--site-border); background: #24283b; user-select: none;
  }}
  .site-menu > summary::-webkit-details-marker {{ display: none; }}
  .site-menu > summary::after {{ content: " \\25be"; }}
  .site-menu[open] > summary {{ border-color: var(--site-link); }}
  .site-menu ul {{
    position: absolute; left: 0; top: calc(100% + 8px); margin: 0; padding: 6px;
    min-width: 340px; max-height: 72vh; overflow: auto; list-style: none;
    background: #171a26; border: 1px solid var(--site-border); border-radius: 10px;
    box-shadow: 0 12px 32px rgba(0,0,0,.55);
  }}
  .site-menu li {{ margin: 0; }}
  .site-menu li a {{ display: block; padding: 6px 10px; border-radius: 6px; color: #b9c7e8 !important; }}
  .site-menu li a:hover {{ background: #24283b; color: var(--site-link) !important; }}
  .site-menu li a[aria-current="true"] {{ background: #24283b; color: var(--site-link) !important; font-weight: 600; }}
  html body[for=html-export]:not([data-presentation-mode]) .crossnote.markdown-preview {{
    min-height: 0;
  }}
  .site-footer {{
    background: #151824; border-top: 1px solid var(--site-border);
    color: var(--site-muted); font-size: 14px; line-height: 1.5;
    padding: 20px clamp(16px, 4vw, 40px) 28px;
  }}
  .site-footer__nav {{
    display: flex; flex-wrap: wrap; gap: 12px; justify-content: space-between;
    max-width: 1100px; margin: 0 auto;
  }}
  .site-footer a {{ color: var(--site-link); text-decoration: none; }}
  .site-footer a:hover {{ color: var(--site-hover); }}
  .site-footer__meta {{ max-width: 1100px; margin: 16px auto 0; }}
  @media print {{ .site-bar, .site-footer {{ display: none; }} }}
</style>
{END}"""


def top_bar(page: Page, sections: list[Section]) -> str:
    prefix = relative_prefix(page)
    items = []
    for section in sections:
        current = ' aria-current="true"' if page.section is section else ""
        items.append(
            f'<li><a href="{prefix}{section.index.rel}"{current}>'
            f"{html.escape(section.title)}</a></li>"
        )

    crumb = ""
    if page.section is not None:
        if page is page.section.index:
            crumb = f'<span class="site-bar__crumb">{html.escape(page.section.title)}</span>'
        else:
            crumb = (
                f'<span class="site-bar__crumb">'
                f'<a href="{prefix}{page.section.index.rel}">{html.escape(page.section.title)}</a>'
                f" / {html.escape(page.title)}</span>"
            )

    return f"""{START}
<header class="site-bar">
  <a class="site-bar__brand" href="{prefix}index.html">{SITE_NAME}</a>
  <details class="site-menu">
    <summary>Sections</summary>
    <ul>
      <li><a href="{prefix}index.html">Home</a></li>
      {"".join(items)}
    </ul>
  </details>
  {crumb}
  <a class="site-bar__spacer" href="{REPO_URL}">GitHub repo</a>
</header>
{END}"""


def footer(page: Page, order: list[Page]) -> str:
    prefix = relative_prefix(page)
    position = order.index(page)
    previous = order[position - 1] if position > 0 else None
    following = order[position + 1] if position < len(order) - 1 else None

    left = (
        f'<a href="{prefix}{previous.rel}">&larr; {html.escape(previous.title)}</a>'
        if previous
        else "<span></span>"
    )
    right = (
        f'<a href="{prefix}{following.rel}">{html.escape(following.title)} &rarr;</a>'
        if following
        else "<span></span>"
    )
    middle = f'<a href="{prefix}index.html">Home</a>'
    if page.section is not None and page is not page.section.index:
        middle = (
            f'<a href="{prefix}{page.section.index.rel}">'
            f"{html.escape(page.section.title)}</a>"
        )

    return f"""{START}
<footer class="site-footer">
  <div class="site-footer__nav">{left}{middle}{right}</div>
  <p class="site-footer__meta">
    {SITE_NAME} &middot; <a href="{REPO_URL}">source on GitHub</a>
  </p>
</footer>
<script>
  (function () {{
    function closeMenus(event) {{
      document.querySelectorAll("details.site-menu[open]").forEach(function (menu) {{
        if (!event || !menu.contains(event.target)) menu.removeAttribute("open");
      }});
    }}
    document.addEventListener("click", closeMenus);
    document.addEventListener("keydown", function (event) {{
      if (event.key === "Escape") closeMenus(null);
    }});
  }})();
</script>
{END}"""


def fix_links(page: Page, source: str, known: set[str]) -> tuple[str, int]:
    """Unlink relative links whose target is not part of the site."""
    removed = 0
    base = Path(page.rel).parent

    def replace(match: re.Match[str]) -> str:
        nonlocal removed
        href = match.group(2)
        if not href or href.startswith(("http://", "https://", "#", "mailto:", "/")):
            return match.group(0)
        target = href.split("#", 1)[0].split("?", 1)[0]
        if not target:
            return match.group(0)
        resolved = os.path.normpath(os.path.join(str(base), target)).replace("\\", "/")
        inside_site = not resolved.startswith("..")
        if inside_site and (resolved in known or (ROOT / resolved).exists()):
            return match.group(0)
        removed += 1
        return match.group(4)

    return ANCHOR_RE.sub(replace, source), removed


def render(page: Page, sections: list[Section], order: list[Page], known: set[str]) -> str:
    source = CHROME_BLOCK.sub("\n", page.source)
    source, removed = fix_links(page, source, known)
    if removed:
        print(f"  unlinked {removed} dead link(s) in {page.rel}")

    title = html.escape(f"{page.title} · {SITE_NAME}")
    if TITLE_RE.search(source):
        source = TITLE_RE.sub(f"<title>{title}</title>", source, count=1)
    else:
        source = source.replace("<head>", f"<head><title>{title}</title>", 1)

    source = HEAD_CLOSE_RE.sub(lambda _: "\n" + styles() + "\n</head>", source, count=1)
    source = BODY_OPEN_RE.sub(
        lambda m: re.sub(r"\s*$", "", m.group(0)) + "\n" + top_bar(page, sections) + "\n",
        source,
        count=1,
    )
    source = BODY_CLOSE_RE.sub(lambda _: "\n" + footer(page, order) + "\n</body>", source, count=1)
    return source


def not_found_page(sections: list[Section]) -> str:
    items = "".join(
        f'<li><a href="{BASE_PATH}{s.index.rel}">{html.escape(s.title)}</a></li>'
        for s in sections
    )
    return f"""<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>Page not found · {SITE_NAME}</title>
<style>
  body {{ font: 16px/1.6 Inter, -apple-system, BlinkMacSystemFont, "Segoe UI", Helvetica, Arial, sans-serif;
         background: #0f111a; color: #d7dae0; margin: 0; padding: 64px 24px; }}
  main {{ max-width: 760px; margin: 0 auto; }}
  h1 {{ color: #7aa2f7; }}
  a {{ color: #7dcfff; text-decoration: none; }}
  a:hover {{ color: #ff79c6; }}
  ul {{ columns: 2; list-style: none; padding: 0; }}
  li {{ margin: 6px 0; break-inside: avoid; }}
</style>
</head>
<body>
<main>
<h1>404 &mdash; page not found</h1>
<p>That note does not exist. Try the <a href="{BASE_PATH}">notebook home</a>, or jump to a section:</p>
<ul>{items}</ul>
</main>
</body>
</html>
"""


def main() -> None:
    home, sections = collect()
    order: list[Page] = [home]
    for section in sections:
        order.extend(section.pages)
    known = {page.rel for page in order} | {"index.html"}

    print(f"Building site: {len(order)} pages across {len(sections)} sections")
    for page in order:
        page.path.write_text(render(page, sections, order, known), encoding="utf-8")

    shutil.copyfile(ROOT / "README.html", ROOT / "index.html")
    (ROOT / "404.html").write_text(not_found_page(sections), encoding="utf-8")
    (ROOT / ".nojekyll").write_text("", encoding="utf-8")
    print("Wrote index.html, 404.html and .nojekyll")


if __name__ == "__main__":
    main()
