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

// "texcoord" wird an den Fragment-Shader weitergegeben, daher als "out" deklariert
out vec2 texcoord;

void main()
{
	// uv koordinate wird aus der rotierten Normalen berechnet
	texcoord = (modelview_matrix * vec4(in_normal, 0)).xy * 0.5 + 0.5;

	// in gl_Position die finalan Vertex-Position geschrieben ("modelview_projection_matrix" * "in_position")
	gl_Position = modelview_projection_matrix * vec4(in_position, 1);
}


