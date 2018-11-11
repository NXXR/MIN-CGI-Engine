#version 330
precision highp float;

// basis Textur und Normalmap und Cubemap
uniform sampler2D color_texture;
uniform sampler2D normalmap_texture;
uniform samplerCube cube_texture; 

// "model_matrix" Matrix wird als Parameter erwartet, vom Typ Matrix4
uniform mat4 model_matrix;

// Parameter fuer direktionales Licht
uniform vec3 light_direction;
uniform vec4 light_ambient_color;
uniform vec4 light_diffuse_color;

// input vom Vertex-Shader
in vec2 fragTexcoord;
in mat3 fragTBN;
in vec3 fragV;

// die finale Farbe
out vec4 outputColor;

void main()
{	
	// die Vertex-Normale berechnen
    vec3 normal = texture(normalmap_texture, fragTexcoord).rgb;
	normal = normalize(normal * 2.0 - 1.0); 
	normal = normalize(fragTBN * normal); 

	// die Texturecoordinate fuer die Cube-Texture ist die Reflexion
	vec3 texcoord = normalize(reflect(fragV, normal));
	vec4 cubeColor = texture(cube_texture, texcoord);
	
	// die Helligkeit berechnen, resultierund aus dem Winkel 
	float brightness = clamp(dot(normalize(normal), light_direction), 0, 1);

	// surfaceColor ist die farbe aus der Textur...
	vec4 surfaceColor = texture(color_texture, fragTexcoord);

	//				 ambient color				          + diffuse 							              + specular color
    // outputColor = (surfaceColor * light_ambient_color) + (surfaceColor * brightness * light_diffuse_color) + specularIntensity * light_specular_color;
	// obere Zeile surfaceColor ausgeklammert
	 outputColor = surfaceColor * (light_ambient_color +  brightness * light_diffuse_color) + cubeColor;

}