#version 330

out vec4 outputColor;
in vec2 texCoord;

uniform sampler2D texture0;
uniform sampler2D texture1;

uniform vec3 objectColor;
uniform vec3 lightColor;

void main()
{
    outputColor = vec4(lightColor * objectColor, 1.0);;
}
