#version 330
precision highp float;

uniform sampler2D color_texture; 
uniform sampler2D normalmap_texture;

in vec2 texcoord;
in vec3 position;
in mat3 fragTBN;

// output
layout (location = 0) out vec4 gColor;
layout (location = 1) out vec3 gPosition;
layout (location = 2) out vec3 gNormal;

void main()
{
	gColor = texture(color_texture, texcoord);
	gPosition = position;
	
	vec3 normal = texture(normalmap_texture, texcoord).rgb;
	normal = normalize(normal * 2.0 - 1.0); 
	normal = normalize(fragTBN * normal); 
	gNormal = normal;   
}