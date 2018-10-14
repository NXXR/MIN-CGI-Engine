using System;
using OpenTK.Graphics.OpenGL;
using cgimin.engine.object3d;
using OpenTK;
using cgimin.engine.camera;

namespace cgimin.engine.material.simplereflection
{
    public class SimpleReflectionMaterial : BaseMaterial
    {

        private int modelviewProjectionMatrixLocation;

        private int modelviewMatrixLocation;

        public SimpleReflectionMaterial()
        {
            // shader-programm is loaded
            CreateShaderProgram("cgimin/engine/material/simplereflection/SimpleReflection_VS.glsl",
                                "cgimin/engine/material/simplereflection/SimpleReflection_FS.glsl");

            // GL.BindAttribLocation, defines which index of the data-structure is assigned to which "in" parameter 
            GL.BindAttribLocation(Program, 0, "in_position");
            GL.BindAttribLocation(Program, 1, "in_normal");
            GL.BindAttribLocation(Program, 2, "in_uv");
            
            // ...has to be done before the final "link" of the shader-program
            GL.LinkProgram(Program);

            // the location of the "uniform"-paramter "modelview_projection_matrix" on the shader is saved to modelviewProjectionMatrixLocation
            modelviewProjectionMatrixLocation = GL.GetUniformLocation(Program, "modelview_projection_matrix");

            // the location of the "uniform"-paramter "modelview_matrix" on the shader is saved to modelviewMatrixLocation
            modelviewMatrixLocation = GL.GetUniformLocation(Program, "modelview_matrix");

        }

        public void Draw(BaseObject3D object3d, int textureID)
        {
            // set the texture
            GL.BindTexture(TextureTarget.Texture2D, textureID);

            // using the Vertex-Array-Object of out object
            GL.BindVertexArray(object3d.Vao);

            // using our shader
            GL.UseProgram(Program);


            // The matrix which we give as "modelview_projection_matrix" is assembled:
            // object-transformation * camera-transformation * perspective projection of the camera
            // on the shader each vertex-position is multiplied by this matrix. The result is the final position on the screen
            Matrix4 modelviewProjection = object3d.Transformation * Camera.Transformation * Camera.PerspectiveProjection;

            // Matrix is passed to the shader
            GL.UniformMatrix4(modelviewProjectionMatrixLocation, false, ref modelviewProjection);

            // The "modelView-matrix is assembled together
            Matrix4 modelviewMatrix = object3d.Transformation * Camera.Transformation;

            // ... and also passed to the shader
            GL.UniformMatrix4(modelviewMatrixLocation, false, ref modelviewMatrix);

            

            // the object is drawn
            GL.DrawElements(PrimitiveType.Triangles, object3d.Indices.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

        }



    }
}
