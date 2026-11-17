/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "mernking (gitlab.com/Godswork) - MIT",
    "DESCRIPTION": "A crossfade with drifting tracking lines and tearing, like a TV losing its signal (Automatically converted by @benoitlahoz 'isf-transitions').",
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

float hash(vec2 p) {
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453);
}

vec4 transition(vec2 uv) {

    float p = progress;
    float strength = sin(p * 3.14159265);

    vec2 tv = uv;

    vec4 fromColor = getFromColor(tv);
    vec4 toColor   = getToColor(tv);

    vec4 color = mix(fromColor, toColor, p);

    float lineY = floor(tv.y * 120.0);

    float noise = hash(vec2(lineY, p * 20.0));

    float line = step(0.92, noise);

    float drift =
        sin(tv.y * 30.0 + p * 10.0)
        * 0.02
        * strength;

    vec4 shiftedFrom = getFromColor(tv + vec2(drift, 0.0));
    vec4 shiftedTo   = getToColor(tv + vec2(drift, 0.0));

    vec4 lineColor = mix(shiftedFrom, shiftedTo, p);

    color = mix(color, lineColor, line * strength);

    float scan =
        sin(tv.y * 900.0) * 0.03;

    color.rgb -= scan * strength;

    return color;
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
