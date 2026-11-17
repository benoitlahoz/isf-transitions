/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "Gilad Levi, github @Giladx - MIT",
    "DESCRIPTION": "A diagonal shockwave breaks the outgoing image into tiny falling glass tiles. Each tile slips away with a brief chromatic split and neon edge glow, revealing the incoming image beneath it. (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "DEFAULT": 0.035,
            "MIN": 0.012,
            "MAX": 0.1
        },
        {
            "LABEL": "scatter",
            "NAME": "scatter",
            "TYPE": "float",
            "DEFAULT": 0.85,
            "MIN": 0,
            "MAX": 1.5
        },
        {
            "LABEL": "diagonal",
            "NAME": "diagonal",
            "TYPE": "float",
            "DEFAULT": 0.9,
            "MIN": 0,
            "MAX": 1
        },
        {
            "LABEL": "prism",
            "NAME": "prism",
            "TYPE": "float",
            "DEFAULT": 0.65,
            "MIN": 0,
            "MAX": 1
        },
        {
            "LABEL": "glow",
            "NAME": "glow",
            "TYPE": "float",
            "DEFAULT": 0.75,
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

float toseeHash(vec2 p) {
   return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453123);
 }

 vec4 transition(vec2 uv) {
   float size = max(tileSize, 0.008);
   vec2 grid = max(vec2(1.0), floor(vec2(ratio, 1.0) / size));
   vec2 cell = floor(uv * grid);
   vec2 cellUV = fract(uv * grid);
   vec2 centerUV = (cell + 0.5) / grid;

   float sweep = mix(
     centerUV.x,
     0.64 * centerUV.x + 0.36 * (1.0 - centerUV.y),
     clamp(diagonal, 0.0, 1.0)
   );

   float noise = toseeHash(cell);
   float start = 0.62 * sweep + 0.07 * noise;
   float t = clamp((progress - start) / 0.29, 0.0, 1.0);
   float movement = t * clamp(scatter, 0.0, 1.5);

   float wobble = toseeHash(cell + vec2(19.3, 7.7)) * 2.0 - 1.0;
   vec2 drift = vec2(0.65 * wobble, -0.95) * movement;
   vec2 movingCellUV = cellUV - drift;

   float inside =
     step(0.012, movingCellUV.x) *
     step(movingCellUV.x, 0.988) *
     step(0.012, movingCellUV.y) *
     step(movingCellUV.y, 0.988);

   float tileShape = mix(1.0, inside, smoothstep(0.0, 0.13, t));
   float oldVisibility = 1.0 - smoothstep(0.48, 0.87, t);
   float oldMask = tileShape * oldVisibility;

   vec2 oldUV = clamp(uv - drift / grid, 0.0, 1.0);
   vec4 oldColor = getFromColor(oldUV);

   float flare = sin(3.14159265 * t);
   vec2 shift = vec2(0.006, -0.002) * prism * flare;
   vec4 offsetColor = getFromColor(
     clamp(oldUV + shift, 0.0, 1.0)
   );
   oldColor.r = offsetColor.r;

   vec4 result = mix(getToColor(uv), oldColor, oldMask);

   float edgeDistance = min(
     min(movingCellUV.x, 1.0 - movingCellUV.x),
     min(movingCellUV.y, 1.0 - movingCellUV.y)
   );

   float edge =
     (1.0 - smoothstep(0.0, 0.10, edgeDistance)) * inside;

   vec3 cyan = vec3(0.12, 0.95, 1.0);
   vec3 pink = vec3(1.0, 0.20, 0.65);

   vec3 rimColor = mix(
     cyan,
     pink,
     toseeHash(cell + vec2(5.4, 28.6))
   );

   result.rgb = clamp(
     result.rgb + rimColor * edge * flare * glow * oldMask * 0.60,
     0.0,
     1.0
   );

   return result;
 }

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
