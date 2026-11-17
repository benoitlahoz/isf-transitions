/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "nwoeanhinnogaehr - MIT",
    "DESCRIPTION": "A crossfade through a spinning kaleidoscope (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "speed",
            "NAME": "speed",
            "TYPE": "float",
            "DEFAULT": 1,
            "MIN": 0,
            "MAX": 4
        },
        {
            "LABEL": "angle",
            "NAME": "angle",
            "TYPE": "float",
            "DEFAULT": 1,
            "MIN": 0,
            "MAX": 4
        },
        {
            "LABEL": "power",
            "NAME": "power",
            "TYPE": "float",
            "DEFAULT": 1.5,
            "MIN": 0,
            "MAX": 6
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
  vec2 p = uv.xy / vec2(1.0).xy;
  vec2 q = p;
  float t = pow(progress, power)*speed;
  p = p -0.5;
  for (int i = 0; i < 7; i++) {
    p = vec2(sin(t)*p.x + cos(t)*p.y, sin(t)*p.y - cos(t)*p.x);
    t += angle;
    p = abs(mod(p, 2.0) - 1.0);
  }
  abs(mod(p, 1.0));
  return mix(
    mix(getFromColor(q), getToColor(q), progress),
    mix(getFromColor(p), getToColor(p), progress), 1.0 - 2.0*abs(progress - 0.5));
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
