/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre",
    "DESCRIPTION": "colorphase - License MIT (Automatically converted by @benoitlahoz 'isf-transitions').",
    "INPUTS": [
        {
            "DEFAULT": false,
            "LABEL": "Flip",
            "NAME": "flip",
            "TYPE": "bool"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Progress",
            "MAX": 1,
            "MIN": 0,
            "NAME": "progress",
            "TYPE": "float"
        },
        {
            "LABEL": "From",
            "NAME": "inputImage",
            "TYPE": "image"
        },
        {
            "LABEL": "To",
            "NAME": "to",
            "TYPE": "image"
        },
        {
            "LABEL": "fromStep",
            "NAME": "fromStep",
            "TYPE": "color",
            
            "DEFAULT": [
              0,
              0.2,
              0.4,
              0
            ],
            
            "MIN": [
              0,
              0,
              0,
              0
            ],
            "MAX": [
              1,
              1,
              1,
              1
            ]
        },
        {
            "LABEL": "toStep",
            "NAME": "toStep",
            "TYPE": "color",
            
            "DEFAULT": [
              0.6,
              0.8,
              1,
              1
            ],
            
            "MIN": [
              0,
              0,
              0,
              0
            ],
            "MAX": [
              1,
              1,
              1,
              1
            ]
        }
    ],
    "ISFVSN": "2"
}
*/
float ratio = RENDERSIZE.x / RENDERSIZE.y;

vec4 getFromColor(vec2 uv) {
  if (flip == true) {
    return IMG_NORM_PIXEL(inputImage, vec2(uv.x, 1. - uv.y));
  } else {
    return IMG_NORM_PIXEL(inputImage, uv);
  }
}

vec4 getToColor(vec2 uv) {
  return IMG_NORM_PIXEL(to, uv);
}

// Author: gre
// License: MIT

// Usage: fromStep and toStep must be in [0.0, 1.0] range 
// and all(fromStep) must be < all(toStep)


vec4 transition (vec2 uv) {
  vec4 a = getFromColor(uv);
  vec4 b = getToColor(uv);
  return mix(a, b, smoothstep(fromStep, toStep, vec4(progress)));
}

void main() {
	if (flip == true) {
    gl_FragColor = transition(vec2(isf_FragNormCoord.x, 1. - isf_FragNormCoord.y));
  } else {
    gl_FragColor = transition(isf_FragNormCoord);
  }
}