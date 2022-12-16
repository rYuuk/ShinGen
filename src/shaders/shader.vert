#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoords;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec2 TexCoords;
out vec3 WorldPos;
out vec3 Normal;

void main(void)
{
    TexCoords =  vec2(aTexCoords.s, 1.0 - aTexCoords.t);;
    mat4 mvp = model * view * projection ;
    gl_Position =   vec4(aPosition, 1.0) * mvp;
    WorldPos = vec3(vec4(aPosition, 1.0) * model);
    Normal = aNormal * mat3(transpose(inverse((model))));
}
