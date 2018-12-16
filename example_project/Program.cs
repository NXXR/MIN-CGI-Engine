

#region --- Using Directives ---

using System;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using cgimin.engine.object3d;
using cgimin.engine.texture;
using cgimin.engine.camera;
using cgimin.engine.material;
using OpenTK.Input;
using static cgimin.engine.material.BaseMaterial;
using Engine.cgimin.engine.octree;
using cgimin.engine.deferred;

using System.Collections.Generic;
using Engine.cgimin.engine.material.simpleblend;

#endregion --- Using Directives ---

namespace Examples.Tutorial
{

    public class CubeExample : GameWindow
    {
        public static int SCREEN_WIDTH = 1280;
        public static int SCREEN_HEIGHT = 720;

        private const int NUMBER_OF_OBJECTS = 150;
        private const int NUMBER_OF_LIGHTS = 50;

        // the objects we load
        private ObjLoaderObject3D cubeObject;
        private ObjLoaderObject3D smoothObject;
        private ObjLoaderObject3D torusObject;
        private ObjLoaderObject3D wallObject;
        private ObjLoaderObject3D pyramideObject;

        // our textur-IDs
        private int checkerColorTexture;
        private int blueMarbleColorTexture;
        private int orangeColorTexture;

        // normal map textures
        private int brickNormalTexture;
        private int stoneNormalTexture;

        // Octree
        private Octree octree;

        // global update counter for animations etc.
        private int updateCounter = 0;

        // Point Light positions
        private List<Vector3> pointLightPositions;
        private List<Vector3> lightColors;

        // store mouse positions
        private int oldMouseX = 0;
        private int oldMouseY = 0;

        // switch to select display
        private int displaySwitch;

        public CubeExample()
            : base(SCREEN_WIDTH, SCREEN_HEIGHT, new GraphicsMode(32, 24, 8, 0), "CGI-MIN Example", GameWindowFlags.Default, DisplayDevice.Default, 3, 0, GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug)
        { }

   
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Initialize Camera
            Camera.Init();
            Camera.SetWidthHeightFov(SCREEN_WIDTH, SCREEN_HEIGHT, 60);
            Camera.SetLookAt(new Vector3(0, 0, 3), new Vector3(0, 0, 0), Vector3.UnitY);

            // Loading the object
            cubeObject = new ObjLoaderObject3D("data/objects/cube.obj", 0.8f, true);
            smoothObject = new ObjLoaderObject3D("data/objects/round_stone.obj", 0.3f, true);
            torusObject = new ObjLoaderObject3D("data/objects/torus_smooth.obj", 0.8f, true);
            wallObject = new ObjLoaderObject3D("data/objects/thin_wall.obj", 20.0f, true);
            pyramideObject = new ObjLoaderObject3D("data/objects/pyramide.obj", 2.0f, true);

            // Loading color textures
            checkerColorTexture = TextureManager.LoadTexture("data/textures/b_checker.png");
            blueMarbleColorTexture = TextureManager.LoadTexture("data/textures/marble_blue.png");
            orangeColorTexture = TextureManager.LoadTexture("data/textures/orange.png");

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
            blendMaterialSettings.colorTexture = orangeColorTexture;
            blendMaterialSettings.SrcBlendFactor = BlendingFactorSrc.SrcColor;
            blendMaterialSettings.DestBlendFactor = BlendingFactorDest.DstColor;


            // Init Octree
            octree = new Octree(new Vector3(-30, -30, -30), new Vector3(30, 30, 30));

            // add wall object
            octree.AddEntity(new OctreeEntity(wallObject, MaterialManager.GetMaterial(Material.GBUFFERLAYOUT), blueShinyStoneSettings, Matrix4.Identity));

            // generate random positions
            Random random = new Random();

            for (int i = 0; i < NUMBER_OF_OBJECTS; i++)
            {
                Matrix4 tranlatePos = Matrix4.CreateTranslation(random.Next(-200, 200) / 10.0f, random.Next(-200, 200) / 10.0f, 1.0f /* random.Next(-200, 200) / 10.0f*/);

                int whichObject = random.Next(5);

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
                        octree.AddEntity(new OctreeEntity(cubeObject, MaterialManager.GetMaterial(Material.GBUFFERLAYOUT), brickGoldSettings, tranlatePos));
                        break;
                    case 4:
                        octree.AddEntity(new OctreeEntity(pyramideObject, MaterialManager.GetMaterial(Material.SIMPLE_BLEND), blendMaterialSettings, tranlatePos));
                        break;
                }
            }

