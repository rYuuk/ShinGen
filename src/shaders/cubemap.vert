#version 330
layout (location = 0) in vec3 aPos;

out vec3 TexCoords;

uniform mat4 projection;
uniform mat4 view;

void main()
{
    TexCoords = vec3(aPos.xy, -aPos.z);
    vec4 pos = vec4(aPos, 1.0) * view * projection;
    gl_Position = pos.xyww;
}  
