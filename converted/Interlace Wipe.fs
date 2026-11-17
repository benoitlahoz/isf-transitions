/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "The incoming image loads over the outgoing one like an interlaced picture on a slow connection: a first pass sweeps down drawing it in big blocky rows, then each new pass sweeps down again at twice the detail until the picture is sharp (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "rows",
            "NAME": "rows",
            "TYPE": "float",
            "DEFAULT": 120,
            "MIN": 24,
            "MAX": 480
        },
        {
            "LABEL": "passes",
            "NAME": "passes",
            "TYPE": "long",
            "DEFAULT": 5,
            "VALUES": [2, 3, 4, 5, 6],
            "LABELS": ["2", "3", "4", "5", "6"]
        },
        {
            "LABEL": "glow",
            "NAME": "glow",
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

vec4 transition(vec2 uv) {
  float lines = max(floor(rows), 1.0);
  float cols = max(floor(lines * ratio + 0.5), 1.0);
  // no more passes than fit: the first pass's blocks are never taller than the picture
  float n = max(min(clamp(float(passes), 2.0, 6.0), 1.0 + floor(log2(lines) + 0.01)), 2.0);
  float row = min(floor((1.0 - uv.y) * lines), lines - 1.0);
  float col = min(floor(uv.x * cols), cols - 1.0);

  float p = mix(progress, progress * progress * (3.0 - 2.0 * progress), 0.5);
  // each pass sweeps the whole frame in time T; the passes start one after another and overlap a little
  float T = 1.5 / (n + 0.5);

  // the finest pass that has drawn this pixel wins: try them from the finest (blocks 1 row tall) up
  float bestB = 0.0;
  float light = 0.0;
  for (int i = 0; i < 6; i++) {
    float k = n - 1.0 - float(i); // order of this pass, from 0 for the first one
    if (k < 0.0) break;
    float b = floor(exp2(float(i)) + 0.5); // block size of this pass, in pixel rows
    float u = clamp((p - (1.0 - T) * k / (n - 1.0)) / T, 0.0, 1.0);
    // the sweep, in block rows; the row it is on fills from left to right block by block.
    // Freshly drawn rows are brighter over a few rows behind it: the sweep runs on past the
    // bottom of the frame until that glow is gone, so the pass is over before its time ends
    float glowRows = max(1.5 * b, 0.05 * lines);
    float blockRows = ceil(lines / b);
    float sweep = u * (blockRows + glowRows / b);
    float behind = sweep - floor(row / b) - (floor(col / b) + 1.0) / ceil(cols / b);
    if (behind >= -1e-4) {
      bestB = b;
      light = 1.0 - smoothstep(0.0, glowRows, behind * b);
      break;
    }
  }

  if (bestB == 0.0) return getFromColor(uv);
  vec4 c;
  if (bestB <= 1.0) {
    c = getToColor(uv);
  } else {
    float top = floor(row / bestB) * bestB;
    float left = floor(col / bestB) * bestB;
    c = getToColor(vec2((left + 0.5) / cols, 1.0 - (top + 0.5) / lines));
  }
  c.rgb += 0.5 * glow * light * (1.0 - c.rgb);
  return c;
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
