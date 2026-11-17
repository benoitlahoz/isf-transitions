/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "P-Seebauer - MIT",
    "DESCRIPTION": "Pixels switch to the incoming image in order of how close their colors are (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "power",
            "NAME": "power",
            "TYPE": "float",
            "DEFAULT": 5,
            "MIN": 0,
            "MAX": 20
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

// ported by gre from https://gist.github.com/P-Seebauer/2a5fa2f77c883dd661f9


vec4 transition(vec2 p) {
  vec4 fTex = getFromColor(p);
  vec4 tTex = getToColor(p);
  float m = step(distance(fTex, tTex), progress);
  return mix(
    mix(fTex, tTex, m),
    tTex,
    pow(progress, power)
  );
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
