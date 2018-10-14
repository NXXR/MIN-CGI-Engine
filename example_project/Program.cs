

#region --- Using Directives ---

using System;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using cgimin.engine.object3d;
using cgimin.engine.texture;
using cgimin.engine.material.simpletexture;
using cgimin.engine.material.wobble1;
using cgimin.engine.material.simplereflection;
using cgimin.engine.camera;

#endregion --- Using Directives ---

namespace Examples.Tutorial
{

    public class CubeExample : GameWindow
    {

        // das Beispiel-Objekt
        private ObjLoaderObject3D exampleObject;

        // unsere textur-ID
        private int textureID;

        // Materialien
        private SimpleReflectionMaterial simpleReflectionMaterial;
        private SimpleTextureMaterial simpleTextureMaterial;
        private Wobble1Material wobble1Material;
        private Wobble2Material wobble2Material;

        private int updateCounter = 0;

        public CubeExample()
            : base(800, 600, new GraphicsMode(), "CGI-MIN Example", 0, DisplayDevice.Default, 3, 0, GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug)
        { }

   
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Initialize Camera
            Camera.Init();
            Camera.SetWidthHeightFov(800, 600, 60);

            // Loading the object
            exampleObject = new ObjLoaderObject3D("data/objects/torus_smooth.obj");

            // Loading the texture
            textureID = TextureManager.LoadTexture("data/textures/simple_reflection.png");

            // initialize materials
            simpleReflectionMaterial = new SimpleReflectionMaterial();
            simpleTextureMaterial = new SimpleTextureMaterial();
            wobble1Material = new Wobble1Material();
            wobble2Material = new Wobble2Material();

            // enebale z-buffer
            GL.Enable(EnableCap.DepthTest);

            // set camera position
            Camera.SetLookAt(new Vector3(0, 0, 3), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
        }
 

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (Keyboard[OpenTK.Input.Key.Escape])
                this.Exit();

            if (Keyboard[OpenTK.Input.Key.F11])
                if (WindowState != WindowState.Fullscreen)
                    WindowState = WindowState.Fullscreen;
                else
                    WindowState = WindowState.Normal;

            // updateCounter ist für den Animationsfortschritt zuständig
            updateCounter++;
        }


        protected override void OnRenderFrame(FrameEventArgs e)
        {
            // the screen and the depth-buffer are cleared
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // rotation of object, around x-axis
            exampleObject.Transformation = Matrix4.CreateRotationX(updateCounter / 50.0f);

            // around y-axis
            exampleObject.Transformation *= Matrix4.CreateRotationY(updateCounter / 110.0f);

            // objekt is drawn
            simpleReflectionMaterial.Draw(exampleObject, textureID);

            SwapBuffers();
        }



        protected override void OnUnload(EventArgs e)
        {
            exampleObject.UnLoad();
        }


        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            Camera.SetWidthHeightFov(Width, Height, 60);
        }


        [STAThread]
        public static void Main()
        {
            using (CubeExample example = new CubeExample())
            {
                example.Run(60.0, 0.0);
            }
        }

      
    }
}

