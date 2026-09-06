#!/usr/bin/env node
/**
 * Exports every Markdown file in the repository to HTML with Markdown Preview
 * Enhanced (crossnote), overwriting any previously generated HTML, and then
 * regenerates the note catalogue embedded in index.html.
 *
 * Usage:
 *   node tools/export-html.cjs [options]
 *
 * Options:
 *   --skip-index      Only export HTML, leave index.html untouched.
 *   --index-only      Only rebuild index.html from the HTML already on disk.
 *   --filter <text>   Only export Markdown files whose path contains <text>.
 */

const fs = require('fs');
const path = require('path');

const REPO_ROOT = path.resolve(__dirname, '..');
const CROSSNOTE_DIR = path.join(REPO_ROOT, '.crossnote');
const INDEX_FILE = path.join(REPO_ROOT, 'index.html');
const INDEX_BEGIN_MARKER = '/* BEGIN GENERATED NOTES */';
const INDEX_END_MARKER = '/* END GENERATED NOTES */';

const IGNORED_DIRS = new Set(['node_modules', 'tools', 'assets', 'images']);

// Folder / file name fragments that should keep their conventional casing.
const ACRONYMS = new Map([
  ['api', 'API'],
  ['rest', 'REST'],
  ['sql', 'SQL'],
  ['uml', 'UML'],
  ['solid', 'SOLID'],
  ['oops', 'OOPs'],
  ['csharp', 'C#'],
  ['ui', 'UI'],
  ['ux', 'UX'],
  ['os', 'OS'],
  ['db', 'DB'],
  ['llm', 'LLM'],
  ['grpc', 'gRPC'],
  ['graphql', 'GraphQL'],
]);

const USAGE = `Usage: node tools/export-html.cjs [options]

Exports every Markdown file in the repository to a sibling HTML file
(overwriting existing output) and rebuilds the note catalogue in index.html.

Options:
  --skip-index      Only export HTML, leave index.html untouched.
  --index-only      Only rebuild index.html from the HTML already on disk.
  --filter <text>   Only export Markdown files whose path contains <text>.
  -h, --help        Show this message.`;

function parseArgs(argv) {
  const options = { skipIndex: false, indexOnly: false, filter: null };
  for (let i = 0; i < argv.length; i += 1) {
    const arg = argv[i];
    if (arg === '--skip-index') options.skipIndex = true;
    else if (arg === '--index-only') options.indexOnly = true;
    else if (arg === '--filter') options.filter = argv[++i] ?? null;
    else if (arg === '--help' || arg === '-h') options.help = true;
    else throw new Error(`Unknown option: ${arg}`);
  }
  return options;
}

/** Recursively collects files matching `extension`, skipping dot and ignored folders. */
function collectFiles(dir, extension, out = []) {
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    if (entry.name.startsWith('.') || IGNORED_DIRS.has(entry.name)) continue;
    const full = path.join(dir, entry.name);
    if (entry.isDirectory()) collectFiles(full, extension, out);
    else if (entry.isFile() && entry.name.toLowerCase().endsWith(extension)) out.push(full);
  }
  return out;
}

function toPosix(relativePath) {
  return relativePath.split(path.sep).join('/');
}

/** "HighLevelDesign" -> "High Level Design", "ApiDesign" -> "API Design". */
function humanizeName(name) {
  const words = name
    .replace(/[-_]+/g, ' ')
    .replace(/([a-z0-9])([A-Z])/g, '$1 $2')
    .replace(/([A-Z]+)([A-Z][a-z])/g, '$1 $2')
    .split(/\s+/)
    .filter(Boolean);

  return words
    .map((word) => ACRONYMS.get(word.toLowerCase()) ?? word)
    .join(' ');
}

/** "05-HighLevelDesign" -> "05 · High Level Design". */
function sectionTitle(folderName) {
  const match = /^(\d+)[-_](.+)$/.exec(folderName);
  if (match) return `${match[1]} · ${humanizeName(match[2])}`;
  return humanizeName(folderName);
}

