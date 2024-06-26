/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "Woohyun Kim",
    "DESCRIPTION": "EdgeTransition - License MIT (Automatically converted by @benoitlahoz 'isf-transitions').",
    "INPUTS": [
        {
            "DEFAULT": false,
            "LABEL": "Flip",
            "NAME": "flip",
            "TYPE": "bool"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Progress",
            "MAX": 1,
            "MIN": 0,
            "NAME": "progress",
            "TYPE": "float"
        },
        {
            "LABEL": "From",
            "NAME": "inputImage",
            "TYPE": "image"
        },
        {
            "LABEL": "To",
            "NAME": "to",
            "TYPE": "image"
        },
        {
            "LABEL": "edge_thickness",
            "NAME": "edge_thickness",
            "DEFAULT": 0.001,
            "TYPE": "float",
            "MAX": 1000,
            "MIN": 0
        },
        {
            "LABEL": "edge_brightness",
            "NAME": "edge_brightness",
            "DEFAULT": 8.0,
            "TYPE": "float",
            "MAX": 1000,
            "MIN": 0
        }
    ],
    "ISFVSN": "2"
}
*/
float ratio = RENDERSIZE.x / RENDERSIZE.y;

vec4 getFromColor(vec2 uv) {
  if (flip == true) {
    return IMG_NORM_PIXEL(inputImage, vec2(uv.x, 1. - uv.y));
  } else {
    return IMG_NORM_PIXEL(inputImage, uv);
  }
}

vec4 getToColor(vec2 uv) {
  return IMG_NORM_PIXEL(to, uv);
}

// Author: Woohyun Kim
// License: MIT


vec4 detectEdgeColor(vec3[9] c) {
  /* adjacent texel array for texel c[4]
    036
    147
    258
  */
  vec3 dx = 2.0 * abs(c[7]-c[1]) + abs(c[2] - c[6]) + abs(c[8] - c[0]);
	vec3 dy = 2.0 * abs(c[3]-c[5]) + abs(c[6] - c[8]) + abs(c[0] - c[2]);
  float delta = length(0.25 * (dx + dy) * 0.5);
	return vec4(clamp(edge_brightness * delta, 0.0, 1.0) * c[4], 1.0);
}

vec4 getFromEdgeColor(vec2 uv) {
	vec3 c[9];
	for (int i=0; i < 3; ++i) for (int j=0; j < 3; ++j)
	{
	  vec4 color = getFromColor(uv + edge_thickness * vec2(i-1,j-1));
    c[3*i + j] = color.rgb;
	}
	return detectEdgeColor(c);
}

vec4 getToEdgeColor(vec2 uv) {
	vec3 c[9];
	for (int i=0; i < 3; ++i) for (int j=0; j < 3; ++j)
	{
	  vec4 color = getToColor(uv + edge_thickness * vec2(i-1,j-1));
    c[3*i + j] = color.rgb;
	}
	return detectEdgeColor(c);
}

vec4 transition (vec2 uv) {
  vec4 start = mix(getFromColor(uv), getFromEdgeColor(uv), clamp(2.0 * progress, 0.0, 1.0));
  vec4 end = mix(getToEdgeColor(uv), getToColor(uv), clamp(2.0 * (progress - 0.5), 0.0, 1.0));
  return mix(
    start,
    end,
    progress
  );
}

void main() {
	if (flip == true) {
    gl_FragColor = transition(vec2(isf_FragNormCoord.x, 1. - isf_FragNormCoord.y));
  } else {
    gl_FragColor = transition(isf_FragNormCoord);
  }
}