using cgimin.engine.object3d;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using cgimin.engine.camera;
using System;
using cgimin.engine.helpers;
using Engine.cgimin.engine.lighting;
using cgimin.engine.deferred;

namespace Engine.cgimin.engine.lighting.local.pointlightPBR
{
    class PointLightPBR : BaseLighting
    {

        private int modelviewProjectionMatrixLocation;

        private int midPositionLocation;
        private int radiusLocation;
        private int colorLocation;
        private int cameraPosLocation;

        private int gNormalLocation;
        private int gPositionLocation;
        private int GColorAndRoughnessLocation;
        private int GMetalnessAndShadowLocation;

        private int screenWidthLocation;
        private int screenHeightLocation;

        public PointLightPBR()
        {
            // Shader-Programm wird aus den externen Files generiert...
            Program = ShaderCompiler.CreateShaderProgram("cgimin/engine/lighting/local/pointlightPBR/PointLightPBR_VS.glsl",
                                                         "cgimin/engine/lighting/local/pointlightPBR/PointLightPBR_FS.glsl");

            // GL.BindAttribLocation, gibt an welcher Index in unserer Datenstruktur welchem "in" Parameter auf unserem Shader zugeordnet wird
            // folgende Befehle müssen aufgerufen werden...
            GL.BindAttribLocation(Program, 0, "in_position");
            GL.BindAttribLocation(Program, 1, "in_normal");
            GL.BindAttribLocation(Program, 2, "in_uv");

            // ...bevor das Shader-Programm "gelinkt" wird.
            GL.LinkProgram(Program);

            // Die Stelle an der im Shader der per "uniform" der Input-Paremeter "modelview_projection_matrix" definiert wird, wird ermittelt.
            modelviewProjectionMatrixLocation = GL.GetUniformLocation(Program, "modelview_projection_matrix");
            midPositionLocation = GL.GetUniformLocation(Program, "midPosition");
            radiusLocation = GL.GetUniformLocation(Program, "radius");
            colorLocation = GL.GetUniformLocation(Program, "color");
            gNormalLocation = GL.GetUniformLocation(Program, "GNormal");
            gPositionLocation = GL.GetUniformLocation(Program, "GPosition");
            GColorAndRoughnessLocation = GL.GetUniformLocation(Program, "GColorAndRoughness");
            GMetalnessAndShadowLocation = GL.GetUniformLocation(Program, "GMetalness");

            screenWidthLocation = GL.GetUniformLocation(Program, "screenWidth");
            screenHeightLocation = GL.GetUniformLocation(Program, "screenHeight");

            cameraPosLocation = GL.GetUniformLocation(Program, "camera_position");

        }

        public void PrepareDraw(BaseObject3D object3d)
        {
            // Textur wird "gebunden"
            GL.BindVertexArray(object3d.Vao);

            // Unser Shader Programm wird benutzt
            GL.UseProgram(Program);

            GL.Uniform1(gNormalLocation, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, DeferredRendering.GNormalBuffer);

            GL.Uniform1(gPositionLocation, 1);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, DeferredRendering.GPositionBuffer);

            GL.Uniform1(GColorAndRoughnessLocation, 2);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, DeferredRendering.GColorRoughnessBuffer);

            GL.Uniform1(GMetalnessAndShadowLocation, 3);
            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, DeferredRendering.GMetalnessShadowBuffer);

        }

        public void FinishDraw()
        {
            // Unbinden des Vertex-Array-Objekt damit andere Operation nicht darauf basieren
            GL.BindVertexArray(0);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, 1);

            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, 2);

            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, 3);

        }

        public void Draw(BaseObject3D object3d, Vector3 position, float radius, Vector3 color, int screenWidth, int screenHeight)
        {
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);


            GL.BindVertexArray(object3d.Vao);

            // Unser Shader Programm wird benutzt
            GL.UseProgram(Program);

            GL.Uniform1(gNormalLocation, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, DeferredRendering.GNormalBuffer);

            GL.Uniform1(gPositionLocation, 1);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, DeferredRendering.GPositionBuffer);

            GL.Uniform1(GColorAndRoughnessLocation, 2);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, DeferredRendering.GColorRoughnessBuffer);

            GL.Uniform1(GMetalnessAndShadowLocation, 3);
            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, DeferredRendering.GMetalnessShadowBuffer);

            Matrix4 model = Matrix4.Identity;
            model *= Matrix4.CreateScale(radius, radius, radius);
            model *= Matrix4.CreateTranslation(position);

            Matrix4 modelviewProjection = model * Camera.Transformation * Camera.PerspectiveProjection;

            // Die Matrix wird dem Shader als Parameter übergeben
            GL.UniformMatrix4(modelviewProjectionMatrixLocation, false, ref modelviewProjection);

            Vector3 midPos = new Vector3(0, 0, 0);
            midPos = Vector3.TransformPosition(midPos, model);
            GL.Uniform3(midPositionLocation, ref midPos);

            GL.Uniform1(radiusLocation, radius);
            GL.Uniform3(colorLocation, color);

            GL.Uniform1(screenWidthLocation, (float)screenWidth);
            GL.Uniform1(screenHeightLocation, (float)screenHeight);

            GL.Uniform3(cameraPosLocation, new Vector3(Camera.Position.X, Camera.Position.Y, Camera.Position.Z));

            // Das Objekt wird gezeichnet
            GL.DrawElements(PrimitiveType.Triangles, object3d.Indices.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

            GL.BindVertexArray(0);
        }


    }
}
