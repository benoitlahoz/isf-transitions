/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "hjm1fb - MIT",
    "DESCRIPTION": "The incoming image spreads over the outgoing one with a hot, glowing edge (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "uLineWidth",
            "NAME": "uLineWidth",
            "TYPE": "float",
            "DEFAULT": 0.1,
            "MIN": 0,
            "MAX": 1
        },
        {
            "LABEL": "uSpreadClr",
            "NAME": "uSpreadClr_isf",
            "TYPE": "color",
            "DEFAULT": [1, 0, 0, 1],
            "MIN": [0, 0, 0, 0],
            "MAX": [1, 1, 1, 1]
        },
        {
            "LABEL": "uHotClr",
            "NAME": "uHotClr_isf",
            "TYPE": "color",
            "DEFAULT": [0.9, 0.9, 0.2, 1],
            "MIN": [0, 0, 0, 0],
            "MAX": [1, 1, 1, 1]
        },
        {
            "LABEL": "uPow",
            "NAME": "uPow",
            "TYPE": "float",
            "DEFAULT": 5,
            "MIN": 0,
            "MAX": 20
        },
        {
            "LABEL": "uIntensity",
            "NAME": "uIntensity",
            "TYPE": "float",
            "DEFAULT": 1,
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

#define uSpreadClr uSpreadClr_isf.rgb
#define uHotClr uHotClr_isf.rgb

vec2 hash(vec2 p)
{
  p = vec2(dot(p, vec2(127.1, 311.7)), dot(p, vec2(269.5, 183.3)));
  return -1.0 + 2.0 * fract(sin(p) * 43758.5453123);
}

float noise(in vec2 p) {
  const float K1 = 0.366025404;  // (sqrt(3)-1)/2;
  const float K2 = 0.211324865;  // (3-sqrt(3))/6;

  vec2 i = floor(p + (p.x + p.y) * K1);
  vec2 a = p - i + (i.x + i.y) * K2;
  float m = step(a.y, a.x);
  vec2 o = vec2(m, 1.0 - m);
  vec2 b = a - o + K2;
  vec2 c = a - 1.0 + 2.0 * K2;
  vec3 h = max(0.5 - vec3(dot(a, a), dot(b, b), dot(c, c)), 0.0);
  vec3 n = h * h * h * h * vec3(dot(a, hash(i + 0.0)), dot(b, hash(i + o)), dot(c, hash(i + 1.0)));
  return dot(n, vec3(70.0));
}

vec4 transition(vec2 uv) {
  vec4 from = getFromColor(uv);
  vec4 to = getToColor(uv);
  vec4 outColor;
  float burn;
  burn = 0.5 + 0.5 * (0.299 * from.r + 0.587 * from.g + 0.114 * from.b);

  float show = burn - progress;
  if (show < 0.001) {
    outColor = to;
  } else {
    float factor = 1.0 - smoothstep(0.0, uLineWidth, show);
    vec3 burnColor = mix(uSpreadClr, uHotClr, factor);
    burnColor = pow(burnColor, vec3(uPow)) * uIntensity;
    vec3 finalRGB = mix(from.rgb, burnColor, factor * step(0.0001, progress));
    outColor = vec4(finalRGB, from.a); // straight alpha, like the images
  }
  return outColor;
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
