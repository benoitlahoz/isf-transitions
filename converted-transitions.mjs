'use strict';

// Every transition of gl-transitions, converted in memory, for the tests.

import { readdirSync, readFileSync } from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { convertGLSLToISF } from './convert.mjs';

const TRANSITIONS_DIR = path.resolve(
  path.dirname(fileURLToPath(import.meta.url)),
  'gl-transitions',
  'transitions'
);

export const transitions = readdirSync(TRANSITIONS_DIR)
  .filter((file) => file.endsWith('.glsl'))
  .map((file) => {
    const name = path.parse(file).name;
    const source = readFileSync(path.join(TRANSITIONS_DIR, file), 'utf-8');
    return { name, ...convertGLSLToISF(name, source) };
  });
