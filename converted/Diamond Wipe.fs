/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "A soft-edged diamond grows from the center to reveal the incoming image (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "center",
            "NAME": "center",
            "TYPE": "point2D",
            "DEFAULT": [0.5, 0.5],
            "MIN": [-2, -2],
            "MAX": [2, 2]
        },
        {
            "LABEL": "smoothness",
            "NAME": "smoothness",
            "TYPE": "float",
            "DEFAULT": 0.03,
            "MIN": 0,
            "MAX": 0.5
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

vec4 transition (vec2 uv) {
  // Measured in square pixels, so the diamond is not stretched
  vec2 d = abs(uv - center) * vec2(ratio, 1.0);
  vec2 far = max(center, 1.0 - center) * vec2(ratio, 1.0);
  float dist = (d.x + d.y) / (far.x + far.y);
  float s = max(smoothness, 0.001);
  float m = smoothstep(0.0, s, progress * (1.0 + s) - dist);
  return mix(getFromColor(uv), getToColor(uv), m);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
