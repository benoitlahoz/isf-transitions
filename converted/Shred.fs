/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "The outgoing image is cut into paper strips that drop away at uneven speeds, tilting as they fall, revealing the incoming image (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "strips",
            "NAME": "strips",
            "TYPE": "long",
            "DEFAULT": 20,
            "VALUES": [4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60],
            "LABELS": ["4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "60"]
        },
        {
            "LABEL": "stagger",
            "NAME": "stagger",
            "TYPE": "float",
            "DEFAULT": 0.4,
            "MIN": 0,
            "MAX": 0.8
        },
        {
            "LABEL": "tilt",
            "NAME": "tilt",
            "TYPE": "float",
            "DEFAULT": 0.1,
            "MIN": 0,
            "MAX": 0.3
        },
        {
            "LABEL": "shadow",
            "NAME": "shadow",
            "TYPE": "float",
            "DEFAULT": 0.5,
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

float hash (float n) {
  n = fract(n * 0.1031);
  n *= n + 33.33;
  n *= n + n;
  return fract(n);
}

vec4 transition (vec2 uv) {
  float n = float(strips);
  float w = ratio / n; // strip width, in frame heights
  vec2 q = vec2(uv.x * ratio, uv.y);
  vec4 color = getToColor(uv);
  float cover = -1.0;
  float shade = 0.0;
  // narrow strips tilt less, so that they only reach over their neighbours
  float maxTilt = min(tilt, 2.0 * w);
  for (int d = -3; d <= 3; d++) {
    float i = floor(uv.x * n) + float(d);
    if (i < 0.0 || i > n - 1.0) continue;
    // about one strip in five starts right away
    float delay = stagger * max(0.0, 1.25 * hash(i + 1.0) - 0.25);
    float speed = 1.0 + 0.5 * hash(i + 71.0);
    float t = clamp((progress - delay) / (1.0 - delay), 0.0, 1.0);
    float fall = (0.4 * t + 0.6 * t * t) * (1.0 + w * (0.5 + tilt)) * speed;
    float angle = maxTilt * (2.0 * hash(i + 37.0) - 1.0) * t;
    float lift = min(1.0, 8.0 * t);
    float gap = 0.06 * w * lift;
    vec2 v = q - vec2((i + 0.5) * w, 0.5 - fall);
    float ca = cos(angle), sa = sin(angle);
    vec2 l = vec2(ca * v.x + sa * v.y, ca * v.y - sa * v.x);
    vec2 edge = vec2(0.5 * w - gap, 0.5) - abs(l);
    vec2 so = abs(l - lift * vec2(0.15 * w, -0.25 * w)) - vec2(0.5 * w - gap, 0.5);
    shade = max(shade, lift * (1.0 - smoothstep(0.0, 0.5 * w, max(so.x, so.y))));
    if (min(edge.x, edge.y) >= 0.0 && fall > cover) {
      vec2 p = vec2((i + 0.5) * w + l.x, 0.5 + l.y);
      vec4 img = getFromColor(vec2(p.x / ratio, p.y));
      float cut = lift * (1.0 - smoothstep(0.0, 0.25 * w, edge.x));
      color = vec4(img.rgb * (1.0 - 0.5 * shadow * cut), img.a);
      cover = fall;
    }
  }
  if (cover < 0.0) {
    color = vec4(color.rgb * (1.0 - 0.6 * shadow * shade), color.a);
  }
  return color;
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
