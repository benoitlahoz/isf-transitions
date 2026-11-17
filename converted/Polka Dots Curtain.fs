/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "bobylito - MIT",
    "DESCRIPTION": "The incoming image appears through growing polka dots (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "dots",
            "NAME": "dots",
            "TYPE": "float",
            "DEFAULT": 20,
            "MIN": 0,
            "MAX": 80
        },
        {
            "LABEL": "center",
            "NAME": "center",
            "TYPE": "point2D",
            "DEFAULT": [0, 0],
            "MIN": [-1, -1],
            "MAX": [1, 1]
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

const float SQRT_2 = 1.414213562373;

vec4 transition(vec2 uv) {
  bool nextImage = distance(fract(uv * dots), vec2(0.5, 0.5)) < ( progress / distance(uv, center));
  return nextImage ? getToColor(uv) : getFromColor(uv);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
