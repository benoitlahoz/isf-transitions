/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "A crossfade in which each color channel changes at a different time (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "fromStep",
            "NAME": "fromStep",
            "TYPE": "color",
            "DEFAULT": [0, 0.2, 0.4, 0],
            "MIN": [0, 0, 0, 0],
            "MAX": [1, 1, 1, 1]
        },
        {
            "LABEL": "toStep",
            "NAME": "toStep",
            "TYPE": "color",
            "DEFAULT": [0.6, 0.8, 1, 1],
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

// Usage: fromStep and toStep must be in [0.0, 1.0] range 
// and all(fromStep) must be < all(toStep)


vec4 transition (vec2 uv) {
  vec4 a = getFromColor(uv);
  vec4 b = getToColor(uv);
  return mix(a, b, smoothstep(fromStep, toStep, vec4(progress)));
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
