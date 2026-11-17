/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "Shimmering heat waves rise through the frame, strongest at the bottom, making the outgoing image wobble like the air over hot asphalt; the incoming image shows through the haze and settles as it calms (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "DEFAULT": 0.04,
            "MIN": 0,
            "MAX": 0.1
        },
        {
            "LABEL": "scale",
            "NAME": "scale",
            "TYPE": "float",
            "DEFAULT": 6,
            "MIN": 1,
            "MAX": 12
        },
        {
            "LABEL": "speed",
            "NAME": "speed",
            "TYPE": "float",
            "DEFAULT": 2,
            "MIN": 0,
            "MAX": 6
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

vec2 mirror(vec2 v) {
  return 1.0 - abs(1.0 - mod(v, 2.0));
}

float hash12(vec2 v) {
  vec3 v3 = fract(vec3(v.xyx) * 0.1031);
  v3 += dot(v3, v3.yzx + 33.33);
  return fract((v3.x + v3.y) * v3.z);
}

float vnoise(vec2 v) {
  vec2 i = floor(v);
  vec2 f = fract(v);
  f = f * f * (3.0 - 2.0 * f);
  return mix(
    mix(hash12(i), hash12(i + vec2(1.0, 0.0)), f.x),
    mix(hash12(i + vec2(0.0, 1.0)), hash12(i + vec2(1.0, 1.0)), f.x),
    f.y
  );
}

// centered on 0, within [-1.5, 1.5]
float fbm(vec2 v) {
  float sum = 0.0;
  float amp = 0.5;
  for (int i = 0; i < 4; i++) {
    sum += amp * vnoise(v);
    v = mat2(1.6, 1.2, -1.2, 1.6) * v + 5.3;
    amp *= 0.5;
  }
  return (sum / 0.9375 - 0.5) * 3.0;
}

vec4 transition(vec2 uv) {
  const float PI = 3.141592653589793;
  float p = progress;
  float amount = sin(PI * p);
  float t = p * speed;
  vec2 q = vec2(uv.x * ratio, uv.y * 2.5) * scale;
  q.y -= t * scale;
  vec2 w = vec2(fbm(q * 0.35 + vec2(0.0, t)), fbm(q * 0.35 + vec2(7.1, -t)));
  q += 1.5 * w;
  vec2 offset = vec2(fbm(q), 0.5 * fbm(q + vec2(3.7, 1.9)));
  float ground = mix(1.0, 0.3, uv.y);
  offset *= strength * amount * ground * vec2(1.0 / ratio, 1.0);

  vec2 d = mirror(uv + offset);
  float m = smoothstep(0.3, 0.7, p);
  if (m <= 0.0) return getFromColor(d);
  if (m >= 1.0) return getToColor(d);
  vec4 a = getFromColor(d);
  vec4 b = getToColor(d);
  // mixed premultiplied, so transparent images do not bleed their color
  vec4 c = mix(vec4(a.rgb * a.a, a.a), vec4(b.rgb * b.a, b.a), m);
  return c.a > 0.0 ? vec4(c.rgb / c.a, c.a) : vec4(0.0);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
