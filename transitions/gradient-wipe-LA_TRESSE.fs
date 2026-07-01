/*{
	"DESCRIPTION": "Gradient Wipe transition with bundled gradient maps. Extends Gradient Wipe 03 with additional built-in gradient images selectable via the importedGradient menu.",
	"CREDIT": "Benoît Lahoz",
	"CATEGORIES": ["Transition"],
	"PASSES": [
		{ "TARGET": "gradStats", "WIDTH": "1", "HEIGHT": "1", "FLOAT": true },
		{ "TARGET": "" }
	],
	"IMPORTED": {
		"gradientHair01": { "PATH": "gradient-wipe-hair-01.jpg" },
		"gradientHair02": { "PATH": "gradient-wipe-hair-02.jpg" },
		"gradientHair03": { "PATH": "gradient-wipe-hair-03.jpg" },
		"gradientHair04": { "PATH": "gradient-wipe-hair-04.jpg" },
		"gradientHair05": { "PATH": "gradient-wipe-hair-05.jpg" },
		"gradientHair06": { "PATH": "gradient-wipe-hair-06.jpg" },
		"gradientHair07": { "PATH": "gradient-wipe-hair-07.jpg" },
		"gradientHair08": { "PATH": "gradient-wipe-hair-08.jpg" },
		"gradientHair09": { "PATH": "gradient-wipe-hair-09-tile-x.jpg" },
		"gradientHair10": { "PATH": "gradient-wipe-hair-10-tile-xy.jpg" },
		"gradientHair11": { "PATH": "gradient-wipe-hair-11-tile-xy.jpg" },
		"gradientHair12": { "PATH": "gradient-wipe-hair-12-tile-y.jpg" },
		"gradientHair13": { "PATH": "gradient-wipe-hair-13-tile-y.jpg" }
	},
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
			"VALUES": [0, 1, 2, 3],
			"LABELS": ["startImage", "endImage", "gradientImage", "importedGradient"],
			"DEFAULT": 3
		},
		{
			"NAME": "importedGradient",
			"TYPE": "long",
			"VALUES": [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
			"LABELS": ["Hair 01", "Hair 02", "Hair 03", "Hair 04", "Hair 05", "Hair 06", "Hair 07", "Hair 08", "Hair 09 (Tile X)", "Hair 10 (Tile XY)", "Hair 11 (Tile XY)", "Hair 12 (Tile Y)", "Hair 13 (Tile Y)"],
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
			"NAME": "normalizeGradient",
			"TYPE": "bool",
			"DEFAULT": false
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
			"NAME": "rotationSpeed",
			"TYPE": "float",
			"MIN": -1.0,
			"MAX": 1.0,
			"DEFAULT": 0.0
		},
		{
			"NAME": "zoomSpeed",
			"TYPE": "float",
			"MIN": 0.0,
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
	else if (gradientSource == 2) return luma(IMG_NORM_PIXEL(gradient, uv));
	// Imported gradient images bundled with the shader
	else if (importedGradient == 0)  return luma(IMG_NORM_PIXEL(gradientHair01, uv));
	else if (importedGradient == 1)  return luma(IMG_NORM_PIXEL(gradientHair02, uv));
	else if (importedGradient == 2)  return luma(IMG_NORM_PIXEL(gradientHair03, uv));
	else if (importedGradient == 3)  return luma(IMG_NORM_PIXEL(gradientHair04, uv));
	else if (importedGradient == 4)  return luma(IMG_NORM_PIXEL(gradientHair05, uv));
	else if (importedGradient == 5)  return luma(IMG_NORM_PIXEL(gradientHair06, uv));
	else if (importedGradient == 6)  return luma(IMG_NORM_PIXEL(gradientHair07, uv));
	else if (importedGradient == 7)  return luma(IMG_NORM_PIXEL(gradientHair08, uv));
	else if (importedGradient == 8)  return luma(IMG_NORM_PIXEL(gradientHair09, uv));
	else if (importedGradient == 9)  return luma(IMG_NORM_PIXEL(gradientHair10, uv));
	else if (importedGradient == 10) return luma(IMG_NORM_PIXEL(gradientHair11, uv));
	else if (importedGradient == 11) return luma(IMG_NORM_PIXEL(gradientHair12, uv));
	else if (importedGradient == 12) return luma(IMG_NORM_PIXEL(gradientHair13, uv));
	return 0.0;
}

// feather: spatial blur of the gradient map.
// Averages neighbouring pixels to smooth isolated dark/bright areas and delay
// their premature transition — distinct from softness, which widens the
// threshold band (After Effects behaviour).
float sampleGradient(vec2 uv) {
	float p = clamp(progress, 0.0, 1.0);

	// Rotation and zoom: both linked to progress, pivot at image centre.
	// Guard ensures default (0) takes the exact original code path.
	vec2 baseUV;
	if (abs(rotationSpeed) > 0.0001 || zoomSpeed > 0.0001) {
		vec2 c = uv - 0.5;

		if (abs(rotationSpeed) > 0.0001) {
			float a    = rotationSpeed * p * 6.2831853;
			float cosA = cos(a);
			float sinA = sin(a);
			c = vec2(cosA * c.x - sinA * c.y,
			         sinA * c.x + cosA * c.y);
		}

		if (zoomSpeed > 0.0001) {
			c /= pow(2.0, zoomSpeed * p);
		}

		baseUV = c + 0.5;
	} else {
		baseUV = uv;
	}

	// Velocity: animated offset + seamless tiling via fract().
	vec2 gUV = fract(baseUV + vec2(velocityX, velocityY) * TIME);

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
	if (PASSINDEX == 0) {
		// Pass 0: sample the raw gradient at a 4x4 grid to estimate its
		// luminance range. Result stored in a 1x1 float texture:
		//   R = average, G = minimum, B = maximum.
		float gMin = 1.0;
		float gMax = 0.0;
		float total = 0.0;
		for (int ix = 0; ix < 4; ix++) {
			for (int iy = 0; iy < 4; iy++) {
				vec2 suv = vec2((float(ix) + 0.5) * 0.25, (float(iy) + 0.5) * 0.25);
				float l  = sampleLuma(suv);
				total   += l;
				gMin     = min(gMin, l);
				gMax     = max(gMax, l);
			}
		}
		gl_FragColor = vec4(total / 16.0, gMin, gMax, 1.0);

	} else {
		// Pass 1: main gradient wipe.
		vec2 uv = isf_FragNormCoord;

		vec4 colA = IMG_NORM_PIXEL(startImage, uv);
		vec4 colB = IMG_NORM_PIXEL(endImage, uv);

		float s = clamp(softness, 0.0, 1.0);
		float p = clamp(progress, 0.0, 1.0);

		float L = sampleGradient(uv);

		// Optional luminance range normalisation using Pass 0 statistics.
		// Stretches the gradient's actual [min, max] to [0, 1], giving a more
		// even distribution — especially useful with startImage / endImage.
		if (normalizeGradient) {
			vec4  stats = IMG_NORM_PIXEL(gradStats, vec2(0.5));
			float gMin  = stats.g;
			float gMax  = stats.b;
			if (gMax - gMin > 0.05)
				L = clamp((L - gMin) / (gMax - gMin), 0.0, 1.0);
		}

		// Gradient contrast: > 1 = sharper mask, < 1 = more diffuse transition.
		L = clamp((L - 0.5) * gradientContrast + 0.5, 0.0, 1.0);

		// Default (AE): dark pixels (low L) transition first.
		// reverse = true: bright pixels transition first.
		if (reverse) L = 1.0 - L;

		// sw: effective band width — same value used for both the threshold
		// position and the division, so the formula is fully consistent.
		float sw = max(s, 0.01);
		float threshold = p * (1.0 + sw) - sw * 0.5;
		float alpha = clamp((threshold - L) / sw + 0.5, 0.0, 1.0);

		gl_FragColor = mix(colA, colB, alpha);
	}
}