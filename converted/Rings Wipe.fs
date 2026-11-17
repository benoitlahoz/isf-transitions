/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "Concentric rings ripple out from the center and, one after another, each sweeps around like a clock hand in the opposite direction of its neighbors to reveal the incoming image, or fills across its width, with optional colored borders between the rings (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "DEFAULT": 6,
            "VALUES": [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20],
            "LABELS": ["1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20"]
        },
        {
            "LABEL": "center",
            "NAME": "center",
            "TYPE": "point2D",
            "DEFAULT": [0.5, 0.5],
            "MIN": [-2, -2],
            "MAX": [2, 2]
        },
        {
            "LABEL": "stagger",
            "NAME": "stagger",
            "TYPE": "float",
            "DEFAULT": 0.5,
            "MIN": 0,
            "MAX": 1
        },
        {
            "LABEL": "radial",
            "NAME": "radial",
            "TYPE": "bool",
            "DEFAULT": false
        },
        {
            "LABEL": "borderWidth",
            "NAME": "borderWidth",
            "TYPE": "float",
            "DEFAULT": 0.008,
            "MIN": 0,
            "MAX": 0.05
        },
        {
            "LABEL": "borderColor",
            "NAME": "borderColor",
            "TYPE": "color",
            "DEFAULT": [1, 1, 1, 1],
            "MIN": [0, 0, 0, 0],
            "MAX": [1, 1, 1, 1]
        },
        {
            "LABEL": "smoothness",
            "NAME": "smoothness",
            "TYPE": "float",
            "DEFAULT": 0.004,
            "MIN": 0,
            "MAX": 0.1
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
const float AA = 0.0015;

// progress of ring i, from 0 (not started) to 1 (done), each ring lasting d and starting
// k * d after the previous one
float ringProgress(float i, float k, float d, float t) {
  return clamp((t - i * k * d) / d, 0.0, 1.0);
}

// how much ring i, of width w and progress lp, shows the incoming image at radius rad and
// turn a (0 to 1)
float ringMask(float i, float lp, float w, float rad, float a, float s) {
  bool odd = mod(i, 2.0) > 0.5;
  if (radial) {
    // the filled part grows across the ring, from its inner edge or from its outer one,
    // starting and ending a soft edge past the ring; measured within the ring, so a ring
    // that has not started does not leak into its neighbor's antialiased boundary
    float e = s / w;
    float f = mix(-e, 1.0 + e, lp);
    float x = clamp((rad - i * w) / w, 0.0, 1.0);
    float dist = (odd ? f - (1.0 - x) : f - x) * w;
    return smoothstep(-s, s, dist);
  }
  // the filled part is a growing arc from the top, clockwise on even rings and
  // counterclockwise on odd ones; both of its ends are soft, and it starts empty and
  // ends full, wrapping around, by a soft edge measured along the circle (no shorter than
  // the softness itself near the center, so the center is not left half blended)
  float aa = odd ? 1.0 - a : a;
  float arc = 2.0 * PI * max(rad, s);
  float e = s / arc;
  float lo = mix(e, -e, lp);
  float hi = mix(-e, 1.0 + e, lp);
  float dist = max(min(aa - lo, hi - aa), min(aa - 1.0 - lo, hi - aa + 1.0));
  return smoothstep(-s, s, dist * arc);
}

vec4 transition(vec2 uv) {
  vec4 a = getFromColor(uv);
  vec4 b = getToColor(uv);

  float n = float(count < 1 ? 1 : count);
  // measured in square pixels, so the rings are round
  vec2 p = (uv - center) * vec2(ratio, 1.0);
  vec2 far = max(center, 1.0 - center) * vec2(ratio, 1.0);
  float R = length(far) + AA;
  float w = R / n;
  float rad = length(p);
  float turn = rad > 0.0 ? fract(atan(p.x, p.y) / (2.0 * PI)) : 0.0;
  float s = max(min(smoothness, 0.25 * w), AA);
  float t = mix(progress, progress * progress * (3.0 - 2.0 * progress), 0.35);
  float k = clamp(stagger, 0.0, 1.0);
  float d = 1.0 / (1.0 + k * (n - 1.0));

  // blend the two rings on either side of the nearest boundary, so it is antialiased
  float j = floor(rad / w + 0.5);
  float inner = clamp(j - 1.0, 0.0, n - 1.0);
  float outer = clamp(j, 0.0, n - 1.0);
  float li = ringProgress(inner, k, d, t);
  float lo = ringProgress(outer, k, d, t);
  float m = mix(
    ringMask(inner, li, w, rad, turn, s),
    ringMask(outer, lo, w, rad, turn, s),
    smoothstep(-AA, AA, rad - j * w)
  );

  // mixed premultiplied, so transparent images do not bleed their color
  vec4 c = mix(vec4(a.rgb * a.a, a.a), vec4(b.rgb * b.a, b.a), m);

  // the border between two rings shows from when the inner one starts until the outer one
  // is done, without dimming when one hands over to the other
  if (borderWidth > 0.0 && j >= 1.0 && j <= n - 1.0) {
    float on = min(1.0, 4.0 * li * (1.0 - lo));
    // never wider than the ring, and thinner than the antialiasing only by fading out
    float hw = min(0.5 * borderWidth, 0.5 * w - AA);
    float line = (1.0 - smoothstep(hw - AA, hw + AA, abs(rad - j * w)))
      * min(1.0, borderWidth / (2.0 * AA));
    float o = borderColor.a * line * on;
    c = vec4(borderColor.rgb * o, o) + c * (1.0 - o);
  }
  return c.a > 0.0 ? vec4(c.rgb / c.a, c.a) : vec4(0.0);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
