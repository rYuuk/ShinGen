﻿#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoords;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec2 TexCoords;

void main(void)
{
    TexCoords = aTexCoords;
    gl_Position = vec4(vec4(aPosition, 1.0f) * model * view * projection);
}
