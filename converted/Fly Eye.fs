/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "The image breaks into a fly's-eye grid of lenses with color separation (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "DEFAULT": 0.04,
            "MIN": 0,
            "MAX": 1
        },
        {
            "LABEL": "zoom",
            "NAME": "zoom",
            "TYPE": "float",
            "DEFAULT": 50,
            "MIN": 0,
            "MAX": 200
        },
        {
            "LABEL": "colorSeparation",
            "NAME": "colorSeparation",
            "TYPE": "float",
            "DEFAULT": 0.3,
            "MIN": 0,
            "MAX": 1.2
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

vec4 transition(vec2 p) {
  float inv = 1. - progress;
  vec2 disp = size*vec2(cos(zoom*p.x), sin(zoom*p.y));
  vec4 texTo = getToColor(p + inv*disp);
  vec4 fromCenter = getFromColor(p + progress*disp);
  vec4 texFrom = vec4(
    getFromColor(p + progress*disp*(1.0 - colorSeparation)).r,
    fromCenter.g,
    getFromColor(p + progress*disp*(1.0 + colorSeparation)).b,
    fromCenter.a);
  return texTo*progress + texFrom*inv;
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
