#version 330
precision highp float;

uniform sampler2D sampler;

// "model_matrix" Matrix wird als Parameter erwartet, vom Typ Matrix4
uniform mat4 model_matrix;

// Parameter fuer direktionales Licht
uniform vec3 light_direction;
uniform vec4 light_ambient_color;
uniform vec4 light_diffuse_color;

// input vom Vertex-Shader
in vec2 fragTexcoord;
in vec3 fragNormal;

// die finale Farbe
out vec4 outputColor;

void main()
{	
	// die Matrix berechnen, mit der die Normale (fragNormal) multipliziert wird.
	mat3 normalMatrix = mat3(model_matrix);

	// die Vertex-Normale berechnen
    vec3 normal = normalize(normalMatrix * fragNormal);

	// die Helligkeit berechnen, resultierund aus dem Winkel 
	float brightness = clamp(dot(normalize(normal), light_direction), 0, 1);

	// surfaceColor ist die farbe aus der Textur...
	vec4 surfaceColor = texture(sampler, fragTexcoord);

    //				 Ambiente Farbe						  + Diffuser Farbanteil
    // outputColor = (surfaceColor * light_ambient_color) + (surfaceColor * brightness * light_diffuse_color);
	// obere Zeile surfaceColor ausgeklammert
	outputColor = surfaceColor * (light_ambient_color + brightness * light_diffuse_color);
}