using System;
using cgimin.engine.object3d;
using OpenTK.Graphics.OpenGL;
using cgimin.engine.helpers;

namespace Engine.cgimin.engine.postprocessing.blur
{
    public class BlurHorizontalMaterial : BasePostprocessing
    {

        private int samplerLocation;

        public BlurHorizontalMaterial()
        {
            // Shader-Programm wird aus den externen Files generiert...
            Program = ShaderCompiler.CreateShaderProgram("cgimin/engine/postprocessing/blur/BlurHorizontal_VS.glsl", "cgimin/engine/postprocessing/blur/BlurHorizontal_FS.glsl");

            // GL.BindAttribLocation, gibt an welcher Index in unserer Datenstruktur welchem "in" Parameter auf unserem Shader zugeordnet wird
            // folgende Befehle müssen aufgerufen werden...
            GL.BindAttribLocation(Program, 0, "in_position");
            GL.BindAttribLocation(Program, 1, "in_normal");
            GL.BindAttribLocation(Program, 2, "in_uv");
            
            // ...bevor das Shader-Programm "gelinkt" wird.
            GL.LinkProgram(Program);

            samplerLocation = GL.GetUniformLocation(Program, "sampler");

            //shiftLocation  = GL.GetUniformLocation(Program, "shift");
            //targetLocation = GL.GetUniformLocation(Program, "target");
        }

        public void Draw(BaseObject3D object3d, int textureID)
        {
            // Textur wird "gebunden"
            //GL.BindTexture(TextureTarget.Texture2D, textureID);

            // das Vertex-Array-Objekt unseres Objekts wird benutzt
            GL.BindVertexArray(object3d.Vao);

            // Unser Shader Programm wird benutzt
            GL.UseProgram(Program);

            GL.Uniform1(samplerLocation, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureID);

            // Das Objekt wird gezeichnet
            GL.DrawElements(PrimitiveType.Triangles, object3d.Indices.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

			// Unbinden des Vertex-Array-Objekt damit andere Operation nicht darauf basieren
			GL.BindVertexArray(0);
        }


    }
}
