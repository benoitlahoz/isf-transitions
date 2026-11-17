/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "Squares grow along a diagonal sweep to reveal the incoming image (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "squares",
            "NAME": "squares_isf",
            "TYPE": "point2D",
            "DEFAULT": [10, 10],
            "MIN": [0, 0],
            "MAX": [20, 20]
        },
        {
            "LABEL": "direction",
            "NAME": "direction",
            "TYPE": "point2D",
            "DEFAULT": [1, -0.5],
            "MIN": [-4, -4],
            "MAX": [4, 4]
        },
        {
            "LABEL": "smoothness",
            "NAME": "smoothness",
            "TYPE": "float",
            "DEFAULT": 1.6,
            "MIN": 0,
            "MAX": 6.4
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

#define squares ivec2(squares_isf)

const vec2 center = vec2(0.5, 0.5);
vec4 transition (vec2 p) {
  vec2 v = normalize(direction);
  v /= abs(v.x)+abs(v.y);
  float d = v.x * center.x + v.y * center.y;
  float offset = smoothness;
  float pr = smoothstep(-offset, 0.0, v.x * p.x + v.y * p.y - (d-0.5+progress*(1.+offset)));
  vec2 squarep = fract(p*vec2(squares));
  vec2 squaremin = vec2(pr/2.0);
  vec2 squaremax = vec2(1.0 - pr/2.0);
  float a = (1.0 - step(progress, 0.0)) * step(squaremin.x, squarep.x) * step(squaremin.y, squarep.y) * step(squarep.x, squaremax.x) * step(squarep.y, squaremax.y);
  return mix(getFromColor(p), getToColor(p), a);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
