/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "A grid of square tiles, each holding two quarter-circle arcs, links into long winding paths; starting from the middle and spreading outward, the paths are drawn as thin lines of the incoming image, then thicken into bands with a dark outline and a soft shadow until they merge over the whole frame (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "cells",
            "NAME": "cells",
            "TYPE": "float",
            "DEFAULT": 8,
            "MIN": 3,
            "MAX": 20
        },
        {
            "LABEL": "outline",
            "NAME": "outline",
            "TYPE": "float",
            "DEFAULT": 0.6,
            "MIN": 0,
            "MAX": 1
        },
        {
            "LABEL": "spread",
            "NAME": "spread",
            "TYPE": "float",
            "DEFAULT": 0.75,
            "MIN": 0,
            "MAX": 1
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

const float AA = 0.0012;
const float LINE = 0.003;
const float SHADOW = 0.025;
// half width of the thin line a path is first drawn as, in tiles
const float STROKE = 0.08;

float hash12(vec2 v) {
  vec3 v3 = fract(vec3(v.xyx) * 0.1031);
  v3 += dot(v3, v3.yzx + 33.33);
  return fract((v3.x + v3.y) * v3.z);
}

float vnoise(vec2 v) {
  vec2 i = floor(v);
  vec2 f = fract(v);
  f = f * f * (3.0 - 2.0 * f);
  return mix(
    mix(hash12(i), hash12(i + vec2(1.0, 0.0)), f.x),
    mix(hash12(i + vec2(0.0, 1.0)), hash12(i + vec2(1.0, 1.0)), f.x),
    f.y
  );
}

vec4 transition(vec2 uv) {
  float n = max(floor(cells + 0.5), 1.0);
  vec2 p = (uv - 0.5) * vec2(ratio, 1.0);
  vec2 g = p * n + 0.5;
  vec2 cell = floor(g);
  vec2 q = fract(g);
  // each tile is mirrored at random, so its two arcs (centered on opposite corners) link up with its neighbors' in two ways
  if (hash12(cell + 17.0) > 0.5) q.x = 1.0 - q.x;
  // distance to the nearest arc, in tiles: continuous across the tiles' edges, at most 0.5 (at the corners)
  float d = min(abs(length(q) - 0.5), abs(length(q - 1.0) - 0.5));

  float T = clamp(length(p) / (0.5 * length(vec2(ratio, 1.0))) + 0.4 * (vnoise(p * 3.0 + 4.0) - 0.5), 0.0, 1.0);
  float lag = 0.8 * clamp(spread, 0.0, 1.0);
  // every point is fully covered at progress 0.95
  float s = clamp((progress - lag * T) / (0.95 - lag), 0.0, 1.0);
  // in tiles: the paths are first drawn as thin lines, then the bands widen past the tiles' corners
  float aa = AA * n;
  float k = clamp((s - 0.25) / 0.75, 0.0, 1.0);
  float w = STROKE * smoothstep(0.0, 0.3, s) + (0.5 + LINE * n + 3.0 * aa - STROKE) * k * k;
  // signed distance to the edge of the bands (negative inside), in tiles
  float e = d - w;

  float band = 1.0 - smoothstep(-2.0 * aa, 0.0, e);
  float gap = max(e + aa, 0.0);
  float fall = 1.0 - smoothstep(0.0, SHADOW * n, gap);
  float edge = max(0.8 * (1.0 - smoothstep(LINE * n, LINE * n + 2.0 * aa, gap)), 0.4 * fall * fall);
  float shade = clamp(outline, 0.0, 1.0) * smoothstep(0.0, STROKE, w) * edge;

  vec4 a = getFromColor(uv);
  a.rgb *= 1.0 - shade;
  vec4 b = getToColor(uv);
  // mixed premultiplied, so transparent images do not bleed their color
  vec4 o = mix(vec4(a.rgb * a.a, a.a), vec4(b.rgb * b.a, b.a), band);
  return o.a > 0.0 ? vec4(o.rgb / o.a, o.a) : vec4(0.0);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
