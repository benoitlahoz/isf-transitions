/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "The outgoing image parts in the middle like stage curtains, gathering into shaded folds as it is pulled to the sides (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "folds",
            "NAME": "folds",
            "TYPE": "float",
            "DEFAULT": 7,
            "MIN": 1,
            "MAX": 16
        },
        {
            "LABEL": "shading",
            "NAME": "shading",
            "TYPE": "float",
            "DEFAULT": 0.7,
            "MIN": 0,
            "MAX": 1
        },
        {
            "LABEL": "bunch",
            "NAME": "bunch",
            "TYPE": "float",
            "DEFAULT": 0.12,
            "MIN": 0,
            "MAX": 0.4
        },
        {
            "LABEL": "sway",
            "NAME": "sway",
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

const float PI = 3.141592653589793;

vec4 transition (vec2 uv) {
  float lag = sway * (1.0 - uv.y);
  float p = progress * (1.0 - lag * (1.0 - progress));
  float open = mix(p, p * p * (3.0 - 2.0 * p), 0.75);
  // each curtain hangs from its outer edge, gathers to the bunch width, then leaves the frame
  float edge = -bunch * open * open;
  float w = mix(0.5, bunch, open);
  float x = min(uv.x, 1.0 - uv.x) - edge;
  if (x >= w) {
    vec4 to = getToColor(uv);
    float shadow = 0.4 * exp((w - x) * 25.0) * clamp((edge + w) * 20.0, 0.0, 1.0);
    return vec4(to.rgb * (1.0 - shadow * open), to.a);
  }
  float t = x / w;
  // pleats: the fabric bunches up where cos(a) > 0 and is stretched elsewhere
  float n = max(1.0, floor(folds * min(ratio, 1.0) + 0.5)); // folds keep their size in portrait
  float a = 2.0 * PI * n * t;
  float f = 1.0 - (1.0 - open) * (1.0 - open) * (1.0 - open);
  float gather = 0.92 * f;
  float s = 0.5 * (t + gather * sin(a) / (2.0 * PI * n));
  vec4 c = getFromColor(vec2(uv.x < 0.5 ? s : 1.0 - s, uv.y));
  float slope = 3.0 * gather * cos(a) * (uv.x < 0.5 ? 1.0 : -1.0);
  float diffuse = (1.0 + 0.5 * slope) / sqrt(1.25 * (1.0 + slope * slope));
  float crease = 0.5 + 0.5 * sin(a); // the back of each pleat is in the dark
  float light = mix(1.0, diffuse * (1.0 - 0.6 * crease * crease), shading * f);
  return vec4(c.rgb * light, c.a);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
