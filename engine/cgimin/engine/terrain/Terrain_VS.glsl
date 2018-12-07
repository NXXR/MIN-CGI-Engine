#version 150
precision highp float;

// input aus der VAO-Datenstruktur
in vec3 in_position;

// die höhen-Textur
uniform sampler2D height_map; 

// textur-Größe
uniform float terrain_size;

// textur-Skalierung
uniform float texture_scale;

// "modelview_projection_matrix" wird als Parameter erwartet, vom Typ Matrix4
uniform mat4 modelview_projection_matrix;

// Die "Model-Matrix" wird als Parameter erwaretet, zur Berechnung von fragV (Blickrichtung)
uniform vec3 cam_sub_position;

// "Die XZ-Terrain Position"
uniform vec2 xzPos;

// "texcoord" wird an den Fragment-Shader weitergegeben, daher als "out" deklariert
out vec2 fragTexcoord;

// die Blickrichtung wird dem Fragment-Shader übergeben
out vec4 fragV;

// die TBN-Matrix wird an den Fragment-Shader übergeben
out mat3 fragTBN;

void main()
{

	vec2 inTexPos = in_position.xz+xzPos;
	fragTexcoord = inTexPos * texture_scale;

	vec2 tanTexDelta = vec2(0.5, 0.0);
	vec2 biTanTexDelta = vec2(0.0, 0.5);

	vec3 pos = in_position;	
	pos.y = texture(height_map, inTexPos / terrain_size).x * 30 - 17;

	vec3 T = normalize(vec3(1.0 / terrain_size, (texture(height_map, (inTexPos - tanTexDelta) / terrain_size).x - texture(height_map, (inTexPos + tanTexDelta) / terrain_size).x), 0.0));
    vec3 B = normalize(vec3(0.0, (texture(height_map, (inTexPos - biTanTexDelta) / terrain_size).x - texture(height_map, (inTexPos + biTanTexDelta) / terrain_size).x), 1.0 / terrain_size));
    vec3 N = normalize(cross(B, T));
    fragTBN = mat3(T, B, N);

	// position übergeben
	fragV = normalize(vec4(in_position,1) - vec4(cam_sub_position,1) );

	// in gl_Position die finalan Vertex-Position geschrieben ("modelview_projection_matrix" * "in_position")
	gl_Position = modelview_projection_matrix * vec4(pos, 1);
}


