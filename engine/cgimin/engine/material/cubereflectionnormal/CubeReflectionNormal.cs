using System;
using cgimin.engine.camera;
using cgimin.engine.object3d;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace cgimin.engine.material.cubereflectionnormal
{
    public class CubeReflectionNormalMaterial : BaseMaterial
    {

        private int modelviewProjectionMatrixLocation;
        private int modelMatrixLocation;
        private int cameraPositionLocation;
        private int cubeTextureLocation;
        private int normalTextureLocation;

        public CubeReflectionNormalMaterial()
        {
            // Shader-Programm wird aus den externen Files generiert...
            CreateShaderProgram("cgimin/engine/material/cubereflectionnormal/CubeReflectionNormal_VS.glsl",
                                "cgimin/engine/material/cubereflectionnormal/CubeReflectionNormal_FS.glsl");

            // GL.BindAttribLocation, gibt an welcher Index in unserer Datenstruktur welchem "in" Parameter auf unserem Shader zugeordnet wird
            // folgende Befehle müssen aufgerufen werden...
            GL.BindAttribLocation(Program, 0, "in_position");
            GL.BindAttribLocation(Program, 1, "in_normal");
            GL.BindAttribLocation(Program, 2, "in_uv");
            GL.BindAttribLocation(Program, 3, "in_tangent");
            GL.BindAttribLocation(Program, 4, "in_bitangent");

            // ...bevor das Shader-Programm "gelinkt" wird.
            GL.LinkProgram(Program);

            // Die Stelle an der im Shader der per "uniform" der Input-Paremeter "modelview_projection_matrix" definiert wird, wird ermittelt.
            modelviewProjectionMatrixLocation = GL.GetUniformLocation(Program, "modelview_projection_matrix");

            // Die Stelle für die den "model_matrix" - Parameter wird ermittelt.
            modelMatrixLocation = GL.GetUniformLocation(Program, "model_matrix");

            // Die Stelle für die Kamera Position.
            cameraPositionLocation = GL.GetUniformLocation(Program, "camera_position");
            cubeTextureLocation = GL.GetUniformLocation(Program, "sampler");
            normalTextureLocation = GL.GetUniformLocation(Program, "normalmap_texture");

        }

        public void Draw(BaseObject3D object3d, int normalTextureID, int cubemapTextureID)
        {
            // Textur wird "gebunden"
            //GL.BindTexture(TextureTarget.TextureCubeMap, cubemapTextureID);

            // das Vertex-Array-Objekt unseres Objekts wird benutzt
            GL.BindVertexArray(object3d.Vao);

            // Unser Shader Programm wird benutzt
            GL.UseProgram(Program);

            // Cube Mapping -Textur wird "gebunden"
            GL.Uniform1(cubeTextureLocation, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, cubemapTextureID);

            // Normalmap-Textur wird "gebunden"
            GL.Uniform1(normalTextureLocation, 1);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, normalTextureID);

            // Die Matrix, welche wir als "modelview_projection_matrix" übergeben, wird zusammengebaut:
            // Objekt-Transformation * Kamera-Transformation * Perspektivische Projektion der kamera.
            // Auf dem Shader wird jede Vertex-Position mit dieser Matrix multipliziert. Resultat ist die Position auf dem Screen.
            Matrix4 modelviewProjection = object3d.Transformation * Camera.Transformation * Camera.PerspectiveProjection;

            // Die Matrix wird dem Shader als Parameter übergeben
            GL.UniformMatrix4(modelviewProjectionMatrixLocation, false, ref modelviewProjection);

            // Die Matrix, welche wir als "model_matrix" übergeben, wird zusammengebaut
            Matrix4 modelMatrix = object3d.Transformation;

            // Die Matrix wird dem Shader als Parameter übergeben
            GL.UniformMatrix4(modelMatrixLocation, false, ref modelMatrix);

            // Positions Parameter
            GL.Uniform4(cameraPositionLocation, new Vector4(Camera.Position.X, Camera.Position.Y, Camera.Position.Z, 1));

            // Das Objekt wird gezeichnet
            GL.DrawElements(PrimitiveType.Triangles, object3d.Indices.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

            // Unbinden des Vertex-Array-Objekt damit andere Operation nicht darauf basieren
            GL.BindVertexArray(0);
        }


        // implementatin for octree drawing logic
        public override void DrawWithSettings(BaseObject3D object3d, MaterialSettings settings)
        {
            Draw(object3d, settings.normalTexture, settings.cubeTexture);
        }



    }
}
