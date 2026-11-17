/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "The camera tracks sideways past a dark, out-of-focus pillar standing close to the lens: the pillar rushes across the frame while the images far behind it drift slowly the same way with a slight push in, and the cut to the incoming image happens hidden behind it (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "DEFAULT": [-1, 0],
            "MIN": [-4, -4],
            "MAX": [4, 4]
        },
        {
            "LABEL": "parallax",
            "NAME": "parallax",
            "TYPE": "float",
            "DEFAULT": 0.1,
            "MIN": 0,
            "MAX": 0.25
        },
        {
            "LABEL": "width",
            "NAME": "width",
            "TYPE": "float",
            "DEFAULT": 0.45,
            "MIN": 0.1,
            "MAX": 1
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
            "LABEL": "pillar",
            "NAME": "pillar",
            "TYPE": "color",
            "DEFAULT": [0.2, 0.17, 0.15, 1],
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

// mirror the coordinates back into the frame
vec2 mirror(vec2 v) {
  return 1.0 - abs(1.0 - mod(v, 2.0));
}

vec4 transition(vec2 uv) {
  // the main axis of the direction, to the left when there is none
  vec2 d = abs(direction.x) >= abs(direction.y)
    ? vec2(direction.x > 0.0 ? 1.0 : -1.0, 0.0)
    : vec2(0.0, direction.y > 0.0 ? 1.0 : -1.0);
  // u: position along the motion in frame heights, from 0 on the side everything comes from to len
  float len = d.x != 0.0 ? ratio : 1.0;
  float u = (dot(uv - 0.5, d) + 0.5) * len;
  // v: across the motion, 0 to 1
  float v = d.x != 0.0 ? uv.y : uv.x;

  float p = progress;
  // the camera's travel: eased in and out, with a little linear motion so it never stands still
  float e = 0.7 * p * p * (3.0 - 2.0 * p) + 0.3 * p;
  float speed = 0.7 * 6.0 * p * (1.0 - p) + 0.3;

  // the images, far away, drift slowly: the outgoing one leaves the way it moves, the incoming one
  // comes from the opposite side, so each is exactly in place at its end of the transition. Each
  // is scaled up just enough that its drift never reaches past its edges
  float drift = clamp(parallax, 0.0, 0.25);
  float da = drift * e;
  float db = drift * (1.0 - e);
  vec4 a = getFromColor(mirror(0.5 + (uv - 0.5) * (1.0 - 2.0 * da) - d * da));
  vec4 b = getToColor(mirror(0.5 + (uv - 0.5) * (1.0 - 2.0 * db) + d * db));

  // the pillar, close to the lens, crosses the whole frame. Its edges are soft: out of focus, and
  // smeared by its own speed on screen, but it starts and ends fully out of frame
  float w = clamp(width, 0.1, 1.0) * min(ratio, 1.0);
  float bl = clamp(blur, 0.0, 1.0);
  float defocus = w * (0.02 + 0.08 * bl);
  // the motion blur follows the pillar's speed on screen, about len + w frame heights per progress
  float smear = 0.035 * bl * (len + w);
  float soft = min(defocus + smear * speed, 0.8 * w);
  // at both ends (where the speed is 0.3) the pillar's blurred edge just touches the frame's edge
  float reach = 0.5 * w + 0.5 * min(defocus + smear * 0.3, 0.8 * w) + 0.001;
  float c = -reach + (len + 2.0 * reach) * e;
  // x: across the pillar, -1 on its leading edge to 1 on its trailing edge
  float x = (c - u) / (0.5 * w);
  float edge = soft / w;
  float cover = 1.0 - smoothstep(1.0 - edge, 1.0 + edge, abs(x));
  if (cover <= 0.0) return u < c ? b : a;

  // the cut happens behind the pillar's opaque core, as a dissolve across it, so a see-through
  // pillar never shows a hard seam
  vec4 bg = mix(a, b, smoothstep(-0.8, 0.8, x));

  // the pillar's shading: a rounded post, lit from the side it moves toward, darker toward one end,
  // with a faint rim where the light of the images behind it wraps around its edges
  float xs = clamp(x, -1.0, 1.0);
  float n = sqrt(max(1.0 - xs * xs, 0.0));
  float lambert = max(0.5 * n - 0.85 * xs, 0.0);
  float spec = pow(max(0.6 * n - 0.8 * xs, 0.0), 12.0);
  float rim = smoothstep(0.6, 1.0, abs(xs)) * (1.0 - cover);
  vec4 side = xs < 0.0 ? a : b;
  vec3 lit = pillar.rgb * (0.35 + 1.1 * lambert) * (0.6 + 0.5 * v) + 0.12 * spec;
  lit += 0.25 * rim * side.rgb * side.a;

  // the pillar over the images, premultiplied
  float pa = pillar.a * cover;
  vec4 col = vec4(lit * pa + bg.rgb * bg.a * (1.0 - pa), pa + bg.a * (1.0 - pa));
  return col.a > 0.0 ? vec4(col.rgb / col.a, col.a) : vec4(0.0);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
