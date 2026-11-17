/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "The outgoing image is cut into standing slats that topple backward like dominoes in a chain reaction across the frame, each one leaning onto the next and knocking it over, until they all lie flat and reveal the incoming image (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "count",
            "NAME": "count",
            "TYPE": "long",
            "DEFAULT": 12,
            "VALUES": [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40],
            "LABELS": ["2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40"]
        },
        {
            "LABEL": "reverse",
            "NAME": "reverse",
            "TYPE": "bool",
            "DEFAULT": false
        },
        {
            "LABEL": "knock",
            "NAME": "knock",
            "TYPE": "float",
            "DEFAULT": 0.2,
            "MIN": 0.05,
            "MAX": 1
        },
        {
            "LABEL": "lean",
            "NAME": "lean",
            "TYPE": "float",
            "DEFAULT": 0.8,
            "MIN": 0,
            "MAX": 1
        },
        {
            "LABEL": "perspective",
            "NAME": "perspective",
            "TYPE": "float",
            "DEFAULT": 0.8,
            "MIN": 0,
            "MAX": 1
        },
        {
            "LABEL": "shading",
            "NAME": "shading",
            "TYPE": "float",
            "DEFAULT": 0.6,
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

const float PI = 3.141592653589793;
const float EDGE = 0.0015;

float knockPart() {
  return clamp(knock, 0.05, 1.0);
}

// the fall of slat j, 0 standing to 1 lying flat
float fallTime(float j, float n) {
  float k = knockPart();
  float d = 1.0 / (1.0 + (n - 1.0) * k);
  return clamp((progress - j * k * d) / d, 0.0, 1.0);
}

float fallAngle(float t) {
  return 0.5 * PI * t * (0.2 + 0.8 * t);
}

// where slat j is seen at the point (xc, y) of the frame: (x across the slat, height on it, its fall, its angle),
// with a negative fall where it is not seen (its height can exceed 1, above its top)
vec4 slatPoint(float j, float n, float w, float xc, float y) {
  vec4 none = vec4(0.0, 0.0, -1.0, 0.0);
  if (j < 0.0 || j > n - 1.0) return none;
  float t = fallTime(j, n);
  if (t >= 1.0) return none;
  float a = fallAngle(t);
  float c = cos(a), s = sin(a);
  // a camera in front of the slat goes down to the level of the hinge as the slat falls,
  // so that the slat ends edge-on
  vec3 eye = vec3((j + 0.5) * w, 0.5 * c, -1.0 / max(perspective, 0.01));
  vec3 dir = vec3(xc, y, 0.0) - eye;
  float den = dir.z * c - dir.y * s;
  if (den <= 0.0) return none;
  float r = (eye.y * s - eye.z * c) / den;
  vec3 hit = eye + r * dir;
  float h = hit.y * c + hit.z * s;
  if (r <= 0.0 || h < 0.0) return none;
  float side = clamp(lean, 0.0, 1.0) * w * min(t / knockPart(), 1.0) * h;
  return vec4(hit.x - side - j * w, h, t, a);
}

// draws slat j, seen at pt, over color; front is where the next slat, which may stand in front of it, is seen
vec4 drawSlat(vec4 color, float j, vec4 pt, vec4 front, float w) {
  float t = pt.z;
  if (t < 0.0) return color;
  float s = sin(pt.w);
  float d = min(pt.x, w - pt.x);
  float m;
  if (t <= 0.0) {
    // standing still: the slats tile the frame exactly
    m = d >= 0.0 && pt.y <= 1.0 ? 1.0 : 0.0;
  } else {
    float e = EDGE * min(10.0 * t, 1.0) * (1.0 + max(perspective, 0.0) * pt.y * s);
    m = clamp(d / e + 0.5, 0.0, 1.0) * clamp((1.0 - pt.y) * max(cos(pt.w), 0.05) / e + 0.5, 0.0, 1.0);
  }
  if (m <= 0.0) return color;
  float sx = (j * w + clamp(pt.x, 0.0, w)) / ratio;
  vec4 img = getFromColor(vec2(reverse ? 1.0 - sx : sx, min(pt.y, 1.0)));
  float shade = 1.0 - shading * s * (0.4 + 0.6 * pt.y);
  if (front.z >= 0.0 && front.z < t && front.x < 0.0) {
    float reach = clamp(1.0 - (front.y - 1.0) / 0.05, 0.0, 1.0);
    shade *= 1.0 - 0.6 * shading * exp(front.x / (0.15 * w)) * reach * min((t - front.z) / knockPart(), 1.0);
  }
  img.rgb *= shade;
  return mix(color, img, m);
}

vec4 transition (vec2 uv) {
  float n = float(count < 1 ? 1 : count);
  float w = ratio / n;
  float xc = (reverse ? 1.0 - uv.x : uv.x) * ratio;
  float col = clamp(floor(xc / w), 0.0, n - 1.0);

  vec4 color = getToColor(uv);
  float t0 = fallTime(col, n);
  color.rgb *= 1.0 - 2.4 * shading * t0 * (1.0 - t0) * exp(-40.0 * uv.y);

  // the slats are seen narrower as they fall, and their tops lean over the next column at most:
  // the slat before this column, behind, then the one of this column
  vec4 cur = slatPoint(col, n, w, xc, uv.y);
  color = drawSlat(color, col - 1.0, slatPoint(col - 1.0, n, w, xc, uv.y), cur, w);
  color = drawSlat(color, col, cur, slatPoint(col + 1.0, n, w, xc, uv.y), w);
  return color;
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
