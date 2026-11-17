/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "Sergey Kosarevsky - MIT",
    "DESCRIPTION": "A crossfade through an out-of-focus blur (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "blurSize",
            "NAME": "blurSize",
            "TYPE": "float",
            "DEFAULT": 0.02,
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

// Ported from https://gist.github.com/corporateshark/b9f8e5675c647e615419


// 12-tap Poisson disk
// https://github.com/spite/Wagner/blob/master/fragment-shaders/poisson-disc-blur-fs.glsl

vec4 transition(vec2 uv) {
  float T = progress;
  float half_ = 0.5;
  float D = (T < half_) ? mix(0.0, blurSize, T / half_) : mix(blurSize, 0.0, (T - half_) / half_);
  vec4 C0 = getFromColor(uv);
  vec4 C1 = getToColor(uv);
  C0 += getFromColor(vec2(-0.326, -0.406) * D + uv);
  C1 += getToColor(vec2(-0.326, -0.406) * D + uv);
  C0 += getFromColor(vec2(-0.840, -0.074) * D + uv);
  C1 += getToColor(vec2(-0.840, -0.074) * D + uv);
  C0 += getFromColor(vec2(-0.696,  0.457) * D + uv);
  C1 += getToColor(vec2(-0.696,  0.457) * D + uv);
  C0 += getFromColor(vec2(-0.203,  0.621) * D + uv);
  C1 += getToColor(vec2(-0.203,  0.621) * D + uv);
  C0 += getFromColor(vec2( 0.962, -0.195) * D + uv);
  C1 += getToColor(vec2( 0.962, -0.195) * D + uv);
  C0 += getFromColor(vec2( 0.473, -0.480) * D + uv);
  C1 += getToColor(vec2( 0.473, -0.480) * D + uv);
  C0 += getFromColor(vec2( 0.519,  0.767) * D + uv);
  C1 += getToColor(vec2( 0.519,  0.767) * D + uv);
  C0 += getFromColor(vec2( 0.185, -0.893) * D + uv);
  C1 += getToColor(vec2( 0.185, -0.893) * D + uv);
  C0 += getFromColor(vec2( 0.507,  0.064) * D + uv);
  C1 += getToColor(vec2( 0.507,  0.064) * D + uv);
  C0 += getFromColor(vec2( 0.896,  0.412) * D + uv);
  C1 += getToColor(vec2( 0.896,  0.412) * D + uv);
  C0 += getFromColor(vec2(-0.322, -0.933) * D + uv);
  C1 += getToColor(vec2(-0.322, -0.933) * D + uv);
  C0 += getFromColor(vec2(-0.792, -0.598) * D + uv);
  C1 += getToColor(vec2(-0.792, -0.598) * D + uv);
  C0 /= 13.0;
  C1 /= 13.0;
  return mix(C0, C1, T);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
