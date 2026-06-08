# isf-transitions

[gl-transitions](https://github.com/gl-transitions/gl-transitions) converted to [Interactive Shader Format](https://editor.isf.video/) + a very basic zero-dependency converter and installer.

## v0.0.2

### Usage

Download the latest release or clone the repository and copy / paste the `converted` folder's content to your ISF directory.

You can also run

`npm run copy`

or

`yarn copy`

to automatically copy the converted transitions in your ISF folder (tested on macOS).

If the `gl-transitions` folder is missing, the conversion script will try to recover it automatically before running. You no longer need to update the submodule manually for a normal conversion run.

If not specifically mentionned below, all shaders work in [Millumin 5](https://www.millumin.com/v5/index.php), [VDMX 5](https://vidvox.net/) and [ISF Editor](https://isf.vidvox.net/desktop-editor/) on macOS. Some of the shaders have also been tested on macOS in After Effects with [ISF plugin](https://github.com/baku89/ISF4AE).

#### Flip

Destination (`endImage` input) image **appears vertically flipped in Millumin**.
A `flip` option has been added to handle this case.

### Conversion

If you want to update and convert the current `gl-transitions` repository:

- Clone this repository.
- If `gl-transitions` is missing, `npm run convert` or `yarn convert` will fetch it automatically.
- Otherwise, update the submodule if you want the latest upstream changes: `git submodule update --remote --merge`.
- `npm run convert` or `yarn convert`.

The scripts intentionally keep `package.json` without a `packageManager` field.

It is strongly suggested to make a **Pull Request** if things have changed :-)!

#### Patches

Some shaders need manual changes to work.
A `patches.json` file helps to make these changes automatically in a very rudimentary way.

| Shader Name       | Comments                  |
| ----------------- | ------------------------- |
| burn              | Expected `vec3` as color. |
| circle            | Expected `vec3` as color. |
| dissolve          | Expected `vec3` as color. |
| fadecolor         | Expected `vec3` as color. |
| undulatingBurnOut | Expected `vec3` as color. |

### Known bugs

| Shader Name    | Bug                               | Why                                           | Fixed |
| -------------- | --------------------------------- | --------------------------------------------- | ----- |
| EdgeTransition | Doesn't compile in ISF4AE plugin. | `Array size must appear after variable name.` |       |
| StereoViewer   | Crashes in Millumin.              |                                               |       |

## TODO

- Tests with [interactive-shader-format.js](https://github.com/msfeldstein/interactive-shader-format-js)
- Documentation / examples site.
- Refactor parsing methods.
- Decouple implementations.
- Search for `vec3` to `vec4` color input conversion automatically.

**PR VERY WELCOME!!!**

If you want to contribute with a 'native ISF' transition, please put it in the `transitions` folder.

**`converted` folder is deleted and generated automatically on conversion.**

## Author

Benoît LAHOZ for this package, initial authors for the transitions (see in each file).

## License

This package: MIT

Shaders: see for each file (as of 20240606 --> MIT, BSD-2 and BSD-3).
