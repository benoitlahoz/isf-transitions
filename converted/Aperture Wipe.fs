/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "A camera aperture opens its turning blades to reveal the incoming image (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "DEFAULT": 6,
            "VALUES": [3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
            "LABELS": ["3", "4", "5", "6", "7", "8", "9", "10", "11", "12"]
        },
        {
            "LABEL": "rotation",
            "NAME": "rotation",
            "TYPE": "float",
            "DEFAULT": 60,
            "MIN": -180,
            "MAX": 180
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

#define PI 3.141592653589793


const float AA = 0.006;

vec4 transition (vec2 uv) {
  // Square pixels, normalized so the frame's corners are at distance 1
  vec2 q = (uv - 0.5) * vec2(ratio, 1.0) / (0.5 * length(vec2(ratio, 1.0)));
  float n = clamp(float(blades), 3.0, 12.0);
  float sector = 2.0 * PI / n;
  float turn = radians(rotation) * progress;
  // Each blade's straight edge is at distance r from the center: the opening is the polygon they enclose
  float r = mix(-AA, 1.0 + AA, progress);
  float dist = -1e3;
  float seam = 0.0;
  for (int i = 0; i < 12; i++) {
    if (float(i) < n) {
      float angle = turn + float(i) * sector;
      float d = dot(q, vec2(cos(angle), sin(angle))) - r;
      float next = dot(q, vec2(cos(angle + sector), sin(angle + sector))) - r;
      dist = max(dist, d);
      float s = 1.0 - smoothstep(0.0, 0.12, -d);
      seam = max(seam, (1.0 - smoothstep(0.0, AA, d)) * s * s * smoothstep(0.0, AA, next));
    }
  }
  float rim = 1.0 - smoothstep(0.0, 0.08, dist);
  float shade = shadow * max(rim * rim, seam) * smoothstep(0.0, 0.1, progress);
  vec4 from = getFromColor(uv);
  from.rgb *= 1.0 - shade;
  return mix(from, getToColor(uv), 1.0 - smoothstep(0.0, AA, dist));
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
