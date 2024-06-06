# isf-transitions

[gl-transitions](https://github.com/gl-transitions/gl-transitions) converted to [Interactive Shader Format](https://editor.isf.video/) + a very basic zero-dependency converter and installer.

## v0.0.1

### Usage

Clone the repository and copy / paste the `converted` folder to your ISF directory.

You can also run

`npm run copy`

or

`yarn copy`

to automatically copy the converted transitions in your ISF folder (tested only on macOS).

If not specifically mentionned below, all shaders work in [Millumin 4](https://www.millumin.com/v4/index.php), [VDMX 5](https://vidvox.net/) and [ISF Editor](https://isf.vidvox.net/desktop-editor/) on macOS. Some of the shaders have also been tested on macOS in After Effects with [ISF plugin](https://github.com/baku89/ISF4AE).

#### Flip

However, destination (`to` input) image appears flipped in Millumin.
A `flip` option has been added to handle this case.

### Conversion

If one wants to update and convert the current `gl-transitions` repository:

- Clone this repository.
- Update submodule: `git submodule update --remote --merge`.
- `npm run convert` or `yarn convert`.

#### Patches

Some shaders need manual changes to work.
A `patches.json` file helps to make these changes automatically in a very rudimentary way.

| Name              | Comments                  |
| ----------------- | ------------------------- |
| burn              | Expected `vec3` as color. |
| circle            | Expected `vec3` as color. |
| dissolve          | Expected `vec3` as color. |
| fadecolor         | Expected `vec3` as color. |
| undulatingBurnOut | Expected `vec3` as color. |

### Known bugs

| Name         | Comments             |
| ------------ | -------------------- |
| StereoViewer | Crashes in Millumin. |

## Thoughts

**_VDMX_** and **_ISF4AE_** allow to get a layer as destination input of the shader (`to`). Would be great that **_Millumin_** implement that.
For the time being, Millumin only allows live inputs as images in ISF shaders.

## Author

Benoît LAHOZ

## License

MIT
