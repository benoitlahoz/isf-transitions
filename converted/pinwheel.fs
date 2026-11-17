/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "Mr Speaker - MIT",
    "DESCRIPTION": "A spinning pinwheel reveals the incoming image (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "speed",
            "NAME": "speed",
            "TYPE": "float",
            "DEFAULT": 2,
            "MIN": 0,
            "MAX": 8
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
  
  vec2 p = uv.xy / vec2(1.0).xy;
  
  float circPos = atan(p.y - 0.5, p.x - 0.5) + progress * speed;
  float modPos = mod(circPos, 3.1415 / 4.);
  // starts just below 0 so the sector edges are not drawn on the first frame
  float signed = sign(progress * (3.1415 / 4. + 0.01) - 0.01 - modPos);
  
  return mix(getToColor(p), getFromColor(p), step(signed, 0.5));
  
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
