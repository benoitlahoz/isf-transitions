'use strict';

// Tests of the conversion, checked with isfjs (https://www.npmjs.com/package/isfjs).
// The shaders are also compiled by WebGL 2 in a headless Chrome, when one is found.

import assert from 'node:assert/strict';
import { describe, it } from 'node:test';
import { IsfJs } from 'isfjs/factory';
import { inspectHeader } from 'isfjs/header';
import { findChrome, runInChrome } from './chrome.mjs';
import { convertGLSLToISF, toDisplayName } from './convert.mjs';
import { transitions } from './converted-transitions.mjs';

const isf = new IsfJs();

const header = (content) => {
  const inspection = inspectHeader(content);
  assert.equal(inspection.status, 'readable');
  return inspection.model;
};

const inputNames = (content) =>
  header(content).inputs.map((input) => input.NAME ?? input.name);

describe('toDisplayName', () => {
  it('inserts a space between words', () => {
    assert.equal(toDisplayName('ColorPhase'), 'Color Phase');
    assert.equal(toDisplayName('HsvFade'), 'Hsv Fade');
    assert.equal(toDisplayName('Ascii'), 'Ascii');
    assert.equal(toDisplayName('BowTie2D'), 'Bow Tie2D');
  });
});

describe('convertGLSLToISF', () => {
  const convert = (uniforms, body = 'return getFromColor(uv);') =>
    convertGLSLToISF(
      'Test',
      `// Author: Me
// License: MIT
// Description: A test
${uniforms}
vec4 transition(vec2 uv) {
  ${body}
}`
    );

  it('has no flip input and keeps the transition as is', () => {
    const { content, errors } = convert('');
    assert.deepEqual(errors, []);
    assert.ok(!inputNames(content).includes('flip'));
    assert.match(content, /gl_FragColor = transition\(isf_FragNormCoord\);/);
  });

  it('reads the header comments', async () => {
    const { metadata } = await isf.language.parse(convert('').content);
    assert.equal(metadata.CREDIT, 'Me - MIT');
    assert.match(metadata.DESCRIPTION, /^A test \(Automatically converted/);
  });

  it('uses the @range as bounds', () => {
    const { content } = convert(
      '// @range(0.5, 2) Speed\nuniform float speed; // = 1.0'
    );
    assert.match(content, /"NAME": "speed",\s+"TYPE": "float",\s+"DEFAULT": 1,\s+"MIN": 0.5,\s+"MAX": 2/);
    assert.ok(!content.includes('uniform float speed'));
    assert.ok(!content.includes('@range'));
  });

  it('converts int to long with the values of the range', () => {
    const { content } = convert(
      '// @range(2, 4, 1) Count\nuniform int count; // = 3'
    );
    assert.match(content, /"TYPE": "long"/);
    assert.match(content, /"VALUES": \[2, 3, 4\]/);
  });

  it('exposes vec3 as a color and ivec2 as a point2D, with a macro', () => {
    const { content, errors } = convert(
      `// @color
uniform vec3 tint; // = vec3(1.0, 0.5, 0.0)
uniform ivec2 grid; // = ivec2(5, 4)`,
      'return vec4(tint, 1.0) * float(grid.x);'
    );
    assert.deepEqual(errors, []);
    assert.match(content, /"NAME": "tint_isf",\s+"TYPE": "color",\s+"DEFAULT": \[1, 0.5, 0, 1\]/);
    assert.match(content, /"NAME": "grid_isf",\s+"TYPE": "point2D",\s+"DEFAULT": \[5, 4\]/);
    assert.match(content, /#define tint tint_isf\.rgb/);
    assert.match(content, /#define grid ivec2\(grid_isf\)/);
  });

  it('reports a macro that conflicts with a local variable', () => {
    const { errors } = convert(
      '// @color\nuniform vec3 tint; // = vec3(1.0)',
      'vec3 tint = vec3(0.0); return vec4(tint, 1.0);'
    );
    assert.equal(errors.length, 1);
    assert.match(errors[0], /'tint' is also declared locally/);
  });

  it('turns extra textures into image inputs', () => {
    const { content } = convert(
      'uniform sampler2D luma;',
      'return vec4(texture2D(luma, uv).rgb, 1.0);'
    );
    assert.deepEqual(inputNames(content), [
      'progress',
      'startImage',
      'endImage',
      'luma',
    ]);
    assert.match(content, /IMG_NORM_PIXEL\(luma, uv\)/);
    assert.ok(!content.includes('texture2D'));
  });

  it('reports a uniform that is neither a parameter nor a texture', () => {
    const { errors } = convert('uniform float lost;');
    assert.equal(errors.length, 1);
    assert.match(errors[0], /'lost'/);
  });
});

describe('gl-transitions', () => {
  it('has transitions', () => {
    assert.ok(transitions.length > 0);
  });

  for (const { name, content, errors } of transitions) {
    describe(name, () => {
      it('is converted without errors', () => {
        assert.deepEqual(errors, []);
      });

      it('has a valid ISF header', () => {
        const names = inputNames(content);
        assert.deepEqual(names.slice(0, 3), ['progress', 'startImage', 'endImage']);
        assert.equal(new Set(names).size, names.length);
        assert.ok(!names.includes('flip'));
        assert.deepEqual(header(content).issues, []);
      });

      it('has every uniform declared by its header', async () => {
        const { metadata } = await isf.language.parse(content);
        const analysis = isf.language.analyzer.analyze(content, metadata);
        assert.deepEqual(analysis.errors, []);
        assert.deepEqual(analysis.missingFromMetadata, []);
      });
    });
  }
});

describe('WebGL', () => {
  it('compiles every shader', { skip: !findChrome() && 'Chrome not found' }, async () => {
    const sources = {
      // A shader that must not compile, to be sure that the check works.
      'not-a-shader': 'void main() { undefined_function(); }',
    };
    for (const { name, content } of transitions) {
      const { fragmentSource } = await isf.language.transpiler.convert(content);
      sources[name] = fragmentSource;
    }

    const result = await runInChrome(`
const sources = ${JSON.stringify(sources).replace(/</g, '\\u003c')};
const gl = document.createElement('canvas').getContext('webgl2');
const failures = {};
for (const [name, source] of Object.entries(sources)) {
  if (!gl) break;
  const shader = gl.createShader(gl.FRAGMENT_SHADER);
  gl.shaderSource(shader, source);
  gl.compileShader(shader);
  if (!gl.getShaderParameter(shader, gl.COMPILE_STATUS)) {
    failures[name] = gl.getShaderInfoLog(shader).split('\\n')[0];
  }
}
report({ webgl: !!gl, failures });`);

    assert.ok(result.webgl, 'WebGL 2 is not available in Chrome');
    assert.deepEqual(Object.keys(result.failures), ['not-a-shader']);
  });
});