function stripInlineMarkdown(text) {
  return text
    .replace(/!\[[^\]]*\]\([^)]*\)/g, '')
    .replace(/\[([^\]]*)\]\([^)]*\)/g, '$1')
    .replace(/<[^>]+>/g, '')
    .replace(/`([^`]*)`/g, '$1')
    .replace(/[*_]{1,3}([^*_]+)[*_]{1,3}/g, '$1')
    .replace(/\s+/g, ' ')
    .trim();
}

function truncate(text, maxLength = 150) {
  if (text.length <= maxLength) return text;
  const clipped = text.slice(0, maxLength);
  const lastSpace = clipped.lastIndexOf(' ');
  return `${clipped.slice(0, lastSpace > 40 ? lastSpace : maxLength).trimEnd()}…`;
}

/** Reads the H1 and a short summary paragraph out of a Markdown file. */
function readNoteMetadata(markdownFile) {
  const raw = fs.readFileSync(markdownFile, 'utf8');
  const withoutFrontMatter = raw.replace(/^---\r?\n[\s\S]*?\r?\n---\r?\n/, '');
  const lines = withoutFrontMatter.split(/\r?\n/);

  let title = null;
  const paragraph = [];
  let inFencedBlock = false;

  for (const line of lines) {
    const trimmed = line.trim();

    if (/^(```|~~~)/.test(trimmed)) {
      inFencedBlock = !inFencedBlock;
      continue;
    }
    if (inFencedBlock) continue;

    if (!title) {
      const heading = /^#\s+(.*)$/.exec(trimmed);
      if (heading) title = stripInlineMarkdown(heading[1]);
      continue;
    }

    if (!trimmed) {
      if (paragraph.length) break;
      continue;
    }
    // A new heading before any prose means the note has no summary paragraph.
    if (trimmed.startsWith('#')) break;
    // Skip tables, horizontal rules, images and standalone list markers.
    if (/^(\||-{3,}|={3,}|!\[)/.test(trimmed)) {
      if (paragraph.length) break;
      continue;
    }

    const candidate = stripInlineMarkdown(trimmed.replace(/^>\s?/, '').replace(/^[-*+]\s+/, ''));
    if (candidate) paragraph.push(candidate);
    else if (paragraph.length) break;
  }

  const fallbackTitle = humanizeName(path.basename(markdownFile, path.extname(markdownFile)));
  const description = paragraph
    .join(' ')
    .replace(/^(core idea|key idea|tl;dr|summary|overview)\s*:\s*/i, '')
    .trim();

  return {
    title: title || fallbackTitle,
    description: description ? truncate(description) : '',
  };
}

/** Fallback used when a note has no prose under its H1 (e.g. a bare problem table). */
function defaultDescription(markdownFile, title) {
  const base = path.basename(markdownFile, '.md').toLowerCase();
  const topic = title.split(/\s+[—–-]\s+/)[0] || humanizeName(path.basename(path.dirname(markdownFile)));
  if (base === 'problems') return `Worked problems and solutions for ${topic}.`;
  if (base === 'readme') return `Overview and index for ${topic}.`;
  return `Notes on ${topic}.`;
}

/**
 * crossnote swallows `style.less` compile errors: instead of throwing it returns a
 * stylesheet containing `.crossnote { display: none !important; }`, so every exported
 * page silently renders blank. Catch that here and abort before any file is written.
 */
function assertGlobalCssCompiled(globalCss) {
  if (typeof globalCss === 'string' && globalCss.includes('Failed to compile `style.less`')) {
    const detail = /Failed to compile `style\.less`\.\s*([\s\S]*?)"\s*!important;/.exec(globalCss);
    throw new Error(
      `Failed to compile .crossnote/style.less — aborting so no blank HTML is written.\n` +
        `${(detail?.[1] ?? '').trim()}\n\n` +
        `Note: Less resolves \`@import url(...)\` over the network at build time. ` +
        `Use \`@import (css) url(...)\` for remote stylesheets so the import is emitted as ` +
        `plain CSS and left for the browser to fetch.`,
    );
  }
}

