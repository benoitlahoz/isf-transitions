'use strict';

import { createRequire } from 'node:module';
const require = createRequire(import.meta.url);

const patches = require('./patches.json');

const intro = (name, inputs, author, license) => `/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "${author}",
    "DESCRIPTION": "${name} - License ${license} (Automatically converted by @benoitlahoz 'isf-transitions').",
    "INPUTS": [
        {
            "DEFAULT": false,
            "LABEL": "Flip",
            "NAME": "flip",
            "TYPE": "bool"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Progress",
            "MAX": 1,
            "MIN": 0,
            "NAME": "progress",
            "TYPE": "float"
        },
        {
            "LABEL": "From",
            "NAME": "inputImage",
            "TYPE": "image"
        },
        {
            "LABEL": "To",
            "NAME": "to",
            "TYPE": "image"
        }${inputs.length > 0 ? ',\n' + inputs : ''}
    ],
    "ISFVSN": "2"
}
*/
float ratio = RENDERSIZE.x / RENDERSIZE.y;

vec4 getFromColor(vec2 uv) {
  if (flip == true) {
    return IMG_NORM_PIXEL(inputImage, vec2(uv.x, 1. - uv.y));
  } else {
    return IMG_NORM_PIXEL(inputImage, uv);
  }
}

vec4 getToColor(vec2 uv) {
  return IMG_NORM_PIXEL(to, uv);
}
`;

const outro = () => `
void main() {
	if (flip == true) {
    gl_FragColor = transition(vec2(isf_FragNormCoord.x, 1. - isf_FragNormCoord.y));
  } else {
    gl_FragColor = transition(isf_FragNormCoord);
  }
}`;

