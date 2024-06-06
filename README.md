# isf-transitions

## v0.0.1

If not mentionned, all shaders work in Millumin, VDMX and ISF Editor on macOS.

| Name         | Comments             |
| ------------ | -------------------- |
| StereoViewer | Crashes in Millumin. |

### Flip

However, `to` image appears flipped in Millumin, not in VDMX and ISF Editor.
A `flip` option has been added to handle this case.

### Patches

Some shaders need manual changes to work.
A `patches.json` file helps to make these changes automatically in a very rudimentary way.

| Name              | Comments                  |
| ----------------- | ------------------------- |
| burn              | Expected `vec3` as color. |
| circle            | Expected `vec3` as color. |
| dissolve          | Expected `vec3` as color. |
| fadecolor         | Expected `vec3` as color. |
| undulatingBurnOut | Expected `vec3` as color. |
