using System;
using cgimin.engine.object3d;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using cgimin.engine.helpers;

namespace Engine.cgimin.engine.postprocessing.blur
{
    public class BlurFullscreenMaterial : BasePostprocessing
    {

        private int shiftLocation;
        private int targetLocation;

        public BlurFullscreenMaterial()
        {
            // Shader-Programm wird aus den externen Files generiert...
            Program = ShaderCompiler.CreateShaderProgram("cgimin/engine/postprocessing/blur/BlurFullscreen_VS.glsl", "cgimin/engine/postprocessing/blur/BlurFullscreen_FS.glsl");

            // GL.BindAttribLocation, gibt an welcher Index in unserer Datenstruktur welchem "in" Parameter auf unserem Shader zugeordnet wird
            // folgende Befehle müssen aufgerufen werden...
            GL.BindAttribLocation(Program, 0, "in_position");
            GL.BindAttribLocation(Program, 1, "in_normal");
            GL.BindAttribLocation(Program, 2, "in_uv");
            
            // ...bevor das Shader-Programm "gelinkt" wird.
            GL.LinkProgram(Program);

            shiftLocation  = GL.GetUniformLocation(Program, "shift");
            targetLocation = GL.GetUniformLocation(Program, "target");
        }


        public void Draw(BaseObject3D object3d, int textureID, int target, float xShift, float yShift)
        {
            // Textur wird "gebunden"
            GL.BindTexture(TextureTarget.Texture2D, textureID);

            // das Vertex-Array-Objekt unseres Objekts wird benutzt
            GL.BindVertexArray(object3d.Vao);

            // Unser Shader Programm wird benutzt
            GL.UseProgram(Program);

            GL.Uniform2(shiftLocation, new Vector2(xShift, yShift));

            GL.Uniform1(targetLocation, target);

            // Das Objekt wird gezeichnet
            GL.DrawElements(PrimitiveType.Triangles, object3d.Indices.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

			// Unbinden des Vertex-Array-Objekt damit andere Operation nicht darauf basieren
			GL.BindVertexArray(0);
        }





    }
}
