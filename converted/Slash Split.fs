/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "A bright blade stroke slashes diagonally across the outgoing image with a flash, then the two cut halves slip along the cut in opposite directions and fly off the frame, casting soft shadows on the incoming image beneath (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "angle",
            "NAME": "angle",
            "TYPE": "float",
            "DEFAULT": 25,
            "MIN": -75,
            "MAX": 75
        },
        {
            "LABEL": "separation",
            "NAME": "separation",
            "TYPE": "float",
            "DEFAULT": 0.06,
            "MIN": 0,
            "MAX": 0.3
        },
        {
            "LABEL": "flash",
            "NAME": "flash",
            "TYPE": "color",
            "DEFAULT": [1, 0.96, 0.88, 1],
            "MIN": [0, 0, 0, 0],
            "MAX": [1, 1, 1, 1]
        },
        {
            "LABEL": "shadow",
            "NAME": "shadow",
            "TYPE": "color",
            "DEFAULT": [0, 0, 0, 0.45],
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

// part of the transition during which the blade crosses the frame
const float CUT = 0.18;
const float EDGE = 0.0015;
const vec2 SHADOW_OFFSET = vec2(0.015, -0.022);
const float SHADOW_BLUR = 0.035;

// signed distance (aspect units) to the frame rectangle centered on 0, negative inside
float boxDistance (vec2 q) {
  vec2 d = abs(q) - 0.5 * vec2(ratio, 1.0);
  return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
}

// one half of the outgoing image, moved by v; s = 1 for the half above the cut, -1 below
// returns the image point of screen point q (xy) and a signed distance to the half (z, negative inside);
// the half overhangs its edges by half an edge width so that the still image covers the frame exactly
vec3 piece (vec2 q, vec2 v, vec2 n, float s) {
  vec2 y = q - v;
  return vec3(y, max(boxDistance(y), -s * dot(y, n)) - 0.5 * EDGE);
}

vec4 transition (vec2 uv) {
  float p = progress;
  float a = radians(angle);
  vec2 T = vec2(cos(a), sin(a));
  vec2 N = vec2(-T.y, T.x);
  vec2 q = (uv - 0.5) * vec2(ratio, 1.0);
  float d = dot(q, N);
  float along = dot(q, T);

  float t = clamp((p - 0.8 * CUT) / (1.0 - 0.8 * CUT), 0.0, 1.0);
  float open = separation * (1.0 - pow(1.0 - t, 3.0));
  float margin = length(SHADOW_OFFSET) + SHADOW_BLUR + 2.0 * EDGE;
  float exitX = (ratio + separation + margin) / max(abs(T.x), 1e-3);
  float exitY = (1.0 + separation + margin) / max(abs(T.y), 1e-3);
  float slide = min(exitX, exitY) * t * t * (1.5 - 0.5 * t);
  vec2 vUp = T * slide + N * open;

  vec4 to = getToColor(uv);
  vec2 sq = q - SHADOW_OFFSET;
  float sdUp = piece(sq, vUp, N, 1.0).z;
  float sdDown = piece(sq, -vUp, N, -1.0).z;
  float sh = shadow.a * smoothstep(0.0, 0.1, t) * (1.0 - smoothstep(-0.5 * SHADOW_BLUR, SHADOW_BLUR, min(sdUp, sdDown)));
  vec4 c = vec4(mix(to.rgb, shadow.rgb, sh), to.a);

  float reach = 0.5 * length(vec2(ratio, 1.0)) + 0.1;
  float k = clamp(p / CUT, 0.0, 1.0);
  float head = mix(-reach, reach, 1.0 - (1.0 - k) * (1.0 - k));
  float behind = head - along;
  float drawn = clamp(0.5 + behind / EDGE, 0.0, 1.0);

  float rim = flash.a * smoothstep(0.0, CUT, p) * (1.0 - 0.6 * t) * drawn;
  for (int i = 0; i < 2; i++) {
    float s = i == 0 ? 1.0 : -1.0;
    vec3 h = piece(q, s * vUp, N, s);
    float cover = clamp(0.5 - h.z / EDGE, 0.0, 1.0);
    vec4 col = getFromColor(clamp(h.xy / vec2(ratio, 1.0) + 0.5, 0.0, 1.0));
    float e = max(s * dot(h.xy, N), 0.0);
    col.rgb = mix(col.rgb, flash.rgb, rim * exp(-e / 0.004) * col.a);
    c = mix(c, col, cover);
  }

  float tail = drawn * (0.35 + 0.65 * exp(-max(behind, 0.0) * 4.0));
  float env = smoothstep(0.0, 0.03, p) * (1.0 - smoothstep(CUT, CUT + 0.12, p));
  float core = exp(-abs(d) / 0.0025) + 0.35 * exp(-abs(d) / 0.02);
  float spark = exp(-length(vec2(behind, d)) / 0.03) * (1.0 - k);
  float glow = flash.a * env * (core * tail + spark);
  float burst = flash.a * 0.25 * smoothstep(0.6 * CUT, CUT, p) * (1.0 - smoothstep(CUT, CUT + 0.15, p));
  c.rgb = mix(c.rgb, flash.rgb, burst * c.a);
  c.rgb += flash.rgb * glow * c.a;
  return c;
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
