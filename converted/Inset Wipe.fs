/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "A rectangle anchored in a corner, on an edge or in the center grows until the incoming image fills the frame, revealing it in place without scaling it; in reverse, the outgoing image shrinks into the rectangle (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "anchor",
            "NAME": "anchor",
            "TYPE": "point2D",
            "DEFAULT": [1, 1],
            "MIN": [-4, -4],
            "MAX": [4, 4]
        },
        {
            "LABEL": "smoothness",
            "NAME": "smoothness",
            "TYPE": "float",
            "DEFAULT": 0,
            "MIN": 0,
            "MAX": 0.2
        },
        {
            "LABEL": "borderWidth",
            "NAME": "borderWidth",
            "TYPE": "float",
            "DEFAULT": 0,
            "MIN": 0,
            "MAX": 0.1
        },
        {
            "LABEL": "borderColor",
            "NAME": "borderColor",
            "TYPE": "color",
            "DEFAULT": [1, 1, 1, 1],
            "MIN": [0, 0, 0, 0],
            "MAX": [1, 1, 1, 1]
        },
        {
            "LABEL": "reverse",
            "NAME": "reverse",
            "TYPE": "bool",
            "DEFAULT": false
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

const float AA = 0.001;

vec4 premultiplied(vec4 c) {
  return vec4(c.rgb * c.a, c.a);
}

vec4 transition(vec2 uv) {
  vec4 a = getFromColor(uv);
  vec4 b = getToColor(uv);

  vec2 an = clamp(anchor, 0.0, 1.0);
  vec2 sq = vec2(ratio, 1.0);
  // the rectangle keeps the proportions that make it reach every far side of the frame
  // at once; measured in square pixels so its edge and border have an even width
  vec2 ext = max(an, 1.0 - an) * sq;
  vec2 d = abs(uv - an) * sq;
  float s = max(smoothness, AA);
  float bw = max(borderWidth, 0.0);

  // eased growth, from a rectangle whose border is not yet in the frame to one whose
  // soft edge has left it on every side
  float t = reverse ? 1.0 - progress : progress;
  float p = mix(t, t * t * (3.0 - 2.0 * t), 0.3);
  vec2 size = p * ext + mix(-(bw + s), s, p);
  // signed distance to the rectangle's edge, square-cornered, < 0 inside
  float e = max(d.x - size.x, d.y - size.y);

  vec4 inside = reverse ? a : b;
  vec4 c = premultiplied(reverse ? b : a);
  if (bw > 0.0) {
    // laid over the image outside the rectangle, so a translucent border tints it
    float m = 1.0 - smoothstep(bw - s, bw + s, e);
    c = premultiplied(borderColor) * m + c * (1.0 - borderColor.a * m);
  }
  c = mix(c, premultiplied(inside), 1.0 - smoothstep(-s, s, e));
  return c.a > 0.0 ? vec4(c.rgb / c.a, c.a) : vec4(0.0);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
