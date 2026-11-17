/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "A crossfade through grayscale (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "intensity",
            "NAME": "intensity",
            "TYPE": "float",
            "DEFAULT": 0.3,
            "MIN": 0,
            "MAX": 1
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

vec3 grayscale (vec3 color) {
  return vec3(0.2126*color.r + 0.7152*color.g + 0.0722*color.b);
}
 
vec4 transition (vec2 uv) {
  vec4 fc = getFromColor(uv);
  vec4 tc = getToColor(uv);
  return mix(
    mix(vec4(grayscale(fc.rgb), fc.a), fc, smoothstep(1.0-intensity, 0.0, progress)),
    mix(vec4(grayscale(tc.rgb), tc.a), tc, smoothstep(    intensity, 1.0, progress)),
    progress);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
