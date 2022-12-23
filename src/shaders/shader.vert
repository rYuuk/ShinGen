#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoords;
layout (location = 3) in ivec4 aBoneIds;
layout (location = 4) in vec4 aWeights;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

const int MAX_BONES = 200;
const int MAX_BONE_INFLUENCE = 4;

struct FinalBonesMatrice {
    mat4 matrix;
};

uniform FinalBonesMatrice finalBonesMatrices[MAX_BONES];

out vec2 TexCoords;
out vec3 WorldPos;
out vec3 Normal;
flat out ivec4 BoneIds;
out vec4 Weights;

void main(void)
{
    vec4 totalPosition = vec4(aPosition, 1.0f);
        for(int i = 0 ; i < MAX_BONE_INFLUENCE ; i++)
        {
            if(aBoneIds[i] == -1)
                continue;
            if(aBoneIds[i] >=MAX_BONES)
            {
                totalPosition = vec4(aPosition,1.0f);
                break;
            }
            vec4 localPosition = finalBonesMatrices[aBoneIds[i]].matrix * vec4(aPosition, 1.0f);
            totalPosition += localPosition * aWeights[i];
            vec3 localNormal = mat3(finalBonesMatrices[aBoneIds[i]].matrix) * aNormal;
        }

//    totalPosition = vec4(aPosition, 1.0f);
    gl_Position = projection * view * model * totalPosition;
    WorldPos = vec3(model * totalPosition);
    Normal = mat3(transpose(inverse(model))) * aNormal;
    TexCoords = aTexCoords;
    
    BoneIds = aBoneIds;
    Weights = aWeights;
}
