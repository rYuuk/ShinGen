#version 330 core

out vec4 FragColor;
in vec2 TexCoords;

uniform sampler2D texture_diffuse1;
uniform sampler2D texture_specular1;

void main()
{
    vec3 diffuse = vec3(texture(texture_diffuse1, TexCoords));
    vec3 specular = 0.2f * vec3(texture(texture_specular1, TexCoords));
    FragColor = vec4(diffuse + specular, 1);
}
