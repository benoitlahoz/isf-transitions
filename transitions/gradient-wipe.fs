/*{
	"DESCRIPTION": "Gradient Wipe transition. A chosen image (source, destination, or external gradient map) drives the wipe through its luminance: dark pixels transition first by default. Feather spatially blurs the gradient map to smooth isolated pixels; softness widens the threshold band (After Effects style); gradientContrast sharpens or diffuses the mask; velocityX/Y animate the map in a seamless loop.",
	"CREDIT": "Benoît Lahoz",
	"CATEGORIES": ["Transition"],
	"INPUTS": [
		{
			"NAME": "startImage",
			"TYPE": "image"
		},
		{
			"NAME": "endImage",
			"TYPE": "image"
		},
		{
			"NAME": "gradient",
			"TYPE": "image"
		},
		{
			"NAME": "gradientSource",
			"TYPE": "long",
			"VALUES": [0, 1, 2],
			"LABELS": ["startImage", "endImage", "gradientImage"],
			"DEFAULT": 0
		},
		{
			"NAME": "progress",
			"TYPE": "float",
			"MIN": 0.0,
			"MAX": 1.0,
			"DEFAULT": 0.0
		},
		{
			"NAME": "feather",
			"TYPE": "float",
			"MIN": 0.0,
			"MAX": 1.0,
			"DEFAULT": 0.15
		},
		{
			"NAME": "softness",
			"TYPE": "float",
			"MIN": 0.0,
			"MAX": 1.0,
			"DEFAULT": 0.0
		},
		{
			"NAME": "gradientContrast",
			"TYPE": "float",
			"MIN": 0.0,
			"MAX": 4.0,
			"DEFAULT": 1.0
		},
		{
			"NAME": "velocityX",
			"TYPE": "float",
			"MIN": -1.0,
			"MAX": 1.0,
			"DEFAULT": 0.0
		},
		{
			"NAME": "velocityY",
			"TYPE": "float",
			"MIN": -1.0,
			"MAX": 1.0,
			"DEFAULT": 0.0
		},
		{
			"NAME": "reverse",
			"TYPE": "bool",
			"DEFAULT": false
		}
	]
}*/

float luma(vec4 c) {
	return dot(c.rgb, vec3(0.299, 0.587, 0.114));
}

float sampleLuma(vec2 uv) {
	if (gradientSource == 0) return luma(IMG_NORM_PIXEL(startImage, uv));
	else if (gradientSource == 1) return luma(IMG_NORM_PIXEL(endImage, uv));
	else return luma(IMG_NORM_PIXEL(gradient, uv));
}

// feather: spatial blur of the gradient map.
// Averages neighbouring pixels to smooth isolated dark/bright areas and delay
// their premature transition — distinct from softness, which widens the
// threshold band (After Effects behaviour).
float sampleGradient(vec2 uv) {
	// Animated offset + seamless tiling via fract()
	vec2 gUV = fract(uv + vec2(velocityX, velocityY) * TIME);

	float radiusPx = feather * 0.15 * max(RENDERSIZE.x, RENDERSIZE.y);
	if (radiusPx < 1.0) return sampleLuma(gUV);

	vec2 r = vec2(radiusPx) / RENDERSIZE.xy;
	float total = sampleLuma(gUV);
	float count = 1.0;

	for (int i = 0; i < 8; i++) {
		float angle = 6.2831853 * float(i) / 8.0;
		vec2 dir = vec2(cos(angle), sin(angle));
		total += sampleLuma(clamp(gUV + dir * r,       0.0, 1.0));
		total += sampleLuma(clamp(gUV + dir * r * 0.5, 0.0, 1.0));
		count += 2.0;
	}

	return total / count;
}

void main() {
	vec2 uv = isf_FragNormCoord;

	vec4 colA = IMG_NORM_PIXEL(startImage, uv);
	vec4 colB = IMG_NORM_PIXEL(endImage, uv);

	float s = clamp(softness, 0.0, 1.0);
	float p = clamp(progress, 0.0, 1.0);

	float L = sampleGradient(uv);

	// Gradient contrast: > 1 = sharper mask, < 1 = more diffuse transition.
	L = clamp((L - 0.5) * gradientContrast + 0.5, 0.0, 1.0);

	// Default (AE): dark pixels (low L) transition first.
	// reverse = true: bright pixels transition first.
	if (reverse) L = 1.0 - L;

	// eps: minimal threshold expansion to guarantee alpha=0 at p=0 and alpha=1
	// at p=1 even when softness=0. Independent of the soft band width.
	// s: width of the transition band (After Effects style).
	float eps = max(s, 0.01);
	float threshold = p * (1.0 + eps) - eps * 0.5;
	float alpha = clamp((threshold - L) / max(s, 0.001) + 0.5, 0.0, 1.0);

	gl_FragColor = mix(colA, colB, alpha);
}