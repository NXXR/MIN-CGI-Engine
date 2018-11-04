

#region --- Using Directives ---

using System;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using cgimin.engine.object3d;
using cgimin.engine.texture;
using cgimin.engine.material.simpletexture;
using cgimin.engine.camera;
using cgimin.engine.material.normalmapping;
using cgimin.engine.light;
using cgimin.engine.material.ambientdiffuse;

#endregion --- Using Directives ---

namespace Examples.Tutorial
{

    public class CubeExample : GameWindow
    {

        // the objects we load
        private ObjLoaderObject3D cubeObject;
        private ObjLoaderObject3D abstractStatueObject;

        // our textur-IDs
        private int towelTexture;
        private int singleColorTexture;

        // normal map textures
        private int sofaPatternNormalTexture;
        private int primitivesNormalTexture;
        private int brickNormalTexture;

        // Materials
        private NormalMappingMaterial normalMappingMaterial;

        // Object switch
        int objSwitch = 0;

        private int updateCounter = 0;

        public CubeExample()
            : base(1280, 720, new GraphicsMode(32, 24, 8, 2), "CGI-MIN Example", GameWindowFlags.Default, DisplayDevice.Default, 3, 0, GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug)
        { }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Initialize Camera
            Camera.Init();
            Camera.SetWidthHeightFov(1280, 720, 60);
            Camera.SetLookAt(new Vector3(0, 0, 3), new Vector3(0, 0, 0), Vector3.UnitY);

            // Initialize Light
            Light.SetDirectionalLight(new Vector3(1, -1, 2), new Vector4(0.3f, 0.3f, 0.3f, 0), new Vector4(0.8f, 0.8f, 0.8f, 0), new Vector4(1, 1, 1, 0));

            // Loading the object
            cubeObject = new ObjLoaderObject3D("data/objects/cube.obj", 0.8f, true);
            abstractStatueObject = new ObjLoaderObject3D("data/objects/sphere.obj", 1.2f, true);

            // Loading color textures
            towelTexture = TextureManager.LoadTexture("data/textures/blue_towel.png");
            singleColorTexture = TextureManager.LoadTexture("data/textures/single_color.png");

            // Loading normal textures
            sofaPatternNormalTexture = TextureManager.LoadTexture("data/textures/sofa_pattern_normal.png");
            primitivesNormalTexture = TextureManager.LoadTexture("data/textures/primitives_normal.png");
            brickNormalTexture = TextureManager.LoadTexture("data/textures/brick_normal.png");

            // initialize material
            normalMappingMaterial = new NormalMappingMaterial();

            // enebale z-buffer
            GL.Enable(EnableCap.DepthTest);

            // backface culling enabled
            //GL.Enable(EnableCap.CullFace);
            //GL.CullFace(CullFaceMode.Front);

            // set keyboard event
            this.KeyDown += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(KeyDownEvent);
        }

        private void KeyDownEvent(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.Key == OpenTK.Input.Key.F11)
            {
                if (WindowState != WindowState.Fullscreen)
                {
                    WindowState = WindowState.Fullscreen;
                }
                else
                {
                    WindowState = WindowState.Normal;
                }
            }

            if (e.Key == OpenTK.Input.Key.Left) objSwitch--;
            if (e.Key == OpenTK.Input.Key.Right) objSwitch++;
            if (objSwitch < 0) objSwitch = 3;
            if (objSwitch > 3) objSwitch = 0;


            if (e.Key == OpenTK.Input.Key.Escape) this.Exit();
        }


        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            // updateCounter simply increaes
            updateCounter++;
        }


        protected override void OnRenderFrame(FrameEventArgs e)
        {
            // the screen and the depth-buffer are cleared
            GL.ClearColor(0.3f, 0.3f, 0.3f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            switch (objSwitch)
            {
                case 0:
                    cubeObject.Transformation = Matrix4.Identity;
                    cubeObject.Transformation *= Matrix4.CreateRotationX(updateCounter * 0.01f);
                    cubeObject.Transformation *= Matrix4.CreateRotationY(updateCounter * 0.015f);
                    cubeObject.Transformation *= Matrix4.CreateRotationZ(updateCounter * 0.01f);
                    normalMappingMaterial.Draw(cubeObject, towelTexture, sofaPatternNormalTexture, 15.0f);
                    break;

                case 1:
                    cubeObject.Transformation = Matrix4.Identity;
                    cubeObject.Transformation *= Matrix4.CreateRotationX(updateCounter * 0.015f);
                    cubeObject.Transformation *= Matrix4.CreateRotationY(updateCounter * 0.01f);
                    cubeObject.Transformation *= Matrix4.CreateRotationZ(updateCounter * 0.01f);
                    normalMappingMaterial.Draw(cubeObject, towelTexture, primitivesNormalTexture, 15.0f);
                    break;

                case 3:
                    abstractStatueObject.Transformation = Matrix4.Identity;
                    abstractStatueObject.Transformation *= Matrix4.CreateRotationX(updateCounter * 0.015f);
                    abstractStatueObject.Transformation *= Matrix4.CreateRotationY(updateCounter * 0.01f);
                    abstractStatueObject.Transformation *= Matrix4.CreateRotationZ(updateCounter * 0.01f);
                    normalMappingMaterial.Draw(abstractStatueObject, singleColorTexture, brickNormalTexture, 15.0f);
                    break;
            }


            SwapBuffers();
        }



        protected override void OnUnload(EventArgs e)
        {
            cubeObject.UnLoad();
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
                example.Run(60.0, 60.0);
            }
        }


    }
}

