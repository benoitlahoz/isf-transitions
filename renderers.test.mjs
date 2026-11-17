'use strict';

// Plays every transition with interactive-shader-format-js and @isf-studio/isf-renderer,
// in a headless Chrome (skipped when none is found): the shader must compile, and the frame must show
// the first image at the start of the transition and the second one at its end.

import assert from 'node:assert/strict';
import { before, describe, it } from 'node:test';
import { build } from 'esbuild';
import { findChrome, runInChrome } from './chrome.mjs';
import { transitions } from './converted-transitions.mjs';

const skip = !findChrome() && 'Chrome not found';

// What the page reports, by transition.
let results = {};

// The first frame has the first image (red) and the last one the second image (blue).
const playsFromRedToBlue = ({ start, end }) => start[0] > start[2] && end[2] > end[0];

describe('renderers', { skip }, () => {
  before(async () => {
    const bundle = await build({
      entryPoints: ['renderers.browser.js'],
      bundle: true,
      format: 'iife',
      write: false,
    });

    const sources = Object.fromEntries(
      transitions.map(({ name, content }) => [name, content])
    );

    const report = await runInChrome(
      `${bundle.outputFiles[0].text}
const sources = ${JSON.stringify(sources).replace(/</g, '\\u003c')};
window.playTransitions(sources).then(report, (error) => report({ pageError: String(error) }));`,
      { timeout: 300000 }
    );

    assert.ok(report.webgl, 'WebGL 2 is not available in Chrome');
    results = report.results;
  });

  for (const { name } of transitions) {
    describe(name, () => {
      it('plays with interactive-shader-format-js', () => {
        const { interactiveShaderFormat: frames } = results[name];
        assert.equal(frames.error, undefined);
        assert.ok(playsFromRedToBlue(frames), JSON.stringify(frames));
      });

      it('plays with isf-renderer', () => {
        const { isfRenderer } = results[name];
        assert.equal(isfRenderer.error, undefined);
        assert.deepEqual(isfRenderer.errors, []);
        assert.ok(playsFromRedToBlue(isfRenderer.frames), JSON.stringify(isfRenderer.frames));
      });
    });
  }
});