async function exportMarkdownFiles(markdownFiles) {
  const { Notebook, loadConfigsInDirectory, wrapNodeFSAsApi } = require('crossnote');

  const fsApi = wrapNodeFSAsApi();
  const config = fs.existsSync(CROSSNOTE_DIR)
    ? await loadConfigsInDirectory(CROSSNOTE_DIR, fsApi, true)
    : {};

  assertGlobalCssCompiled(config.globalCss);

  const notebook = await Notebook.init({
    notebookPath: REPO_ROOT,
    config: {
      ...config,
      previewTheme: 'github-light.css',
      codeBlockTheme: 'auto.css',
      breakOnSingleNewLine: true,
      enableScriptExecution: false,
    },
    fs: fsApi,
  });

  const failures = [];
  let exported = 0;

  for (const markdownFile of markdownFiles) {
    const relative = toPosix(path.relative(REPO_ROOT, markdownFile));
    try {
      const engine = notebook.getNoteMarkdownEngine(markdownFile);
      await engine.htmlExport({ offline: false, runAllCodeChunks: false });
      exported += 1;
      console.log(`  ok    ${relative}`);
    } catch (error) {
      failures.push({ relative, error });
      console.error(`  FAIL  ${relative} :: ${error && error.message}`);
    }
  }

  return { exported, failures };
}

/** Builds the section/note tree that index.html renders. */
function buildIndexSections() {
  const markdownFiles = collectFiles(REPO_ROOT, '.md').filter((file) =>
    fs.existsSync(file.replace(/\.md$/i, '.html')),
  );

  const rootNotes = [];
  const sectionBuckets = new Map();

  for (const markdownFile of markdownFiles) {
    const relative = toPosix(path.relative(REPO_ROOT, markdownFile));
    const segments = relative.split('/');
    const note = {
      title: '',
      href: relative.replace(/\.md$/i, '.html'),
      description: '',
      _file: markdownFile,
      _segments: segments,
    };

    const metadata = readNoteMetadata(markdownFile);
    note.title = metadata.title;
    note.description = metadata.description || defaultDescription(markdownFile, metadata.title);

    if (segments.length === 1) {
      rootNotes.push(note);
      continue;
    }

    const sectionKey = segments[0];
    if (!sectionBuckets.has(sectionKey)) sectionBuckets.set(sectionKey, []);
    sectionBuckets.get(sectionKey).push(note);
  }

  const noteSortKey = (note) => {
    const segments = note._segments;
    const fileName = segments[segments.length - 1].toLowerCase();
    const folder = segments.slice(0, -1).join('/').toLowerCase();
    const fileRank = fileName === 'readme.md' ? 0 : fileName === 'problems.md' ? 2 : 1;
    return `${folder}\u0000${fileRank}\u0000${fileName}`;
  };

  // Plain code-unit comparison: localeCompare would ignore the \u0000 separators.
  const bySortKey = (a, b) => {
    const [x, y] = [noteSortKey(a), noteSortKey(b)];
    return x < y ? -1 : x > y ? 1 : 0;
  };

  const sections = [];

  if (rootNotes.length) {
    sections.push({
      title: 'Start Here',
      notes: rootNotes.sort(bySortKey).map(stripInternalFields),
    });
  }

  for (const sectionKey of [...sectionBuckets.keys()].sort()) {
    const notes = sectionBuckets.get(sectionKey);
    const direct = [];
    const subsectionBuckets = new Map();

    for (const note of notes) {
      // segments[0] is the section folder, the last segment is the file name.
      const depth = note._segments.length - 2;
      if (depth <= 1) {
        direct.push(note);
        continue;
      }
      const subsectionKey = note._segments[1];
      if (!subsectionBuckets.has(subsectionKey)) subsectionBuckets.set(subsectionKey, []);
      subsectionBuckets.get(subsectionKey).push(note);
    }

    const section = { title: sectionTitle(sectionKey) };
    if (direct.length) section.notes = direct.sort(bySortKey).map(stripInternalFields);

    const subsections = [...subsectionBuckets.keys()].sort().map((key) => ({
      title: humanizeName(key),
      notes: subsectionBuckets.get(key).sort(bySortKey).map(stripInternalFields),
    }));
    if (subsections.length) section.subsections = subsections;

    sections.push(section);
  }

  return sections;
}

