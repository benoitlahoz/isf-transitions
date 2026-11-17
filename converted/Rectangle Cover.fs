/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "martiniti - MIT",
    "DESCRIPTION": "A rectangle grows from the center to cover the outgoing image, then shrinks to reveal the incoming one (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "bgcolor",
            "NAME": "bgcolor",
            "TYPE": "color",
            "DEFAULT": [0, 0, 0, 1],
            "MIN": [0, 0, 0, 0],
            "MAX": [1, 1, 1, 1]
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
  float s = pow(2.0 * abs(progress - 0.5), 3.0);
  
   vec2 sq = p.xy / vec2(1.0).xy;
   
    vec2 bl = step(vec2(abs(1. - 2.*progress)), sq + .25);
    float dist = bl.x * bl.y;

    vec2 tr = step(vec2(abs(1. - 2.*progress)), 1.25-sq);
    dist *= 1. * tr.x * tr.y;
  
  return mix(
    progress < 0.5 ? getFromColor(p) : getToColor(p),
    bgcolor,
    step(s, dist)
  );
  
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
