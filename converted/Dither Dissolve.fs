/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "The incoming image appears through a regular 8x8 ordered dither pattern, like a retro computer screen (Automatically converted by @benoitlahoz 'isf-transitions').",
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
        },
        {
            "LABEL": "dotSize",
            "NAME": "dotSize",
            "TYPE": "float",
            "DEFAULT": 0.02,
            "MIN": 0.002,
            "MAX": 0.05
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

// Threshold of the 8x8 Bayer matrix at integer cell c, in (0, 1)
float bayer(vec2 c) {
  float v = 0.0;
  float scale = 16.0;
  for (int i = 0; i < 3; i++) {
    vec2 bit = mod(floor(c), 2.0);
    // a 2x2 step: 0 2 / 3 1, i.e. 2 * (x xor y) + y
    v += scale * (2.0 * abs(bit.x - bit.y) + bit.y);
    c *= 0.5;
    scale *= 0.25;
  }
  return (v + 0.5) / 64.0;
}

vec4 transition(vec2 uv) {
  vec2 cell = floor(uv * vec2(ratio, 1.0) / dotSize);
  return progress < bayer(cell) ? getFromColor(uv) : getToColor(uv);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
