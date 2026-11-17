/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "A camera's focal-plane shutter fires across the frame, seen through the camera's gate: thin metal blades fan down from a tight stack above it until they overlap over the whole frame, then keep running down and stack below it, uncovering the incoming image from the top (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "blades",
            "NAME": "blades",
            "TYPE": "long",
            "DEFAULT": 5,
            "VALUES": [3, 4, 5, 6, 7, 8, 9, 10],
            "LABELS": ["3", "4", "5", "6", "7", "8", "9", "10"]
        },
        {
            "LABEL": "tilt",
            "NAME": "tilt",
            "TYPE": "float",
            "DEFAULT": 0.06,
            "MIN": 0,
            "MAX": 0.2
        },
        {
            "LABEL": "gate",
            "NAME": "gate",
            "TYPE": "float",
            "DEFAULT": 0.035,
            "MIN": 0,
            "MAX": 0.08
        },
        {
            "LABEL": "color",
            "NAME": "color",
            "TYPE": "color",
            "DEFAULT": [0.09, 0.09, 0.1, 1],
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

// spacing of the blades in their stacks, and how far the stacks rest outside the frame, in frame heights
const float STACK = 0.016;
const float REST = 0.05;

vec4 transition(vec2 uv) {
  float n = clamp(float(blades), 3.0, 10.0);
  // tall enough to still overlap, past their blurred edges, when they are spread the most
  float h = 1.7 / n + 0.02;
  float pile = (n - 1.0) * STACK;
  // the bottoms of the blades run from a stack above the frame to a stack below it: the
  // lowest blade leads, the highest one trails
  float travel = 1.0 + 2.0 * REST + h + pile;
  vec2 t = clamp((progress - vec2(0.0, 0.3)) / 0.7, 0.0, 1.0);
  vec2 pos = 1.0 + REST + vec2(0.0, pile) - travel * t * t * (3.0 - 2.0 * t);
  vec2 vel = travel * 6.0 * t * (1.0 - t) / 0.7;

  float x = (uv.x - 0.5) * ratio;
  vec4 col = progress < 0.5 ? getFromColor(uv) : getToColor(uv);
  // painted from the lowest blade to the highest, each one over the top of the one below it
  for (int i = 0; i < 10; i++) {
    float k = float(i) / (n - 1.0);
    if (k < 1.0001) {
      float v = mix(vel.x, vel.y, k);
      float d = uv.y - mix(pos.x, pos.y, k) + tilt * 0.1 * v * x;
      float aa = 0.002 + 0.005 * v;
      float sh = (1.0 - smoothstep(0.0, 0.03 + 0.012 * v, -d)) * (1.0 - smoothstep(-aa, aa, d));
      col.rgb *= 1.0 - 0.65 * color.a * sh * sh;
      float l = clamp(d / h, 0.0, 1.0);
      float lip = smoothstep(0.003, 0.007, d) * (1.0 - smoothstep(0.007, 0.014, d));
      float s = (uv.y - d - 0.55 + 0.3 * x) * 2.5;
      float sheen = 0.3 * exp(-s * s) * (1.0 - l) * (1.0 - l);
      vec3 metal = color.rgb * (1.3 - 0.75 * l) + (0.25 * lip + sheen) * (0.6 + 0.4 * color.rgb) * color.a;
      metal *= 0.4 + 0.6 * smoothstep(0.0, 0.005, d);
      float cover = smoothstep(-aa, aa, d) * (1.0 - smoothstep(h - aa, h + aa, d));
      col = mix(col, vec4(clamp(metal, 0.0, 1.0), color.a), cover);
    }
  }
  float g = gate * smoothstep(0.0, 0.12, progress) * (1.0 - smoothstep(0.88, 1.0, progress));
  vec2 c = abs(uv - 0.5) * vec2(ratio, 1.0) - vec2(0.5 * ratio, 0.5) + 1.5 * g;
  float e = length(max(c, 0.0)) + min(max(c.x, c.y), 0.0) - 0.5 * g;
  float on = smoothstep(0.0, 0.002, gate) * smoothstep(0.0, 0.04, progress) * (1.0 - smoothstep(0.96, 1.0, progress));
  col.rgb *= 1.0 - 0.5 * color.a * on * smoothstep(-0.4 * g - 0.001, 0.0, e);
  vec3 housing = min(color.rgb * (1.5 - smoothstep(0.0, 0.2 * g + 0.001, e)), 1.0);
  return mix(col, vec4(housing, color.a), on * smoothstep(-0.002, 0.002, e));
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
