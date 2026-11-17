/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "Thibaut Foussard - MIT",
    "DESCRIPTION": "Like Directional, with both images scaled down while they slide (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "direction",
            "NAME": "direction",
            "TYPE": "point2D",
            "DEFAULT": [0, 1],
            "MIN": [-4, -4],
            "MAX": [4, 4]
        },
        {
            "LABEL": "scale",
            "NAME": "scale",
            "TYPE": "float",
            "DEFAULT": 0.7,
            "MIN": 0,
            "MAX": 2.8
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

// based on Directional transition by Gaëtan Renaudeau
// https://gl-transitions.com/editor/Directional

#define PI acos(-1.0)


float parabola(float x) {
  float y = pow(sin(x * PI), 1.);
  return y;
}

vec4 transition (vec2 uv) {
  float easedProgress = pow(sin(progress  * PI / 2.), 3.);
  vec2 p = uv + easedProgress * sign(direction);
  vec2 f = fract(p);
  
  float s = 1. - (1. - (1. / scale)) * parabola(progress);
  f = (f - 0.5) * s  + 0.5;
  
  float mixer = step(0.0, p.y) * step(p.y, 1.0) * step(0.0, p.x) * step(p.x, 1.0);
  vec4 col = mix(getToColor(f), getFromColor(f), mixer);
  
  float border = step(0., f.x) * step(0., (1. - f.x)) * step(0., f.y) * step(0., 1. - f.y);
  col *= border;
  
  return col;
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
