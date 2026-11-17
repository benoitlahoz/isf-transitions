/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "Alternating bands wipe in from opposite sides together and interleave like fingers, while both images stay still (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "bands",
            "NAME": "bands",
            "TYPE": "long",
            "DEFAULT": 8,
            "VALUES": [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40],
            "LABELS": ["2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40"]
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
            "DEFAULT": 0.02,
            "MIN": 0,
            "MAX": 0.2
        },
        {
            "LABEL": "stagger",
            "NAME": "stagger",
            "TYPE": "float",
            "DEFAULT": 0.1,
            "MIN": 0,
            "MAX": 0.8
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

const float AA = 0.0015;

// how far band i has wiped in (0..1) at position a along the bands (0..l), alternate bands
// entering from opposite ends
float bandMask(float i, float n, float a, float l, float s) {
  float k = vertical ? i : n - 1.0 - i;
  float g = clamp(stagger, 0.0, 0.95);
  float t = clamp((progress - g * k / max(n - 1.0, 1.0)) / (1.0 - g), 0.0, 1.0);
  t = mix(t, t * t * (3.0 - 2.0 * t), 0.5);
  float x = mod(i, 2.0) > 0.5 ? l - a : a;
  // the edge starts fully before the band and ends fully past it
  float c = mix(-s, l + s, t);
  return 1.0 - smoothstep(c - s, c + s, x);
}

vec4 transition(vec2 uv) {
  float n = float(bands < 1 ? 1 : bands);
  // along: position along the bands, in frame heights; across: position across them (0..1)
  float along = vertical ? uv.y : uv.x * ratio;
  float across = vertical ? uv.x : uv.y;
  float l = vertical ? 1.0 : ratio;
  float aaAcross = vertical ? AA / ratio : AA;
  float s = max(smoothness, AA);

  float f = across * n;
  float i = clamp(floor(f), 0.0, n - 1.0);
  float m = bandMask(i, n, along, l, s);

  // near a cut between two bands, blend with the neighbor band over a pixel or so so the cut
  // does not alias (at the frame's borders the neighbor is the band itself)
  float fr = f - i;
  float edge = min(fr, 1.0 - fr) / n;
  if (edge < aaAcross) {
    float j = clamp(fr < 0.5 ? i - 1.0 : i + 1.0, 0.0, n - 1.0);
    m = mix(bandMask(j, n, along, l, s), m, 0.5 + 0.5 * smoothstep(0.0, aaAcross, edge));
  }

  if (m <= 0.0) return getFromColor(uv);
  if (m >= 1.0) return getToColor(uv);
  vec4 a = getFromColor(uv);
  vec4 b = getToColor(uv);
  // mixed premultiplied, so transparent images do not bleed their color
  vec4 c = mix(vec4(a.rgb * a.a, a.a), vec4(b.rgb * b.a, b.a), m);
  return c.a > 0.0 ? vec4(c.rgb / c.a, c.a) : vec4(0.0);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
