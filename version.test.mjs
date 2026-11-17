'use strict';

import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import path from 'node:path';
import { describe, it } from 'node:test';
import { fileURLToPath } from 'node:url';

const ROOT = path.dirname(fileURLToPath(import.meta.url));
const json = (...file) =>
  JSON.parse(readFileSync(path.join(ROOT, ...file), 'utf-8'));

describe('the version', () => {
  const { version } = json('gl-transitions', 'packages', 'gl-transitions', 'package.json');

  it('is the one of gl-transitions (run `npm run convert`)', () => {
    assert.equal(json('package.json').version, version);
  });

  it('has its section in the README', () => {
    assert.match(readFileSync(path.join(ROOT, 'README.md'), 'utf-8'), new RegExp(`^## v${version.replaceAll('.', '\\.')}$`, 'm'));
  });
});
