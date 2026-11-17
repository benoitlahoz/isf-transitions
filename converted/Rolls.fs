/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "Mark Craig - MIT",
    "DESCRIPTION": "The outgoing image rolls away around a corner, revealing the incoming one (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "type",
            "NAME": "type",
            "TYPE": "long",
            "DEFAULT": 0,
            "VALUES": [0, 1, 2, 3],
            "LABELS": ["0", "1", "2", "3"]
        },
        {
            "LABEL": "RotDown",
            "NAME": "RotDown",
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

// mrmcsoftware on github and youtube ( http://www.youtube.com/MrMcSoftware )

// Rolls Transition by Mark Craig (Copyright © 2022)

// type (0-3): Rotate/Roll from which corner
// RotDown: if true rotate old image down, otherwise rotate old image up

#define M_PI 3.14159265358979323846

vec4 transition(vec2 uv)
{
float theta, c1, s1;
vec2 iResolution = vec2(ratio, 1.0);
vec2 uvi;
// I used if/else instead of switch in case it's an old GPU
if (type == 0) { theta = (RotDown ? M_PI : -M_PI) / 2.0 * progress; uvi.x = 1.0 - uv.x; uvi.y = uv.y; }
else if (type == 1) { theta = (RotDown ? M_PI : -M_PI) / 2.0 * progress; uvi = uv; }
else if (type == 2) { theta = (RotDown ? -M_PI : M_PI) / 2.0 * progress; uvi.x = uv.x; uvi.y = 1.0 - uv.y; }
else if (type == 3) { theta = (RotDown ? -M_PI : M_PI) / 2.0 * progress; uvi = 1.0 - uv; }
c1 = cos(theta); s1 = sin(theta);
vec2 uv2;
uv2.x = (uvi.x * iResolution.x * c1 - uvi.y * iResolution.y * s1);
uv2.y = (uvi.x * iResolution.x * s1 + uvi.y * iResolution.y * c1);
if ((uv2.x >= 0.0) && (uv2.x <= iResolution.x) && (uv2.y >= 0.0) && (uv2.y <= iResolution.y))
	{
	uv2 /= iResolution;
	if (type == 0) { uv2.x = 1.0 - uv2.x; }
	else if (type == 2) { uv2.y = 1.0 - uv2.y; }
	else if (type == 3) { uv2 = 1.0 - uv2; }
	return(getFromColor(uv2));
	}
return(getToColor(uv));
}

void main() {
  gl_FragColor = transition(isf_FragNormCoord);
}
