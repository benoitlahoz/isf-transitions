/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "The outgoing image wraps itself onto a shaded ball that shrinks and rolls away, revealing the incoming image under its shadow (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "size",
            "NAME": "size",
            "TYPE": "float",
            "DEFAULT": 0.3,
            "MIN": 0.1,
            "MAX": 0.5
        },
        {
            "LABEL": "shading",
            "NAME": "shading",
            "TYPE": "float",
            "DEFAULT": 0.6,
            "MIN": 0,
            "MAX": 1
        },
        {
            "LABEL": "shadow",
            "NAME": "shadow",
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

const float PI = 3.14159265;
const float AA = 0.0015;
const vec3 LIGHT = vec3(-0.5, 0.6, 1.0) / 1.2689;

vec4 transition(vec2 uv) {
  vec4 b = getToColor(uv);

  vec2 dir = length(direction) > 0.0 ? normalize(direction) : vec2(1.0, 0.0);
  vec2 side = vec2(-dir.y, dir.x);
  vec2 p = (uv - 0.5) * vec2(ratio, 1.0);
  float shorter = min(ratio, 1.0);
  float r0 = clamp(size, 0.1, 0.5) * shorter;

  // the image curves into a ball (infinite radius: exactly flat at the start), then the
  // ball shrinks while rolling away until it and its shadow have left the frame
  float w = clamp(progress / 0.45, 0.0, 1.0);
  w = 1.0 - (1.0 - w) * (1.0 - w);
  float away = smoothstep(0.3, 1.0, progress);
  away *= away;
  float wrapped = r0 / max(w, 1e-4);
  float radius = wrapped * mix(1.0, 0.5, away);
  // the image is pulled tighter as it wraps, so that it ends up covering the whole ball
  // (its shortest half extent reaches around to the back)
  float tight = mix(1.0, min(1.0, 0.48 * shorter / (PI * r0)), w * w);
  float paper = radius * tight;
  float e = 0.5 * (abs(dir.x) * ratio + abs(dir.y));
  // far enough that the ball (0.5 * r0 by then) and its shadow (within 0.9 * r0 of it) have left
  float full = e + 1.1 * r0 + 2.0 * AA;
  float travel = away * full;
  vec2 center = dir * travel;
  // rolling without slipping: the integral of dtravel / radius while the radius shrinks
  float angle = -2.0 * full * log(1.0 - 0.5 * away) / wrapped;

  vec2 d = p - center;
  float r = length(d);

  vec2 sd = d - vec2(0.25, -0.3) * radius * w;
  float sh = shadow * w * (1.0 - smoothstep(0.6 * radius, 1.4 * radius, length(sd)));
  vec4 bg = vec4(b.rgb * b.a * (1.0 - sh), b.a);

  vec4 ball = vec4(0.0);
  float cover = 1.0 - smoothstep(radius - AA, radius + AA, r);
  if (cover > 0.0) {
    // normal of the visible point of the ball (no squared radius, which overflows when it is huge)
    vec2 nd = d / radius;
    vec3 n = vec3(nd, sqrt(max(1.0 - dot(nd, nd), 0.0)));
    float u = dot(n.xy, dir);
    float s = dot(n.xy, side);
    float cu = cos(angle), su = sin(angle);
    vec3 q = vec3(u * cu - n.z * su, s, u * su + n.z * cu);
    // the image is wrapped around the ball from its front point, distances kept along the surface
    float lq = length(q.xy);
    float arc = atan(lq, q.z) * paper;
    vec2 img = lq > 0.0 ? (q.x * dir + q.y * side) / lq * arc : vec2(0.0);
    vec2 iuv = img / vec2(ratio, 1.0) + 0.5;
    vec2 edge = min(iuv, 1.0 - iuv) * vec2(ratio, 1.0);
    float inside = cover * smoothstep(-AA, AA, min(edge.x, edge.y));
    if (inside > 0.0) {
      vec4 c = getFromColor(clamp(iuv, 0.0, 1.0));
      float diff = max(dot(n, LIGHT), 0.0);
      float spec = pow(max(reflect(-LIGHT, n).z, 0.0), 24.0);
      float k = shading * w;
      vec3 lit = c.rgb * mix(1.0, 0.25 + 0.85 * diff, k) + 0.35 * k * spec;
      ball = vec4(clamp(lit, 0.0, 1.0) * c.a, c.a) * inside;
    }
  }

  // the ball over the incoming image, premultiplied so transparent images do not bleed their color
  vec4 c = ball + bg * (1.0 - ball.a);
  return c.a > 0.0 ? vec4(c.rgb / c.a, c.a) : vec4(0.0);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
