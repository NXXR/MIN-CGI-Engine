#version 130
precision highp float;

uniform sampler2D sampler; 

uniform vec2 shift;

uniform int target;

in vec2 texcoord;
out vec4 outputColor;

const int gaussRadius = 11;
const float gaussFilter[gaussRadius] = float[gaussRadius](0.0402,0.0623,0.0877,0.1120,0.1297,0.1362,0.1297,0.1120,0.0877,0.0623,0.0402);

void main()
{	
	vec2 texCoord = texcoord - float(int(gaussRadius/2)) * shift;
	vec4 color = vec4(0.0, 0.0, 0.0, 0.0);

	for (int i=0; i<gaussRadius; ++i) { 
		color += gaussFilter[i] * texture2D(sampler, texCoord).xyzw;
		texCoord += shift;
	}

	gl_FragData[target] = color;
}