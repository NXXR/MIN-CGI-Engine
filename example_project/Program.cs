

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
using cgimin.engine.material;
using OpenTK.Input;
using static cgimin.engine.material.BaseMaterial;
using Engine.cgimin.engine.octree;
using Engine.cgimin.engine.material.simpleblend;
using Engine.cgimin.engine.terrain;
using cgimin.engine.deferred;
using System.Collections.Generic;

#endregion --- Using Directives ---

namespace Examples.Tutorial
{

    public class CubeExample : GameWindow
    {
        private const int NUMBER_OF_OBJECTS = 500;
       
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

        // Octree
        private Octree octree;

        // Terrain
        private Terrain terrain;

        // global update counter for animations etc.
        private int updateCounter = 0;

        public CubeExample()
            : base(1280, 720, new GraphicsMode(32, 24, 8, 2), "CGI-MIN Example", GameWindowFlags.Default, DisplayDevice.Default, 3, 2, GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug)
        { }

   
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Initialize Camera
            Camera.Init();
            Camera.SetWidthHeightFov(1280, 720, 60);
            Camera.SetLookAt(new Vector3(0, 0, 3), new Vector3(0, 0, 0), Vector3.UnitY);

            // Initialize Light
            Light.SetDirectionalLight(new Vector3(1, -1, 2), new Vector4(0.3f, 0.3f, 0.3f, 0), new Vector4(0.8f, 0.8f, 0.8f, 0), new Vector4(1,1,1,0));

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


            // enebale z-buffer
            GL.Enable(EnableCap.DepthTest);

            // backface culling enabled
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);

            // set keyboard event
            this.KeyDown += new EventHandler<KeyboardKeyEventArgs>(KeyDownEvent);


            // init matrial settings

            // 'golden brick'
            MaterialSettings brickGoldSettings = new MaterialSettings();
            brickGoldSettings.colorTexture = checkerColorTexture;
            brickGoldSettings.normalTexture = brickNormalTexture;

            // 'completely mirrored cube'
            MaterialSettings cubeReflectSettings = new MaterialSettings();
            cubeReflectSettings.colorTexture = blueMarbleColorTexture;
            cubeReflectSettings.normalTexture = stoneNormalTexture;

            // 'blue shiny stone"
            MaterialSettings blueShinyStoneSettings = new MaterialSettings();
            blueShinyStoneSettings.colorTexture = blueMarbleColorTexture;
            blueShinyStoneSettings.normalTexture = stoneNormalTexture;

            // transparent blended material
            MaterialSettings blendMaterialSettings = new MaterialSettings();
            blendMaterialSettings.colorTexture = checkerColorTexture;
            blendMaterialSettings.normalTexture = brickNormalTexture;


            // Init Octree
            octree = new Octree(new Vector3(-30, -30, -30), new Vector3(30, 30, 30));

            // generate random positions
            Random random = new Random();

            for (int i = 0; i < NUMBER_OF_OBJECTS; i++)
            {
                Matrix4 tranlatePos = Matrix4.CreateTranslation(random.Next(-200, 200) / 10.0f, random.Next(-200, 200) / 10.0f, random.Next(-200, 200) / 10.0f);

                int whichObject = random.Next(4);

                switch (whichObject) {
                    case 0:
                        octree.AddEntity(new OctreeEntity(smoothObject, MaterialManager.GetMaterial(Material.GBUFFERLAYOUT), blueShinyStoneSettings, tranlatePos));
                        break;
                    case 1:
                        octree.AddEntity(new OctreeEntity(cubeObject, MaterialManager.GetMaterial(Material.GBUFFERLAYOUT), cubeReflectSettings, tranlatePos));
                        break;
                    case 2:
                        octree.AddEntity(new OctreeEntity(torusObject, MaterialManager.GetMaterial(Material.GBUFFERLAYOUT), brickGoldSettings, tranlatePos));
                        break;
                    case 3:
                        octree.AddEntity(new OctreeEntity(cubeObject, MaterialManager.GetMaterial(Material.GBUFFERLAYOUT), blendMaterialSettings, tranlatePos));
                        break;
                }
            }

            DeferredRendering.Init(1280, 720);

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

         

        }


        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            KeyboardState keyboardState = Keyboard.GetState();

            // update the fly-cam with keyboard input
            Camera.UpdateFlyCamera(keyboardState[Key.Left], keyboardState[Key.Right], keyboardState[Key.Up], keyboardState[Key.Down],
                                   keyboardState[Key.W], keyboardState[Key.S]);

            updateCounter++;
        }


        protected override void OnRenderFrame(FrameEventArgs e)
        {
            DeferredRendering.StartGBufferRendering();

            octree.Draw();

            DeferredRendering.DebugFullscreen();

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

