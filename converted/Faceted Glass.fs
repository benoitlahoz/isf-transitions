/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "The frame is seen through a pane of irregular cut-glass facets, each one showing its own shifted, zoomed and tilted view of the image, with bright beveled edges and thin dark seams; the facets turn to the incoming image one by one, then the glass flattens out and clears (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "facets",
            "NAME": "facets",
            "TYPE": "float",
            "DEFAULT": 5,
            "MIN": 2,
            "MAX": 20
        },
        {
            "LABEL": "refraction",
            "NAME": "refraction",
            "TYPE": "float",
            "DEFAULT": 0.1,
            "MIN": 0,
            "MAX": 0.3
        },
        {
            "LABEL": "bevel",
            "NAME": "bevel",
            "TYPE": "float",
            "DEFAULT": 0.6,
            "MIN": 0,
            "MAX": 1
        },
        {
            "LABEL": "highlight",
            "NAME": "highlight",
            "TYPE": "color",
            "DEFAULT": [1, 0.98, 0.92, 1],
            "MIN": [0, 0, 0, 0],
            "MAX": [1, 1, 1, 1]
        },
        {
            "LABEL": "seed",
            "NAME": "seed",
            "TYPE": "float",
            "DEFAULT": 0,
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

const float AA = 0.0015;

vec2 hash22(vec2 p) {
  vec3 p3 = fract(vec3(p.xyx) * vec3(0.1031, 0.1030, 0.0973));
  p3 += dot(p3, p3.yzx + 33.33);
  return fract((p3.xx + p3.yz) * p3.zy);
}

vec2 mirror(vec2 v) {
  return 1.0 - abs(1.0 - mod(v, 2.0));
}

vec4 transition(vec2 uv) {
  float h = 1.0 - abs(2.0 * progress - 1.0);
  float amount = h * h * (3.0 - 2.0 * h);

  float n = max(facets, 1.0);
  vec2 aspect = vec2(ratio, 1.0);
  // measured in square pixels, so the facets are not stretched
  vec2 p = uv * aspect * n;
  vec2 i = floor(p);
  vec2 f = p - i;
  vec2 offset = vec2(17.31, 41.97) * seed;
  float bw = 0.18 * amount;

  vec2 mg = vec2(0.0);
  vec2 mr = vec2(0.0);
  float md = 8.0;
  for (int y = -1; y <= 1; y++) {
    for (int x = -1; x <= 1; x++) {
      vec2 g = vec2(float(x), float(y));
      vec2 r = g + 0.1 + 0.8 * hash22(i + g + offset) - f;
      float d = dot(r, r);
      if (d < md) {
        md = d;
        mr = r;
        mg = g;
      }
    }
  }

  // distance to the nearest facet edge, and the direction toward the edges, blended
  // between neighboring edges near the corners so the bevel lighting turns smoothly
  float edge = 8.0;
  vec2 normal = vec2(0.0);
  for (int y = -2; y <= 2; y++) {
    for (int x = -2; x <= 2; x++) {
      vec2 g = mg + vec2(float(x), float(y));
      vec2 r = g + 0.1 + 0.8 * hash22(i + g + offset) - f;
      vec2 dr = r - mr;
      if (dot(dr, dr) > 0.00001) {
        vec2 dn = normalize(dr);
        float e = dot(0.5 * (mr + r), dn);
        edge = min(edge, e);
        float w = max(1.0 - e / bw, 0.0);
        normal += dn * w * w;
      }
    }
  }
  float nl = length(normal);
  normal = nl > 0.0 ? normal / nl : vec2(0.0);

  vec2 id = i + mg + offset;
  vec2 h1 = hash22(id + 31.7);
  vec2 h2 = hash22(id + 59.3);
  vec2 h3 = hash22(id + 97.1);
  float angle = 6.2831853 * h1.x;
  vec2 dir = vec2(cos(angle), sin(angle));
  vec2 shift = dir * (0.4 + 0.6 * h1.y) * refraction;
  float zoom = (h2.x - 0.5) * 6.0 * refraction;
  float tilt = (h2.y - 0.5) * 2.0 * refraction;

  vec2 q = -mr / n;
  vec2 view = -q * zoom + vec2(-q.y, q.x) * sin(tilt) + q * (cos(tilt) - 1.0);

  float bv = 1.0 - smoothstep(0.0, bw, edge);
  bv *= bv;
  vec2 bend = normal * bv * 0.4 * refraction;

  float start = 0.15 + 0.5 * h3.x;
  float m = smoothstep(start, start + 0.2, progress);
  vec2 d = amount * (view + bend);
  vec2 s = amount * shift;
  vec4 ca = getFromColor(mirror(uv + (s + d) / aspect));
  vec4 cb = getToColor(mirror(uv + (d - s) / aspect));
  // mixed premultiplied, so transparent images do not bleed their color
  vec4 c = mix(vec4(ca.rgb * ca.a, ca.a), vec4(cb.rgb * cb.a, cb.a), m);

  vec2 light = normalize(vec2(-0.6, 0.8));
  vec2 slope = dir * (0.4 + 0.6 * h1.y);
  float facetLight = 0.12 * amount * dot(slope, light);
  float lit = dot(normal, light);
  float k = clamp(bevel, 0.0, 1.0);
  float glow = k * bv * max(lit, 0.0) * 1.1 * amount;
  float shade = k * bv * max(-lit, 0.0) * 0.6 * amount;
  float turn = sin(3.14159265 * m);
  glow += k * 0.15 * amount * turn * turn;
  float seam = 0.6 * amount * (1.0 - smoothstep(0.0, 2.0 * AA, edge / n - 0.5 * AA));

  c.rgb *= (1.0 + facetLight) * (1.0 - shade) * (1.0 - seam);
  c.rgb += highlight.rgb * highlight.a * glow * c.a;
  return c.a > 0.0 ? vec4(clamp(c.rgb / c.a, 0.0, 1.0), c.a) : vec4(0.0);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
