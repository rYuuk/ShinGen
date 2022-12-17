// using SharpGLTF.Runtime;
// using SharpGLTF.Schema2;
// using SharpGLTF.Validation;
// using GLTFMesh = SharpGLTF.Schema2.Mesh;
// using GLTFMaterial = SharpGLTF.Schema2.Material;
// using PrimitiveType = SharpGLTF.Schema2.PrimitiveType;
//
// namespace OpenGLEngine
// {
//     public class GLTFImporter
//     {
//         private readonly List<Texture> loadedTextures;
//         private readonly List<Mesh> meshes;
//
//         public GLTFImporter()
//         {
//             meshes = new List<Mesh>();
//             loadedTextures = new List<Texture>();
//         }
//
//         public List<Mesh> Import(string path) =>
//             ParseGLTFMeshToNativeMesh(GetGLTFMeshes(path));
//
//         public void Dispose()
//         {
//             for (var i = 0; i < loadedTextures.Count; i++)
//                 TextureLoader.Dispose(loadedTextures[i].ID);
//         }
//
//         private IEnumerable<GLTFMesh> GetGLTFMeshes(string path)
//         {
//
//             // var a = new ReadSettings();
//             // a.Validation = ValidationMode.TryFix;
//             // a.ImageDecoder = ImageDecoder;
//             var srcModel = ModelRoot.Load(path);
//
//             var templates = srcModel.LogicalScenes
//                 .Select(item => SceneTemplate.Create(item, new RuntimeOptions { IsolateMemory = false }))
//                 .ToArray();
//
//             var srcMeshes = templates
//                 .SelectMany(item => item.LogicalMeshIds)
//                 .Distinct()
//                 .Select(idx => srcModel.LogicalMeshes[idx]);
//
//             return srcMeshes;
//         }
//
//         private List<Mesh> ParseGLTFMeshToNativeMesh(IEnumerable<GLTFMesh> gltfMeshes)
//         {
//             foreach (var gltfMesh in gltfMeshes)
//             {
//                 var vertices = new List<Vertex>();
//                 var indices = new List<uint>();
//                 var textures = new List<Texture>();
//
//                 var gltfMeshPrims = GetValidPrimitives(gltfMesh);
//                 foreach (var gltfMeshPrim in gltfMeshPrims)
//                 {
//                     var positions = gltfMeshPrim.GetVertexAccessor("POSITION").AsVector3Array();
//                     var normals = gltfMeshPrim.GetVertexAccessor("NORMAL").AsVector3Array();
//                     var texCoord0 = gltfMeshPrim.GetVertexAccessor("TEXCOORD_0").AsVector2Array();
//                     // var tangents = gltfMeshPrim.GetVertexAccessor("TANGENT")?.AsVector4Array();
//                     // var color0 = gltfMeshPrim.GetVertexAccessor("COLOR_0")?.AsColorArray();
//                     var triangleIndices = gltfMeshPrim.GetTriangleIndices().ToArray();
//
//                     vertices.AddRange(positions.Select((position, i) => new Vertex
//                     {
//                         Position = position,
//                         Normal = normals[i],
//                         TexCoords = texCoord0[i]
//                     }));
//
//                     indices.AddRange(triangleIndices.SelectMany(i => new[] { (uint) i.A, (uint) i.B, (uint) i.C }));
//
//                     foreach (var texture in GetTextures(gltfMesh.Name, gltfMeshPrim.Material))
//                     {
//                         if (textures.Exists(x => x.Path == texture.Path))
//                             continue;
//
//                         if (!loadedTextures.Contains(texture))
//                         {
//                             texture.Load();
//                             loadedTextures.Add(texture);
//                         }
//
//                         textures.Add(texture);
//                     }
//                 }
//
//                 meshes.Add(new Mesh(vertices.ToArray(), indices.ToArray(), textures.ToArray()));
//             }
//             return meshes;
//         }
//
//         private List<Texture> GetTextures(string meshName, GLTFMaterial gltfMaterial)
//         {
//             var textures = new List<Texture>();
//             foreach (var materialChannel in gltfMaterial.Channels)
//             {
//                 var textureKey = meshName + "_" + materialChannel.Key;
//
//                 if (materialChannel.Texture == null ||
//                     textures.Exists(x => x.Path == textureKey))
//                     continue;
//
//                 var param = materialChannel.Parameters.Aggregate(string.Empty,
//                     (current, parameter) => current + (parameter.Name + " " + parameter.Value + ","));
//
//                 Console.WriteLine(meshName + ", " + textureKey + ", Param: " + param);
//
//                 var type = materialChannel.Key switch
//                 {
//                     "BaseColor" => "albedoMap",
//                     "MetallicRoughness" => "metallicMap",
//                     "Normal" => "normalMap",
//                     _ => string.Empty
//                 };
//
//                 if (type == string.Empty)
//                     continue;
//
//                 var texture = new Texture
//                 {
//                     Path = textureKey,
//                     Type = type,
//                     // Bytes = materialChannel.Texture.PrimaryImage.Content.Content.ToArray()
//                 };
//
//                 textures.Add(texture);
//             }
//
//             return textures;
//         }
//
//         private static IEnumerable<MeshPrimitive> GetValidPrimitives(GLTFMesh gltfMesh)
//         {
//             foreach (var gltfMeshPrim in gltfMesh.Primitives)
//             {
//                 var ppp = gltfMeshPrim.GetVertexAccessor("POSITION");
//                 if (ppp.Count < 3) continue;
//
//                 switch (gltfMeshPrim.DrawPrimitiveType)
//                 {
//                     case PrimitiveType.POINTS:
//                     case PrimitiveType.LINES:
//                     case PrimitiveType.LINE_LOOP:
//                     case PrimitiveType.LINE_STRIP:
//                         continue;
//                     default:
//                         yield return gltfMeshPrim;
//                         break;
//                 }
//             }
//         }
//     }
// }
