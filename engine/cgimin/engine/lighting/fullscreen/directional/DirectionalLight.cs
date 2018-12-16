using cgimin.engine.material;
using cgimin.engine.object3d;
using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using cgimin.engine.camera;
using Engine.cgimin.engine.lighting;
using cgimin.engine.helpers;

namespace Engine.cgimin.engine.material.fullscreen.directional
{
    class DirectionalLight : BaseLighting
    {

        private Vector3 lightDirection;
        private Vector4 lightAmbient;
        private Vector4 lightDiffuse;
        private Vector4 lightSpecular;
        private float lightShininess;

        private int gColorLocation;
        private int gNormalLocation;
        private int gPositionLocation;

        private int cameraPosLocation;

        private int lightDirectionLocation;
        private int lightAmbientColorLocation;
        private int lightDiffuseColorLocation;
        private int lightSpecularColorLocation;
        private int specularShininessLocation;

        public DirectionalLight()
        {
            // Shader-Programm loaded from external files
            Program = ShaderCompiler.CreateShaderProgram("cgimin/engine/lighting/fullscreen/directional/DirectionalLight_VS.glsl",
                                                         "cgimin/engine/lighting/fullscreen/directional/DirectionalLight_FS.glsl");

            // GL.BindAttribLocation, gibt an welcher Index in unserer Datenstruktur welchem "in" Parameter auf unserem Shader zugeordnet wird
            // folgende Befehle müssen aufgerufen werden...
            GL.BindAttribLocation(Program, 0, "in_position");
            GL.BindAttribLocation(Program, 1, "in_normal");
            GL.BindAttribLocation(Program, 2, "in_uv");

            // ...bevor das Shader-Programm "gelinkt" wird.
            GL.LinkProgram(Program);

            gColorLocation = GL.GetUniformLocation(Program, "gColor");
            gNormalLocation = GL.GetUniformLocation(Program, "gNormal");
            gPositionLocation = GL.GetUniformLocation(Program, "gPosition");

            cameraPosLocation = GL.GetUniformLocation(Program, "cameraPosition");

            lightDirectionLocation = GL.GetUniformLocation(Program, "lightDirection");
            lightAmbientColorLocation = GL.GetUniformLocation(Program, "lightAmbientColor");
            lightDiffuseColorLocation = GL.GetUniformLocation(Program, "lightDiffuseColor");
            lightSpecularColorLocation = GL.GetUniformLocation(Program, "lightSpecularColor");
            specularShininessLocation = GL.GetUniformLocation(Program, "specularShininess");

        }


        public void SetProperties(Vector3 direction, Vector4 ambient, Vector4 diffuse, Vector4 specular, float shininess)
        {
            lightDirection = Vector3.Normalize(direction);
            lightAmbient = ambient;
            lightDiffuse = diffuse;
            lightSpecular = specular;
            lightShininess = shininess;
        }

        public void Draw(BaseObject3D object3d, int colorID, int normalTexID, int positionTexID)
        {
            GL.UseProgram(Program);

            // das Vertex-Array-Objekt unseres Objekts wird benutzt
            GL.BindVertexArray(object3d.Vao);

            GL.Uniform1(gColorLocation, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, colorID);

            GL.Uniform1(gNormalLocation, 1);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, normalTexID);

            GL.Uniform1(gPositionLocation, 2);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, positionTexID);

            // Die Licht Parameter werden übergeben
            GL.Uniform3(lightDirectionLocation, lightDirection);
            GL.Uniform4(lightAmbientColorLocation, lightAmbient);
            GL.Uniform4(lightDiffuseColorLocation, lightDiffuse);
            GL.Uniform4(lightSpecularColorLocation, lightSpecular);
            GL.Uniform1(specularShininessLocation, lightShininess);

            GL.Uniform3(cameraPosLocation, Camera.Position);


            // Die Matrix, welche wir als "modelview_projection_matrix" übergeben, wird zusammengebaut:
            // Objekt-Transformation * Kamera-Transformation * Perspektivische Projektion der kamera.
            // Auf dem Shader wird jede Vertex-Position mit dieser Matrix multipliziert. Resultat ist die Position auf dem Screen.
            Matrix4 modelviewProjection = object3d.Transformation * Camera.Transformation * Camera.PerspectiveProjection;

            // Das Objekt wird gezeichnet
            GL.DrawElements(PrimitiveType.Triangles, object3d.Indices.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

            GL.BindVertexArray(0);

        }

       
    }
}
