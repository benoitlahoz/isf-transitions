/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "The outgoing image falls over backward, hinged on its bottom edge, revealing the incoming one behind it (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "perspective",
            "NAME": "perspective",
            "TYPE": "float",
            "DEFAULT": 0.8,
            "MIN": 0,
            "MAX": 1
        },
        {
            "LABEL": "shading",
            "NAME": "shading",
            "TYPE": "float",
            "DEFAULT": 0.7,
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

const float PI = 3.141592653589793;

vec4 transition (vec2 uv) {
  float fall = progress * (0.25 + 0.75 * progress);
  float angle = 0.5 * PI * fall;
  float c = cos(angle), s = sin(angle);
  // a camera in front of the middle of the image goes down to the level of the hinge,
  // so that the image ends edge-on
  float dist = 1.0 / max(perspective, 0.01);
  vec3 eye = vec3(0.5 * ratio, 0.5 * c, -dist);
  vec3 dir = vec3(uv.x * ratio, uv.y, 0.0) - eye;
  float den = dir.z * c - dir.y * s;
  if (den > 0.0) {
    float t = (eye.y * s - eye.z * c) / den;
    vec3 hit = eye + t * dir;
    vec2 puv = vec2(hit.x / ratio, hit.y * c + hit.z * s);
    if (t > 0.0 && puv.y > 0.0 && puv.y <= 1.0 && puv.x >= 0.0 && puv.x <= 1.0) {
      vec4 col = getFromColor(puv);
      return vec4(col.rgb * (1.0 - shading * s), col.a);
    }
  }
  return getToColor(uv);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
