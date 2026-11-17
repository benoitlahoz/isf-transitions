/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "The outgoing image folds up like a pleated paper toward one side while the incoming one unfolds out of the other, both in 3D with lit and shadowed pleats (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "pleats",
            "NAME": "pleats",
            "TYPE": "long",
            "DEFAULT": 4,
            "VALUES": [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
            "LABELS": ["1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12"]
        },
        {
            "LABEL": "direction",
            "NAME": "direction",
            "TYPE": "point2D",
            "DEFAULT": [-1, 0],
            "MIN": [-4, -4],
            "MAX": [4, 4]
        },
        {
            "LABEL": "shading",
            "NAME": "shading",
            "TYPE": "float",
            "DEFAULT": 0.7,
            "MIN": 0,
            "MAX": 1
        },
        {
            "LABEL": "perspective",
            "NAME": "perspective",
            "TYPE": "float",
            "DEFAULT": 1.5,
            "MIN": 0,
            "MAX": 3
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

const float LIGHT_SIDE = 0.55;
const float LIGHT_FRONT = 0.835;
const float EDGE = 0.0015;

// brightness of a panel folded by angle (c, s) = (cos, sin), facing the light (sign 1) or away (-1)
float panelLight(float c, float s, float facing) {
  float lambert = (c * LIGHT_FRONT + facing * s * LIGHT_SIDE) / LIGHT_FRONT;
  return mix(1.0, clamp(lambert, 0.08, 1.25), shading);
}

// f: position along the image (0 at its anchored side), c: cos of the fold angle, len: its length, b: position across it (frame heights)
vec4 pleated(float f, float c, float len, float b, float bLen, bool incoming, bool flipA, bool vertical) {
  float m = 2.0 * float(pleats > 1 ? pleats : 1);
  float s = sqrt(max(1.0 - c * c, 0.0));
  float u = f * m;
  // f = 1 is the end of the last panel, not the start of one more
  float panel = min(floor(u), m - 1.0);
  float fu = u - panel;
  // the ridges (odd boundaries) stay in the frame's plane, the valleys (even ones) recede
  float tri = 1.0 - abs(mod(u, 2.0) - 1.0);
  float panelLen = len / m;
  float z = panelLen * s * (1.0 - tri);
  float scale = 1.0 / (1.0 + perspective * z);
  float bb = (b - 0.5 * bLen) / scale / bLen + 0.5;
  // soft top and bottom edges while folded (sharp when flat, to keep the first and last frames exact)
  float edge = EDGE * min(1.0, 20.0 * s * c) + 1e-6;
  float cover = clamp(min(bb, 1.0 - bb) * bLen * scale / edge, 0.0, 1.0);
  float a = flipA ? 1.0 - f : f;
  vec2 st = clamp(vertical ? vec2(bb, a) : vec2(a, bb), 0.0, 1.0);
  vec4 img = incoming ? getToColor(st) : getFromColor(st);
  float facing = mod(panel, 2.0) < 0.5 ? 1.0 : -1.0;
  float toEdge = min(fu, 1.0 - fu) * panelLen * c;
  float k = 0.5 + 0.5 * clamp(toEdge / EDGE, 0.0, 1.0);
  float light = mix(panelLight(c, s, -facing), panelLight(c, s, facing), k);
  float valley = 1.0 - tri;
  light *= 1.0 - 0.4 * shading * s * valley * valley;
  return mix(backColor, vec4(img.rgb * light, img.a), cover);
}

vec4 transition(vec2 uv) {
  bool vertical = abs(direction.y) > abs(direction.x);
  bool positive = vertical ? direction.y > 0.0 : direction.x > 0.0;
  // a: along the motion, 0 on the side the outgoing image folds up toward; b: across it
  float a = vertical ? uv.y : uv.x;
  if (positive) a = 1.0 - a;
  float b = vertical ? uv.x : uv.y;
  float len = vertical ? 1.0 : ratio;
  float bLen = vertical ? ratio : 1.0;

  float p = progress;
  float e = p * p * (3.0 - 2.0 * p);
  e = mix(p, e, 0.8);
  float cFrom = 1.0 - e;
  float cTo = e;
  if (a < cFrom || cTo <= 0.0) {
    return pleated(a / cFrom, cFrom, len, b * bLen, bLen, false, positive, vertical);
  }
  return pleated(1.0 - (1.0 - a) / cTo, cTo, len, b * bLen, bLen, true, positive, vertical);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
