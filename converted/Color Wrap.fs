/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "Yoni Maltsman @friendlyspinach - MIT",
    "DESCRIPTION": "Each color channel of the outgoing image is shifted and wraps around into psychedelic bands, while crossfading to the incoming image (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "r_strength",
            "NAME": "r_strength",
            "TYPE": "float",
            "DEFAULT": 1,
            "MIN": -2,
            "MAX": 2
        },
        {
            "LABEL": "g_strength",
            "NAME": "g_strength",
            "TYPE": "float",
            "DEFAULT": 1,
            "MIN": -2,
            "MAX": 2
        },
        {
            "LABEL": "b_strength",
            "NAME": "b_strength",
            "TYPE": "float",
            "DEFAULT": 1,
            "MIN": -2,
            "MAX": 2
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
  vec4 from = getFromColor(uv);
  vec4 to = getToColor(uv);
  vec3 c = from.rgb + vec3(r_strength, g_strength, b_strength)*progress;
  // The wrap flips bright (or, with negative strengths, dark) channels at once: fade it in over the first frames
  from.rgb = mix(from.rgb, mod(c, 1.0), smoothstep(0.0, 0.1, progress));
  return mix(from, to, progress);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
