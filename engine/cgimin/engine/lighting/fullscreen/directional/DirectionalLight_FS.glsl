#version 330 core
precision highp float;

uniform sampler2D gColor; 
uniform sampler2D gNormal;
uniform sampler2D gPosition;

uniform vec3 cameraPosition;

uniform vec3 lightDirection;
uniform vec4 lightAmbientColor;
uniform vec4 lightDiffuseColor;
uniform vec4 lightSpecularColor;
uniform float specularShininess;

in vec2 texcoord;

out vec4 outputColor;

void main()
{
    
	// get normal normal
    vec3 normal = texture2D(gNormal, texcoord).rgb;

	// view vector is the camera position - the position in world-space
	vec3 view = normalize(cameraPosition - texture2D(gPosition, texcoord).rgb);
	vec3 h = normalize(lightDirection + view);
	float ndoth = dot(normal, h);
	float specularIntensity = pow(ndoth, specularShininess);

	// calculate brighness, resulting from angle between normal and light direction
	float brightness = clamp(dot(normalize(normal), lightDirection), -1, 1);
	
	// surfaceColor is the color from texture...
	vec4 surfaceColor = texture(gColor, texcoord);
	

	outputColor = surfaceColor * (lightAmbientColor +  brightness * lightDiffuseColor) + specularIntensity * lightSpecularColor;

}