/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "Ripples spread from the center while crossfading (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "amplitude",
            "NAME": "amplitude",
            "TYPE": "float",
            "DEFAULT": 100,
            "MIN": 0,
            "MAX": 400
        },
        {
            "LABEL": "speed",
            "NAME": "speed",
            "TYPE": "float",
            "DEFAULT": 50,
            "MIN": 0,
            "MAX": 200
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

vec4 transition (vec2 uv) {
  vec2 dir = uv - vec2(.5);
  float dist = length(dir);
  vec2 offset = dir * (sin(progress * dist * amplitude - progress * speed) + .5) / 30. * progress;
  return mix(
    getFromColor(uv + offset),
    getToColor(uv),
    smoothstep(0.2, 1.0, progress)
  );
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
