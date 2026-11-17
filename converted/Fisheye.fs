/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "The outgoing image bulges toward the viewer through a strong fisheye lens, then the incoming one flattens back into place (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "strength",
            "NAME": "strength",
            "TYPE": "float",
            "DEFAULT": 0.8,
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

vec4 transition(vec2 uv) {
  // lens amount: grows, peaks in the middle, then relaxes, eased so that
  // the magnification of the center (1 / (1 - k)) changes at an even pace
  float h = smoothstep(0.0, 1.0, 1.0 - abs(2.0 * progress - 1.0));
  float k = 1.0 - pow(max(1.0 - strength, 0.01), h);
  // round lens: aspect-corrected coordinates, 1.0 at the corners
  vec2 aspect = vec2(ratio, 1.0);
  vec2 c = (uv - 0.5) * aspect;
  float r = length(c) / length(0.5 * aspect);
  // barrel mapping that keeps the corners in place: the center is magnified,
  // the edges squeezed, and every sample stays inside the image
  vec2 q = 0.5 + (uv - 0.5) * (1.0 - k + k * r * r);
  return mix(getFromColor(q), getToColor(q), smoothstep(0.38, 0.62, progress));
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
