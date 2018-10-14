using System;
using OpenTK.Graphics.OpenGL;
using cgimin.engine.object3d;
using OpenTK;
using cgimin.engine.camera;

namespace cgimin.engine.material.wobble1
{
    public class Wobble2Material : BaseMaterial
    {

        private int modelviewProjectionMatrixLocation;
        private int wobbleValueLocation;

        private int drawUpdate;

        public Wobble2Material()
        {

            // Shader-Programm wird aus den externen Files generiert...
            CreateShaderProgram("cgimin/engine/material/wobble2/Wobble2_VS.glsl",
                                "cgimin/engine/material/wobble2/Wobble2_FS.glsl");

            // GL.BindAttribLocation, gibt an welcher Index in unserer Datenstruktur welchem "in" Parameter auf unserem Shader zugeordnet wird
            // folgende Befehle müssen aufgerufen werden...
            GL.BindAttribLocation(Program, 0, "in_position");
            GL.BindAttribLocation(Program, 1, "in_normal");
            GL.BindAttribLocation(Program, 2, "in_uv");

            // ...bevor das Shader-Programm "gelinkt" wird.
            GL.LinkProgram(Program);

            // Die Stelle an der im Shader der per "uniform" der Input-Paremeter "modelview_projection_matrix" definiert wird, wird ermittelt.
            modelviewProjectionMatrixLocation = GL.GetUniformLocation(Program, "modelview_projection_matrix");

            // Die Stelle an der im Shader per "uniform" der "wobbleValue" Parameter definiert wird, wird ermittelt.
            wobbleValueLocation = GL.GetUniformLocation(Program, "wobbleValue");

        }


        public void Draw(BaseObject3D object3d, int textureID)
        {
            // Textur wird "gebunden"
            GL.BindTexture(TextureTarget.Texture2D, textureID);

            // das Vertex-Array-Objekt unseres Objekts wird benutzt
            GL.BindVertexArray(object3d.Vao);

            // Unser Shader Programm wird benutzt
            GL.UseProgram(Program);

            // Die Matrix, welche wir als "modelview_projection_matrix" übergeben, wird zusammengebaut:
            // Objekt-Transformation * Kamera-Transformation * Perspektivische Projektion der kamera.
            // Auf dem Shader wird jede Vertex-Position mit dieser Matrix multipliziert. Resultat ist die Position auf dem Screen.
            Matrix4 modelviewProjection = object3d.Transformation * Camera.Transformation * Camera.PerspectiveProjection;

            // Die Matrix wird dem Shader als Parameter übergeben
            GL.UniformMatrix4(modelviewProjectionMatrixLocation, false, ref modelviewProjection);

            drawUpdate++;  // updates gehören nicht in Draw :o 

            // der Shader-Parameter "wobbleValue" wird dem Shader übergeben. In diesem Fall einfach ein Wert der immer höher gezählt wird.            
            GL.Uniform1(wobbleValueLocation, drawUpdate / 10.0f);

            // Das Objekt wird gezeichnet
            GL.DrawElements(PrimitiveType.Triangles, object3d.Indices.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

        }



    }
}

