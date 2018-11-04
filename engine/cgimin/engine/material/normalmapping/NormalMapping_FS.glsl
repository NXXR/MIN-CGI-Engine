#version 330
precision highp float;

// basic texture and normalmap
uniform sampler2D color_texture;
uniform sampler2D normalmap_texture;

// "model_matrix"
uniform mat4 model_matrix;

// parameter for directional light
uniform vec3 light_direction;
uniform vec4 light_ambient_color;
uniform vec4 light_diffuse_color;
uniform vec4 light_specular_color;
uniform vec4 camera_position;

// parameter for "shininess"
uniform float specular_shininess;

// input from Vertex-Shader
in vec2 fragTexcoord;
in mat3 fragTBN;
in vec4 fragPosition;

// the final color
out vec4 outputColor;

void main()
{	
	// calculate normal from texture
    vec3 normal = texture(normalmap_texture, fragTexcoord).rgb;
	normal = normalize(normal * 2.0 - 1.0); 
	normal = normalize(fragTBN * normal); 

	// calculate rotation matrix
	mat3 normalMatrix = mat3(model_matrix);
	
	// calculate view direction
	vec4 v = normalize(camera_position - model_matrix * fragPosition);

	// calculate halfway vector
	vec3 h = normalize(light_direction + vec3(v));
	float ndoth = dot( normal, h );
	float specularIntensity = pow(ndoth, specular_shininess);

	// caclulate brightness
	float brightness = clamp(dot(normalize(normal), light_direction), 0, 1);
	
	// surfaceColor is the color from the texture
	vec4 surfaceColor = texture(color_texture, fragTexcoord);

	//				 Ambient						      + Diffuse 								          + Specular 
    // outputColor = (surfaceColor * light_ambient_color) + (surfaceColor * brightness * light_diffuse_color) + specularIntensity * light_specular_color;
	// upper line 
	 outputColor = surfaceColor * (light_ambient_color +  brightness * light_diffuse_color) + specularIntensity * light_specular_color;

}