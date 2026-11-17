/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "Solid color panels, slightly staggered, sweep across and cover the frame, then sweep off the far side and reveal the incoming image (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "DEFAULT": [1, 0],
            "MIN": [-4, -4],
            "MAX": [4, 4]
        },
        {
            "LABEL": "panels",
            "NAME": "panels",
            "TYPE": "long",
            "DEFAULT": 3,
            "VALUES": [1, 2, 3],
            "LABELS": ["1", "2", "3"]
        },
        {
            "LABEL": "color1",
            "NAME": "color1",
            "TYPE": "color",
            "DEFAULT": [0.98, 0.76, 0.22, 1],
            "MIN": [0, 0, 0, 0],
            "MAX": [1, 1, 1, 1]
        },
        {
            "LABEL": "color2",
            "NAME": "color2",
            "TYPE": "color",
            "DEFAULT": [0.93, 0.33, 0.36, 1],
            "MIN": [0, 0, 0, 0],
            "MAX": [1, 1, 1, 1]
        },
        {
            "LABEL": "color3",
            "NAME": "color3",
            "TYPE": "color",
            "DEFAULT": [0.16, 0.2, 0.42, 1],
            "MIN": [0, 0, 0, 0],
            "MAX": [1, 1, 1, 1]
        },
        {
            "LABEL": "stagger",
            "NAME": "stagger",
            "TYPE": "float",
            "DEFAULT": 0.35,
            "MIN": 0,
            "MAX": 1
        },
        {
            "LABEL": "skew",
            "NAME": "skew",
            "TYPE": "float",
            "DEFAULT": 0.25,
            "MIN": -1,
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

// the frame is fully covered from cut - hold to cut + hold, where the image switches
const float cut = 0.46;
const float hold = 0.01;
const float aa = 0.0015;

float ease(float t) {
  float s = clamp(t, 0.0, 1.0);
  float u = 1.0 - s * s * (3.0 - 2.0 * s);
  return 1.0 - u * u * u;
}

float cover(float x, float back, float front) {
  return smoothstep(back - aa, back + aa, x) * (1.0 - smoothstep(front - aa, front + aa, x));
}

vec4 transition(vec2 uv) {
  vec2 dir = length(direction) > 0.0 ? normalize(direction) : vec2(1.0, 0.0);
  vec2 p = (uv - 0.5) * vec2(ratio, 1.0);
  float k = clamp(skew, -1.0, 1.0);
  vec2 a = (dir + k * vec2(-dir.y, dir.x)) / sqrt(1.0 + k * k);
  float x = dot(p, a);
  float e = 0.5 * (abs(a.x) * ratio + abs(a.y));
  float start = -e - 2.0 * aa;
  float span = 2.0 * (e + 2.0 * aa);

  int n = panels < 1 ? 1 : panels > 3 ? 3 : panels;
  float m = float(n - 1);
  float st = clamp(stagger, 0.0, 1.0);
  vec2 d = vec2(cut - hold, 1.0 - cut - hold) / (1.0 + m * st);

  // the first panel enters first and leaves last; both images are sampled so no texture read sits in a divergent branch
  float front0 = start + span * ease(progress / d.x);
  float back0 = start + span * ease((progress - 1.0 + d.y) / d.y);
  vec4 c = mix(getToColor(uv), getFromColor(uv), step(0.5 * (front0 + back0), x));
  c = mix(c, color1, cover(x, back0, front0));

  for (int i = 1; i < 3; i++) {
    if (i < n) {
      vec2 t = float(i) * st * d;
      float front = start + span * ease((progress - t.x) / d.x);
      float back = start + span * ease((progress - 1.0 + d.y + t.y) / d.y);
      c = mix(c, i == 1 ? color2 : color3, cover(x, back, front));
    }
  }
  return c.a > 0.0 ? c : vec4(0.0);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
