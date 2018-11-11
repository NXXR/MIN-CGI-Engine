

#region --- Using Directives ---

using System;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using cgimin.engine.object3d;
using cgimin.engine.texture;
using cgimin.engine.camera;
using cgimin.engine.light;
using cgimin.engine.material.normalmapping;
using cgimin.engine.material.cubereflectionnormal;
using cgimin.engine.material.normalmappingcubespecular;
using System.Collections.Generic;

#endregion --- Using Directives ---

namespace Examples.Tutorial
{

    public class CubeExample : GameWindow
    {
        private const int NUMBER_OF_OBJECTS = 200;

        // the objects we load
        private ObjLoaderObject3D cubeObject;
        private ObjLoaderObject3D smoothObject;
        private ObjLoaderObject3D torusObject;

        // our textur-IDs
        private int checkerColorTexture;
        private int blueMarbleColorTexture;

        // normal map textures
        private int brickNormalTexture;
        private int stoneNormalTexture;

        // cubical environment reflection texture
        private int environmentCubeTexture;
        private int darkerEnvCubeTexture;

        // Materials
        private NormalMappingMaterial normalMappingMaterial;
        private CubeReflectionNormalMaterial cubeReflectionNormalMaterial;
        private NormalMappingCubeSpecularMaterial normalMappingCubeSpecularMaterial;

        // objects positions
        private List<Vector3> positions;

        // material
        private int materialSwitch;

        // global update counter for animations etc.
        private int updateCounter = 0;

        // Keys
        private bool keyLeft;
        private bool keyRight;
        private bool keyUp;
        private bool keyDown;
        private bool keyW;
        private bool keyS;

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
            smoothObject = new ObjLoaderObject3D("data/objects/round_stone.obj", 0.3f, true);
            torusObject = new ObjLoaderObject3D("data/objects/torus_smooth.obj", 0.8f, true);

            // Loading color textures
            checkerColorTexture = TextureManager.LoadTexture("data/textures/b_checker.png");
            blueMarbleColorTexture = TextureManager.LoadTexture("data/textures/marble_blue.png");

            // Loading normal textures
            brickNormalTexture = TextureManager.LoadTexture("data/textures/brick_normal.png");
            stoneNormalTexture = TextureManager.LoadTexture("data/textures/stone_normal.png");

            // Load cube textures
            environmentCubeTexture = TextureManager.LoadCubemap(new List<string>{ "data/textures/env_reflect_left.png", "data/textures/env_reflect_right.png",
                                                                                  "data/textures/env_reflect_top.png",  "data/textures/env_reflect_bottom.png",
                                                                                  "data/textures/env_reflect_back.png", "data/textures/env_reflect_front.png"});

            darkerEnvCubeTexture = TextureManager.LoadCubemap(new List<string>{ "data/textures/cmap2_left.png", "data/textures/cmap2_right.png",
                                                                                "data/textures/cmap2_top.png",  "data/textures/cmap2_bottom.png",
                                                                                "data/textures/cmap2_back.png", "data/textures/cmap2_front.png"});




            // initialize material
            normalMappingMaterial = new NormalMappingMaterial();
            cubeReflectionNormalMaterial = new CubeReflectionNormalMaterial();
            normalMappingCubeSpecularMaterial = new NormalMappingCubeSpecularMaterial();

            // enebale z-buffer
            GL.Enable(EnableCap.DepthTest);

            // backface culling enabled
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);

            // set keyboard event
            this.KeyDown += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(KeyDownEvent);
            this.KeyUp += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(KeyUpEvent);

            // generate random positions
            Random random = new Random();
            positions = new List<Vector3>();
            for (int i = 0; i < NUMBER_OF_OBJECTS; i++)
                positions.Add(new Vector3(random.Next(-100, 100) / 10.0f, random.Next(-100, 100) / 10.0f, random.Next(-100, 100) / 10.0f));

            materialSwitch = 0;

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

            if (e.Key == OpenTK.Input.Key.Escape) this.Exit();
            if (e.Key == OpenTK.Input.Key.Number1) materialSwitch = 0;
            if (e.Key == OpenTK.Input.Key.Number2) materialSwitch = 1;
            if (e.Key == OpenTK.Input.Key.Number3) materialSwitch = 2;

            if (e.Key == OpenTK.Input.Key.Left) keyLeft = true;
            if (e.Key == OpenTK.Input.Key.Right) keyRight = true;
            if (e.Key == OpenTK.Input.Key.Up) keyUp = true;
            if (e.Key == OpenTK.Input.Key.Down) keyDown = true;
            if (e.Key == OpenTK.Input.Key.W) keyW = true;
            if (e.Key == OpenTK.Input.Key.S) keyS = true;
        }

        private void KeyUpEvent(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.Key == OpenTK.Input.Key.Left) keyLeft = false;
            if (e.Key == OpenTK.Input.Key.Right) keyRight = false;
            if (e.Key == OpenTK.Input.Key.Up) keyUp = false;
            if (e.Key == OpenTK.Input.Key.Down) keyDown = false;
            if (e.Key == OpenTK.Input.Key.W) keyW = false;
            if (e.Key == OpenTK.Input.Key.S) keyS = false;
        }


        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            // update the fly-cam with keyboard input
            Camera.UpdateFlyCamera(keyLeft, keyRight, keyUp, keyDown, keyW, keyS);

            // updateCounter simply increaes
            updateCounter++;
        }


        protected override void OnRenderFrame(FrameEventArgs e)
        {
            // the screen and the depth-buffer are cleared
            GL.ClearColor(0.3f, 0.3f, 0.3f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            int objectsInViewCount = 0;

            for (int i = 0; i < NUMBER_OF_OBJECTS; i++)
            {
                Matrix4 transform = Matrix4.Identity;
                transform *= Matrix4.CreateRotationX(updateCounter * 0.015f);
                transform *= Matrix4.CreateRotationY(updateCounter * 0.01f);
                transform *= Matrix4.CreateRotationZ(updateCounter * 0.01f);
                transform *= Matrix4.CreateTranslation(positions[i]);

                BaseObject3D objectToDraw;

                switch (i % 3)
                {
                    case 0:
                        objectToDraw = cubeObject;
                        break;
                    case 1:
                        objectToDraw = smoothObject;
                        break;
                    default:
                        objectToDraw = torusObject;
                        break;
                }

                objectToDraw.Transformation = transform;
                if (objectToDraw.IsInView())
                {
                    objectsInViewCount++;

                    switch (materialSwitch)
                    {
                        case 0:
                            normalMappingMaterial.Draw(objectToDraw, checkerColorTexture, brickNormalTexture, 15.0f);
                            break;
                        case 1:
                            cubeReflectionNormalMaterial.Draw(objectToDraw, brickNormalTexture, environmentCubeTexture);
                            break;
                        case 2:
                            normalMappingCubeSpecularMaterial.Draw(objectToDraw, blueMarbleColorTexture, stoneNormalTexture, darkerEnvCubeTexture);
                            break;
                    }
                }

            }

            Console.WriteLine(objectsInViewCount.ToString());

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

