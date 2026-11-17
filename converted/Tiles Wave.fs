/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "numb3r23 - MIT",
    "DESCRIPTION": "The incoming image is revealed tile by tile in a diagonal wave (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "tileCount",
            "NAME": "tileCount_isf",
            "TYPE": "point2D",
            "DEFAULT": [8, 8],
            "MIN": [0, 0],
            "MAX": [16, 16]
        },
        {
            "LABEL": "flipX",
            "NAME": "flipX",
            "TYPE": "bool",
            "DEFAULT": true
        },
        {
            "LABEL": "flipY",
            "NAME": "flipY",
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

#define tileCount ivec2(tileCount_isf)

// Ported from https://gist.github.com/numb3r23/169781bb76f310e2bfde


vec4 transition(vec2 uv) {
  vec2 tileSize = 1.0 / vec2(tileCount);
  vec2 posInTile = fract(uv * vec2(tileCount));
  vec2 tileNum = floor(uv * vec2(tileCount));
  float countTiles = float(tileCount.x * tileCount.y);

  // Diagonal wave from bottom-left to top-right
  float offset = (tileNum.y + tileNum.x * float(tileCount.y)) / countTiles;
  float timeOffset = clamp((progress - offset) * countTiles, 0.0, 0.5);
  float sinTime = 1.0 - abs(cos(fract(timeOffset) * 3.1415926));

  vec2 texC = posInTile;

  if (sinTime <= 0.5) {
    if (flipX) {
      if (texC.x < sinTime || texC.x > 1.0 - sinTime)
        return getFromColor(uv);
      texC.x = texC.x < 0.5
        ? (texC.x - sinTime) * 0.5 / (0.5 - sinTime)
        : (texC.x - 0.5) * 0.5 / (0.5 - sinTime) + 0.5;
    }
    if (flipY) {
      if (texC.y < sinTime || texC.y > 1.0 - sinTime)
        return getFromColor(uv);
      texC.y = texC.y < 0.5
        ? (texC.y - sinTime) * 0.5 / (0.5 - sinTime)
        : (texC.y - 0.5) * 0.5 / (0.5 - sinTime) + 0.5;
    }
    vec2 globalUV = tileNum * tileSize + texC * tileSize;
    return getFromColor(globalUV);
  } else {
    if (flipX) {
      if (texC.x > sinTime || texC.x < 1.0 - sinTime)
        return getToColor(uv);
      texC.x = texC.x < 0.5
        ? (texC.x - sinTime) * 0.5 / (0.5 - sinTime)
        : (texC.x - 0.5) * 0.5 / (0.5 - sinTime) + 0.5;
      texC.x = 1.0 - texC.x;
    }
    if (flipY) {
      if (texC.y > sinTime || texC.y < 1.0 - sinTime)
        return getToColor(uv);
      texC.y = texC.y < 0.5
        ? (texC.y - sinTime) * 0.5 / (0.5 - sinTime)
        : (texC.y - 0.5) * 0.5 / (0.5 - sinTime) + 0.5;
      texC.y = 1.0 - texC.y;
    }
    vec2 globalUV = tileNum * tileSize + texC * tileSize;
    return getToColor(globalUV);
  }
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
