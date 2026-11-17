/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "Adrian Purser - MIT",
    "DESCRIPTION": "The outgoing image drops out of the frame with a few bounces, revealing the incoming one (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "shadow_colour",
            "NAME": "shadow_colour",
            "TYPE": "color",
            "DEFAULT": [0, 0, 0, 0.6],
            "MIN": [0, 0, 0, 0],
            "MAX": [1, 1, 1, 1]
        },
        {
            "LABEL": "shadow_height",
            "NAME": "shadow_height",
            "TYPE": "float",
            "DEFAULT": 0.075,
            "MIN": 0.01,
            "MAX": 0.5
        },
        {
            "LABEL": "bounces",
            "NAME": "bounces",
            "TYPE": "float",
            "DEFAULT": 3,
            "MIN": 1,
            "MAX": 10
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

const float PI = 3.14159265358;

vec4 transition (vec2 uv) {
  float time = progress;
  float stime = sin(time * PI / 2.);
  float phase = time * PI * bounces;
  float y = (abs(cos(phase))) * (1.0 - stime);
  float d = uv.y - y;
  vec4 to = getToColor(uv);
  return mix(
    mix(
      to,
      vec4(shadow_colour.rgb, 1.0), // shadow_colour.a is in the amount: the shadow darkens, it does not make the image transparent
      step(d, shadow_height) * (1. - mix(
        ((d / shadow_height) * shadow_colour.a) + (1.0 - shadow_colour.a),
        1.0,
        smoothstep(0.95, 1., progress)
      )) * to.a // cast on the incoming image, not on its transparent areas
    ),
    getFromColor(vec2(uv.x, uv.y + (1.0 - y))),
    step(d, 0.0)
  );
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
