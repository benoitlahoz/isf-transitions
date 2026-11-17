/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "Sergey Kosarevsky - MIT",
    "DESCRIPTION": "The image twists into a round swirl around the center that peaks mid-way, where the outgoing image dissolves into the incoming one, then untwists (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "strength",
            "NAME": "strength",
            "TYPE": "float",
            "DEFAULT": 1,
            "MIN": -3,
            "MAX": 3
        },
        {
            "LABEL": "radius",
            "NAME": "radius",
            "TYPE": "float",
            "DEFAULT": 0.9,
            "MIN": 0.1,
            "MAX": 2
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

// ( http://www.linderdaum.com )
// ported by gre from https://gist.github.com/corporateshark/cacfedb8cca0f5ce3f7c

// Center of the swirl

const float PI = 3.141592653589793;
const float TAU = 6.283185307179586;
const float ZOOM = 0.12;
// spread of the motion blur, as a fraction of the swirl's speed
const float BLUR = 0.05;
const int TAPS = 12;

// fold the coordinates back into the frame, so the image stays filled without smearing its border
vec2 mirrored(vec2 v) {
  vec2 m = mod(v, 2.0);
  return mix(m, 2.0 - m, step(1.0, m));
}

vec4 transition(vec2 uv) {
  // the swirl eases in and out, and is exactly 0 at both ends
  float s = sin(PI * progress);
  float amount = s * s;
  // its speed, used for the motion blur, also 0 at both ends
  float speed = PI * sin(TAU * progress);
  float reveal = smoothstep(0.3, 0.7, progress);

  vec2 aspect = vec2(ratio, 1.0);
  vec2 d = (uv - center) * aspect;
  float x = clamp(length(d) / max(radius, 1e-3), 0.0, 1.0);
  float falloff = (1.0 - x) * (1.0 - x);
  float twist = strength * TAU * falloff;
  float angle = twist * amount;
  // capped so the blur taps never spread apart into visible streaks
  float spread = clamp(twist * speed * BLUR, -1.2, 1.2);
  d *= 1.0 - ZOOM * amount * falloff;

  vec4 col = vec4(0.0);
  float total = 0.0;
  for (int i = 0; i < TAPS; i++) {
    float k = float(i) / float(TAPS - 1) - 0.5;
    float w = 1.0 - abs(k);
    float a = angle + spread * k;
    float c = cos(a);
    float sn = sin(a);
    vec2 q = mirrored(center + vec2(c * d.x - sn * d.y, sn * d.x + c * d.y) / aspect);
    col += mix(getFromColor(q), getToColor(q), reveal) * w;
    total += w;
  }
  return col / total;
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
