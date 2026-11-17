/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "pthrasher - MIT",
    "DESCRIPTION": "The incoming image appears through a crosshatch of lines spreading from the center (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "threshold",
            "NAME": "threshold",
            "TYPE": "float",
            "DEFAULT": 3,
            "MIN": 0,
            "MAX": 12
        },
        {
            "LABEL": "fadeEdge",
            "NAME": "fadeEdge",
            "TYPE": "float",
            "DEFAULT": 0.1,
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

// adapted by gre from https://gist.github.com/pthrasher/04fd9a7de4012cbb03f6


float rand(vec2 co) {
  return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}
vec4 transition(vec2 p) {
  float dist = distance(center, p) / threshold;
  float r = progress - min(rand(vec2(p.y, 0.0)), rand(vec2(0.0, p.x)));
  return mix(getFromColor(p), getToColor(p), mix(0.0, mix(step(dist, r), 1.0, smoothstep(1.0-fadeEdge, 1.0, progress)), smoothstep(0.0, fadeEdge, progress)));    
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
