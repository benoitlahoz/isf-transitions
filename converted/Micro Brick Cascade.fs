/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "Gilad Levi - MIT",
    "DESCRIPTION": "The outgoing image fractures into a dense wall of tiny square tiles. A diagonal cascade releases the tiles in staggered waves, causing them to fall and drift aside while the incoming image is revealed beneath. (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "tileSize",
            "NAME": "tileSize",
            "TYPE": "float",
            "DEFAULT": 0.018,
            "MIN": 0.008,
            "MAX": 0.06
        },
        {
            "LABEL": "fall",
            "NAME": "fall",
            "TYPE": "float",
            "DEFAULT": 1.1,
            "MIN": 0,
            "MAX": 2
        },
        {
            "LABEL": "drift",
            "NAME": "drift",
            "TYPE": "float",
            "DEFAULT": 0.35,
            "MIN": 0,
            "MAX": 1.5
        },
        {
            "LABEL": "diagonal",
            "NAME": "diagonal",
            "TYPE": "float",
            "DEFAULT": 0.82,
            "MIN": 0,
            "MAX": 1
        },
        {
            "LABEL": "stagger",
            "NAME": "stagger",
            "TYPE": "float",
            "DEFAULT": 0.45,
            "MIN": 0,
            "MAX": 1
        },
        {
            "LABEL": "softness",
            "NAME": "softness",
            "TYPE": "float",
            "DEFAULT": 0.22,
            "MIN": 0,
            "MAX": 1
        },
        {
            "LABEL": "glow",
            "NAME": "glow",
            "TYPE": "float",
            "DEFAULT": 0.45,
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

float hash12(vec2 p) {
  return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453123);
}

vec4 transition(vec2 uv) {
  float s = max(tileSize, 0.004);
  vec2 gridCount = max(vec2(1.0), floor(vec2(ratio, 1.0) / s));
  vec2 tile = floor(uv * gridCount);
  vec2 tileUV = fract(uv * gridCount);
  vec2 tileCenter = (tile + 0.5) / gridCount;

  float waveCoord = mix(
    tileCenter.x,
    0.65 * tileCenter.x + 0.35 * (1.0 - tileCenter.y),
    clamp(diagonal, 0.0, 1.0)
  );

  // random stagger, so that the wall does not break apart too uniformly
  float r1 = hash12(tile);
  float r2 = hash12(tile + vec2(19.1, 7.3));
  float r3 = hash12(tile + vec2(41.7, 13.9));

  float start = 0.08 + 0.60 * waveCoord + stagger * 0.12 * (r1 - 0.5);
  float t = clamp((progress - start) / 0.28, 0.0, 1.0);

  float sideways = (r2 * 2.0 - 1.0) * drift * t;
  float downward = fall * t * t;

  vec2 movedTileUV = tileUV - vec2(sideways, -downward);

  float inside =
    step(0.0, movedTileUV.x) *
    step(movedTileUV.x, 1.0) *
    step(0.0, movedTileUV.y) *
    step(movedTileUV.y, 1.0);

  float fadeStart = mix(0.40, 0.15, softness);
  float fadeEnd   = mix(0.95, 0.65, softness);
  float visibility = inside * (1.0 - smoothstep(fadeStart, fadeEnd, t));

  // the tile carries its image fragment with it
  vec2 shiftedUV = uv - vec2(sideways, -downward) / gridCount;
  vec4 fromColor = getFromColor(clamp(shiftedUV, 0.0, 1.0));

  vec4 toColor = getToColor(uv);

  vec4 color = mix(toColor, fromColor, visibility);

  float edgeDistance = min(
    min(movedTileUV.x, 1.0 - movedTileUV.x),
    min(movedTileUV.y, 1.0 - movedTileUV.y)
  );

  float edge = (1.0 - smoothstep(0.0, 0.08, edgeDistance)) * inside;
  float sparkle = sin(t * 3.14159265) * glow * visibility;

  vec3 edgeColor = mix(
    vec3(0.12, 0.90, 1.0),
    vec3(1.0, 0.35, 0.65),
    r3
  );

  color.rgb += edgeColor * edge * sparkle * 0.45;
  color.rgb = clamp(color.rgb, 0.0, 1.0);

  return color;
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
