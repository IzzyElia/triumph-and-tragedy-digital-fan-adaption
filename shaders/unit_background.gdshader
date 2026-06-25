shader_type spatial;

uniform float border_threshold = 0.4;
uniform float vertex_drop_threshold = 0.2;
uniform vec3 background_color : source_color;
uniform vec3 border_color : source_color;

void vertex() {
	if (VERTEX.x > vertex_drop_threshold 
	|| VERTEX.x < -vertex_drop_threshold
	|| VERTEX.y > vertex_drop_threshold
	|| VERTEX.y < -vertex_drop_threshold)
		VERTEX = VERTEX + vec3(0.0, -0.0, -0.25);
}

void fragment() {
	vec2 centeredUV = UV - vec2(0.5);
	if (centeredUV.x > border_threshold
	|| centeredUV.x < -border_threshold
	|| centeredUV.y > border_threshold
	|| centeredUV.y < -border_threshold) {
		EMISSION = vec3(0.2) * border_color;
		ALBEDO = border_color;
	}
	else {
		EMISSION = vec3(0.1) * background_color;
		ALBEDO = background_color;
	}
	EMISSION *= 1.0 - sqrt(abs(centeredUV.x * centeredUV.y) * 3.0 + 0.1);
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
