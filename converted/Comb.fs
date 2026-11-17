/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "The frame is cut into interleaved strips that slide in opposite directions, each pushing the outgoing image out and the incoming one in (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "strips",
            "NAME": "strips",
            "TYPE": "long",
            "DEFAULT": 10,
            "VALUES": [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40],
            "LABELS": ["2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40"]
        },
        {
            "LABEL": "vertical",
            "NAME": "vertical",
            "TYPE": "bool",
            "DEFAULT": false
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
  // a: the axis along which the strips slide, b: the axis across them
  float a = vertical ? uv.y : uv.x;
  float b = vertical ? uv.x : uv.y;
  float dir = mod(floor(b * float(strips)), 2.0) < 0.5 ? 1.0 : -1.0;
  // ease in and out, but keep moving from the first frame
  float t = mix(progress, progress * progress * (3.0 - 2.0 * progress), 0.6);
  float fa = a - dir * t;
  if (fa >= 0.0 && fa <= 1.0) {
    return getFromColor(vertical ? vec2(b, fa) : vec2(fa, b));
  }
  float ta = fa + dir;
  return getToColor(vertical ? vec2(b, ta) : vec2(ta, b));
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
