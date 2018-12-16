#version 400
precision highp float;

// input aus der VAO-Datenstruktur
in vec3 in_position;
in vec3 in_normal; 
in vec2 in_uv; 

// "modelview_projection_matrix" wird als Parameter erwartet, vom Typ Matrix4
uniform mat4 modelviewProjectionMatrix;
uniform mat4 modelMatrix;

out vec3 position;

void main()
{

	position = vec3(modelMatrix * vec4(in_position, 1));

	gl_Position = modelviewProjectionMatrix * vec4(in_position, 1);
}


