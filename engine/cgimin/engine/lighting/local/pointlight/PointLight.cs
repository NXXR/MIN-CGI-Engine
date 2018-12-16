using cgimin.engine.material;
using cgimin.engine.object3d;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using cgimin.engine.camera;
using System;
using cgimin.engine.helpers;
using Engine.cgimin.engine.lighting;

namespace Engine.cgimin.engine.material.local.pointlight
{
    class PointLight : BaseLighting
    {
        private int modelviewProjectionMatrixLocation;
        private int modelMatrixLocation;
        private int objectPositionLocation;
        private int ambientColorLocation;
        private int diffuseColorLocation;
        private int specularColorLocation;
        private int radiusLocation;
        private int cameraPositionLocation;
        private int screenWidthLocation;
        private int screenHeightLocation;
        private int specularShininessLocation;

        private int gNormalLocation;
        private int gPositionLocation;

        public PointLight()
        {
            // Shader-Programm loaded from external files
            Program = ShaderCompiler.CreateShaderProgram("cgimin/engine/lighting/local/pointlight/PointLight_VS.glsl",
                                                         "cgimin/engine/lighting/local/pointlight/PointLight_FS.glsl");

            // GL.BindAttribLocation, gibt an welcher Index in unserer Datenstruktur welchem "in" Parameter auf unserem Shader zugeordnet wird
            // folgende Befehle müssen aufgerufen werden...
            GL.BindAttribLocation(Program, 0, "in_position");
            GL.BindAttribLocation(Program, 1, "in_normal");
            GL.BindAttribLocation(Program, 2, "in_uv");

            // ...bevor das Shader-Programm "gelinkt" wird.
            GL.LinkProgram(Program);

            gNormalLocation = GL.GetUniformLocation(Program, "gNormal");
            gPositionLocation = GL.GetUniformLocation(Program, "gPosition");

            modelviewProjectionMatrixLocation = GL.GetUniformLocation(Program, "modelviewProjectionMatrix");
            modelMatrixLocation = GL.GetUniformLocation(Program, "modelMatrix");
            objectPositionLocation = GL.GetUniformLocation(Program, "objectPosition");
            ambientColorLocation = GL.GetUniformLocation(Program, "ambientColor");
            diffuseColorLocation = GL.GetUniformLocation(Program, "diffuseColor");
            specularColorLocation = GL.GetUniformLocation(Program, "specularColor");
            specularShininessLocation = GL.GetUniformLocation(Program, "specularShininess");
            radiusLocation = GL.GetUniformLocation(Program, "radius");
            cameraPositionLocation = GL.GetUniformLocation(Program, "cameraPosition");

            screenWidthLocation = GL.GetUniformLocation(Program, "screenWidth");
            screenHeightLocation = GL.GetUniformLocation(Program, "screenHeight");

        }


        public void Draw(BaseObject3D sphereObj, Vector3 pos, float shininess, Vector3 ambientColor, Vector3 diffuseColor, Vector3 specularColor, float radius, int normalTexID, int positionTexID, int screenWidth, int screenHeight)
        {
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            GL.UseProgram(Program);

            GL.BindVertexArray(sphereObj.Vao);

            GL.Uniform1(gNormalLocation, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, normalTexID);

            GL.Uniform1(gPositionLocation, 1);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, positionTexID);

            Matrix4 model = Matrix4.Identity;
            model *= Matrix4.CreateScale(radius, radius, radius);
            model *= Matrix4.CreateTranslation(pos);

            GL.UniformMatrix4(modelMatrixLocation, false, ref model);
            sphereObj.Transformation = model;

            Matrix4 modelViewProjection = model * Camera.Transformation * Camera.PerspectiveProjection;
            GL.UniformMatrix4(modelviewProjectionMatrixLocation, false, ref modelViewProjection);

            GL.Uniform3(objectPositionLocation, pos);

            GL.Uniform1(radiusLocation, radius);
            GL.Uniform3(cameraPositionLocation, Camera.Position);
            GL.Uniform3(ambientColorLocation, ambientColor);
            GL.Uniform3(diffuseColorLocation, diffuseColor);
            GL.Uniform3(specularColorLocation, specularColor);
            GL.Uniform1(specularShininessLocation, shininess);

            GL.Uniform1(screenWidthLocation, (float)screenWidth);
            GL.Uniform1(screenHeightLocation, (float)screenHeight);

            // light is drawn
            GL.DrawElements(PrimitiveType.Triangles, sphereObj.Indices.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

            GL.BindVertexArray(0);

        }


    }
}
