/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "The bright points of the outgoing image sparkle into crisp four-point star glints that swell and twinkle up to the cut, then shrink back into the highlights of the incoming image, which stays sharp (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "size",
            "NAME": "size",
            "TYPE": "float",
            "DEFAULT": 0.15,
            "MIN": 0.02,
            "MAX": 0.3
        },
        {
            "LABEL": "threshold",
            "NAME": "threshold",
            "TYPE": "float",
            "DEFAULT": 0.6,
            "MIN": 0,
            "MAX": 1
        },
        {
            "LABEL": "intensity",
            "NAME": "intensity",
            "TYPE": "float",
            "DEFAULT": 2,
            "MIN": 0,
            "MAX": 4
        },
        {
            "LABEL": "diagonals",
            "NAME": "diagonals",
            "TYPE": "bool",
            "DEFAULT": false
        },
        {
            "LABEL": "rotation",
            "NAME": "rotation",
            "TYPE": "float",
            "DEFAULT": 0,
            "MIN": -45,
            "MAX": 45
        },
        {
            "LABEL": "rainbow",
            "NAME": "rainbow",
            "TYPE": "float",
            "DEFAULT": 0.35,
            "MIN": 0,
            "MAX": 1
        },
        {
            "LABEL": "tint",
            "NAME": "tint_isf",
            "TYPE": "color",
            "DEFAULT": [1, 0.95, 0.85, 1],
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

#define tint tint_isf.rgb

const float REACH = 6.0;

float hash12(vec2 v) {
  vec3 v3 = fract(vec3(v.xyx) * 0.1031);
  v3 += dot(v3, v3.yzx + 33.33);
  return fract((v3.x + v3.y) * v3.z);
}

// one thin ray along x, from the glint center to its tip at x = len (in cells)
float ray(vec2 d, float len, float width) {
  float a = abs(d.x) / len;
  if (a >= 1.0) return 0.0;
  float fall = (1.0 - a) * (1.0 - a);
  float w = width * (1.0 - 0.7 * a);
  return fall * exp(-d.y * d.y / (w * w));
}

vec3 spectrum(float a) {
  return 0.5 + 0.5 * cos(6.2831853 * (a + vec3(0.0, 0.33, 0.67)));
}

vec4 transition(vec2 uv) {
  float p = progress;
  float t = 1.0 - abs(2.0 * p - 1.0);
  float amount = t * (2.0 - t);
  float m = smoothstep(0.42, 0.58, p);
  if (amount <= 0.0) return m < 0.5 ? getFromColor(uv) : getToColor(uv);

  vec4 fromColor = getFromColor(uv);
  vec4 toColor = getToColor(uv);
  vec4 c = mix(vec4(fromColor.rgb * fromColor.a, fromColor.a), vec4(toColor.rgb * toColor.a, toColor.a), m);
  if (c.a <= 0.0) return vec4(0.0);
  vec3 base = c.rgb / c.a;

  // a grid of glint cells, turned with the cross so that its long rays run along the rows and columns
  float cell = size / REACH;
  float ang = radians(rotation);
  vec2 cs = vec2(cos(ang), sin(ang));
  vec2 aspect = vec2(ratio, 1.0);
  vec2 o = (uv - 0.5) * aspect;
  vec2 q = vec2(o.x * cs.x + o.y * cs.y, o.y * cs.x - o.x * cs.y) / cell;
  vec2 home = floor(q);
  // thin rays, in cells, thin enough to vanish before the next row of cells
  float width = min(0.0022 / cell, 0.07);

  vec3 light = vec3(0.0);
  // the glints of the own cell, of the cells along its row and column (whose long rays reach this
  // pixel) and of its four diagonal neighbours (for their short diagonal rays and their cores)
  for (int k = 0; k < 29; k++) {
    float fk = float(k);
    vec2 off = vec2(0.0);
    if (k >= 1 && k <= 12) off = vec2(fk < 7.0 ? fk - 7.0 : fk - 6.0, 0.0);
    else if (k >= 13 && k <= 24) off = vec2(0.0, fk < 19.0 ? fk - 19.0 : fk - 18.0);
    else if (k >= 25) off = vec2(mod(fk, 2.0) * 2.0 - 1.0, fk < 27.0 ? -1.0 : 1.0);
    // the rays are never longer than this, so farther glints cannot reach the pixel
    if (max(abs(off.x), abs(off.y)) > REACH * amount + 1.0) continue;
    vec2 id = home + off;
    if (hash12(id + 89.5) > 0.55) continue;
    float h0 = hash12(id + 0.5);
    float h1 = hash12(id + 19.5);
    float h2 = hash12(id + 41.5);
    float h3 = hash12(id + 67.5);
    // the glint: somewhere in its cell, so that the grid never shows
    vec2 center = id + 0.5 + 0.6 * (vec2(h0, h1) - 0.5);
    vec2 cw = center * cell;
    vec2 st = vec2(cw.x * cs.x - cw.y * cs.y, cw.x * cs.y + cw.y * cs.x) / aspect + 0.5;
    if (st.x < 0.0 || st.x > 1.0 || st.y < 0.0 || st.y > 1.0) continue;
    float turn = 0.15 + 0.7 * h3;
    vec4 src = m < turn ? getFromColor(st) : getToColor(st);
    float lum = dot(src.rgb, vec3(0.2126, 0.7152, 0.0722)) * src.a;
    float knee = threshold + 0.8 * (1.0 - threshold) * h2;
    float b = smoothstep(knee, knee + 0.2 * (1.0 - threshold) + 1e-3, lum);
    if (b <= 0.0) continue;
    float twinkle = (0.6 + 0.4 * sin(6.2831853 * (h2 + (1.5 + h0) * p))) * smoothstep(0.0, 0.12, abs(m - turn));
    float len = REACH * amount * (0.45 + 0.55 * b) * (0.3 + 0.7 * h1 * h1) * (0.85 + 0.15 * twinkle);
    vec2 d = q - center;
    float r = length(d);
    float star = ray(d, len, width) + ray(d.yx, len, width);
    if (diagonals) {
      vec2 e = vec2(d.x + d.y, d.x - d.y) * 0.70710678;
      star += 0.5 * (ray(e, 0.25 * len, width) + ray(e.yx, 0.25 * len, width));
    }
    float cr = r / (0.08 * len);
    star += exp(-cr * cr) + 0.3 * exp(-r / 0.2);
    float along = r / len;
    vec3 hue = mix(vec3(1.0), spectrum(1.3 * along + h0), rainbow * smoothstep(0.05, 0.4, along));
    vec3 col = mix(src.rgb / max(max(src.r, max(src.g, src.b)), 1e-3), vec3(1.0), 0.6);
    light += b * twinkle * star * hue * col;
  }
  light *= tint * intensity * amount * amount;
  // compressed, so that crossing rays never flatten into white
  light = 1.0 - exp(-light);
  return vec4(clamp(base + light - base * light, 0.0, 1.0), c.a);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
