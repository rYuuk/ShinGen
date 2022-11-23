#version 330 core

out vec4 FragColor;
in vec2 TexCoords;

uniform sampler2D texture_diffuse1;
uniform sampler2D texture_diffuse2;
uniform sampler2D texture_diffuse3;
uniform sampler2D texture_diffuse4;
uniform sampler2D texture_diffuse5;
uniform sampler2D texture_specular1;

void main()
{
    vec2 flipTexcoord = vec2(TexCoords.s, 1.0 - TexCoords.t);
    vec3 diffuse = 1.0f * vec3(texture(texture_diffuse1, flipTexcoord));
    vec3 diffuse2 = 1.0f * vec3(texture(texture_diffuse2, flipTexcoord));
    vec3 diffuse3 = 1.0f * vec3(texture(texture_diffuse3, flipTexcoord));
    vec3 diffuse4 = 1.0f * vec3(texture(texture_diffuse4, flipTexcoord));
    vec3 diffuse5 = 1.0f * vec3(texture(texture_diffuse5, flipTexcoord));
    vec3 specular = 0.0f * vec3(texture(texture_specular1, flipTexcoord));
    FragColor = vec4(diffuse + diffuse2 + diffuse3 + diffuse4 + diffuse5 + specular, 1);
}

