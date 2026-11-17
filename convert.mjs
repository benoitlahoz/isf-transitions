'use strict';

// Converts a gl-transition to an ISF shader.
// The header comments, the `// = default` values and the `@range` / `@color` hints
// are read by the parser of the gl-transitions repository itself.

import {
  parseTransition,
  parseUniformNames,
} from './gl-transitions/scripts/catalog/parse-transition.mjs';

const MAX_LONG_VALUES = 1001;

const asArray = (value, size) =>
  Array.isArray(value) ? value : Array(size).fill(value);

// Range of a parameter without `@range`, guessed from its default value.
const guessRange = (type, values) => {
  const max = Math.max(...values.map(Math.abs));

  if (type === 'int') {
    return { min: Math.min(0, ...values), max: Math.max(10, max * 2) };
  }

  const hi = Math.max(1, max * 4);
  return { min: type === 'float' && values.every((v) => v >= 0) ? 0 : -hi, max: hi };
};

const getRange = (param, type) => {
  const values = [].concat(param.default);
  const guessed = guessRange(type, values);
  const min = param.min ?? guessed.min;
  const max = param.max ?? guessed.max;

  // The default value must stay in the range.
  return {
    min: Math.min(min, ...values),
    max: Math.max(max, ...values),
  };
};

const longValues = (param) => {
  const { min, max } = getRange(param, 'int');
  const step = param.step > 0 ? param.step : 1;

  const values = [];
  for (let v = min; v <= max && values.length < MAX_LONG_VALUES; v += step) {
    values.push(v);
  }
  if (!values.includes(param.default)) {
    values.push(param.default);
    values.sort((a, b) => a - b);
  }
  return values;
};

// ISF has no `ivec2` nor `vec3` inputs: they are exposed as a `point2D` and a `color`
// (a `vec4`), and the name used by the shader is a macro converting them back.
const exposed = (name, expansion, input) => ({
  input: { LABEL: name, NAME: `${name}_isf`, ...input },
  macro: `#define ${name} ${expansion(`${name}_isf`)}`,
  name,
});

const convertParam = (name, param) => {
  const { type } = param;

  switch (type) {
    case 'bool':
      return {
        input: { LABEL: name, NAME: name, TYPE: 'bool', DEFAULT: param.default },
      };
    case 'float': {
      const { min, max } = getRange(param, type);
      return {
        input: {
          LABEL: name,
          NAME: name,
          TYPE: 'float',
          DEFAULT: param.default,
          MIN: min,
          MAX: max,
        },
      };
    }
    case 'int': {
      const values = longValues(param);
      return {
        input: {
          LABEL: name,
          NAME: name,
          TYPE: 'long',
          DEFAULT: param.default,
          VALUES: values,
          LABELS: values.map(String),
        },
      };
    }
    case 'vec2':
    case 'ivec2': {
      const { min, max } = getRange(param, type === 'vec2' ? 'vec2' : 'int');
      const input = {
        TYPE: 'point2D',
        DEFAULT: param.default,
        MIN: [min, min],
        MAX: [max, max],
      };
      return type === 'vec2'
        ? { input: { LABEL: name, NAME: name, ...input } }
        : exposed(name, (isf) => `ivec2(${isf})`, input);
    }
    case 'vec3':
    case 'vec4': {
      const rgba = type === 'vec3' ? [...param.default, 1] : param.default;
      const input = {
        TYPE: 'color',
        DEFAULT: rgba,
        MIN: [0, 0, 0, 0],
        MAX: [1, 1, 1, 1],
      };
      return type === 'vec4'
        ? { input: { LABEL: name, NAME: name, ...input } }
        : exposed(name, (isf) => `${isf}.rgb`, input);
    }
    default:
      return null;
  }
};

// Arrays of numbers (colors, points, long values) are kept on one line.
const compactArrays = (json) =>
  json.replace(/\[\s*[-\d.eE"]+(?:,\s*[-\d.eE"]+)*\s*\]/g, (array) =>
    array.replace(/\s*\n\s*/g, ' ').replace(/\[\s+/, '[').replace(/\s+\]/, ']')
  );

