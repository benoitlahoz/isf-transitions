/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "Zeh Fernando - MIT",
    "DESCRIPTION": "The outgoing image melts down in uneven columns, like the Doom screen wipe (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "bars",
            "NAME": "bars",
            "TYPE": "long",
            "DEFAULT": 30,
            "VALUES": [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60],
            "LABELS": ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "60"]
        },
        {
            "LABEL": "amplitude",
            "NAME": "amplitude",
            "TYPE": "float",
            "DEFAULT": 2,
            "MIN": 0,
            "MAX": 8
        },
        {
            "LABEL": "noise",
            "NAME": "noise",
            "TYPE": "float",
            "DEFAULT": 0.1,
            "MIN": 0,
            "MAX": 1
        },
        {
            "LABEL": "frequency",
            "NAME": "frequency",
            "TYPE": "float",
            "DEFAULT": 0.5,
            "MIN": 0,
            "MAX": 2
        },
        {
            "LABEL": "dripScale",
            "NAME": "dripScale",
            "TYPE": "float",
            "DEFAULT": 0.5,
            "MIN": 0,
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

// Number of total bars/columns

// Multiplier for speed ratio. 0 = no variation when going down, higher = some elements go much faster

// Further variations in speed. 0 = no noise, 1 = super noisy (ignore frequency)

// Speed variation horizontally. the bigger the value, the shorter the waves

// How much the bars seem to "run" from the middle of the screen first (sticking to the sides). 0 = no drip, 1 = curved drip


float rand(int num) {
  return fract(mod(float(num) * 67123.313, 12.0) * sin(float(num) * 10.3) * cos(float(num)));
}

float wave(int num) {
  float fn = float(num) * frequency * 0.1 * float(bars);
  return cos(fn * 0.5) * cos(fn * 0.13) * sin((fn+10.0) * 0.3) / 2.0 + 0.5;
}

float drip(int num) {
  return sin(float(num) / float(bars - 1) * 3.141592) * dripScale;
}

float pos(int num) {
  return (noise == 0.0 ? wave(num) : mix(wave(num), rand(num), noise)) + (dripScale == 0.0 ? 0.0 : drip(num));
}

vec4 transition(vec2 uv) {
  int bar = int(uv.x * (float(bars)));
  float scale = 1.0 + pos(bar) * amplitude;
  float phase = progress * scale;
  float posY = uv.y / vec2(1.0).y;
  vec2 p;
  vec4 c;
  if (phase + posY < 1.0) {
    p = vec2(uv.x, uv.y + mix(0.0, vec2(1.0).y, phase)) / vec2(1.0).xy;
    c = getFromColor(p);
  } else {
    p = uv.xy / vec2(1.0).xy;
    c = getToColor(p);
  }

  return c;
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
