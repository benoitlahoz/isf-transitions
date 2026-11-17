/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "The frame is cut into 3D prisms that turn in a cascade, like a rotating billboard, from the outgoing image on one face to the incoming one on the next (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "slats",
            "NAME": "slats",
            "TYPE": "long",
            "DEFAULT": 8,
            "VALUES": [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40],
            "LABELS": ["2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40"]
        },
        {
            "LABEL": "stagger",
            "NAME": "stagger",
            "TYPE": "float",
            "DEFAULT": 0.45,
            "MIN": 0,
            "MAX": 0.9
        },
        {
            "LABEL": "perspective",
            "NAME": "perspective",
            "TYPE": "float",
            "DEFAULT": 1,
            "MIN": 0,
            "MAX": 4
        },
        {
            "LABEL": "shading",
            "NAME": "shading",
            "TYPE": "float",
            "DEFAULT": 0.6,
            "MIN": 0,
            "MAX": 1
        },
        {
            "LABEL": "horizontal",
            "NAME": "horizontal",
            "TYPE": "bool",
            "DEFAULT": false
        },
        {
            "LABEL": "backColor",
            "NAME": "backColor",
            "TYPE": "color",
            "DEFAULT": [0, 0, 0, 1],
            "MIN": [0, 0, 0, 0],
            "MAX": [1, 1, 1, 1]
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

const float PI = 3.141592653589793;
const float EDGE = 0.002;

vec4 transition (vec2 uv) {
  float n = float(slats);
  // a: the axis across the prisms, b: the axis they turn around
  float a = horizontal ? uv.y : uv.x;
  float b = horizontal ? uv.x : uv.y;
  // the prisms' half width in frame heights, and the perspective from their depth
  float hw = 0.5 / n * (horizontal ? 1.0 : ratio);
  float k = min(perspective * hw, 0.5);
  float bScale = horizontal ? ratio : 1.0;
  // a turning prism is wider than its strip: keep the two nearest faces
  vec4 near1 = vec4(0.0), near2 = vec4(0.0);
  float cover1 = 0.0, cover2 = 0.0;
  float z1 = -2.0, z2 = -2.0;
  for (int d = -1; d <= 1; d++) {
    float i = floor(a * n) + float(d);
    if (i < 0.0 || i > n - 1.0) continue;
    float order = (horizontal ? n - 1.0 - i : i) / (n - 1.0);
    float t = clamp((progress - stagger * order) / (1.0 - stagger), 0.0, 1.0);
    t = mix(t, t * t * (3.0 - 2.0 * t), 0.7);
    float c = cos(0.5 * PI * t);
    float s = sin(0.5 * PI * t);
    // soft edges while turning (sharp at rest, to keep the first and last frames exact)
    float edge = EDGE * min(1.0, 20.0 * s * c) + 1e-6;
    // x: position across the strip, -1 to 1; the prism's square section has its faces at distance 1
    float x = 2.0 * (a * n - i) - 1.0;
    for (int f = 0; f < 2; f++) {
      // f = 0: the outgoing face turning away, f = 1: the incoming face turning in
      float cf = f == 0 ? c : s;
      if (cf < 1e-4) continue;
      // u: position on the face, z: depth (1 for a face at rest)
      float u = f == 0 ? (x + s) / c : (x - c) / s;
      float z = f == 0 ? u * s + c : s - u * c;
      if (abs(u) > 1.0 || z <= z2) continue;
      float scale = 1.0 - k * (z - 1.0);
      float y = (b - 0.5) * scale + 0.5;
      if (y < 0.0 || y > 1.0) continue;
      // soften the outer edges only: the faces meet at the prism's corner
      float outer = f == 0 ? 1.0 + u : 1.0 - u;
      float cover = clamp(outer * hw * cf / edge, 0.0, 1.0)
        * clamp(min(y, 1.0 - y) * bScale / scale / edge, 0.0, 1.0);
      float sa = (i + 0.5 + 0.5 * u) / n;
      vec2 p = horizontal ? vec2(y, sa) : vec2(sa, y);
      vec4 img = f == 0 ? getFromColor(p) : getToColor(p);
      float lambert = clamp(f == 0 ? c - 0.6 * s : s + 0.6 * c, 0.0, 1.0);
      vec4 color = vec4(img.rgb * mix(1.0 - shading, 1.0, lambert), img.a);
      if (z > z1) {
        near2 = near1; cover2 = cover1; z2 = z1;
        near1 = color; cover1 = cover; z1 = z;
      } else {
        near2 = color; cover2 = cover; z2 = z;
      }
    }
  }
  return mix(mix(backColor, near2, cover2), near1, cover1);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
