/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "A plus-shaped cross grows from the center and widens its arms until the incoming image fills the frame (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "smoothness",
            "NAME": "smoothness",
            "TYPE": "float",
            "DEFAULT": 0.01,
            "MIN": 0,
            "MAX": 0.1
        },
        {
            "LABEL": "reverse",
            "NAME": "reverse",
            "TYPE": "bool",
            "DEFAULT": false
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

float boxDist (vec2 p, vec2 halfSize) {
  vec2 d = abs(p) - halfSize;
  return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
}

float plus (vec2 q, float t, float s) {
  vec2 frame = vec2(0.5 * ratio, 0.5);
  float k = min(frame.x, frame.y);
  // The arms start equally thick, then widen with the frame's proportions,
  // so the four corners left uncovered vanish together
  vec2 halfWidth = k * t + (frame - k) * t * t;
  float halfLength = 3.0 * max(frame.x, frame.y) * t;
  float d = min(boxDist(q, vec2(halfLength, halfWidth.y)), boxDist(q, vec2(halfWidth.x, halfLength)));
  // The soft edge slides from outside to inside the cross, so nothing shows at
  // t = 0 and everything at t = 1, whatever the softness
  return 1.0 - smoothstep(0.0, s, d + s * (1.0 - t));
}

vec4 transition (vec2 uv) {
  // Measured in square pixels, so the cross is not stretched
  vec2 q = (uv - 0.5) * vec2(ratio, 1.0);
  float s = max(smoothness, 0.001);
  float m = reverse ? 1.0 - plus(q, 1.0 - progress, s) : plus(q, progress, s);
  return mix(getFromColor(uv), getToColor(uv), m);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
