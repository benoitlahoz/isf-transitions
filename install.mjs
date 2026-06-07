import path from 'path';
import { promises as fs } from 'fs';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const INPUT = 'converted';
const OUTPUT = 'isf-transitions';

const install = async () => {
  let folder;
  if (process.platform === 'darwin') {
    folder = `${process.env.HOME}/Library/Graphics/ISF`;
  } else if (process.platform === 'linux') {
    folder = `${process.env.HOME}/.local/share/ISF`;
  } else if (process.platform.includes('win')) {
    folder = `${process.env.PROGRAMDATA}/ISF`;
  }

  if (!folder) {
    throw new Error('Unable to install files, platform unrecognized.');
  }

  const targetFolder = path.resolve(folder, OUTPUT);

  await fs.rm(targetFolder, {
    recursive: true,
    force: true,
  });

  await fs.mkdir(targetFolder, {
    recursive: true,
  });

  // Copy files to main directory.
  await copy(targetFolder);
};

const copy = async (folder) => {
  const files = await fs.readdir(path.resolve(__dirname, INPUT), {
    withFileTypes: true,
  });

  for await (const file of files) {
    const content = await fs.readFile(
      path.resolve(file.path, file.name),
      'utf-8'
    );

    await fs.writeFile(path.join(folder, file.name), content, 'utf-8');
  }
};

install()
  .then(() => {
    console.log(`Successfully installed ISF shaders.`);
  })
  .catch((err) => console.error(err));
