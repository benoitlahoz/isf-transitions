'use strict';

// Runs a page in headless Chrome (software WebGL 2) for the tests that need a browser.

import { execFile } from 'node:child_process';
import { existsSync, rmSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import path from 'node:path';
import { promisify } from 'node:util';

const execFileAsync = promisify(execFile);

const CANDIDATES = [
  process.env.CHROME_PATH,
  '/Applications/Google Chrome.app/Contents/MacOS/Google Chrome',
  '/usr/bin/google-chrome',
  '/usr/bin/chromium',
  '/usr/bin/chromium-browser',
].filter(Boolean);

export const findChrome = () =>
  CANDIDATES.find((candidate) => existsSync(candidate));

// The page gets `report(value)` and calls it once, when it is done.
// `script` is the code of the page; the value comes back as JSON.
export const runInChrome = async (script, { timeout = 60000 } = {}) => {
  const page = path.join(tmpdir(), `isf-transitions-${process.pid}-${Date.now()}.html`);
  writeFileSync(
    page,
    `<script>
const report = (value) =>
  document.write('<pre>' + 'ISF_TRANSITIONS_' + 'RESULT' + JSON.stringify(value) + 'ISF_TRANSITIONS_' + 'END</pre>');
// A headless page run by --dump-dom never gets a frame: a timer stands for it.
window.requestAnimationFrame = (callback) => setTimeout(() => callback(performance.now()), 16);
window.cancelAnimationFrame = (id) => clearTimeout(id);
window.addEventListener('error', (e) => report({ pageError: String(e.message) }));
</script>
<script>${script.replace(/<\/script/gi, '<\\/script')}</script>`
  );

  try {
    const { stdout } = await execFileAsync(
      findChrome(),
      [
        '--headless=new',
        '--disable-gpu',
        '--use-angle=swiftshader',
        '--enable-unsafe-swiftshader',
        `--virtual-time-budget=${timeout}`,
        '--dump-dom',
        `file://${page}`,
      ],
      { maxBuffer: 1024 * 1024 * 256, timeout: timeout * 3 }
    );

    const result = stdout.match(/ISF_TRANSITIONS_RESULT(.*?)ISF_TRANSITIONS_END/s);
    if (!result) throw new Error('The page did not report a result.' + stdout.slice(-300));

    const value = JSON.parse(result[1]);
    if (value.pageError) throw new Error(value.pageError);
    return value;
  } finally {
    rmSync(page, { force: true });
  }
};
