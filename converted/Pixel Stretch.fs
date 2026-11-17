/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "A line sweeps across the frame and drags the outgoing image's pixels behind it as long streaks, which shorten as it goes and reveal the incoming image (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "stretch",
            "NAME": "stretch",
            "TYPE": "float",
            "DEFAULT": 0.45,
            "MIN": 0,
            "MAX": 1
        },
        {
            "LABEL": "chroma",
            "NAME": "chroma",
            "TYPE": "float",
            "DEFAULT": 0.5,
            "MIN": 0,
            "MAX": 1
        },
        {
            "LABEL": "softness",
            "NAME": "softness",
            "TYPE": "float",
            "DEFAULT": 0.03,
            "MIN": 0,
            "MAX": 0.2
        },
        {
            "LABEL": "jitter",
            "NAME": "jitter",
            "TYPE": "float",
            "DEFAULT": 0.5,
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

float hash(float n) {
  n = fract(n * 0.1031);
  n *= n + 33.33;
  n *= n + n;
  return fract(n);
}

// smooth max with a compact support: exactly max(x, y) when they are k apart
float smax(float x, float y, float k) {
  float h = max(k - abs(x - y), 0.0) / k;
  return max(x, y) + h * h * k * 0.25;
}

// position s along the sweep (0 to 1 across the frame) to a frame coordinate
vec2 at(vec2 uv, float s0, float s, vec2 d, float ext) {
  return clamp(uv + d * (s - s0) * ext, 0.0, 1.0);
}

vec4 transition(vec2 uv) {
  vec2 d = normalize(direction + vec2(1e-6, 0.0));
  float ext = abs(d.x) + abs(d.y);
  float s = 0.5 + dot(uv - 0.5, d) / ext;
  // rows across the streaks (about one per pixel line), in frame heights
  vec2 n = vec2(-d.y, d.x);
  float t = dot((uv - 0.5) * vec2(ratio, 1.0), n);
  float row = floor(t * 540.0);
  float clump = floor(t * 60.0);
  float rnd = 0.6 * hash(row + 17.0) + 0.4 * hash(clump + 311.0);

  float p = progress;
  float k = softness + 1e-4;
  // the line starts and ends at rest just beyond the frame, past its soft edge, so both ends are exact;
  // it enters at once and slows down a little toward the end
  float margin = k + 0.002;
  float a = mix(-margin, 1.0 + margin, mix(p, p * p * (3.0 - 2.0 * p), 0.5));

  // the streaks are longest when the line crosses the middle and vanish as it reaches the edges,
  // so they never repeat the frame's own border (often dark, or letterboxing)
  float h = clamp(4.0 * a * (1.0 - a), 0.0, 1.0);
  float len = stretch * h * (1.0 - 0.6 * jitter * rnd);
  float split = 0.02 * chroma * h;

  // ahead of the line the outgoing image is intact; behind it, each pixel repeats the pixel at the line
  vec4 fr = getFromColor(at(uv, s, smax(s, a + split, k), d, ext));
  vec4 fg = getFromColor(at(uv, s, smax(s, a, k), d, ext));
  vec4 fb = getFromColor(at(uv, s, smax(s, a - split, k), d, ext));
  vec4 streak = vec4(fr.r, fg.g, fb.b, fg.a);
  float behind = step(s, a);
  streak.rgb *= 1.0 + 0.06 * jitter * h * (hash(row * 3.0 + 5.0) - 0.5) * behind;

  float w = smoothstep(len - k, len + k, a - s);
  return mix(streak, getToColor(uv), w);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
