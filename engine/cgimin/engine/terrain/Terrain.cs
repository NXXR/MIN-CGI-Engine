using cgimin.engine.camera;
using cgimin.engine.light;
using cgimin.engine.helpers;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace Engine.cgimin.engine.terrain
{
    public class Terrain
    {

        private struct TerrainTile
        {
            public int Vao;
            public int IndexCount;

            public Vector3 midPoint;
        }

        private List<TerrainTile> tiles;

        private int program;

        private int modelviewProjectionMatrixLocation;
        private int camSubPositionLocation;

        private int terrainXZPosLocation;
        private int terrainSizeLocation;

        private int heightMapLocation;
        private int colorTextureLocation;
        private int normalTextureLocation;
        private int textureScaleLocation;

        private int materialShininessLocation;
        private int lightDirectionLocation;
        private int lightAmbientLocation;
        private int lightDiffuseLocation;
        private int lightSpecularLocation;


        public Terrain()
        {
            tiles = new List<TerrainTile>();

            for (int xTile = -10; xTile < 11; xTile++)
            {
                for (int zTile = -10; zTile < 11; zTile++)
                {
                    int lod = (int)Math.Sqrt(xTile * xTile + zTile * zTile);
                    //lod -= 2;
                    if (lod < 1) lod = 1;
                    lod = lod * lod;
                    if (lod > 8) lod = 8;

                    tiles.Add(generateTerrainTile(xTile * 64, zTile * 64, lod));
                }
            }
            CreateTerrainShaders();
        }


        private TerrainTile generateTerrainTile(float xMargin, float zMargin, int lod)
        {
            TerrainTile returnTile = new TerrainTile();
            List<float> tarrainData = new List<float>();
            List<int> indices = new List<int>();

            returnTile.midPoint = new Vector3(xMargin, 0, zMargin);

            int ind = 0;
            int loopStart = -32 / lod;
            int loopEnd = 32 / lod + 1;
            int indexSecRowLeft = 64 / lod + 1;
            int indexSecRowRight = 64 / lod + 2;
            int lastRowCompare = 32 / lod;

            for (int x = loopStart; x < loopEnd; x++)
            {
                for (int z = loopStart; z < loopEnd; z++)
                {
                    tarrainData.Add(x * lod + xMargin);
                    tarrainData.Add(0);
                    tarrainData.Add(z * lod + zMargin);

                    if (x < lastRowCompare && z < lastRowCompare)
                    {
                        indices.Add(ind);
                        indices.Add(ind + indexSecRowLeft);
                        indices.Add(ind + indexSecRowRight);

                        indices.Add(ind);
                        indices.Add(ind + indexSecRowRight);
                        indices.Add(ind + 1);
                    }
                    ind++;
                }
            }

            returnTile.IndexCount = indices.Count;

            int terrainVBO;
            GL.GenBuffers(1, out terrainVBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, terrainVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(tarrainData.Count * sizeof(float)), tarrainData.ToArray(), BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            int indexBuffer;
            GL.GenBuffers(1, out indexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, new IntPtr(sizeof(uint) * indices.Count), indices.ToArray(), BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            GL.GenVertexArrays(1, out returnTile.Vao);
            GL.BindVertexArray(returnTile.Vao);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
            GL.BindBuffer(BufferTarget.ArrayBuffer, terrainVBO);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, true, Vector3.SizeInBytes, 0);
            GL.BindVertexArray(0);

            return returnTile;
        }


        private void CreateTerrainShaders()
        {
            program = ShaderCompiler.CreateShaderProgram("cgimin/engine/terrain/Terrain_VS.glsl", "cgimin/engine/terrain/Terrain_FS.glsl");

            GL.BindAttribLocation(program, 0, "in_position");
            GL.LinkProgram(program);

            modelviewProjectionMatrixLocation = GL.GetUniformLocation(program, "modelview_projection_matrix");
            camSubPositionLocation = GL.GetUniformLocation(program, "cam_sub_position");

            terrainXZPosLocation = GL.GetUniformLocation(program, "xzPos");
            terrainSizeLocation = GL.GetUniformLocation(program, "terrain_size");


            materialShininessLocation = GL.GetUniformLocation(program, "specular_shininess");

            lightDirectionLocation = GL.GetUniformLocation(program, "light_direction");
            lightAmbientLocation = GL.GetUniformLocation(program, "light_ambient_color");
            lightDiffuseLocation = GL.GetUniformLocation(program, "light_diffuse_color");
            lightSpecularLocation = GL.GetUniformLocation(program, "light_specular_color");
            heightMapLocation = GL.GetUniformLocation(program, "height_map");
            colorTextureLocation = GL.GetUniformLocation(program, "color_texture");
            normalTextureLocation = GL.GetUniformLocation(program, "normalmap_texture");
            textureScaleLocation = GL.GetUniformLocation(program, "texture_scale");

        }

        public void Draw(int heightMapTextureID, int terrainTextureSize, int textureID, int normalTextureID, float textureScale, float shininess)
        {

            GL.BindTexture(TextureTarget.Texture2D, heightMapTextureID);

            GL.UseProgram(program);

            // Höhen-Textur wird "gebunden"
            GL.Uniform1(colorTextureLocation, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, heightMapTextureID);

            // Farb-Textur wird "gebunden"
            GL.Uniform1(colorTextureLocation, 1);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, textureID);

            // Normalmap-Textur wird "gebunden"
            GL.Uniform1(normalTextureLocation, 2);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, normalTextureID);

            // Die Licht Parameter werden übergeben
            GL.Uniform3(lightDirectionLocation, Light.lightDirection);
            GL.Uniform4(lightAmbientLocation, Light.lightAmbient);
            GL.Uniform4(lightDiffuseLocation, Light.lightDiffuse);
            GL.Uniform4(lightSpecularLocation, Light.lightSpecular);

            // Shininess
            GL.Uniform1(materialShininessLocation, shininess);

            // Die Transformierung des Terrians, abhängig von der Kamera
            Matrix4 terrainTransformation = Camera.Transformation;
            // Die Position wird abgezogen, das Terrain "rotiert" lediglich mit der Kamera. Die "Positionierung" erfolgt auf dem Vertex-Shader 
            terrainTransformation *= Matrix4.CreateTranslation(-Camera.Transformation.M41, -Camera.Transformation.M42, -Camera.Transformation.M43);
            // Die Translation innerhalb der höchsten Kachel-Einheit 8 wird erstellt... 
            Vector4 camInTilePart = new Vector4(-((Camera.Position.X) % 8.0f), -Camera.Position.Y, -((Camera.Position.Z) % 8.0f), 1);
            // ... multipliziert mit der Terrain-Transformation...
            camInTilePart *= terrainTransformation;
            // ... und wieder zur Terrain-Transformation hinzugefügt-
            terrainTransformation *= Matrix4.CreateTranslation(camInTilePart.X, camInTilePart.Y, camInTilePart.Z);

            // ModelView-Projection in diesem Fall nur die terrainTransfomration (in der die Kamera rotation enthalten ist) *  Perspective-Projection  
            Matrix4 modelviewProjection = terrainTransformation * Camera.PerspectiveProjection;
            GL.UniformMatrix4(modelviewProjectionMatrixLocation, false, ref modelviewProjection);

            // Die ModelView-Matrix wird ebenfalls übergeben
            GL.Uniform3(camSubPositionLocation, new Vector3(((Camera.Position.X) % 8.0f), Camera.Position.Y, ((Camera.Position.Z) % 8.0f)));

            // Die XZ Postition für den Look-Up für die Höhe des Terrains wird berechnet. Jeweils die Kamera-Position - 
            Vector2 texXZPos = new Vector2(Camera.Position.X - ((Camera.Position.X) % 8.0f), Camera.Position.Z - ((Camera.Position.Z) % 8.0f));
            GL.Uniform2(terrainXZPosLocation, ref texXZPos);

            // Die Dimension der Height-Map angeben
            GL.Uniform1(terrainSizeLocation, (float)terrainTextureSize);

            // Die Skalierung der Oberflcäehntextur
            GL.Uniform1(textureScaleLocation, textureScale);


            for (int i = 0; i < tiles.Count; i++)
            {
                GL.BindVertexArray(tiles[i].Vao);
                GL.DrawElements(PrimitiveType.Triangles, tiles[i].IndexCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
            }


            GL.BindVertexArray(0);

        }



    }
}
