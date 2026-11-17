/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "Discs of three solid colors burst out of a point one after another, each covering the last, then a hole bursts open from the same point in the last one to reveal the incoming image (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "center",
            "NAME": "center",
            "TYPE": "point2D",
            "DEFAULT": [0.5, 0.5],
            "MIN": [-2, -2],
            "MAX": [2, 2]
        },
        {
            "LABEL": "color1",
            "NAME": "color1",
            "TYPE": "color",
            "DEFAULT": [0.99, 0.79, 0.29, 1],
            "MIN": [0, 0, 0, 0],
            "MAX": [1, 1, 1, 1]
        },
        {
            "LABEL": "color2",
            "NAME": "color2",
            "TYPE": "color",
            "DEFAULT": [0.95, 0.38, 0.33, 1],
            "MIN": [0, 0, 0, 0],
            "MAX": [1, 1, 1, 1]
        },
        {
            "LABEL": "color3",
            "NAME": "color3",
            "TYPE": "color",
            "DEFAULT": [0.13, 0.15, 0.3, 1],
            "MIN": [0, 0, 0, 0],
            "MAX": [1, 1, 1, 1]
        },
        {
            "LABEL": "stagger",
            "NAME": "stagger",
            "TYPE": "float",
            "DEFAULT": 0.3,
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

const float AA = 0.0015;
const float SW = 0.03;
const float SO = 0.32;

float phase(float s, float d) {
  return clamp((progress - s) / d, 0.0, 1.0);
}

float radius(float t, float R) {
  float u = 1.0 - t;
  return mix(-SW - AA, R, mix(t, 1.0 - u * u * u, 0.8));
}

vec2 mirror(vec2 v) {
  return 1.0 - abs(1.0 - mod(abs(v), 2.0));
}

// a layer of color c and radius r laid over p (premultiplied), with its shadow around it, at distance x
vec4 layer(vec4 p, vec4 c, float r, float x) {
  float k = smoothstep(r - AA, r + AA, x);
  float s = SO * c.a * k * (1.0 - smoothstep(0.0, SW, x - r));
  p = p * (1.0 - s) + vec4(0.0, 0.0, 0.0, s);
  k = c.a * (1.0 - k);
  return p * (1.0 - k) + vec4(c.rgb, 1.0) * k;
}

vec4 transition(vec2 uv) {
  vec2 asp = vec2(ratio, 1.0);
  float x = length((uv - center) * asp);
  // past the farthest corner, with room for the shadows
  float R = length(max(center, 1.0 - center) * asp) + SW + AA;
  float k = clamp(stagger, 0.0, 1.0);
  float d = 1.0 / (1.0 + 3.0 * k);

  float t = phase(0.0, d);
  vec4 a = getFromColor(mirror(center + (uv - center) / (1.0 + 0.06 * t * (2.0 - t))));
  vec4 p = vec4(a.rgb * a.a, a.a);
  p = layer(p, color1, radius(t, R), x);
  p = layer(p, color2, radius(phase(k * d, d), R), x);
  float r = radius(phase(2.0 * k * d, d), R);
  p = layer(p, color3, r, x);

  t = phase(3.0 * k * d, d);
  float h = radius(t, R);
  float u = 1.0 - t;
  vec4 b = getToColor(mirror(center + (uv - center) / (1.0 + 0.14 * u * u * (1.0 - 2.4 * t))));
  float s = SO * color3.a * clamp((r - h) / SW, 0.0, 1.0) * (1.0 - smoothstep(0.0, SW, h - x));
  vec4 q = vec4(b.rgb * b.a * (1.0 - s), b.a * (1.0 - s) + s);
  p = mix(p, q, 1.0 - smoothstep(h - AA, h + AA, x));
  return p.a > 0.0 ? vec4(p.rgb / p.a, p.a) : vec4(0.0);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
