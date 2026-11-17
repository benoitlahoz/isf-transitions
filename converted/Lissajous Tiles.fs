/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "Boundless <info@boundless-beta.com> - MIT",
    "DESCRIPTION": "The image breaks into a grid of tiles that swirl along Lissajous curves, in a Scanimate style, and flip one by one to the incoming image before settling back (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "DEFAULT": 0.5,
            "MIN": 0,
            "MAX": 2
        },
        {
            "LABEL": "freq",
            "NAME": "freq",
            "TYPE": "point2D",
            "DEFAULT": [2, 3],
            "MIN": [-12, -12],
            "MAX": [12, 12]
        },
        {
            "LABEL": "offset",
            "NAME": "offset",
            "TYPE": "float",
            "DEFAULT": 2,
            "MIN": 0,
            "MAX": 8
        },
        {
            "LABEL": "zoom",
            "NAME": "zoom",
            "TYPE": "float",
            "DEFAULT": 0.8,
            "MIN": 0,
            "MAX": 2
        },
        {
            "LABEL": "fade",
            "NAME": "fade",
            "TYPE": "float",
            "DEFAULT": 3,
            "MIN": 0,
            "MAX": 10
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

const vec2 grid = vec2(10.,10.);
const float TAU = 6.283185307;

vec4 transition (vec2 uv) {
  // The tiles leave and come back with zero speed, so the image does not break apart in the first frame
  float p = 1.-pow(1.-smoothstep(0.,1.,1.-abs(1.-2.*progress)),3.);
  // Find the last tile that covers this pixel (looping backwards, the first hit), then sample the images once
  for (float jBig = grid.y-1.; jBig >= 0.; jBig--) {
    for (float iBig = grid.x-1.; iBig >= 0.; iBig--) {
      float i = iBig/grid.x;
      float j = jBig/grid.y;
      float k = i/grid.y+j; // order of the tile along the curve
      vec2 uv0 = uv + vec2(i,j) - 0.5 + 0.5/grid + vec2(cos(k*TAU*freq.x+progress*6.*speed),sin(k*TAU*freq.y+progress*6.*(1.+offset)*speed))*zoom/2.;
      uv0 = mix(uv, uv0, p);
      if (uv0.x >= i && uv0.x < i+1./grid.x && uv0.y >= j && uv0.y < j+1./grid.y) {
        float tileMix = clamp(progress*((1.+fade)*2.*progress)-fade+k*fade, 0., 1.);
        return mix(getFromColor(uv0), getToColor(uv0), tileMix);
      }
    }
  }
  return vec4(0.); // between the tiles: transparent
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
