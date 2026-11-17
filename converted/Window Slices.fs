/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "The incoming image appears through vertical slices that widen (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "count",
            "NAME": "count",
            "TYPE": "float",
            "DEFAULT": 10,
            "MIN": 0,
            "MAX": 40
        },
        {
            "LABEL": "smoothness",
            "NAME": "smoothness",
            "TYPE": "float",
            "DEFAULT": 0.5,
            "MIN": 0,
            "MAX": 2
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

vec4 transition (vec2 p) {
  float pr = smoothstep(-smoothness, 0.0, p.x - progress * (1.0 + smoothness));
  float s = step(pr, fract(count * p.x));
  return mix(getFromColor(p), getToColor(p), s);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
