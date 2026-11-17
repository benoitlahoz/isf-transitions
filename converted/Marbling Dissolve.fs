/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "The incoming image floats onto the outgoing one in thin bands of ink that a rake drags into zigzags and a fine comb pulls into feathers, like marbled paper; the bands widen until they cover the frame and the combs pull back out, leaving the incoming image clean (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "tines",
            "NAME": "tines",
            "TYPE": "float",
            "DEFAULT": 6,
            "MIN": 2,
            "MAX": 16
        },
        {
            "LABEL": "pull",
            "NAME": "pull",
            "TYPE": "float",
            "DEFAULT": 0.12,
            "MIN": 0,
            "MAX": 0.2
        },
        {
            "LABEL": "bands",
            "NAME": "bands",
            "TYPE": "float",
            "DEFAULT": 12,
            "MIN": 3,
            "MAX": 30
        },
        {
            "LABEL": "veins",
            "NAME": "veins",
            "TYPE": "float",
            "DEFAULT": 0.5,
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

// how sharply the picture is pulled into a point right at a tooth, relative to the spacing of the teeth
const float CUSP = 0.12;
// teeth of the fine comb pulled across, per frame height
const float COMB = 9.0;

// folds coordinates back into the frame, so a dragged image never shows its clamped edges
vec2 mirror(vec2 v) {
  return 1.0 - abs(1.0 - mod(v, 2.0));
}

// pull of a tooth on the bath at distance t (in tooth spacings, 0..1): 1 at the tooth, 0 at the next one,
// pointed at the tooth like ink dragged by a needle
float drag(float t) {
  float far = CUSP / (1.0 + CUSP);
  return (CUSP / (t + CUSP) - far) / (1.0 - far);
}

float ease(float x) {
  x = clamp(x, 0.0, 1.0);
  return x * x * (3.0 - 2.0 * x);
}

// where the bath at p (in frame heights) was before the rake and the comb went through it
vec2 unrake(vec2 p, float rake, float comb) {
  // fine comb pulled across the frame, all teeth going the same way: feathers
  float c = p.y * COMB;
  float t = fract(c);
  p.x -= comb * drag(2.0 * min(t, 1.0 - t));
  // rake pulled up and down, its teeth going in turns one way and the other: zigzags
  // spaced along the longer side of the frame, with a tooth in the middle
  float s = max(ratio, 1.0) / max(tines, 1.0);
  float k = (p.x - 0.5 * ratio) / s;
  float u = fract(k);
  float side = mod(floor(k), 2.0) < 0.5 ? 1.0 : -1.0;
  p.y -= rake * side * (drag(u) - drag(1.0 - u));
  return p;
}

float bandPhase(vec2 q) {
  return q.y * bands + 0.35 * sin(q.x * 2.3 + 1.0) + 0.2 * sin(q.x * 5.1 - q.y * 3.0);
}

vec4 transition(vec2 uv) {
  float rake = pull * ease(progress / 0.35) * ease((1.0 - progress) / 0.35);
  float comb = 0.6 * pull * ease((progress - 0.15) / 0.3) * ease((0.85 - progress) / 0.3);
  vec2 p = vec2(uv.x * ratio, uv.y);
  vec2 q = unrake(p, rake, comb);
  // phase gradient on screen, to antialias the bands where the rake squeezed them
  float e = 0.001;
  float ph = bandPhase(q);
  float gx = bandPhase(unrake(p + vec2(e, 0.0), rake, comb)) - ph;
  float gy = bandPhase(unrake(p + vec2(0.0, e), rake, comb)) - ph;
  float aa = max(length(vec2(gx, gy)), 0.002);
  float cover = ease((progress - 0.08) / 0.84);
  // half width of the bands: from just below 0 to just above 0.5, so they are exactly empty then full
  float w = cover * (0.5 + 2.0 * aa) - aa;
  float d = abs(fract(ph) - 0.5); // 0 in the middle of a band, 0.5 between two
  float m = 1.0 - smoothstep(w - aa, w + aa, d);
  vec2 suv = mirror(vec2(q.x / ratio, q.y));
  vec4 col = mix(getFromColor(suv), getToColor(suv), m);
  float line = 1.0 - smoothstep(0.0, 2.5 * aa + 0.02, abs(d - w));
  float show = veins * 4.0 * cover * (1.0 - cover); // exactly 0 when the bands are empty or full
  col.rgb *= 1.0 - 0.6 * show * line;
  return col;
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
