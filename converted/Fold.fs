/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "nwoeanhinnogaehr - MIT",
    "DESCRIPTION": "The incoming image unfolds from the left while the outgoing one folds away to the right (Automatically converted by @benoitlahoz 'isf-transitions').",
    "INPUTS": [
        {
            "LABEL": "Progress",
            "NAME": "progress",
            "TYPE": "float",
            "DEFAULT": 0,
            "MIN": 0,
            "MAX": 1
        },
        {
            "LABEL": "From",
            "NAME": "startImage",
            "TYPE": "image"
        },
        {
            "LABEL": "To",
            "NAME": "endImage",
            "TYPE": "image"
        }
    ],
    "ISFVSN": "2"
}
*/

float ratio = RENDERSIZE.x / RENDERSIZE.y;

vec4 getFromColor(vec2 uv) {
  return IMG_NORM_PIXEL(startImage, uv);
}

vec4 getToColor(vec2 uv) {
  return IMG_NORM_PIXEL(endImage, uv);
}

// Ported from https://gist.github.com/nwoeanhinnogaehr/f6fc39f4cfcbb97f96a6

vec4 transition(vec2 uv) {
  // Only sample the visible side: the other one divides by zero at progress 0 and 1.
  if (progress == 0.0 || uv.x > progress) return getFromColor((uv - vec2(progress, 0.0)) / vec2(1.0 - progress, 1.0));
  return getToColor(uv / vec2(progress, 1.0));
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
