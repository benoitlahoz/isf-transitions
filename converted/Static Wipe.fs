/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "Ben Lucas - MIT",
    "DESCRIPTION": "A wipe from top to bottom with a band of TV static at its edge (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "u_transitionUpToDown",
            "NAME": "u_transitionUpToDown",
            "TYPE": "bool",
            "DEFAULT": true
        },
        {
            "LABEL": "u_max_static_span",
            "NAME": "u_max_static_span",
            "TYPE": "float",
            "DEFAULT": 0.5,
            "MIN": 0,
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

#define PI 3.14159265359

float rnd (vec2 st) {
    return fract(sin(dot(st.xy,
                         vec2(10,70)))*
        12345.5453123);
}


vec4 transition (vec2 uv) {
  

  float span = u_max_static_span*pow(sin(PI*progress),0.5);
  
  float transitionEdge = u_transitionUpToDown ? 1.0-uv.y : uv.y;
  float mixRatio = 1.0 - step(progress, transitionEdge);

  vec4 transitionMix = mix(
    getFromColor(uv),
    getToColor(uv),
    mixRatio
  );
  
  float noiseEnvelope = smoothstep(progress-span, progress, transitionEdge) * (1.0 - smoothstep(progress, progress + span, transitionEdge));
  vec4 noise = vec4(vec3(rnd(uv*(1.0+progress))), transitionMix.a); // static where the image is
  

  return mix(transitionMix, noise, noiseEnvelope);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
