#version 330 core

uniform sampler2D image;
uniform bool horizontal;

in vec2 texcoord;
out vec4 outputColor;

uniform float weight[5] = float[] (0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216);

void main()
{             
    
	vec2 tex_offset = 1.0 / textureSize(image, 0);

    vec4 result = texture(image, texcoord) * weight[0];
	for(int i = 1; i < 5; ++i)
    {
        result += texture(image, texcoord + vec2(tex_offset.x * i, 0.0)) * weight[i];
        result += texture(image, texcoord - vec2(tex_offset.x * i, 0.0)) * weight[i];
    }

	outputColor = result;

}