/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "mikolalysenko - MIT",
    "DESCRIPTION": "A crossfade with a gentle wavy distortion (Automatically converted by @benoitlahoz 'isf-transitions').",
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

vec2 offset(float progress, float x, float theta) {
  float phase = progress*progress + progress + theta;
  float shifty = 0.03*progress*cos(10.0*(progress+x));
  return vec2(0, shifty);
}
vec4 transition(vec2 p) {
  return mix(getFromColor(p + offset(progress, p.x, 0.0)), getToColor(p + offset(1.0-progress, p.x, 3.14)), progress);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
