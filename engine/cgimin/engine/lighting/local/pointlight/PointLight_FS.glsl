#version 330 core
precision highp float;

uniform sampler2D gNormal;
uniform sampler2D gPosition;

in vec3 position;

uniform vec3 cameraPosition;
uniform vec3 ambientColor;
uniform vec3 diffuseColor;
uniform vec3 specularColor;
uniform float specularShininess;

uniform float radius;
uniform vec3 objectPosition;

uniform float screenWidth;
uniform float screenHeight;

out vec4 outputColor;

void main()
{
	// getting screen coords in range 0..1 (coord for texture lookup in gBuffer textures)
	vec2 fragmentScreenCoordinates = vec2(gl_FragCoord.x / screenWidth, gl_FragCoord.y / screenHeight);
    
	// the world position
	vec3 worldPosition = texture2D(gPosition, fragmentScreenCoordinates).rgb;

	vec3 posMid = objectPosition - worldPosition;
	float distance    = length(posMid);
	float attenuation = clamp(1.0 - distance / radius, 0.0, 1.0);
	//float attenuation = clamp(1.0 - distance*distance/(radius*radius), 0.0, 1.0);
	attenuation*= attenuation;

	vec3 lightDirection = posMid / distance;
    vec3 normal = texture2D(gNormal, fragmentScreenCoordinates).rgb;

	// view vector is the camera position - the position in world-space
	vec3 view = normalize(cameraPosition - worldPosition);
	vec3 h = normalize(lightDirection + view);
	float ndoth = dot( normal, h );
	float specularIntensity = pow(ndoth, specularShininess);

	// calculate brighness, resulting from angle between normal and light direction
	float brightness = clamp(dot(normalize(normal), lightDirection), -1, 1);
	
	outputColor = vec4(clamp((ambientColor +  brightness * diffuseColor) + specularIntensity * specularColor, 0, 1) * attenuation, 1);
	//outputColor = vec4(((ambientColor +  brightness * diffuseColor) + specularIntensity * specularColor)  * attenuation, 1);
}