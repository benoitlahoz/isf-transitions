/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "The outgoing image rolls faster and faster, like an old TV losing its vertical hold, then the roll slows down and locks on the incoming image (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "rolls",
            "NAME": "rolls",
            "TYPE": "long",
            "DEFAULT": 3,
            "VALUES": [1, 2, 3, 4, 5, 6, 7, 8],
            "LABELS": ["1", "2", "3", "4", "5", "6", "7", "8"]
        },
        {
            "LABEL": "barHeight",
            "NAME": "barHeight",
            "TYPE": "float",
            "DEFAULT": 0.12,
            "MIN": 0,
            "MAX": 0.5
        },
        {
            "LABEL": "barColor",
            "NAME": "barColor",
            "TYPE": "color",
            "DEFAULT": [0, 0, 0, 1],
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

// The rolling picture at a vertical position y, in frames of height 1 + barHeight
vec4 picture(vec2 uv, float y, float m) {
  float period = 1.0 + barHeight;
  y = mod(y, period);
  if (y > 1.0) return barColor;
  vec2 q = vec2(uv.x, y);
  return mix(getFromColor(q), getToColor(q), m);
}

vec4 transition(vec2 uv) {
  float period = 1.0 + barHeight;
  // smooth start and stop: the picture locks again after a whole number of frames
  float s = progress * progress * (3.0 - 2.0 * progress);
  float offset = period * fract(float(rolls) * s);
  float speed = 4.0 * progress * (1.0 - progress); // 0 at both ends, 1 in the middle
  float m = smoothstep(0.42, 0.58, progress); // the channel changes at full speed
  vec2 p = uv + vec2(0.004 * speed * sin(uv.y * 9.0 + progress * 30.0), 0.0);
  vec4 c = vec4(0.0);
  for (int i = 0; i < 5; i++) {
    c += picture(p, p.y + offset - 0.0015 * float(rolls) * speed * float(i), m);
  }
  c /= 5.0;
  float scan = 0.5 + 0.5 * sin(uv.y * 400.0);
  float flicker = 0.08 * (0.5 + 0.5 * sin(progress * 97.0));
  c.rgb *= 1.0 - speed * (0.08 * scan + flicker);
  return c;
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
