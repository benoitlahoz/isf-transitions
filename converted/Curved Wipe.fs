/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "The outgoing image is pulled away by its middle: the wipe edge bulges into a smooth curve that leads the way, then flattens as it reaches the far side (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "direction",
            "NAME": "direction",
            "TYPE": "point2D",
            "DEFAULT": [0, 1],
            "MIN": [-4, -4],
            "MAX": [4, 4]
        },
        {
            "LABEL": "curvature",
            "NAME": "curvature",
            "TYPE": "float",
            "DEFAULT": 0.6,
            "MIN": 0,
            "MAX": 1
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
            "LABEL": "shadow",
            "NAME": "shadow",
            "TYPE": "color",
            "DEFAULT": [0, 0, 0, 0.4],
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

// a gentle ease in and out, which does not linger at the ends
float ease (float t) {
  return 0.5 * (t + t * t * (3.0 - 2.0 * t));
}

vec4 transition (vec2 uv) {
  vec2 dir = length(direction) > 0.0 ? normalize(direction) : vec2(0.0, 1.0);
  vec2 p = (uv - 0.5) * vec2(ratio, 1.0);
  float e = 0.5 * (abs(dir.x) * ratio + abs(dir.y));
  float w = 0.5 * (abs(dir.y) * ratio + abs(dir.x));
  vec2 across = vec2(-dir.y, dir.x);
  float d = dot(p, dir);
  // where the pixel is across the frame, from -1 to 1 (on a diagonal, these are the corners); it is the same
  // all along the direction, so the edge is a single curve, centered on the middle of the frame
  float x = clamp(dot(p, across) / w, -1.0, 1.0);
  float s = max(smoothness, 0.001);
  // each point of the edge runs on its own clock: the middle runs ahead and slows down at the far side,
  // where the sides catch up with it; the lead keeps the curve's shape alike on wide and narrow frames
  float k = clamp(curvature, 0.0, 1.0) * min(1.0, w / e) * (1.0 - x * x);
  float t = progress + k * progress * (1.0 - progress);
  float travel = 2.0 * (e + s);
  float edge = -e - s + travel * ease(t);
  float side = -e - s + travel * ease(progress);
  float m = 1.0 - smoothstep(-s, s, d - edge);
  float lift = edge - side;
  float spread = 0.012 + 0.25 * lift;
  vec4 to = getToColor(uv);
  vec2 caster = clamp((p + (edge - d) * dir) / vec2(ratio, 1.0) + 0.5, 0.0, 1.0);
  float sh = shadow.a * sin(PI * progress) * exp(-max(edge - d, 0.0) / spread) * m * getFromColor(caster).a * to.a;
  to.rgb = mix(to.rgb, shadow.rgb, sh);
  return mix(getFromColor(uv), to, m);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
