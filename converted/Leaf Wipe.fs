/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "@Flexi23 - MIT",
    "DESCRIPTION": "A leaf shape grows from the center to reveal the incoming image (Automatically converted by @benoitlahoz 'isf-transitions').",
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

// inspired by http://www.wolframalpha.com/input/?i=cannabis+curve

vec4 transition (vec2 uv) {
  vec2 leaf_uv = (uv - vec2(0.5))/10./max(pow(progress,3.5), 1e-6);
	leaf_uv.y += 0.35;
	float r = 0.18;
	float o = atan(leaf_uv.y, leaf_uv.x);
  return mix(getFromColor(uv), getToColor(uv), 1.-step(1. - length(leaf_uv)+r*(1.+sin(o))*(1.+0.9 * cos(8.*o))*(1.+0.1*cos(24.*o))*(0.9+0.05*cos(200.*o)), 1.));
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
