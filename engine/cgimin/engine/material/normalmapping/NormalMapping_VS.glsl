#version 330
precision highp float;

// input from VAO
in vec3 in_position;
in vec3 in_normal; 
in vec2 in_uv;
in vec3 in_tangent;
in vec3 in_bitangent;

// "modelview_projection_matrix"
uniform mat4 modelview_projection_matrix;

// "Model-Matrix" as parameter to transform TBN-Matrix into word-space.
uniform mat4 model_matrix;

// "texcoord" given to fragment-shader
out vec2 fragTexcoord;

// position is also passed to the fragment-shader
out vec4 fragPosition;

// TBN-Matrix is also passed to the fragment-shader
out mat3 fragTBN;

void main()
{
	vec3 T = normalize(vec3(model_matrix * vec4(in_tangent, 0.0)));
    vec3 B = normalize(vec3(model_matrix * vec4(in_bitangent, 0.0)));
    vec3 N = normalize(vec3(model_matrix * vec4(in_normal, 0.0)));
    fragTBN = mat3(T, B, N);

	// "in_uv" to fragment-shader
	fragTexcoord = in_uv;

	// position to fragment-shader
	fragPosition = vec4(in_position,1);

	// final vertex position
	gl_Position = modelview_projection_matrix * vec4(in_position, 1);
}


