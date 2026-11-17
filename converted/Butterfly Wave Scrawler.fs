/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "mandubian - MIT",
    "DESCRIPTION": "Wavy, color-separated ripples scramble the outgoing image into the incoming one (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "DEFAULT": 1,
            "MIN": 0,
            "MAX": 4
        },
        {
            "LABEL": "waves",
            "NAME": "waves",
            "TYPE": "float",
            "DEFAULT": 30,
            "MIN": 0,
            "MAX": 120
        },
        {
            "LABEL": "colorSeparation",
            "NAME": "colorSeparation",
            "TYPE": "float",
            "DEFAULT": 0.3,
            "MIN": 0,
            "MAX": 1.2
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

const float PI = 3.14159265358979323846264;
float compute(vec2 p, float progress, vec2 center) {
vec2 o = p*sin(progress * amplitude)-center;
vec2 h = vec2(1., 0.);
// butterfly polar function (don't ask me why this one :))
float theta = acos(dot(o, h)) * waves;
float s = sin((2.*theta - PI) / 24.);
float s2 = s * s;
return (exp(cos(theta)) - 2.*cos(4.*theta) + s2 * s2 * s) / 10.;
}
vec4 transition(vec2 uv) {
  vec2 p = uv;
  float inv = 1. - progress;
  float disp = compute(p, progress, vec2(0.5, 0.5));
  vec4 texTo = getToColor(p + inv*disp);
  vec4 fromCenter = getFromColor(p + progress*disp);
  vec4 texFrom = vec4(
    getFromColor(p + progress*disp*(1.0 - colorSeparation)).r,
    fromCenter.g,
    getFromColor(p + progress*disp*(1.0 + colorSeparation)).b,
    fromCenter.a);
  return texTo*progress + texFrom*inv;
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