const header = (credit, description, inputs) =>
  `/*${compactArrays(
    JSON.stringify(
      {
        CATEGORIES: ['Transition', 'gl-transitions'],
        CREDIT: credit,
        DESCRIPTION: description,
        INPUTS: inputs,
        ISFVSN: '2',
      },
      null,
      4
    )
  )}\n*/`;

const PREAMBLE = `float ratio = RENDERSIZE.x / RENDERSIZE.y;

vec4 getFromColor(vec2 uv) {
  return IMG_NORM_PIXEL(startImage, uv);
}

vec4 getToColor(vec2 uv) {
  return IMG_NORM_PIXEL(endImage, uv);
}`;

const EPILOGUE = `void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}`;

// What is only metadata for ISF: header lines, parameter hints and uniforms
// (declared by the ISF header instead).
const stripMetadata = (glsl) =>
  glsl
    .replace(/^[ \t]*\/\/\s*(?:author|license|description|tags)\s*:.*\n?/gim, '')
    .replace(/^[ \t]*\/\/\s*@(?:range|color|param)\b.*\n?/gm, '')
    .replace(/^[ \t]*uniform\s+[^;\n]+;[^\n]*\n?/gm, '');

// 'ColorPhase' -> 'Color Phase', 'HsvFade' -> 'Hsv Fade'.
export const toDisplayName = (name) =>
  name
    .replace(/([a-z])([A-Z])/g, '$1 $2')
    .replace(/([A-Z]+)([A-Z][a-z])/g, '$1 $2');

export const convertGLSLToISF = (filename, source) => {
  const { transition, errors: parseErrors } = parseTransition(
    source,
    `${filename}.glsl`
  );
  const errors = parseErrors.map((error) => `${filename}: ${error}`);

  const inputs = [
    {
      LABEL: 'Progress',
      NAME: 'progress',
      TYPE: 'float',
      DEFAULT: 0,
      MIN: 0,
      MAX: 1,
    },
    { LABEL: 'From', NAME: 'startImage', TYPE: 'image' },
    { LABEL: 'To', NAME: 'endImage', TYPE: 'image' },
  ];
  const macros = [];

  for (const [name, param] of Object.entries(transition.params)) {
    const converted = convertParam(name, param);
    if (!converted) {
      errors.push(
        `${filename}: uniform named '${name}' has unrecognized type '${param.type}'.`
      );
      continue;
    }
    inputs.push(converted.input);
    if (converted.macro) macros.push(converted);
  }

  for (const name of transition.textures) {
    inputs.push({ LABEL: name, NAME: name, TYPE: 'image' });
  }

  // A uniform that is neither a parameter nor a texture can't be provided.
  const known = new Set([...Object.keys(transition.params), ...transition.textures]);
  for (const name of parseUniformNames(source)) {
    if (!known.has(name)) {
      errors.push(`${filename}: uniform named '${name}' is neither a parameter nor a texture.`);
    }
  }

  const body = stripMetadata(transition.glsl)
    .replace(/\btexture2D\s*\(/g, 'IMG_NORM_PIXEL(')
    .trim();

  // A macro would also replace a local variable or argument with the same name.
  for (const { name } of macros) {
    if (
      new RegExp(
        `\\b(?:bool|int|float|[i]?vec[234]|mat[234])\\s+${name}\\b`
      ).test(body)
    ) {
      errors.push(
        `${filename}: '${name}' is also declared locally, which conflicts with its ISF conversion.`
      );
    }
  }

  const description = `${
    transition.description || toDisplayName(filename)
  } (Automatically converted by @benoitlahoz 'isf-transitions').`;

  const content = [
    header(`${transition.author} - ${transition.license}`, description, inputs),
    PREAMBLE,
    ...(macros.length > 0 ? [macros.map((m) => m.macro).join('\n')] : []),
    body,
    EPILOGUE,
  ].join('\n\n');

  return {
    content: `${content}\n`,
    errors,
    author: transition.author,
    license: transition.license,
    description: transition.description,
  };
};
