#version 130
precision highp float;

// input aus der VAO-Datenstruktur
in vec3 in_position;
in vec2 add_pos;
in vec2 in_uv; 

// "modelview_projection_matrix" wird als Parameter erwartet, vom Typ Matrix4

uniform vec2 width_height;
uniform mat4 modelview_projection_matrix;

// "texcoord" wird an den Fragment-Shader weitergegeben, daher als "out" deklariert
out vec2 texcoord;

void main()
{
	// "in_uv" (Texturkoordinate) wird direkt an den Fragment-Shader weitergereicht
	texcoord = in_uv;

	// in gl_Position die finalan Vertex-Position geschrieben ("modelview_projection_matrix" * "in_position")

	vec3 pp = vec3(in_position.x + add_pos.x * width_height.x, in_position.y + add_pos.y * width_height.y, in_position.z);

	gl_Position = modelview_projection_matrix * vec4(pp, 1);

}


