/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "The camera flies down a tunnel lined with the outgoing image toward the incoming one, which opens at its far end and grows to fill the frame (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "DEFAULT": 6,
            "MIN": 0.5,
            "MAX": 10
        },
        {
            "LABEL": "twist",
            "NAME": "twist",
            "TYPE": "float",
            "DEFAULT": 0.5,
            "MIN": -2,
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

vec2 rotate(vec2 v, float a) {
  float c = cos(a);
  float s = sin(a);
  return vec2(c * v.x - s * v.y, s * v.x + c * v.y);
}

vec4 transition(vec2 uv) {
  vec2 aspect = vec2(ratio, 1.0);
  vec2 c = (uv - 0.5) * aspect;
  float r = length(c);
  // A tube as wide as the frame's corners runs from the camera to an end wall
  // at depth `far`; the outgoing image is projected onto it from the starting
  // point of view, so it looks flat at first. The camera flies to one unit
  // before the end wall, where the incoming image hangs.
  float R = length(0.5 * aspect);
  float far = 1.0 + speed;
  // the distance to the end wall shrinks geometrically, so the end grows evenly
  float e = mix(progress, progress * progress * (3.0 - 2.0 * progress), 0.7);
  float d = pow(far, 1.0 - e);
  float t = far - d; // distance flown
  float k = r * d < R ? d / far : R / (R + t * r); // image scale at that point
  float a = twist * min(t * r / R, t / d); // spiral, continuous at the end wall
  vec4 walls = getFromColor(0.5 + rotate(c * k, a) / aspect);
  walls.rgb *= 0.4 + 0.6 * sqrt(k); // darker where the walls are close
  float open = smoothstep(0.0, 0.6, progress);
  vec2 q = 0.5 + (uv - 0.5) * d / max(open, 1e-4);
  vec2 inside = step(0.0, q) * step(q, vec2(1.0));
  return mix(walls, getToColor(q), inside.x * inside.y * step(r * d, R));
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
