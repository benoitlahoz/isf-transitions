/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "Handk - MIT",
    "DESCRIPTION": "Zooms into the outgoing image, then the incoming one wipes in from right to left (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "zoom_quickness",
            "NAME": "zoom_quickness",
            "TYPE": "float",
            "DEFAULT": 0.8,
            "MIN": 0,
            "MAX": 3.2
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

vec2 zoom(vec2 uv, float amount) {
  if(amount<0.5)
  return 0.5 + ((uv - 0.5) * (1.0-amount));
  else
  return 0.5 + ((uv - 0.5) * (amount));
  
}

vec4 transition (vec2 uv) {
  float nQuick = clamp(zoom_quickness,0.0,0.5);
  if(progress<0.5){
    vec4 c= mix(
      getFromColor(zoom(uv, smoothstep(0.0, nQuick, progress))),
      getToColor(uv),
     step(0.5, progress)
    );
    
    return c;
  }
  else{
    vec2 p=uv.xy/vec2(1.0).xy;
    vec4 d=getFromColor(p);
    vec4 e=getToColor(p);
    vec4 f= mix(d, e, step(1.0-p.x,(progress-0.5)*2.0));
    
    return f;
  }
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
