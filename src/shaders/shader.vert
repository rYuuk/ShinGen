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

uniform bool enableAnimation;
uniform FinalBonesMatrice finalBonesMatrices[MAX_BONES];

out vec2 TexCoords;
out vec3 WorldPos;
out vec3 Normal;
flat out ivec4 BoneIds;
out vec4 Weights;

void main(void)
{
    vec4 pos = vec4(aPosition, 1.0f);
    if (enableAnimation)
    {
        mat4 boneTransform = mat4(1.0f);
        vec3 totalNormal = vec3(0.0f);
        for (int i = 0; i < MAX_BONE_INFLUENCE; i++)
        {
            if (aBoneIds[i] == -1)
            {
                continue;
            }
            if (aBoneIds[i] >= MAX_BONES)
            {
                break;
            }

            boneTransform += finalBonesMatrices[aBoneIds[i]].matrix * aWeights[i];
        }
        pos = boneTransform * vec4(aPosition, 1.0f);
    }

    gl_Position = projection * view * model * pos;
    WorldPos = vec3(model * vec4(aPosition, 1.0f));
    Normal = mat3(transpose(inverse(model))) * aNormal;
    TexCoords = aTexCoords;

    BoneIds = aBoneIds;
    Weights = aWeights;
}
