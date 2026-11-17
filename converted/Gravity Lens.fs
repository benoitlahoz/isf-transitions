/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "gre - MIT",
    "DESCRIPTION": "A black disk appears and grows, bending the outgoing image around it like a heavy mass bends light: the image is stretched into a bright ring at the disk's edge and swallowed until the frame is black, then the disk shrinks back, unbending the incoming image (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "center",
            "NAME": "center",
            "TYPE": "point2D",
            "DEFAULT": [0.5, 0.5],
            "MIN": [-2, -2],
            "MAX": [2, 2]
        },
        {
            "LABEL": "strength",
            "NAME": "strength",
            "TYPE": "float",
            "DEFAULT": 0.6,
            "MIN": 0,
            "MAX": 1.5
        },
        {
            "LABEL": "glow",
            "NAME": "glow",
            "TYPE": "float",
            "DEFAULT": 0.6,
            "MIN": 0,
            "MAX": 1
        },
        {
            "LABEL": "hole",
            "NAME": "hole",
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

// Center of the disk

// how far past the farthest corner the disk grows, so the frame is fully covered for a brief moment
const float OVERSHOOT = 0.12;
// moment the frame is black: the swallow takes a little longer than the release
const float PEAK = 0.55;
const float AA = 0.002;
const vec3 WARM = vec3(1.0, 0.86, 0.68);

// fold the coordinates back into the frame, so the image stays filled without smearing its border
vec2 mirrored(vec2 v) {
  vec2 m = mod(v, 2.0);
  return mix(m, 2.0 - m, step(1.0, m));
}

vec4 transition(vec2 uv) {
  // the disk grows with an accelerating swallow, then shrinks back a little faster: exactly 0 at both ends
  bool swallowing = progress < PEAK;
  float u = swallowing ? progress / PEAK : (1.0 - progress) / (1.0 - PEAK);
  float u2 = u * u;
  float k = u2 * (0.6 + 0.4 * u2);

  vec2 aspect = vec2(ratio, 1.0);
  vec2 far = max(center, 1.0 - center) * aspect;
  float rh = length(far) * (1.0 + OVERSHOOT) * k;
  // radius where the light coming from right behind the disk shows up as a ring
  float re = rh * (1.0 + max(strength, 0.0));

  vec2 d = (uv - center) * aspect;
  float r = max(length(d), 1e-5);
  // a point mass deflects the light by an angle falling off as 1/r
  vec2 q = mirrored(center + d * (1.0 - re * re / (r * r)) / aspect);
  vec4 c = swallowing ? getFromColor(q) : getToColor(q);
  // the shading fades in with the disk, so it is exactly 0 at both ends
  float amount = smoothstep(0.0, 0.04, rh);
  float inner = clamp((r - rh) / (re - rh + 1e-4), 0.0, 1.0);
  c.rgb *= 1.0 - 0.7 * amount * (1.0 - inner * inner * (3.0 - 2.0 * inner));

  float g = max(glow, 0.0) * amount;
  float einstein = (r - re) / (0.3 * (re - rh) + 0.01);
  float photon = max(r - rh, 0.0) / (0.006 + 0.012 * rh);
  float light = g * (0.7 * exp(-einstein * einstein) + 1.6 * exp(-photon * photon));
  c.rgb = min(c.rgb * (1.0 + light) + WARM * (0.35 * light * c.a), vec3(1.0));

  // antialiased horizon, kept inside the disk so it vanishes with it
  float cov = clamp((rh - r) / (2.0 * AA), 0.0, 1.0);
  return mix(c, hole, cov);
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
