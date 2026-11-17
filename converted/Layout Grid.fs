/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "The frame splits into an asymmetric magazine grid as paper gutters open between the panels; panel after panel in reading order, the incoming image pushes the outgoing one out of its panel from a different side, a colored accent band between them, then the gutters close over the full incoming image (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "gutter",
            "NAME": "gutter",
            "TYPE": "float",
            "DEFAULT": 0.04,
            "MIN": 0,
            "MAX": 0.08
        },
        {
            "LABEL": "overlap",
            "NAME": "overlap",
            "TYPE": "float",
            "DEFAULT": 0.55,
            "MIN": 0,
            "MAX": 0.9
        },
        {
            "LABEL": "band",
            "NAME": "band",
            "TYPE": "float",
            "DEFAULT": 0.22,
            "MIN": 0,
            "MAX": 0.6
        },
        {
            "LABEL": "shadow",
            "NAME": "shadow",
            "TYPE": "float",
            "DEFAULT": 0.4,
            "MIN": 0,
            "MAX": 1
        },
        {
            "LABEL": "paper",
            "NAME": "paper",
            "TYPE": "color",
            "DEFAULT": [0.96, 0.95, 0.91, 1],
            "MIN": [0, 0, 0, 0],
            "MAX": [1, 1, 1, 1]
        },
        {
            "LABEL": "accent",
            "NAME": "accent",
            "TYPE": "color",
            "DEFAULT": [0.93, 0.27, 0.2, 1],
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

const int PANELS = 6;
// the gutters open over [0, OPEN] and close over [1 - OPEN, 1]
const float OPEN = 0.28;
// the first push starts at START, the last one ends at 1 - START
const float START = 0.1;
const float EDGE = 0.0025;

// panel i of the layout: xy its bottom-left corner, zw its top-right corner, in uv; panels in reading order
vec4 panelRect(int i, bool portrait) {
  if (portrait) {
    if (i == 0) return vec4(0.0, 0.68, 0.62, 1.0);
    if (i == 1) return vec4(0.62, 0.68, 1.0, 1.0);
    if (i == 2) return vec4(0.0, 0.36, 1.0, 0.68);
    if (i == 3) return vec4(0.0, 0.0, 0.46, 0.36);
    if (i == 4) return vec4(0.46, 0.17, 1.0, 0.36);
    return vec4(0.46, 0.0, 1.0, 0.17);
  }
  if (i == 0) return vec4(0.0, 0.42, 0.4, 1.0);
  if (i == 1) return vec4(0.4, 0.64, 1.0, 1.0);
  if (i == 2) return vec4(0.4, 0.0, 0.7, 0.64);
  if (i == 3) return vec4(0.7, 0.32, 1.0, 0.64);
  if (i == 4) return vec4(0.7, 0.0, 1.0, 0.32);
  return vec4(0.0, 0.0, 0.4, 0.42);
}

// direction the outgoing image of panel i is pushed toward (the incoming one enters from the opposite side)
vec2 panelDir(int i, bool portrait) {
  if (portrait) {
    if (i == 0) return vec2(1.0, 0.0);
    if (i == 1) return vec2(0.0, -1.0);
    if (i == 2) return vec2(-1.0, 0.0);
    if (i == 3) return vec2(0.0, 1.0);
    if (i == 4) return vec2(1.0, 0.0);
    return vec2(-1.0, 0.0);
  }
  if (i == 0) return vec2(0.0, -1.0);
  if (i == 1) return vec2(-1.0, 0.0);
  if (i == 2) return vec2(0.0, 1.0);
  if (i == 3) return vec2(1.0, 0.0);
  if (i == 4) return vec2(-1.0, 0.0);
  return vec2(1.0, 0.0);
}

float easeInOut(float t) {
  return t < 0.5 ? 4.0 * t * t * t : 1.0 - 4.0 * (1.0 - t) * (1.0 - t) * (1.0 - t);
}

// coverage of the interval [lo, hi] at x, with antialiased ends: the coverages of adjacent intervals add up to 1
float span1(float x, float lo, float hi, float aa) {
  return smoothstep(lo - aa, lo + aa, x) * (1.0 - smoothstep(hi - aa, hi + aa, x));
}

float boxDist(vec2 p, vec2 lo, vec2 hi) {
  vec2 d = abs(p - 0.5 * (lo + hi)) - 0.5 * (hi - lo);
  return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
}

vec4 transition(vec2 uv) {
  bool portrait = ratio < 1.0;
  vec2 aspect = vec2(ratio, 1.0) / min(ratio, 1.0);
  vec2 p = uv * aspect;
  float open = smoothstep(0.0, OPEN, progress) * (1.0 - smoothstep(1.0 - OPEN, 1.0, progress));
  float g = gutter * open;
  float ov = clamp(overlap, 0.0, 0.9);
  float span = (1.0 - 2.0 * START) / (1.0 + float(PANELS - 1) * (1.0 - ov));

  vec4 color = vec4(0.0);
  float cover = 0.0;
  float shade = 0.0;
  for (int i = 0; i < PANELS; i++) {
    vec4 r = panelRect(i, portrait);
    vec2 lo = r.xy * aspect + 0.5 * g;
    vec2 hi = r.zw * aspect - 0.5 * g;
    // no gutter along the frame's border: the panels there run past it
    lo = mix(lo, vec2(-1.0), step(r.xy, vec2(0.0)));
    hi = mix(hi, aspect + 1.0, step(vec2(1.0), r.zw));
    float t = clamp((progress - START - float(i) * span * (1.0 - ov)) / span, 0.0, 1.0);
    float lift = 1.0 + 1.2 * sin(3.141592653589793 * t);
    float sd = boxDist(p - vec2(0.2, -0.35) * g * lift, lo, hi);
    shade = max(shade, (1.0 - smoothstep(-0.2 * g, 0.6 * g * lift + 1e-4, sd)) / sqrt(lift));
    // separable coverage, so that the panels of the grid add up to exactly 1 once the gutters close
    float inside = span1(p.x, lo.x, hi.x, EDGE) * span1(p.y, lo.y, hi.y, EDGE);
    if (inside > 0.0) {
      vec2 dir = panelDir(i, portrait);
      vec4 c;
      if (t <= 0.0) {
        c = getFromColor(uv);
      } else if (t >= 1.0) {
        c = getToColor(uv);
      } else {
        float e = easeInOut(t);
        // a train slides along dir through the panel: outgoing image, accent band, incoming image
        float len = abs(dot(r.zw - r.xy, dir));
        float w = band * len;
        vec2 q = uv - dir * e * (len + w);
        // position along dir from the outgoing image's trailing edge (inside it when >= 0)
        float a = dot(q - (dir.x + dir.y < 0.0 ? r.zw : r.xy), dir);
        float af = a * abs(dot(aspect, dir));
        float wf = w * abs(dot(aspect, dir));
        float fromW = smoothstep(-EDGE, 0.0, af);
        float toW = 1.0 - smoothstep(-wf, -wf + EDGE, af);
        float bandShade = 1.0 - shadow * 0.5 * (1.0 - smoothstep(-wf, -wf + 0.35 * wf + 1e-4, af));
        vec4 b = vec4(accent.rgb * bandShade, accent.a);
        c = mix(b, getFromColor(clamp(q, r.xy, r.zw)), fromW);
        c = mix(c, getToColor(clamp(q + dir * (len + w), r.xy, r.zw)), toW);
      }
      color += c * inside;
      cover += inside;
    }
  }
  color /= max(cover, 1.0);
  cover = min(cover, 1.0);
  vec4 bg = vec4(paper.rgb * (1.0 - shadow * shade * open), paper.a);
  return color + bg * (1.0 - cover);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
