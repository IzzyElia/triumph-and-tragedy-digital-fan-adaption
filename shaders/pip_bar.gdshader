shader_type spatial;

uniform float max_pips = 1.0;
uniform float actual_pips = 1.0;
uniform vec4 full_health_color : source_color;
uniform vec4 min_health_color : source_color;

void fragment() {
	bool show = true;
	float s = cos(UV.x * PI * 2.0 * max_pips);
	if (s > 0.7) show = false;
	float health_ratio = actual_pips / max_pips;
	if (UV.x > (health_ratio)) show = false;
	
	if (show) {
		ALBEDO = vec3(1.0);
		EMISSION = mix(min_health_color.rgb, full_health_color.rgb, ((actual_pips - 1.0) / (max_pips - 1.0)));
	}
	else {
		EMISSION = vec3(0.0);
		ALPHA = 0.0;
	}
	
	ALPHA_SCISSOR_THRESHOLD = 0.5;
}
