#version 330
precision highp float;

uniform sampler2D normalmap_texture;

uniform vec3 color;
uniform float roughness;
uniform float metalness;
uniform float glow;

in vec2 texcoord;
in vec3 position;
in mat3 fragTBN;

// output
layout (location = 0) out vec4 gColorRoughness;
layout (location = 1) out vec3 gPosition;
layout (location = 2) out vec3 gNormal;
layout (location = 3) out vec3 gMetalnessShadow;
layout (location = 4) out vec3 gGlow;

void main()
{
	gColorRoughness = vec4(color, roughness);
	gPosition = position;
	
	vec3 normal = texture(normalmap_texture, texcoord).rgb;
	normal = normalize(normal * 2.0 - 1.0); 
	normal = normalize(fragTBN * normal); 
	gNormal = normal;
	
	gMetalnessShadow.r = metalness;
	gMetalnessShadow.g = 0; // no shadow

	gGlow = glow * color;
}