/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "The frame is cut into a grid of blocks that slide out one after another in a staggered cascade, some of them shrinking as they go, while the blocks of the incoming image slide in behind them to take their place (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "grid",
            "NAME": "grid_isf",
            "TYPE": "point2D",
            "DEFAULT": [5, 4],
            "MIN": [0, 0],
            "MAX": [10, 10]
        },
        {
            "LABEL": "direction",
            "NAME": "direction",
            "TYPE": "point2D",
            "DEFAULT": [1, 0],
            "MIN": [-4, -4],
            "MAX": [4, 4]
        },
        {
            "LABEL": "stagger",
            "NAME": "stagger",
            "TYPE": "float",
            "DEFAULT": 0.6,
            "MIN": 0,
            "MAX": 0.95
        },
        {
            "LABEL": "zoom",
            "NAME": "zoom",
            "TYPE": "float",
            "DEFAULT": 0.35,
            "MIN": 0,
            "MAX": 0.9
        },
        {
            "LABEL": "background",
            "NAME": "background",
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

#define grid ivec2(grid_isf)

const int MAX_BLOCKS = 40;

float hash(vec2 p) {
  vec3 p3 = fract(vec3(p.xyx) * 0.1031);
  p3 += dot(p3, p3.yzx + 33.33);
  return fract((p3.x + p3.y) * p3.z);
}

vec4 transition(vec2 uv) {
  bool vertical = abs(direction.y) > abs(direction.x);
  bool backward = vertical ? direction.y < 0.0 : direction.x < 0.0;
  vec2 g = max(vec2(grid), 1.0);
  // a: position along the motion, toward which the blocks slide, b: across it
  float a = vertical ? uv.y : uv.x;
  float b = vertical ? uv.x : uv.y;
  if (backward) a = 1.0 - a;
  float n = min(vertical ? g.y : g.x, float(MAX_BLOCKS));
  float strips = vertical ? g.x : g.y;
  // every block of a strip slides along it, so a point can only show a block of its own strip
  float r = min(floor(b * strips), strips - 1.0);
  float rowOrder = strips > 1.0 ? (vertical ? r : strips - 1.0 - r) / (strips - 1.0) : 0.0;
  float st = clamp(stagger, 0.0, 0.95);
  float z = clamp(zoom, 0.0, 0.9);
  float an = a * n;
  float bn = b * strips - r;

  // the incoming blocks follow the last outgoing block to leave, the rear one: they all
  // stay behind it, so no block ever overlaps another, and each one gets closer to the
  // block ahead of it until they all arrive together
  float rear = clamp((progress - st * mix(rowOrder, 1.0, 0.2)) / (1.0 - st), 0.0, 1.0);
  rear = rear * rear * (3.0 - 2.0 * rear);

  for (int k = 0; k < MAX_BLOCKS; k++) {
    float i = float(k);
    if (i >= n) break;
    // the block at the front sets off first, so that the outgoing blocks move apart
    float order = mix(rowOrder, n > 1.0 ? (n - 1.0 - i) / (n - 1.0) : 0.0, 0.2);
    float p = clamp((progress - st * order) / (1.0 - st), 0.0, 1.0);
    p = p * p * (3.0 - 2.0 * p);
    float pin = max(0.0, 1.0 - (1.0 - rear) * (1.0 + (n - 1.0 - i) / n));
    float zi = hash(vec2(i, r) + 3.7) < 0.5 ? z : 0.0;
    // position in the block from its corner: outgoing (xy) leaves over a frame's length, incoming (zw) starts one length behind
    float sFrom = 1.0 - zi * p * p;
    float sTo = 1.0 - zi * (1.0 - pin) * (1.0 - pin);
    vec4 l = vec4(
      (an - i - p * n - 0.5 * (1.0 - sFrom)) / sFrom,
      (bn - 0.5 * (1.0 - sFrom)) / sFrom,
      (an - i - (pin - 1.0) * n - 0.5 * (1.0 - sTo)) / sTo,
      (bn - 0.5 * (1.0 - sTo)) / sTo);
    bool inFrom = all(greaterThanEqual(l.xy, vec2(0.0))) && all(lessThan(l.xy, vec2(1.0)));
    bool inTo = all(greaterThanEqual(l.zw, vec2(0.0))) && all(lessThan(l.zw, vec2(1.0)));
    if (inFrom || inTo) {
      vec2 q = (vec2(i, r) + (inTo ? l.zw : l.xy)) / vec2(n, strips);
      if (backward) q.x = 1.0 - q.x;
      q = vertical ? q.yx : q;
      return inTo ? getToColor(q) : getFromColor(q);
    }
  }
  return background;
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
