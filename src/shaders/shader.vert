#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoords;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

//out vec3 Normal;
//out vec3 FragPos;
out vec2 TexCoords;

void main(void)
{
    TexCoords = aTexCoords;
    gl_Position = vec4(vec4(aPosition, 1.0f) * model * view * projection);
//    FragPos = vec3(vec4(aPosition, 1.0f) * model);
//    Normal = vec3(aNormal * mat3(transpose(inverse(model))));
//    TexCoords = aTexCoords;
}
