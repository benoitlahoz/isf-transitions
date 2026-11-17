/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "pthrasher - MIT",
    "DESCRIPTION": "A circle with a wavy edge burns out from the center to reveal the incoming image (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "smoothness",
            "NAME": "smoothness",
            "TYPE": "float",
            "DEFAULT": 0.03,
            "MIN": 0,
            "MAX": 1
        },
        {
            "LABEL": "center",
            "NAME": "center",
            "TYPE": "point2D",
            "DEFAULT": [0.5, 0.5],
            "MIN": [-2, -2],
            "MAX": [2, 2]
        },
        {
            "LABEL": "color",
            "NAME": "color",
            "TYPE": "color",
            "DEFAULT": [0, 0, 0, 1],
            "MIN": [0, 0, 0, 0],
            "MAX": [1, 1, 1, 1]
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

// adapted by gre from https://gist.github.com/pthrasher/8e6226b215548ba12734


const float M_PI = 3.14159265358979323846;

float quadraticInOut(float t) {
  float p = 2.0 * t * t;
  return t < 0.5 ? p : -p + (4.0 * t) - 1.0;
}

float getGradient(float r, float dist) {
  float d = r - dist;
  return mix(
    smoothstep(-smoothness, 0.0, r - dist * (1.0 + smoothness)),
    -1.0 - step(0.005, d),
    step(-0.005, d) * step(d, 0.01)
  );
}

float getWave(vec2 p){
  vec2 _p = p - center;
  float rads = atan(_p.y, _p.x);
  float degs = degrees(rads) + 180.0;
  vec2 range = vec2(0.0, M_PI * 30.0);
  vec2 domain = vec2(0.0, 360.0);
  float ratio = (M_PI * 30.0) / 360.0;
  degs = degs * ratio;
  float x = progress;
  float magnitude = mix(0.02, 0.09, smoothstep(0.0, 1.0, x));
  float offset = mix(40.0, 30.0, smoothstep(0.0, 1.0, x));
  float ease_degs = quadraticInOut(sin(degs));
  float deg_wave_pos = (ease_degs * magnitude) * sin(x * offset);
  // The soft edge reaches `smoothness` beyond the radius and the burn band 0.005 inside it:
  // start the radius below both so nothing but the outgoing image shows at progress 0
  // (unchanged from progress 0.1 on).
  return x - max(smoothness, 0.01) * (1.0 - smoothstep(0.0, 0.1, x)) + deg_wave_pos;
}

vec4 transition(vec2 p) {
  float dist = distance(center, p);
  float m = getGradient(getWave(p), dist);
  vec4 cfrom = getFromColor(p);
  vec4 cto = getToColor(p);
  return mix(mix(cfrom, cto, m), mix(cfrom, color, 0.75), step(m, -2.0));
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
