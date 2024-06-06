/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "Ben Zhang",
    "DESCRIPTION": "Overexposure - License MIT (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "strength",
            "NAME": "strength",
            "DEFAULT": 0.6,
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

// Author: Ben Zhang
// License: MIT

const float PI = 3.141592653589793;

vec4 transition (vec2 uv) {
  vec4 from = getFromColor(uv);
  vec4 to = getToColor(uv);

  // Multipliers
  float from_m = 1.0 - progress + sin(PI * progress) * strength;
  float to_m = progress + sin(PI * progress) * strength;
  
  return vec4(
    from.r * from.a * from_m + to.r * to.a * to_m,
    from.g * from.a * from_m + to.g * to.a * to_m,
    from.b * from.a * from_m + to.b * to.a * to_m,
    mix(from.a, to.a, progress)
  );
}

void main() {
	if (flip == true) {
    gl_FragColor = transition(vec2(isf_FragNormCoord.x, 1. - isf_FragNormCoord.y));
  } else {
    gl_FragColor = transition(isf_FragNormCoord);
  }
}