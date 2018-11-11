#version 330
precision highp float;

uniform samplerCube sampler; 
uniform sampler2D normalmap_texture;

in vec2 fragTexcoord;
in mat3 fragTBN;
in vec3 fragV;

out vec4 outputColor;

void main()
{
	// calculate normal
    vec3 normal = texture(normalmap_texture, fragTexcoord).rgb;
	normal = normalize(normal * 2.0 - 1.0); 
	normal = normalize(fragTBN * normal); 

	vec3 texcoord = normalize(reflect(fragV, normal));

    outputColor = texture(sampler, texcoord);
}