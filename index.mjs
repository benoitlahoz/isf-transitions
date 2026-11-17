'use strict';

import { promises as fs } from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';
import { buildModules } from './modules.mjs';
import {
  buildThirdPartyLicenses,
  extractLicenseNotice,
} from './licenses.mjs';
import { execFile } from 'child_process';
import { promisify } from 'util';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const execFileAsync = promisify(execFile);

const DIR = 'converted';
const THIRD_PARTY_LICENSES = 'THIRD_PARTY_LICENSES.md';
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

// The version of this package is the one of the gl-transitions package it is converted from.
const syncVersion = async () => {
  const { version } = JSON.parse(
    await fs.readFile(
      path.join(GL_TRANSITIONS_DIR, 'packages', 'gl-transitions', 'package.json'),
      'utf-8'
    )
  );

  const packageJsonPath = path.resolve(__dirname, 'package.json');
  const packageJson = JSON.parse(await fs.readFile(packageJsonPath, 'utf-8'));
  if (packageJson.version === version) return;

  packageJson.version = version;
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
    await syncVersion();

    // Loaded now because it relies on the gl-transitions repository.
    const { convertGLSLToISF, toDisplayName } = await import('./convert.mjs');

    const files = (await fs.readdir(GL_TRANSITIONS_TRANSITIONS_DIR)).filter(
      (file) => path.extname(file) === '.glsl'
    );

    const licenses = [];
    const modules = [];

    for (const file of files) {
      const name = path.parse(file).name;
      const content = await fs.readFile(
        path.join(GL_TRANSITIONS_TRANSITIONS_DIR, file),
        'utf-8'
      );
      const converted = convertGLSLToISF(name, content);

      await fs.writeFile(
        path.join(__dirname, DIR, `${toDisplayName(name)}.fs`),
        converted.content,
        'utf-8'
      );

      errors.push(...converted.errors);
      modules.push({
        name,
        title: toDisplayName(name),
        file: `${toDisplayName(name)}.fs`,
        author: converted.author,
        license: converted.license,
        description: converted.description,
        content: converted.content,
      });
      licenses.push({
        file: `${toDisplayName(name)}.fs`,
        author: converted.author,
        license: converted.license,
        notice:
          converted.license === 'MIT' ? undefined : extractLicenseNotice(content),
      });
    }

    const built = buildModules(modules);
    for (const [file, content] of [
      ['transitions.mjs', built.transitions],
      ['transitions.d.ts', built.transitionsTypes],
      ['catalog.mjs', built.catalog],
      ['catalog.d.ts', built.catalogTypes],
    ]) {
      await fs.writeFile(path.join(__dirname, file), content, 'utf-8');
    }

    await fs.writeFile(
      path.join(__dirname, THIRD_PARTY_LICENSES),
      buildThirdPartyLicenses(
        licenses,
        await fs.readFile(path.join(GL_TRANSITIONS_DIR, 'LICENSE'), 'utf-8')
      ),
      'utf-8'
    );

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