export const convertGLSLToISF = (filename, fileContent) => {
  const errors = [];

  const clean = fileContent
    .replace('texture2D', 'IMG_NORM_PIXEL')
    .replace('texture2DRect', 'IMG_PIXEL');

  const lines = clean.split('\n');

  const authorLines = lines.filter((line) =>
    line.toLowerCase().includes('author')
  );

  const authors = authorLines.map((line) => {
    const split = line.split(':');
    const potentialName = split[split.length - 1];
    return potentialName.trim();
  });

  const license = getLicense(lines);

  const inputs = [];

  const uniformLines = lines.filter((line) =>
    line.toLowerCase().startsWith('uniform')
  );

  for (const uniform of uniformLines) {
    // Get uniform and comment (where we suppose a default value can be indicated).
    let [variable, comment] = uniform.split('//');

    const [_, type, name] = variable.replace(';', '').trim().split(' ');
    let input = '';

    const cleanComment = parseComment(comment);

    switch (type) {
      case 'float': {
        const defaultValue =
          cleanComment && isNumberString(cleanComment)
            ? `"DEFAULT": ${cleanComment},`
            : '';

        input = `
        {
            "LABEL": "${name}",
            "NAME": "${name}",
            ${defaultValue}
            "TYPE": "float",
            "MAX": 1000,
            "MIN": 0
        }`;
        break;
      }
      case 'uint':
      case 'int': {
        // Available values from -1000 to 1000.

        const arr = Array(1001)
          .fill(0)
          .map((_, index) => index);
        const rev = [...arr].reverse().map((index) => -index);
        rev.pop();

        const values = rev.concat(arr);

        // Default value if successfully parsed.

        const defaultValue =
          cleanComment && isIntegerString(cleanComment)
            ? `"DEFAULT": ${cleanComment},`
            : '';

        input = `
        {
            "LABEL": "${name}",
            "NAME": "${name}",
            "TYPE": "long",
            ${defaultValue}
            "VALUES": [${values}],
            "LABELS": [${values.map((index) => `"${index}"`)}]
        }`;
        break;
      }
      case 'ivec2':
      case 'vec2': {
        let defaultValue = '';

        if (cleanComment) {
          const parsedVec = cleanComment.match(/\((.*)\)/)?.pop();
          if (parsedVec) {
            const split = parsedVec.split(',');
            defaultValue =
              split.length === 2
                ? `
            "DEFAULT": [
              ${split[0].trim()},
              ${split[1].trim()}
            ],
            `
                : `
            "DEFAULT": [
              ${split[0].trim()},
              ${split[0].trim()}
            ],
            `;
          }
        }

        input = `
        {
            "LABEL": "${name}",
            "NAME": "${name}",
            "TYPE": "point2D",
            ${defaultValue}
            "MIN": [
              0,
              0
            ],
            "MAX": [
              9000,
              9000
            ]
        }`;
        break;
      }
      case 'vec3':
      case 'vec4': {
        let defaultValue = '';

        if (cleanComment) {
          const parsedVec = cleanComment.match(/\((.*)\)/)?.pop();
          if (parsedVec) {
            const split = parsedVec.split(',');

            defaultValue =
              split.length === 1
                ? `
            "DEFAULT": [
              ${parseFloat(split[0])},
              ${parseFloat(split[0])},
              ${parseFloat(split[0])},
              ${parseFloat(split[0])}
            ],
            `
                : split.length === 3
                ? `
            "DEFAULT": [
              ${parseFloat(split[0])},
              ${parseFloat(split[1])},
              ${parseFloat(split[2])},
              0
            ],
            `
                : `
            "DEFAULT": [
              ${parseFloat(split[0])},
              ${parseFloat(split[1])},
              ${parseFloat(split[2])},
              ${parseFloat(split[3])}
            ],
            `;
          }
        }

        input = `
        {
            "LABEL": "${name}",
            "NAME": "${name}",
            "TYPE": "color",
            ${defaultValue}
            "MIN": [
              0,
              0,
              0,
              0
            ],
            "MAX": [
              1,
              1,
              1,
              1
            ]
        }`;
        break;
      }
      case 'bool': {
        const defaultValue =
          cleanComment && cleanComment.trim() === 'true'
            ? `"DEFAULT": true,`
            : cleanComment && cleanComment.trim() === 'false'
            ? `"DEFAULT": false,`
            : '';

        input = `
        {
            "LABEL": "${name}",
            "NAME": "${name}",
            ${defaultValue}
            "TYPE": "bool"
        }`;
        break;
      }
      case 'sampler2D': {
        input = `
        {
            "LABEL": "${name}",
            "NAME": "${name}",
            "TYPE": "image"
        }`;
        break;
      }
      default: {
        errors.push(
          `${filename}: uniform named '${name}' has unrecognized type '${type}'.`
        );
      }
    }

    const index = uniformLines.indexOf(uniform);
    if (index < uniformLines.length - 1) input += ',';

    inputs.push(input);

    // console.log(type, name);
    if (comment) {
      // console.log(comment.replace(/=|;| /g, ''));
    }

    const globalIndex = lines.indexOf(uniform);
    lines.splice(globalIndex, 1);

    // console.log(filename, variable, comment);
  }

  // console.log(uniformLines);
  // console.log(inputs);

  let content = `${intro(
    filename,
    inputs.join('').replace('\n', ''),
    authors.join(', '),
    license
  )}\n${lines.join('\n').trim()}\n${outro()}`;

  if (Object.keys(patches).includes(filename)) {
    const patch = patches[filename];

    for (const original in patch) {
      content = content.replaceAll(original, patch[original]);
    }
  }

  return {
    content,
    errors,
  };
};

const getLicense = (lines) => {
  const licenseLines = lines.filter((line) =>
    line.toLowerCase().includes('license')
  );

  let license = 'unknown';
  if (licenseLines.length > 0) {
    license = licenseLines.map((line) => {
      const split = line.split(':');
      const potentialName = split[split.length - 1];
      return potentialName.trim();
    })[0];
  }

  return license;
};

const parseComment = (comment) => {
  const res = comment && comment.replace(/=|;| /g, '');
  return res;
};

export const isUIntegerString = (str) => {
  return /^\d+$/.test(str);
};

export const isNumberString = (str) => {
  return /^\d+\.\d+$|^\d+$/.test(str);
};

const isIntegerString = (str) => {
  return /^-?\d+$/.test(str);
};
