using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using cgimin.engine.object3d;
using System.IO;
using Engine.cgimin.engine.octree;

namespace cgimin.engine.material
{
	public abstract class BaseMaterial
	{
		// struct contains all possible options dor each material 
		public struct MaterialSettings
		{
			public int colorTexture;
			public int normalTexture;
			public int cubeTexture;
			public float shininess;

			// values for blending
			public BlendingFactor SrcBlendFactor;

			public BlendingFactor DestBlendFactor;
		}

		private readonly List<OctreeEntity> objectsToDraw;

		private int VertexObject;
		private int FragmentObject;

		protected int Program;

        // flag, if this material needs tp be sorted
        protected bool SortFarToNear = false;

		protected BaseMaterial()
		{
			objectsToDraw = new List<OctreeEntity>();
		}

		protected void CreateShaderProgram(string pathVS, string pathFS)
		{
			// shader files are read (text)
			string vs = File.ReadAllText(pathVS);
			string fs = File.ReadAllText(pathFS);

			int status_code;
			string info;

			// vertex and fragment shaders are created
			VertexObject = GL.CreateShader(ShaderType.VertexShader);
			FragmentObject = GL.CreateShader(ShaderType.FragmentShader);

			// compiling vertex-shader 
			GL.ShaderSource(VertexObject, vs);
			GL.CompileShader(VertexObject);
			GL.GetShaderInfoLog(VertexObject, out info);
			GL.GetShader(VertexObject, ShaderParameter.CompileStatus, out status_code);

			if (status_code != 1)
				throw new ApplicationException(info);

			// compiling fragment shader
			GL.ShaderSource(FragmentObject, fs);
			GL.CompileShader(FragmentObject);
			GL.GetShaderInfoLog(FragmentObject, out info);
			GL.GetShader(FragmentObject, ShaderParameter.CompileStatus, out status_code);

			if (status_code != 1)
				throw new ApplicationException(info);

			// final shader program is created using rhw fragment and the vertex program
			Program = GL.CreateProgram();
			GL.AttachShader(Program, FragmentObject);
			GL.AttachShader(Program, VertexObject);

			// hint: "Program" is not linked yet
		}

		/// <summary>
		/// Override to set all States that are shared between the objects.
		/// </summary>
		protected abstract void PreDraw();
		
		// abstract, to force each material to implement
		protected abstract void DrawWithSettings(BaseObject3D object3d, MaterialSettings settings);

		
		/// <summary>
		/// Override to reset all States that are set in PreDraw().
		/// </summary>
		protected abstract void PostDraw();
		
		/// <summary>
		/// Adds an entity to be drawn this frame.
		/// </summary>
		/// <param name="entity">The entity to be drawn this frame.</param>
		public void RegisterForDraw(OctreeEntity entity)
		{
			objectsToDraw.Add(entity);
		}

		/// <summary>
		/// Draws all objects, that are registered to be drawn this frame.
		/// </summary>
		public void DrawAll()
		{
			PreDraw();

            if (SortFarToNear)
            {
                foreach (OctreeEntity entity in objectsToDraw)
                    entity.SquaredCamDistance = (entity.Transform.ExtractTranslation() - camera.Camera.Position).LengthSquared;

                objectsToDraw.Sort((x, y) => y.SquaredCamDistance.CompareTo(x.SquaredCamDistance));
            }

            for (var i = 0; i < objectsToDraw.Count; i++)
            {
                var entity = objectsToDraw[i];
                entity.Object3d.Transformation = entity.Transform;
                DrawWithSettings(entity.Object3d, entity.MaterialSetting);
            }
             
            objectsToDraw.Clear();
			PostDraw();
		}
	}
}