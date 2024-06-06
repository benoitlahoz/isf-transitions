/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre",
    "DESCRIPTION": "fadecolor - License MIT (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "color",
            "NAME": "color",
            "TYPE": "color",
            
            "DEFAULT": [
              0,
              0,
              0,
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
            "LABEL": "colorPhase",
            "NAME": "colorPhase",
            "DEFAULT": 0.4,
            "TYPE": "float",
            "MAX": 1000,
            "MIN": 0
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

// author: gre
// License: MIT
vec4 transition (vec2 uv) {
  return mix(
    mix(vec4(color.rgb, 1.0), getFromColor(uv), smoothstep(1.0-colorPhase, 0.0, progress)),
    mix(vec4(color.rgb, 1.0), getToColor(uv), smoothstep(    colorPhase, 1.0, progress)),
    progress);
}

void main() {
	if (flip == true) {
    gl_FragColor = transition(vec2(isf_FragNormCoord.x, 1. - isf_FragNormCoord.y));
  } else {
    gl_FragColor = transition(isf_FragNormCoord);
  }
}