            // Init deferred rendering
            DeferredRendering.Init(SCREEN_WIDTH, SCREEN_HEIGHT);

            // Initialize Deferred directional Light
            DeferredRendering.SetDirectionalLight(new Vector3(1, -1, 0.2f), new Vector4(0.3f, 0.3f, 0.3f, 0), new Vector4(0.8f, 0.8f, 0.8f, 0), new Vector4(1, 1, 1, 0), 50);

            // Point Light positions
            pointLightPositions = new List<Vector3>();
            for (int i = 0; i < NUMBER_OF_LIGHTS; i++)
            {
                pointLightPositions.Add(new Vector3(random.Next(-200, 200) / 10.0f, random.Next(-200, 200) / 10.0f, 1));
            }

            // some predefined colors for the point lights
            lightColors = new List<Vector3>();
            lightColors.Add(new Vector3(1, 0, 0));
            lightColors.Add(new Vector3(0, 1, 0));
            lightColors.Add(new Vector3(0, 0, 1));
            lightColors.Add(new Vector3(1, 1, 0));
            lightColors.Add(new Vector3(0, 1, 1));
            lightColors.Add(new Vector3(1, 0, 1));

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
            // update the fly-cam with keyboard input
            //Camera.UpdateFlyCamera(Keyboard[OpenTK.Input.Key.Left], Keyboard[OpenTK.Input.Key.Right], Keyboard[OpenTK.Input.Key.Up], Keyboard[OpenTK.Input.Key.Down],
            //                       Keyboard[OpenTK.Input.Key.W], Keyboard[OpenTK.Input.Key.S]);


            if (Keyboard[OpenTK.Input.Key.Number1]) displaySwitch = 0;
            if (Keyboard[OpenTK.Input.Key.Number2]) displaySwitch = 1;
            if (Keyboard[OpenTK.Input.Key.Number3]) displaySwitch = 2;
            if (Keyboard[OpenTK.Input.Key.Number4]) displaySwitch = 3;


            if (oldMouseX > 0) Camera.UpdateMouseCamera(0.3f, Keyboard[OpenTK.Input.Key.A], Keyboard[OpenTK.Input.Key.D], 
                                                              Keyboard[OpenTK.Input.Key.W],Keyboard[OpenTK.Input.Key.S], 
                                                              (oldMouseY - this.Mouse.Y) / 200.0f, (oldMouseX - this.Mouse.X) / 200.0f);

            oldMouseX = this.Mouse.X;
            oldMouseY = this.Mouse.Y;


            // updateCounter simply increaes
            updateCounter++;
        }


        protected override void OnRenderFrame(FrameEventArgs e)
        {
            // initilaize drawing into g-buffers
            DeferredRendering.StartGBufferRendering();

            // draw all objects into the gbuffers
            GL.CullFace(CullFaceMode.Front);
            octree.DrawSolidMaterials();
            
            if (displaySwitch == 0)
            {

                // render directional light
                DeferredRendering.DrawDirectionalLight();

                // set to additive blending
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);

                // draw all point light with the now enebaled additive blending
                Vector3 addPos = new Vector3();
                for (int i = 0; i < pointLightPositions.Count; i++)
                {
                    addPos.X = (float)(Math.Sin(updateCounter * 0.01 + i * 20) + Math.Cos(updateCounter * 0.026 + i * 31)) * 4;
                    addPos.Y = (float)(Math.Sin(updateCounter * 0.017 + i * 20) + Math.Cos(updateCounter * 0.023 + i * 30)) * 4;
                    DeferredRendering.DrawPointLight(pointLightPositions[i] + addPos, lightColors[i % lightColors.Count], lightColors[i % lightColors.Count], new Vector3(1, 1, 1), 3, 5);
                }
                GL.Disable(EnableCap.Blend);

                // copy depth from deferred gBuffer Framebuffer to main screen
                DeferredRendering.CopyDepthToMainScreen();

                // draw all transparent objects on screen
                //octree.DrawTransparentMaterials();

            }
            else
            {
                DeferredRendering.DrawDebugFullscreen(displaySwitch - 1);
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

