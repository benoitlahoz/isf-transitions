'use strict';

import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import path from 'node:path';
import { describe, it } from 'node:test';
import { fileURLToPath } from 'node:url';
import { inspectHeader } from 'isfjs/header';
import { toDisplayName } from './convert.mjs';
import { transitions } from './converted-transitions.mjs';
import { buildModules } from './modules.mjs';

const ROOT = path.dirname(fileURLToPath(import.meta.url));
const read = (file) => readFileSync(path.join(ROOT, file), 'utf-8');

const built = buildModules(
  transitions.map((transition) => ({
    ...transition,
    title: toDisplayName(transition.name),
    file: `${toDisplayName(transition.name)}.fs`,
  }))
);

describe('the modules of the package', () => {
  it('are up to date (run `npm run convert`)', () => {
    assert.equal(read('transitions.mjs'), built.transitions);
    assert.equal(read('transitions.d.ts'), built.transitionsTypes);
    assert.equal(read('catalog.mjs'), built.catalog);
    assert.equal(read('catalog.d.ts'), built.catalogTypes);
  });

  it('give the source of a transition with its PascalCase name', async () => {
    // The package imports itself by its name, as its users do.
    const { ColorPhase } = await import('isf-transitions');
    assert.equal(ColorPhase, read('converted/Color Phase.fs'));
    assert.equal(inspectHeader(ColorPhase).status, 'readable');
  });

  it('export every transition', async () => {
    const exported = await import('isf-transitions');
    assert.deepEqual(
      Object.keys(exported).sort(),
      transitions.map(({ name }) => name).sort()
    );
    for (const source of Object.values(exported)) {
      assert.equal(typeof source, 'string');
    }
  });

  it('list the transitions in the catalog', async () => {
    const { catalog } = await import('isf-transitions/catalog');
    assert.equal(catalog.length, transitions.length);

    const phase = catalog.find(({ name }) => name === 'ColorPhase');
    assert.equal(phase.title, 'Color Phase');
    assert.equal(phase.file, 'Color Phase.fs');
    assert.equal(phase.license, 'MIT');
    assert.ok(phase.description.length > 0);
  });

  it('load a transition by name', async () => {
    const { loadTransition } = await import('isf-transitions/catalog');
    const { CrossZoom } = await import('isf-transitions');
    assert.equal(await loadTransition('CrossZoom'), CrossZoom);
    await assert.rejects(() => loadTransition('NotATransition'), /Unknown transition 'NotATransition'/);
    await assert.rejects(() => loadTransition('toString'), /Unknown transition/);
  });

  it('give access to the converted files', () => {
    assert.ok(
      import.meta.resolve('isf-transitions/converted/Color Phase.fs').endsWith('/converted/Color%20Phase.fs')
    );
  });
});
