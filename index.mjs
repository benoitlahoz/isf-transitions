'use strict';

import { promises as fs, statSync } from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';
import { convertGLSLToISF } from './convert.mjs';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const DIR = 'converted';
const errors = [
  `isf-transitions: gl-transitions conversion`,
  `${new Date()}`,
  '',
  '--------------------------------------------------------------------------------',
  '',
];

const makeDir = async () => {
  try {
    await fs.stat(path.resolve(__dirname, DIR));
    await fs.rm(path.resolve(__dirname, DIR), {
      recursive: true,
      force: true,
    });
    await fs.mkdir(path.resolve(__dirname, DIR));
  } catch (err) {
    if (err.errno === -2) {
      await fs.mkdir(path.resolve(__dirname, DIR));
    } else {
      throw err;
    }
  }
};

const convert = async () => {
  try {
    const res = await fs.readdir(
      path.resolve(__dirname, 'gl-transitions', 'transitions'),
      { withFileTypes: true }
    );

    const directories = res.filter((file) =>
      statSync(path.resolve(file.path, file.name)).isDirectory()
    );
    const files = res.filter(
      (file) => !statSync(path.resolve(file.path, file.name)).isDirectory()
    );

    // console.log(directories, files);
    // TODO: directories.

    for await (const file of files) {
      const content = await fs.readFile(
        path.resolve(file.path, file.name),
        'utf-8'
      );
      const res = convertGLSLToISF(path.parse(file.name).name, content);

      await fs.writeFile(
        path.join(__dirname, DIR, `${path.parse(file.name).name}.fs`),
        res.content,
        'utf-8'
      );

      errors.push(...res.errors);
    }

    for await (const dir of directories) {
      const res = await fs.readdir(path.resolve(dir.path, dir.name), {
        withFileTypes: true,
      });

      for (const file of res) {
        const content = await fs.readFile(
          path.resolve(file.path, file.name),
          'utf-8'
        );
        const converted = convertGLSLToISF(path.parse(file.name).name, content);

        await fs.writeFile(
          path.join(__dirname, DIR, `${path.parse(file.name).name}.fs`),
          converted.content,
          'utf-8'
        );

        errors.push(...converted.errors);
      }
    }

    errors.push(
      '',
      '--------------------------------------------------------------------------------'
    );
  } catch (err) {
    throw err;
  }
};

const run = () => {
  return new Promise(async (resolve, reject) => {
    try {
      await makeDir();
      await convert();
      await fs.writeFile(
        path.resolve(__dirname, 'isf-transitions.errors.log'),
        errors.join('\n'),
        'utf-8'
      );
      resolve();
    } catch (err) {
      reject(err);
    }
  });
};

run()
  .then(async () => {
    console.log(`Successfully converted GLSL shaders to ISF.`);
  })
  .catch((err) => {
    console.error(err);
  });
