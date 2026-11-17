/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "Xaychru - MIT",
    "DESCRIPTION": "A radial wipe that sweeps around the center like a clock hand (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "smoothness",
            "NAME": "smoothness",
            "TYPE": "float",
            "DEFAULT": 1,
            "MIN": 0,
            "MAX": 4
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

// ported by gre from https://gist.github.com/Xaychru/ce1d48f0ce00bb379750


const float PI = 3.141592653589;

vec4 transition(vec2 p) {
  vec2 rp = p*2.-1.;
  return mix(
    getToColor(p),
    getFromColor(p),
    smoothstep(0., smoothness, atan(rp.y,rp.x) + PI + smoothness - progress * (2.0 * PI + smoothness))
  );
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
