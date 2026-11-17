/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "Blocks switch to the incoming image one at a time, row after row from the top (or column after column from the left), each line running the opposite way to the one before, like a snake winding across the frame (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "grid",
            "NAME": "grid_isf",
            "TYPE": "point2D",
            "DEFAULT": [8, 6],
            "MIN": [0, 0],
            "MAX": [16, 16]
        },
        {
            "LABEL": "vertical",
            "NAME": "vertical",
            "TYPE": "bool",
            "DEFAULT": false
        },
        {
            "LABEL": "smoothness",
            "NAME": "smoothness",
            "TYPE": "float",
            "DEFAULT": 0.5,
            "MIN": 0,
            "MAX": 4
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

#define grid ivec2(grid_isf)

vec4 transition(vec2 uv) {
  vec4 a = getFromColor(uv);
  vec4 b = getToColor(uv);

  vec2 g = max(vec2(grid), 1.0);
  vec2 cell = min(floor(vec2(uv.x, 1.0 - uv.y) * g), g - 1.0);
  float lines = vertical ? g.x : g.y;
  float len = vertical ? g.y : g.x;
  float line = vertical ? cell.x : cell.y;
  float pos = vertical ? cell.y : cell.x;
  if (mod(line, 2.0) > 0.5) pos = len - 1.0 - pos;
  float order = line * len + pos;

  // block k fades in from k to k + w, on a timeline from 0 to n - 1 + w
  float n = lines * len;
  float w = max(smoothness, 0.001);
  float m = clamp((progress * (n - 1.0 + w) - order) / w, 0.0, 1.0);

  // mixed premultiplied, so transparent images do not bleed their color
  vec4 c = mix(vec4(a.rgb * a.a, a.a), vec4(b.rgb * b.a, b.a), m);
  return c.a > 0.0 ? vec4(c.rgb / c.a, c.a) : vec4(0.0);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