function stripInternalFields(note) {
  return { title: note.title, href: note.href, description: note.description };
}

function renderSectionsSnippet(sections) {
  const json = JSON.stringify(sections, null, 2)
    .split('\n')
    .map((line, i) => (i === 0 ? line : `      ${line}`))
    .join('\n');
  return `      ${INDEX_BEGIN_MARKER}\n      const sections = ${json};\n      ${INDEX_END_MARKER}`;
}

function updateIndexHtml(sections) {
  if (!fs.existsSync(INDEX_FILE)) {
    throw new Error(`index.html not found at ${INDEX_FILE}`);
  }

  const html = fs.readFileSync(INDEX_FILE, 'utf8');
  const start = html.indexOf(INDEX_BEGIN_MARKER);
  const end = html.indexOf(INDEX_END_MARKER);

  if (start === -1 || end === -1 || end < start) {
    throw new Error(
      `index.html is missing the "${INDEX_BEGIN_MARKER}" / "${INDEX_END_MARKER}" markers around the sections array.`,
    );
  }

  const lineStart = html.lastIndexOf('\n', start) + 1;
  const lineEnd = html.indexOf('\n', end + INDEX_END_MARKER.length);
  const updated =
    html.slice(0, lineStart) +
    renderSectionsSnippet(sections) +
    (lineEnd === -1 ? '' : html.slice(lineEnd));

  fs.writeFileSync(INDEX_FILE, updated, 'utf8');
}

function countNotes(sections) {
  return sections.reduce(
    (total, section) =>
      total +
      (section.notes?.length ?? 0) +
      (section.subsections ?? []).reduce((sub, s) => sub + s.notes.length, 0),
    0,
  );
}

async function main() {
  let options;
  try {
    options = parseArgs(process.argv.slice(2));
  } catch (error) {
    console.error(error.message);
    console.error(`\n${USAGE}`);
    process.exit(2);
  }

  if (options.help) {
    console.log(USAGE);
    return;
  }

  let failures = [];

  if (!options.indexOnly) {
    let markdownFiles = collectFiles(REPO_ROOT, '.md').sort();
    if (options.filter) {
      markdownFiles = markdownFiles.filter((file) =>
        toPosix(file).toLowerCase().includes(options.filter.toLowerCase()),
      );
    }

    console.log(`Exporting ${markdownFiles.length} Markdown file(s) to HTML…`);
    const result = await exportMarkdownFiles(markdownFiles);
    failures = result.failures;
    console.log(`Exported ${result.exported}/${markdownFiles.length} file(s).`);
  }

  if (!options.skipIndex) {
    const sections = buildIndexSections();
    updateIndexHtml(sections);
    console.log(
      `Updated index.html with ${countNotes(sections)} note(s) across ${sections.length} section(s).`,
    );
  }

  if (failures.length) {
    console.error(`\n${failures.length} file(s) failed to export:`);
    for (const failure of failures) console.error(`  ${failure.relative}: ${failure.error.stack}`);
    process.exit(1);
  }
}

main().then(
  () => process.exit(0),
  (error) => {
    console.error(error);
    process.exit(1);
  },
);
