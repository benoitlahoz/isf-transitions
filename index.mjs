'use strict';

import { promises as fs, statSync } from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';
import { execFile } from 'child_process';
import { promisify } from 'util';
import { convertGLSLToISF } from './convert.mjs';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const execFileAsync = promisify(execFile);

const DIR = 'converted';
const GL_TRANSITIONS_DIR = path.resolve(__dirname, 'gl-transitions');
const GL_TRANSITIONS_TRANSITIONS_DIR = path.resolve(
  GL_TRANSITIONS_DIR,
  'transitions'
);
const errors = [
  `isf-transitions: gl-transitions conversion`,
  `${new Date()}`,
  '',
  '--------------------------------------------------------------------------------',
  '',
];

const removePackageManagerField = async () => {
  const packageJsonPath = path.resolve(__dirname, 'package.json');
  const packageJsonContent = await fs.readFile(packageJsonPath, 'utf-8');
  const packageJson = JSON.parse(packageJsonContent);

  if (!Object.prototype.hasOwnProperty.call(packageJson, 'packageManager')) {
    return;
  }

  delete packageJson.packageManager;
  await fs.writeFile(
    packageJsonPath,
    `${JSON.stringify(packageJson, null, 2)}\n`,
    'utf-8'
  );
};

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

const pathExists = async (filePath) => {
  try {
    await fs.access(filePath);
    return true;
  } catch {
    return false;
  }
};

const ensureTransitionsDirectory = async () => {
  if (await pathExists(GL_TRANSITIONS_TRANSITIONS_DIR)) {
    return;
  }

  console.log('gl-transitions not found, trying to retrieve it...');

  try {
    await execFileAsync(
      'git',
      ['submodule', 'update', '--init', '--recursive', 'gl-transitions'],
      { cwd: __dirname }
    );
  } catch {
    await fs.rm(GL_TRANSITIONS_DIR, {
      recursive: true,
      force: true,
    });

    await execFileAsync(
      'git',
      ['clone', 'https://github.com/gl-transitions/gl-transitions.git', 'gl-transitions'],
      { cwd: __dirname }
    );
  }

  if (!(await pathExists(GL_TRANSITIONS_TRANSITIONS_DIR))) {
    throw new Error(
      `Unable to find ${GL_TRANSITIONS_TRANSITIONS_DIR} after repository retrieval.`
    );
  }
};

const convert = async () => {
  try {
    await ensureTransitionsDirectory();

    const res = await fs.readdir(GL_TRANSITIONS_TRANSITIONS_DIR, {
      withFileTypes: true,
    });

    const directories = res.filter((file) =>
      statSync(path.resolve(file.path, file.name)).isDirectory()
    );
    const files = res.filter(
      (file) => !statSync(path.resolve(file.path, file.name)).isDirectory()
    );

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
      await removePackageManagerField();
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
