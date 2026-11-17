/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "Two hands sweep away from each other around the center, opening a widening wedge that reveals the incoming image (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "angle",
            "NAME": "angle",
            "TYPE": "float",
            "DEFAULT": 0,
            "MIN": -180,
            "MAX": 180
        },
        {
            "LABEL": "smoothness",
            "NAME": "smoothness",
            "TYPE": "float",
            "DEFAULT": 0.01,
            "MIN": 0,
            "MAX": 0.2
        },
        {
            "LABEL": "center",
            "NAME": "center",
            "TYPE": "point2D",
            "DEFAULT": [0.5, 0.5],
            "MIN": [-2, -2],
            "MAX": [2, 2]
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

#define PI 3.141592653589793


vec4 transition (vec2 uv) {
  // Measured in square pixels, so the angles are not stretched
  vec2 q = (uv - center) * vec2(ratio, 1.0);
  float a = radians(angle);
  // Local frame: y along the starting direction, x across it (the wedge is symmetric)
  vec2 p = vec2(abs(q.x * cos(a) - q.y * sin(a)), q.x * sin(a) + q.y * cos(a));
  float spread = PI * progress;
  // (float PI rounds above pi, so sin(spread) would turn slightly negative at the end)
  vec2 hand = vec2(max(sin(spread), 0.0), cos(spread));
  // Signed distance to the wedge: the nearest hand, negative inside
  float l = length(p);
  float d = length(p - hand * clamp(dot(p, hand), 0.0, l)) * sign(hand.y * p.x - hand.x * p.y);
  // The soft edge keeps an even width and slides from outside to inside the hands,
  // so nothing is revealed at the start and everything is at the end
  float s = max(smoothness, 0.001);
  float m = 1.0 - smoothstep(0.0, s, d + s * (1.0 - progress));
  return mix(getFromColor(uv), getToColor(uv), m);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
