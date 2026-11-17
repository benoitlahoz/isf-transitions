/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "OllyOllyOlly - MIT",
    "DESCRIPTION": "The halves of both images slide vertically in opposite directions, out and in at once (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "reverse",
            "NAME": "reverse",
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

const vec2 boundMin = vec2(0.0, 0.0);
const vec2 boundMax = vec2(1.0, 1.0);

bool inBounds (vec2 p) {
  return all(lessThan(boundMin, p)) && all(lessThan(p, boundMax));
}

vec4 transition (vec2 uv) {
  float modifier = (uv.x > 0.5 ? 1.0 : -1.0) * (reverse ? -1.0 : 1.0) ;
  vec2 fromP = vec2(uv.x, uv.y + (progress * modifier));
  vec2 toP = vec2(uv.x, (uv.y + (progress * modifier)) - modifier);

  return inBounds(fromP) ? getFromColor(fromP) : getToColor(toP);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
