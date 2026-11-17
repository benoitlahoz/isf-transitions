# isf-transitions

[gl-transitions](https://github.com/gl-transitions/gl-transitions) converted to [Interactive Shader Format](https://editor.isf.video/) + the converter, built on the `gl-transitions` parser, and an installer for your ISF folder. No runtime dependency: only `node` and `git` are needed.

## v3.3.0

The version of this package is the one of the [`gl-transitions`](https://www.npmjs.com/package/gl-transitions) package it is converted from (`npm run convert` updates `package.json`).

### Changes

- The version now follows the one of `gl-transitions` (it was `0.0.2`).
- The converter relies on the parser of `gl-transitions` itself (`gl-transitions/scripts/catalog/parse-transition.mjs`): headers (`Author`, `License`, `Description`), `// = default` values and `@range` / `@color` hints are no longer parsed by hand. Parameters now get their real bounds and defaults.
- Compatible with the new `gl-transitions` layout (flat `transitions` folder, PascalCase names) and with recent Node versions (the conversion and `npm run copy` crashed on `Dirent.path`).
- The `flip` input has been removed.
- `vec3` and `ivec2` uniforms are converted automatically (`color` / `point2D` input + `#define`), so `patches.json` is gone.
- Extra `sampler2D` uniforms (`LumaWipe`, `DisplacementMap`) become `image` inputs.
- The shader description comes from the `Description` header, and the license is added to the credit (`CREDIT`: `Author - License`).
- Converted files are named with a space between words (`Color Phase.fs`).
- The package exports every transition by name (`import { ColorPhase } from 'isf-transitions'`, with types) and a `catalog` with `loadTransition(name)` (`isf-transitions/catalog`); `main` no longer points to the converter.
- Tests with [isfjs](https://www.npmjs.com/package/isfjs) (`npm test` / `yarn test`), and every transition played with `@isf-studio/isf-renderer` and interactive-shader-format-js in headless Chrome.
- `THIRD_PARTY_LICENSES.md` is generated on conversion, from the headers of the transitions (the license of each one, with the full text of the BSD notices).
- Examples with [`@isf-studio/isf-renderer`](https://www.npmjs.com/package/@isf-studio/isf-renderer) and [interactive-shader-format-js](https://github.com/msfeldstein/interactive-shader-format-js).

### Usage

#### From npm

```sh
npm install isf-transitions
```

- The package contains the converted shaders (`converted`), the modules to import them (see [Examples](#examples)), `THIRD_PARTY_LICENSES.md` and `install.mjs`. The converter is only in the repository.
- To copy the shaders to your ISF folder, run `node node_modules/isf-transitions/install.mjs`.
- The shaders keep the license of their original transition (`Author - License` in the `CREDIT` of each one): if you redistribute them, keep `THIRD_PARTY_LICENSES.md` with them.

#### From the repository

Download the latest release or clone the repository and copy / paste the `converted` folder's content to your ISF directory.

You can also run

`npm run copy`

or

`yarn copy`

to automatically copy the converted transitions in your ISF folder (tested on macOS).

If the `gl-transitions` folder is missing, the conversion script will try to recover it automatically before running. You no longer need to update the submodule manually for a normal conversion run.

If not specifically mentionned below, all shaders work in [Millumin 5](https://www.millumin.com/v5/index.php), [VDMX 5](https://vidvox.net/) and [ISF Editor](https://isf.vidvox.net/desktop-editor/) on macOS. Some of the shaders have also been tested on macOS in After Effects with [ISF plugin](https://github.com/baku89/ISF4AE).

### Examples

A transition has three inputs in common: `startImage`, `endImage` and `progress` (from `0` to `1`). The others are its parameters, with the bounds and the defaults of `gl-transitions`.

`isf-transitions` exports the text of each shader, named like its `gl-transitions` identifier (`ColorPhase` for `Color Phase.fs`): no bundler plugin is needed, and a bundler only keeps the transitions you import.

```ts
import { ColorPhase, CrossZoom } from 'isf-transitions';
```

`isf-transitions/catalog` lists them (`name`, `title`, `file`, `author`, `license`, `description`) without their sources, and `loadTransition(name)` gives the source of one of them when you need it (its sources are loaded on demand: the catalog stays light):

```ts
import { catalog, loadTransition } from 'isf-transitions/catalog';

catalog[0]; // { name: 'Accordion', title: 'Accordion', file: 'Accordion.fs', author: 'gre', license: 'MIT', description: '…' }

const source = await loadTransition('ColorPhase'); // rejects for a name that is not in the catalog
```

A menu of all the transitions, played with `@isf-studio/isf-renderer`:

```ts
import { IsfRenderer } from '@isf-studio/isf-renderer';
import { catalog, loadTransition, type TransitionName } from 'isf-transitions/catalog';

const select = document.querySelector('select')!;
select.replaceChildren(
  ...catalog.map(({ name, title, author, license }) => new Option(`${title} (${author}, ${license})`, name))
);

const renderer = new IsfRenderer();
renderer.bind(document.querySelector('canvas')!);

const play = async (name: TransitionName) => {
  await renderer.load(await loadTransition(name));
  renderer.set('startImage', '/images/a.jpg');
  renderer.set('endImage', '/images/b.jpg');
  renderer.set('progress', 0.5);
};

select.addEventListener('change', () => play(select.value as TransitionName));
await play(catalog[0].name);
```

The `.fs` files are also available as `isf-transitions/converted/Color Phase.fs`, for a host that reads files (`npm run copy` installs them in your ISF folder).

#### With `@isf-studio/isf-renderer`

[`@isf-studio/isf-renderer`](https://www.npmjs.com/package/@isf-studio/isf-renderer) plays a shader on a canvas.

```ts
import { IsfRenderer } from '@isf-studio/isf-renderer';
import { ColorPhase } from 'isf-transitions';

const renderer = new IsfRenderer();
renderer.bind(document.querySelector('canvas')!);
await renderer.load(ColorPhase);

renderer.set('startImage', '/images/a.jpg'); // a URL or a Blob
renderer.set('endImage', '/images/b.jpg');
renderer.set('fromStep', 0.2); // a parameter of this transition

// Two seconds from the first image to the second one.
const start = performance.now();
const tick = (now: number) => {
  const progress = Math.min((now - start) / 2000, 1);
  renderer.set('progress', progress);
  if (progress < 1) requestAnimationFrame(tick);
};
requestAnimationFrame(tick);

// renderer.dispose() when you are done.
```

`renderer.inputs` lists the inputs of the loaded transition, to build your own controls.

#### With `interactive-shader-format-js`

[interactive-shader-format-js](https://github.com/msfeldstein/interactive-shader-format-js) (`npm install interactive-shader-format`) draws with a WebGL context you give it. It takes images, videos or canvases as image inputs.

```js
import { Renderer } from 'interactive-shader-format';
import { ColorPhase } from 'isf-transitions';

const canvas = document.querySelector('canvas');
const renderer = new Renderer(canvas.getContext('webgl2'));

renderer.loadSource(ColorPhase);
if (!renderer.valid) throw new Error(renderer.error);

const imageA = document.querySelector('#a'); // <img>, <video> or <canvas>
const imageB = document.querySelector('#b');

const start = performance.now();
const tick = (now) => {
  const progress = Math.min((now - start) / 2000, 1);
  // The images are given again before each frame, as in the examples of the library
  // (in version 2.8.1, the first image given before the very first frame is drawn black).
  renderer.setValue('startImage', imageA);
  renderer.setValue('endImage', imageB);
  renderer.setValue('progress', progress);
  renderer.draw(canvas);
  if (progress < 1) requestAnimationFrame(tick);
};
requestAnimationFrame(tick);
```

All the converted transitions are played with both libraries by the tests (see [Tests](#tests)).

### Conversion

If you want to update and convert the current `gl-transitions` repository:

- Clone this repository.
- If `gl-transitions` is missing, `npm run convert` or `yarn convert` will fetch it automatically.
- Otherwise, update the submodule if you want the latest upstream changes: `git submodule update --remote --merge`.
- `npm run convert` or `yarn convert`.

The scripts intentionally keep `package.json` without a `packageManager` field.

It is strongly suggested to make a **Pull Request** if things have changed :-)!

#### File names

The converted files are named after the `gl-transitions` identifiers, with a space inserted between words: `ColorPhase.glsl` becomes `Color Phase.fs`.

#### Parameters

Parameters are read with the parser of `gl-transitions` itself (headers, `// = default` values and `@range` / `@color` hints).

- `float`, `int` and `bool` uniforms become `float`, `long` and `bool` inputs (with the `@range` as bounds).
- `vec2` uniforms become `point2D` inputs; `vec4` uniforms become `color` inputs.
- ISF has neither `vec3` nor `ivec2` inputs: they are exposed as a `color` / `point2D` named `<name>_isf`, and a `#define <name>` converts them back for the shader.
- Extra `sampler2D` uniforms become `image` inputs.

### Tests

`npm test` or `yarn test` (run `yarn` first to install the dev dependencies: [isfjs](https://www.npmjs.com/package/isfjs), the two renderers and esbuild to bundle them for the browser).

For each `gl-transitions` shader, the tests check with `isfjs` that the conversion has no error, that the ISF header is valid and declares every uniform of the shader.

If Google Chrome is found (or set with `CHROME_PATH`), headless Chrome also runs:

- every shader, compiled by WebGL 2;
- every transition, played with [`@isf-studio/isf-renderer`](https://www.npmjs.com/package/@isf-studio/isf-renderer) and with [interactive-shader-format-js](https://github.com/msfeldstein/interactive-shader-format-js) between a red and a blue image: the shader must compile, `isf-renderer` must report no error, and the frame drawn must be red at the start of the transition and blue at its end.

Without Chrome, these tests are skipped. They take about a minute.

### Known bugs

| Shader Name    | Bug                               | Why                                           | Fixed |
| -------------- | --------------------------------- | --------------------------------------------- | ----- |
| EdgeTransition | Doesn't compile in ISF4AE plugin. | `Array size must appear after variable name.` |       |
| StereoViewer   | Crashes in Millumin.              |                                               |       |

## CONTRIBUTING

**PR VERY WELCOME!!!**

If you want to contribute with a 'native ISF' transition, please put it in the `transitions` folder.

**`converted` folder is deleted and generated automatically on conversion, and so are `THIRD_PARTY_LICENSES.md`, `transitions.mjs`, `transitions.d.ts`, `catalog.mjs` and `catalog.d.ts`.**

## ISF Studio

<a href="https://isf-studio.app">
    <div style="width: 100%; text-align: center;">
    <picture>
        <source media="(prefers-color-scheme: dark)" srcset="assets/icon-white.png">
        <img src="assets/icon-black.png" alt="ISF Studio" width="128" align="middle">
    </picture>
    </div>
</a>

<br><br>

[ISF Studio](https://isf-studio.app) is a desktop application for ISF, made by the author of this package.

Write GLSL with a live preview or connect nodes in a pipeline, keep your whole library of shaders in order, and export ISF for VDMX, Resolume or Millumin, OpenFX plug-ins (e.g. DaVinci Resolve), videos (ProRes, MP4) and images.

- A library is an ordinary folder: you can open the `converted` folder in it, to browse and tweak these transitions.
- [`isfjs`](https://www.npmjs.com/package/isfjs) is its engine, and [`@isf-studio/isf-renderer`](https://www.npmjs.com/package/@isf-studio/isf-renderer) plays its pipelines and shaders on the web: both are used in the examples and the tests of this package.
- The transitions are being tested in ISF Studio itself.

## Author

Benoît LAHOZ for this package, initial authors for the transitions (see in each file).

## License

This package: MIT

Shaders: see `THIRD_PARTY_LICENSES.md` (generated) and the `CREDIT` of each file, `Author - License`. As of 20261117: 278 MIT, 1 BSD-3 Clause (`InvertedPageCurl`) and 1 BSD-2 Clause (`StereoViewer`).
