﻿using cgimin.engine.material;
using cgimin.engine.object3d;
using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using cgimin.engine.camera;

namespace Engine.cgimin.engine.material.simpleblend
{
    public class SimpleBlendMaterial : BaseMaterial
    {

        private int modelviewProjectionMatrixLocation;

        public SimpleBlendMaterial()
        {
            // Shader-Programm loaded from external files
            CreateShaderProgram("cgimin/engine/material/simple/simpleblend/Simple_VS.glsl",
                                "cgimin/engine/material/simple/simpleblend/Simple_FS.glsl");

            // GL.BindAttribLocation, gibt an welcher Index in unserer Datenstruktur welchem "in" Parameter auf unserem Shader zugeordnet wird
            // folgende Befehle müssen aufgerufen werden...
            GL.BindAttribLocation(Program, 0, "in_position");
            GL.BindAttribLocation(Program, 1, "in_normal");
            GL.BindAttribLocation(Program, 2, "in_uv");

            // ...bevor das Shader-Programm "gelinkt" wird.
            GL.LinkProgram(Program);

            // Die Stelle an der im Shader der per "uniform" der Input-Paremeter "modelview_projection_matrix" definiert wird, wird ermittelt.
            modelviewProjectionMatrixLocation = GL.GetUniformLocation(Program, "modelview_projection_matrix");

            // set sort flag to true
            this.SortFarToNear = true;

        }

        public void Draw(BaseObject3D object3d, int textureID, BlendingFactor srcBlendFactor, BlendingFactor destBlendFactor)
        {
            GL.BlendFunc(srcBlendFactor, destBlendFactor);

            // das Vertex-Array-Objekt unseres Objekts wird benutzt
            GL.BindVertexArray(object3d.Vao);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureID);

            // Die Matrix, welche wir als "modelview_projection_matrix" übergeben, wird zusammengebaut:
            // Objekt-Transformation * Kamera-Transformation * Perspektivische Projektion der kamera.
            // Auf dem Shader wird jede Vertex-Position mit dieser Matrix multipliziert. Resultat ist die Position auf dem Screen.
            Matrix4 modelviewProjection = object3d.Transformation * Camera.Transformation * Camera.PerspectiveProjection;

            // Die Matrix wird dem Shader als Parameter übergeben
            GL.UniformMatrix4(modelviewProjectionMatrixLocation, false, ref modelviewProjection);

            // Das Objekt wird gezeichnet
            GL.DrawElements(PrimitiveType.Triangles, object3d.Indices.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
            
            GL.BindVertexArray(0);

        }

        protected override void PreDraw() 
        {
            GL.Enable(EnableCap.Blend);
            
            // Unser Shader Programm wird benutzt
            GL.UseProgram(Program);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);
        }

        protected override void PostDraw() 
        {
            GL.Disable(EnableCap.Blend);
        }

        // implementatin for octree drawing logic
        protected override void DrawWithSettings(BaseObject3D object3d, MaterialSettings settings)
        {
            Draw(object3d, settings.colorTexture, settings.SrcBlendFactor, settings.DestBlendFactor);
        }

    }


}