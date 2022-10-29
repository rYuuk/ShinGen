﻿#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec3 Normal;
out vec3 FragPos;

void main(void)
{
    gl_Position = vec4(vec4(aPosition, 1.0f) * model * view * projection);
    FragPos = vec3(model * vec4(aPosition, 1.0f));
    Normal = aNormal;
}
