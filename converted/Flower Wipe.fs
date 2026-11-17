/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "Fernando Kuteken - MIT",
    "DESCRIPTION": "A flower shape with several petals grows to reveal the incoming image (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "segments",
            "NAME": "segments",
            "TYPE": "long",
            "DEFAULT": 5,
            "VALUES": [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10],
            "LABELS": ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10"]
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

#define PI 3.14159265359


vec4 transition (vec2 uv) {
  
  float angle = atan(uv.y - 0.5, uv.x - 0.5) - 0.5 * PI;
  float normalized = (angle + 1.5 * PI) * (2.0 * PI);
  
  float radius = (cos(float(segments) * angle) + 4.0) / 4.0;
  float difference = length(uv - vec2(0.5, 0.5));
  
  if (difference > radius * progress)
    return getFromColor(uv);
  else
    return getToColor(uv);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
