/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "Curved eyelids close from the top and bottom over the outgoing image, which blurs and dims, then open on the incoming image as it comes back into focus, as if the viewer blinked (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "blur",
            "NAME": "blur",
            "TYPE": "float",
            "DEFAULT": 0.5,
            "MIN": 0,
            "MAX": 1
        },
        {
            "LABEL": "softness",
            "NAME": "softness",
            "TYPE": "float",
            "DEFAULT": 0.12,
            "MIN": 0.01,
            "MAX": 0.3
        },
        {
            "LABEL": "blinks",
            "NAME": "blinks",
            "TYPE": "long",
            "DEFAULT": 1,
            "VALUES": [1, 2],
            "LABELS": ["1", "2"]
        },
        {
            "LABEL": "color",
            "NAME": "color",
            "TYPE": "color",
            "DEFAULT": [0.03, 0.02, 0.02, 1],
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

const int TAPS = 16;
// how much the eyelids curve: the opening is widest at the center and the corners close first
const float CURVE = 0.25;
// where the eyelids meet, below the middle: the upper lid travels further than the lower one
const float MEET = 0.44;

vec2 mirror(vec2 v) {
  return 1.0 - abs(1.0 - mod(v, 2.0));
}

// a blink fully closed at c (held for 2 * hold): the lids close over wc before it
// and open over wo after it, slower, like a real blink
float lidBlink(float t, float c, float wc, float wo, float hold) {
  return 1.0 - smoothstep(hold, t < c ? wc : wo, abs(t - c));
}

vec4 image(vec2 uv, bool incoming) {
  vec4 c = incoming ? getToColor(uv) : getFromColor(uv);
  return vec4(c.rgb * c.a, c.a);
}

vec4 transition(vec2 uv) {
  bool two = blinks > 1;
  float cut = two ? 0.3 : 0.42;
  float closed = two
    ? max(lidBlink(progress, cut, 0.26, 0.26, 0.015), lidBlink(progress, 0.7, 0.14, 0.24, 0.01))
    : lidBlink(progress, cut, 0.36, 0.5, 0.02);
  bool incoming = progress >= cut;

  // the rim of each eyelid is an arc; fully open, even its corners clear the frame by the widest shadow
  float soft = clamp(softness, 0.001, 0.3);
  float x = 2.0 * uv.x - 1.0;
  float rim = (1.0 - closed) * (1.31 + CURVE) - CURVE * x * x;
  float y = uv.y - MEET;
  float d = rim - max(y / (1.0 - MEET), -y / MEET);
  float s = 1.0 - smoothstep(0.0, soft, d);
  float lid = s * s;
  if (lid >= 1.0) return color;

  float f = progress < cut ? progress / cut : (1.0 - progress) / (1.0 - cut);
  f = clamp(f, 0.0, 1.0);
  f = max(f * (2.0 - f), closed) * blur;

  vec4 c;
  if (f <= 0.0) {
    c = incoming ? getToColor(uv) : getFromColor(uv);
  } else {
    float r = 0.025 * f;
    c = vec4(0.0);
    for (int i = 0; i < TAPS; i++) {
      float k = float(i) + 0.5;
      float a = 2.39996 * k;
      vec2 o = sqrt(k / float(TAPS)) * vec2(cos(a), sin(a)) * r;
      c += image(mirror(uv + o * vec2(1.0 / ratio, 1.0)), incoming);
    }
    c /= float(TAPS);
    c = c.a > 0.0 ? vec4(c.rgb / c.a, c.a) : vec4(0.0);
    c.rgb *= 1.0 - 0.35 * f;
  }
  return mix(c, color, lid);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
