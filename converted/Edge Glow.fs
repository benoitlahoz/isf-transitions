/*{
    "CATEGORIES": [
        "Transition",
        "gl-transitions"
    ],
    "CREDIT": "Woohyun Kim - MIT",
    "DESCRIPTION": "Both images fade through black with their edges glowing (Automatically converted by @benoitlahoz 'isf-transitions').",
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
            "LABEL": "edge_thickness",
            "NAME": "edge_thickness",
            "TYPE": "float",
            "DEFAULT": 0.001,
            "MIN": 0,
            "MAX": 1
        },
        {
            "LABEL": "edge_brightness",
            "NAME": "edge_brightness",
            "TYPE": "float",
            "DEFAULT": 8,
            "MIN": 0,
            "MAX": 32
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

vec3 detectEdgeColor(vec3[9] c) {
  /* adjacent texel array for texel c[4]
    036
    147
    258
  */
  vec3 dx = 2.0 * abs(c[7]-c[1]) + abs(c[2] - c[6]) + abs(c[8] - c[0]);
	vec3 dy = 2.0 * abs(c[3]-c[5]) + abs(c[6] - c[8]) + abs(c[0] - c[2]);
  float delta = length(0.25 * (dx + dy) * 0.5);
	return clamp(edge_brightness * delta, 0.0, 1.0) * c[4];
}

vec4 getFromEdgeColor(vec2 uv) {
	vec3 c[9];
	float alpha = 0.0;
	for (int i=0; i < 3; ++i) for (int j=0; j < 3; ++j)
	{
	  vec4 color = getFromColor(uv + edge_thickness * vec2(i-1,j-1));
    c[3*i + j] = color.rgb;
    if (i == 1 && j == 1) alpha = color.a;
	}
	return vec4(detectEdgeColor(c), alpha);
}

vec4 getToEdgeColor(vec2 uv) {
	vec3 c[9];
	float alpha = 0.0;
	for (int i=0; i < 3; ++i) for (int j=0; j < 3; ++j)
	{
	  vec4 color = getToColor(uv + edge_thickness * vec2(i-1,j-1));
    c[3*i + j] = color.rgb;
    if (i == 1 && j == 1) alpha = color.a;
	}
	return vec4(detectEdgeColor(c), alpha);
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
  gl_FragColor = transition(isf_FragNormCoord);
}
