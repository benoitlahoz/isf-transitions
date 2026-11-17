/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "The outgoing image rises like a theater festoon curtain: cords lift its hem into scalloped swags while the fabric gathers into shaded ruffles, and it is drawn up out of the frame over the incoming image (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "swags",
            "NAME": "swags",
            "TYPE": "float",
            "DEFAULT": 3,
            "MIN": 1,
            "MAX": 8
        },
        {
            "LABEL": "sag",
            "NAME": "sag",
            "TYPE": "float",
            "DEFAULT": 0.14,
            "MIN": 0,
            "MAX": 0.3
        },
        {
            "LABEL": "ruffles",
            "NAME": "ruffles",
            "TYPE": "float",
            "DEFAULT": 14,
            "MIN": 4,
            "MAX": 30
        },
        {
            "LABEL": "shading",
            "NAME": "shading",
            "TYPE": "float",
            "DEFAULT": 0.7,
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
  float p = progress;
  float f = 1.0 - (1.0 - p) * (1.0 - p) * (1.0 - p);
  float e = mix(p, p * p * (3.0 - 2.0 * p), 0.4);
  float s = sag * f;
  float lift = mix(-0.01, 1.0 + sag + 0.08, e);
  // across a swag, -1 and 1 at the cords
  float n = max(1.0, floor(swags * sqrt(ratio / 1.78) + 0.5)); // fewer swags in portrait
  float u = fract(uv.x * n) * 2.0 - 1.0;
  float bump = 1.0 - u * u;
  float hem = lift - s * bump;
  float aa = 0.002 + 0.004 * f;
  float inside = smoothstep(hem - aa, hem + aa, uv.y);

  vec4 c = vec4(0.0);
  if (inside > 0.0) {
    // q goes from 0 at the top rod to 1 at the hem; the whole height of fabric is gathered into it
    float h = max(1.0 - max(hem, 0.0), 1e-4);
    float q = clamp((1.0 - uv.y) / h, 0.0, 1.0);
    // ruffles: the fabric bunches where cos(a) > 0, rows parallel to the scalloped hem
    // as the curtain shortens, its ruffles merge so none gets thinner than 2% of the frame
    float m = min(max(1.0, floor(ruffles + 0.5)), h / 0.02);
    float a = 2.0 * PI * m * q;
    float g = 0.85 * f * min(1.0, 0.3 + 5.0 * (1.0 - h)) * (0.6 + 0.4 * bump);
    float v = clamp(q + g * sin(a) / (2.0 * PI * m), 0.0, 1.0);
    float pull = 0.25 * f * q * q * sin(PI * u) / (PI * n);
    c = getFromColor(vec2(uv.x - pull, 1.0 - v));

    float slope = 2.5 * g * cos(a);
    float diffuse = (1.0 - 0.35 * slope) / sqrt(1.0 + slope * slope);
    float crease = 0.5 + 0.5 * sin(a);
    float light = mix(1.0, diffuse * (1.0 - 0.45 * crease * crease), shading * g);
    light += 0.18 * shading * g * smoothstep(0.4, 1.0, -cos(a));
    float depth = f * q * (0.3 + 0.7 * q);
    light *= 1.0 - shading * depth * (0.35 * u * u + 0.5 * exp(-(1.0 - abs(u)) * 14.0));
    light *= 1.0 - 0.35 * shading * f * exp(-(uv.y - hem) * 120.0);
    c.rgb *= light;
    if (inside >= 1.0) return c;
  }
  vec4 to = getToColor(uv);
  float below = max(hem - uv.y, 0.0);
  float shadow = 0.6 * shading * exp(-below * 22.0) * smoothstep(0.0, 0.05, lift) * (1.0 - smoothstep(1.0, 1.06, lift - s));
  to.rgb *= 1.0 - shadow;
  return mix(to, c, inside);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
