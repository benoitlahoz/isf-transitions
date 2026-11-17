'use strict';

import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import path from 'node:path';
import { describe, it } from 'node:test';
import { fileURLToPath } from 'node:url';
import { buildThirdPartyLicenses, extractLicenseNotice } from './licenses.mjs';

const TRANSITIONS_DIR = path.resolve(
  path.dirname(fileURLToPath(import.meta.url)),
  'gl-transitions',
  'transitions'
);

const read = (name) =>
  readFileSync(path.join(TRANSITIONS_DIR, `${name}.glsl`), 'utf-8');

describe('extractLicenseNotice', () => {
  it('reads a notice written in a block comment', () => {
    const notice = extractLicenseNotice(read('InvertedPageCurl'));
    assert.match(notice, /^Copyright \(c\) 2010 Hewlett-Packard/);
    assert.match(notice, /Neither the name of Hewlett-Packard/);
    assert.match(notice, /SUCH DAMAGE\.$/);
    assert.ok(!notice.includes('texCoord'));
  });

  it('reads a notice written in // comments', () => {
    const notice = extractLicenseNotice(read('StereoViewer'));
    assert.match(notice, /^Copyright \(c\) 2016, Theodore K Schundler/);
    assert.match(notice, /Redistributions in binary form/);
    assert.match(notice, /SUCH DAMAGE\.$/);
    assert.ok(!notice.includes('//'));
  });

  it('returns null without a notice', () => {
    assert.equal(extractLicenseNotice(read('Directional')), null);
  });
});

describe('buildThirdPartyLicenses', () => {
  const text = buildThirdPartyLicenses(
    [
      { file: 'Zed.fs', author: 'Zed', license: 'MIT' },
      { file: 'Alpha.fs', author: 'Alpha', license: 'MIT' },
      { file: 'Curl.fs', author: 'HP', license: 'BSD 3 Clause', notice: 'The notice.' },
      { file: 'Other.fs', author: 'Me', license: 'ISC' },
    ],
    'The upstream license.\n'
  );

  it('starts with the license of gl-transitions', () => {
    assert.match(text, /## gl-transitions\n\nThe upstream license\.\n/);
  });

  it('lists the MIT transitions first, sorted', () => {
    assert.match(text, /## MIT \(2\)\n\n- Alpha\.fs: Alpha\n- Zed\.fs: Zed/);
    assert.ok(text.indexOf('## MIT') < text.indexOf('## BSD 3 Clause'));
  });

  it('keeps the notice of the other licenses', () => {
    assert.match(text, /## BSD 3 Clause \(1\)\n\n- Curl\.fs: HP\n\n### Curl\.fs\n\nThe notice\./);
    assert.match(text, /The notice of Other\.fs is in the header of the shader\./);
  });
});

describe('the BSD transitions of gl-transitions', () => {
  it('have a notice', () => {
    for (const name of ['InvertedPageCurl', 'StereoViewer']) {
      assert.ok(extractLicenseNotice(read(name)), name);
    }
  });
});
