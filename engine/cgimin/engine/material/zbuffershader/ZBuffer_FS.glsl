#version 130
precision highp float;


in vec4 fragPosition;

out vec4 outputColor;

void main()
{
	float value = pow(2, fragPosition.z);
    outputColor = vec4(value, value, value, 1.0f); //vec4(1+fragPosition.z / 5, 1+fragPosition.z/5, 1+fragPosition.z/5, 1.0f);
}