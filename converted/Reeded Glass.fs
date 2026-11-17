/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "A wide pane of reeded glass slides across the frame: behind its rounded vertical ribs the picture breaks into narrow, flipped and squeezed slices with faint color fringes, the outgoing image on the far side of the pane and the incoming one behind it, and where the pane has passed the incoming image is left clear (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "paneWidth",
            "NAME": "paneWidth",
            "TYPE": "float",
            "DEFAULT": 0.55,
            "MIN": 0.1,
            "MAX": 1
        },
        {
            "LABEL": "ribs",
            "NAME": "ribs",
            "TYPE": "float",
            "DEFAULT": 20,
            "MIN": 4,
            "MAX": 60
        },
        {
            "LABEL": "strength",
            "NAME": "strength",
            "TYPE": "float",
            "DEFAULT": 1,
            "MIN": 0,
            "MAX": 2
        },
        {
            "LABEL": "shine",
            "NAME": "shine",
            "TYPE": "float",
            "DEFAULT": 0.6,
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

const float AA = 0.002;
// width of the pane's shadow, in frame heights
const float SHADOW = 0.03;

vec2 mirror(vec2 v) {
  return 1.0 - abs(1.0 - mod(v, 2.0));
}

vec4 behind(float x, float y, float cut, float soft) {
  vec2 p = mirror(vec2(x, y));
  return mix(getToColor(p), getFromColor(p), smoothstep(cut - soft, cut + soft, x));
}

vec4 transition(vec2 uv) {
  float pw = clamp(paneWidth, 0.05, 1.0);
  float t = mix(progress, progress * progress * (3.0 - 2.0 * progress), 0.6);
  // the pane starts and ends far enough off frame that its shadow is off frame too
  float margin = SHADOW / ratio + 0.005;
  float left = mix(-pw - margin, 1.0 + margin, t);
  float right = left + pw;
  float cut = left + 0.5 * pw;
  float x = uv.x;

  float rw = pw / max(floor(pw * ribs + 0.5), 1.0);
  float r = (x - left) / rw;
  float u = 2.0 * fract(r) - 1.0;
  // strength 0 leaves the image as is; from about 0.25 each rib flips the slice behind it
  float bend = rw * strength * (0.5 * u + 1.6 * u * sqrt(1.0 - 0.6 * u * u));
  float soft = 0.5 * rw;
  float disp = 0.05 * strength;
  vec4 g = behind(x - bend, uv.y, cut, soft);
  vec4 c = vec4(
    behind(x - bend * (1.0 + disp), uv.y, cut, soft).r,
    g.g,
    behind(x - bend * (1.0 - disp), uv.y, cut, soft).b,
    g.a
  );

  float seam = smoothstep(0.82, 1.0, abs(u));
  float hl = (u + 0.45) / 0.16;
  float rim = 1.0 - smoothstep(0.0, 0.006, min(x - left, right - x) * ratio);
  vec3 lit = c.rgb * (1.0 - shine * (0.08 * u + 0.45 * seam + 0.4 * rim));
  lit += shine * (0.22 * exp(-hl * hl) + 0.05) * c.a;
  c.rgb = mix(lit, vec3(0.93, 0.96, 0.97) * c.a, 0.05 * shine);

  float inside = smoothstep(left - AA, left + AA, x) * (1.0 - smoothstep(right - AA, right + AA, x));
  vec4 clear = x < cut ? getToColor(uv) : getFromColor(uv);
  float shadow = 0.2 * shine * smoothstep(right - AA, right + AA, x) * (1.0 - smoothstep(0.0, SHADOW, (x - right) * ratio));
  clear.rgb *= 1.0 - shadow;
  return mix(clear, c, inside);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
