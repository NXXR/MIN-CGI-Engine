#version 130
precision highp float;


// input aus der VAO-Datenstruktur
in vec3 in_position;
in vec3 in_normal; 
in vec2 in_uv; 

// "modelview_projection_matrix" wird als Parameter erwartet, vom Typ Matrix4
uniform mat4 modelview_projection_matrix;

// "modelview_matrix" wird als Parameter erwartet, vom Typ Matrix4 
uniform mat4 modelview_matrix;

out vec4 fragPosition;

void main()
{
	fragPosition = modelview_matrix * vec4(in_position, 1);

	// in gl_Position die finalan Vertex-Position geschrieben ("modelview_projection_matrix" * "in_position")
	gl_Position = modelview_projection_matrix * vec4(in_position, 1);
}


