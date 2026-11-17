/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "The outgoing image is printed in two inks, shadows first, as a bold high-contrast duotone; the incoming image is printed over it in the same two inks, its dark tones landing before its highlights, then its own colors come back, highlights first (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "shadowInk",
            "NAME": "shadowInk",
            "TYPE": "color",
            "DEFAULT": [0.11, 0.09, 0.46, 1],
            "MIN": [0, 0, 0, 0],
            "MAX": [1, 1, 1, 1]
        },
        {
            "LABEL": "highlightInk",
            "NAME": "highlightInk",
            "TYPE": "color",
            "DEFAULT": [1, 0.45, 0.62, 1],
            "MIN": [0, 0, 0, 0],
            "MAX": [1, 1, 1, 1]
        },
        {
            "LABEL": "contrast",
            "NAME": "contrast",
            "TYPE": "float",
            "DEFAULT": 0.6,
            "MIN": 0,
            "MAX": 1
        },
        {
            "LABEL": "grain",
            "NAME": "grain",
            "TYPE": "float",
            "DEFAULT": 0.4,
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

float dfHash(vec2 v) {
  vec3 v3 = fract(vec3(v.xyx) * 0.1031);
  v3 += dot(v3, v3.yzx + 33.33);
  return fract((v3.x + v3.y) * v3.z);
}

float dfLuma(vec4 c) {
  return clamp(dot(c.rgb, vec3(0.299, 0.587, 0.114)), 0.0, 1.0);
}

vec4 transition(vec2 uv) {
  vec4 a = getFromColor(uv);
  vec4 b = getToColor(uv);
  float la = dfLuma(a);
  float lb = dfLuma(b);
  // fixed print grain, in cells of about 1/360 of the shorter side
  vec2 q = uv * 360.0 * vec2(max(ratio, 1.0), max(1.0 / ratio, 1.0));
  float g = dfHash(floor(q)) - 0.5;

  float inks = smoothstep(0.0, 0.35, progress);
  float lift = smoothstep(0.65, 1.0, progress);
  float amount = min(
    clamp(inks * 1.35 - 0.35 * la, 0.0, 1.0),
    clamp((1.0 - lift) * 1.35 - 0.35 * lb, 0.0, 1.0)
  );
  amount = amount * amount * (3.0 - 2.0 * amount);

  float x = smoothstep(0.25, 0.75, progress) * 1.6 - 0.3;
  float t = lb + 0.2 * grain * g;
  float s = smoothstep(t - 0.15, t + 0.15, x);

  float c = 0.4 * contrast;
  float tone = smoothstep(c + 0.03, 1.03 - c, mix(la, lb, s) + 0.08 * grain * g);
  vec4 ink = mix(shadowInk, highlightInk, tone);

  // the images are blended with premultiplied alpha, and where neither is drawn nothing is printed
  vec4 base = mix(vec4(a.rgb * a.a, a.a), vec4(b.rgb * b.a, b.a), s);
  if (base.a <= 0.0) return vec4(0.0);
  return vec4(mix(base.rgb / base.a, ink.rgb, amount * ink.a), base.a);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
