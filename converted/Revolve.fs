/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "bread - MIT",
    "DESCRIPTION": "The outgoing image swirls and zooms with motion blur into the incoming one (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "center",
            "NAME": "center",
            "TYPE": "point2D",
            "DEFAULT": [0.46, 0.52],
            "MIN": [-2.08, -2.08],
            "MAX": [2.08, 2.08]
        },
        {
            "LABEL": "direction",
            "NAME": "direction",
            "TYPE": "float",
            "DEFAULT": -1,
            "MIN": -4,
            "MAX": 4
        },
        {
            "LABEL": "maxRotation",
            "NAME": "maxRotation",
            "TYPE": "float",
            "DEFAULT": 1.95,
            "MIN": 0,
            "MAX": 7.8
        },
        {
            "LABEL": "peakZoom",
            "NAME": "peakZoom",
            "TYPE": "float",
            "DEFAULT": 2.22,
            "MIN": 0,
            "MAX": 8.88
        },
        {
            "LABEL": "swirl",
            "NAME": "swirl",
            "TYPE": "float",
            "DEFAULT": 2.85,
            "MIN": 0,
            "MAX": 11.4
        },
        {
            "LABEL": "barrel",
            "NAME": "barrel",
            "TYPE": "float",
            "DEFAULT": 0.38,
            "MIN": 0,
            "MAX": 1.52
        },
        {
            "LABEL": "motionBlur",
            "NAME": "motionBlur",
            "TYPE": "float",
            "DEFAULT": 1,
            "MIN": 0,
            "MAX": 4
        },
        {
            "LABEL": "switchStart",
            "NAME": "switchStart",
            "TYPE": "float",
            "DEFAULT": 0.3,
            "MIN": 0,
            "MAX": 1.2
        },
        {
            "LABEL": "switchEnd",
            "NAME": "switchEnd",
            "TYPE": "float",
            "DEFAULT": 0.5,
            "MIN": 0,
            "MAX": 2
        },
        {
            "LABEL": "shadow",
            "NAME": "shadow",
            "TYPE": "float",
            "DEFAULT": 0.16,
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

float sat(float x) {
  return clamp(x, 0.0, 1.0);
}

float ease(float x) {
  x = sat(x);
  return x * x * (3.0 - 2.0 * x);
}

float revolveEnvelope(float t) {
  float rise = ease((t - 0.10) / 0.33);
  float fall = 1.0 - ease((t - 0.43) / 0.29);
  return rise * fall;
}

vec2 rotate2(vec2 p, float a) {
  float s = sin(a);
  float c = cos(a);
  return vec2(c * p.x - s * p.y, s * p.x + c * p.y);
}

vec2 warpUv(vec2 uv, float t) {
  float e = revolveEnvelope(t);

  vec2 p = uv - center;
  p.x *= ratio;

  float r = length(p);
  float edgeSpin = maxRotation * e;
  float coreSpin = swirl * e * pow(1.0 - sat(r / 0.96), 1.55);
  float visibleAngle = direction * (edgeSpin + coreSpin);

  p = rotate2(p, -visibleAngle);

  float sc = 1.0 + (peakZoom - 1.0) * pow(e, 0.85);
  p /= sc;

  float rr = length(p);
  p *= 1.0 + barrel * e * rr * rr * 2.8;

  p.x /= ratio;
  return clamp(p + center, vec2(0.001), vec2(0.999));
}

vec4 sampleRevolve(vec2 uv, float t) {
  vec2 p = warpUv(uv, t);
  float reveal = smoothstep(switchStart, switchEnd, t);
  return mix(getFromColor(p), getToColor(p), reveal);
}

vec4 transition(vec2 uv) {
  float e = revolveEnvelope(progress);
  float span = 0.060 * motionBlur * e;

  vec4 color = vec4(0.0);
  float total = 0.0;

  for (int i = -8; i <= 8; i++) {
    float x = float(i) / 8.0;
    float w = 1.0 - abs(x);
    w = w * w + 0.01;

    float t = sat(progress + x * span);
    color += sampleRevolve(uv, t) * w;
    total += w;
  }

  color /= total;

  vec2 q = uv - vec2(0.5);
  q.x *= ratio;
  float vignette = 1.0 - shadow * e * smoothstep(0.35, 0.95, length(q));
  color.rgb *= vignette;

  return color;
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
