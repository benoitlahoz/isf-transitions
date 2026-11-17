/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "lql - MIT",
    "DESCRIPTION": "A chessboard pattern of squares reveals the incoming image (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "grid_num",
            "NAME": "grid_num",
            "TYPE": "float",
            "DEFAULT": 10,
            "MIN": 0,
            "MAX": 40
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

vec4 transition(vec2 uv) {
    vec2 st = uv * grid_num;
    vec2 idx = floor(st);
    vec2 grid = fract(st);

    vec4 a = getFromColor(uv);
    vec4 b = getToColor(uv);

    float checker = mod(idx.x + idx.y, 2.0);
    float mixFactor;

    if (progress <= 0.5) {
        mixFactor = (checker > 0.5) ? step(grid.x, progress * 2.0) : 0.0;
    } else {
        mixFactor = (checker < 0.5) ? step(grid.x, (progress - 0.5) * 2.0) : 1.0;
    }

    return mix(a, b, mixFactor);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
