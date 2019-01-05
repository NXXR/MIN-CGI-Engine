

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
using Engine.cgimin.engine.skybox;
using Engine.cgimin.engine.shadowmapping;
using cgimin.engine.material.gbufferlayout;
using cgimin.engine.material.castshadow;
using Engine.cgimin.engine.gui;

#endregion --- Using Directives ---

namespace Examples.Tutorial
{

    public class CubeExample : GameWindow
    {
        private class IBLData
        {
            public SkyBox IBLSkyBox;
            public int IrradianceCubeTex;
            public int SpecularCubeTex;
            public Vector3 lightDirection;
        }

        public static int SCREEN_WIDTH = 1920;
        public static int SCREEN_HEIGHT = 1080;

        private const int NUMBER_OF_OBJECTS = 150;
        private const int NUMBER_OF_LIGHTS = 50;

        // the objects we load
        private ObjLoaderObject3D cubeObject;
        private ObjLoaderObject3D smoothObject;
        private ObjLoaderObject3D torusObject;
        private ObjLoaderObject3D wallObject;
        private ObjLoaderObject3D sphereObject;
        private ObjLoaderObject3D monkeyObject;

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

        // ibl data
        private List<IBLData> iblSources;
        private IBLData currentIBL;

        // textures for dynamic objects
        private int rustTextureNormalID;
        private int rustTextureColorID;

        private int harshbricksNormalID;
        private int harshbricksColorID;
        private int harshbricksAOID;
        private int harshbricksGlowID;
        private int harshbricksMetalnessID;
        private int harshbricksRoughnessID;

        // GUI
        private GUIContainer containerStage;


        public CubeExample()
            : base(SCREEN_WIDTH, SCREEN_HEIGHT, new GraphicsMode(32, 24, 8, 0), "CGI-MIN Example", GameWindowFlags.Fullscreen, DisplayDevice.Default, 3, 0, GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug)
        { }

   
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Initialize Camera
            Camera.Init();
            Camera.SetWidthHeightFov(SCREEN_WIDTH, SCREEN_HEIGHT, 60);
            Camera.SetLookAt(new Vector3(0, 0, 3), new Vector3(0, 0, 0), Vector3.UnitY);

            // Init Shadow Mapping
            CascadedShadowMapping.Init(4096, 2048, 1024, 15, 20, 90, 1);

            // Loading the object
            cubeObject = new ObjLoaderObject3D("data/objects/cube.obj", 0.8f, true);
            smoothObject = new ObjLoaderObject3D("data/objects/round_stone.obj", 0.3f, true);
            torusObject = new ObjLoaderObject3D("data/objects/torus_smooth.obj", 0.8f, true);
            wallObject = new ObjLoaderObject3D("data/objects/thin_wall.obj", 20.0f, true);
            sphereObject = new ObjLoaderObject3D("data/objects/sphere_detail.obj", 5.0f, true);
            monkeyObject = new ObjLoaderObject3D("data/objects/monkey.obj", 5.0f, true);

            // load textures
            int normalStd = TextureManager.LoadTexture("data/textures/normalmap.png");
            rustTextureNormalID = TextureManager.LoadTexture("data/textures/rustiron_normal.tif");
            rustTextureColorID = TextureManager.LoadTexture("data/textures/rustiron_basecolor.tif");

            
            harshbricksColorID = TextureManager.LoadTexture("data/textures/harshbricks-albedo.png");
            harshbricksNormalID = TextureManager.LoadTexture("data/textures/harshbricks-normal.png");
            harshbricksAOID = TextureManager.LoadTexture("data/textures/harshbricks-ao.png");
            harshbricksGlowID = TextureManager.LoadTexture("data/textures/harshbricks-glow.png");
            harshbricksMetalnessID = TextureManager.LoadTexture("data/textures/harshbricks-metalness.png");
            harshbricksRoughnessID = TextureManager.LoadTexture("data/textures/harshbricks-roughness.png");
            

            iblSources = new List<IBLData>();

            // Loading night IBL
            // ibl maps              // front                , back                    , right,                  , left                    , up                      , down
            IBLData blueSky = new IBLData();
            blueSky.IBLSkyBox = new SkyBox("data/ibl/sky_m00_c05.bmp", "data/ibl/sky_m00_c04.bmp", "data/ibl/sky_m00_c01.bmp", "data/ibl/sky_m00_c00.bmp", "data/ibl/sky_m00_c02.bmp", "data/ibl/sky_m00_c03.bmp");
            blueSky.IrradianceCubeTex = TextureManager.LoadIBLIrradiance("data/ibl/sky", "png");
            blueSky.SpecularCubeTex = TextureManager.LoadIBLSpecularMap("data/ibl/sky", "bmp");
            blueSky.lightDirection = new Vector3(0, 1.0f, 0.1f);

            // ibl maps              // front                , back                    , right,                  , left                    , up                      , down
            IBLData night = new IBLData();
            night.IBLSkyBox = new SkyBox("data/ibl/night_c05.bmp", "data/ibl/night_c04.bmp", "data/ibl/night_c01.bmp", "data/ibl/night_c00.bmp", "data/ibl/night_c02.bmp", "data/ibl/night_c03.bmp");
            night.IrradianceCubeTex = TextureManager.LoadIBLIrradiance("data/ibl/night", "bmp");
            night.SpecularCubeTex = TextureManager.LoadIBLSpecularMap("data/ibl/night", "bmp");
            night.lightDirection = new Vector3(0, 1, 1.5f);

            iblSources.Add(blueSky);
            iblSources.Add(night);

            currentIBL = iblSources[1];

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
            brickGoldSettings.colorTexture = TextureManager.LoadTexture("data/textures/b_checker.tif");
            brickGoldSettings.normalTexture = TextureManager.LoadTexture("data/textures/brick_normal.tif");

            // 'rust iron settings'
            MaterialSettings rustIronSettings = new MaterialSettings();
            rustIronSettings.colorTexture = rustTextureColorID;
            rustIronSettings.normalTexture = rustTextureNormalID;

            // Init Octree
            octree = new Octree(new Vector3(-100, -100, -100), new Vector3(100, 100, 100));

            // custom color / roughness / metalness
            for (int i = 0; i < 10; i++)
            {
                MaterialSettings custom1 = new MaterialSettings();
                custom1.normalTexture = normalStd;
                custom1.color = new Vector3(0.549f, 0.556f, 0.554f);
                custom1.roughness = 0.1f * i;
                custom1.metalness = 1.0f;
                custom1.glow = i / 10.0f;
                octree.AddEntity(new OctreeEntity(monkeyObject, MaterialManager.GetMaterial(Material.GBUFFERVALUESET), custom1, Matrix4.CreateScale(0.5f) * Matrix4.CreateTranslation(-20 + i * 4, 7, -17)));
            }


            // add objetcs to octree
            octree.AddEntity(new OctreeEntity(wallObject, MaterialManager.GetMaterial(Material.GBUFFERLAYOUT), brickGoldSettings, Matrix4.Identity));

            // generate random positions
            Random random = new Random();

            for (int i = 0; i < NUMBER_OF_OBJECTS; i++)
            {
                Matrix4 tranlatePos = Matrix4.CreateTranslation(random.Next(-200, 200) / 10.0f, 1.0f, random.Next(-200, 200) / 10.0f);

                int whichObject = random.Next(5);

                switch (whichObject) {
                    case 0:
                        octree.AddEntity(new OctreeEntity(smoothObject, MaterialManager.GetMaterial(Material.GBUFFERLAYOUT), brickGoldSettings, tranlatePos));
                        break;
                    case 1:
                        octree.AddEntity(new OctreeEntity(cubeObject, MaterialManager.GetMaterial(Material.GBUFFERLAYOUT), brickGoldSettings, tranlatePos));
                        break;
                    case 2:
                        octree.AddEntity(new OctreeEntity(torusObject, MaterialManager.GetMaterial(Material.GBUFFERLAYOUT), brickGoldSettings, tranlatePos));
                        break;
                    case 3:
                        octree.AddEntity(new OctreeEntity(cubeObject, MaterialManager.GetMaterial(Material.GBUFFERLAYOUT), brickGoldSettings, tranlatePos));
                        break;
                }
            }

            // Init deferred rendering
            DeferredRendering.Init(SCREEN_WIDTH, SCREEN_HEIGHT);

            // Point Light positions
            pointLightPositions = new List<Vector3>();
            for (int i = 0; i < NUMBER_OF_LIGHTS; i++)
            {
                pointLightPositions.Add(new Vector3(random.Next(-200, 200) / 10.0f, 1, random.Next(-200, 200) / 10.0f));
            }

            // some predefined colors for the point lights
            lightColors = new List<Vector3>();
            lightColors.Add(new Vector3(1, 0, 0));
            lightColors.Add(new Vector3(0, 1, 0));
            lightColors.Add(new Vector3(0, 0, 1));
            lightColors.Add(new Vector3(1, 1, 0));
            lightColors.Add(new Vector3(0, 1, 1));
            lightColors.Add(new Vector3(1, 0, 1));

            // Gui
            containerStage = new GUIContainer();
            containerStage.X = -1920 / 2;
            containerStage.Y = -1280 / 2;

            GUIGraphic testGraphic = new GUIGraphic(TextureManager.LoadGUITexture("data/textures/hochschule.png"), 0, 0, 255, 255);
            containerStage.AddChild(testGraphic);
            testGraphic.X = 40;
            testGraphic.Y = 200;

            BitmapFont abelFont = new BitmapFont("data/fonts/abel_normal.fnt", "data/fonts/abel_normal.png");

            GUIText guiText = new GUIText(abelFont);
            containerStage.AddChild(guiText);
            guiText.X = 40;
            guiText.Y = 350;
            guiText.Text = "MIN-CGI Beispielprojekt";

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
            // update th gui
            containerStage.Update();

            // update the fly-cam with keyboard input
            // Camera.UpdateFlyCamera(Keyboard[OpenTK.Input.Key.Left], Keyboard[OpenTK.Input.Key.Right], Keyboard[OpenTK.Input.Key.Up], Keyboard[OpenTK.Input.Key.Down],
            //                        Keyboard[OpenTK.Input.Key.W], Keyboard[OpenTK.Input.Key.S]);



            if (Keyboard[OpenTK.Input.Key.Number1]) displaySwitch = 0;
            if (Keyboard[OpenTK.Input.Key.Number2]) displaySwitch = 1;
            if (Keyboard[OpenTK.Input.Key.Number3]) displaySwitch = 2;
            if (Keyboard[OpenTK.Input.Key.Number4]) displaySwitch = 3;
            if (Keyboard[OpenTK.Input.Key.Number5]) displaySwitch = 4;
            if (Keyboard[OpenTK.Input.Key.Number6]) displaySwitch = 5;


            if (oldMouseX > 0) Camera.UpdateMouseCamera(0.3f, Keyboard[OpenTK.Input.Key.A], Keyboard[OpenTK.Input.Key.D], 
                                                              Keyboard[OpenTK.Input.Key.W],Keyboard[OpenTK.Input.Key.S], 
                                                              (oldMouseY - this.Mouse.Y) / 200.0f, (oldMouseX - this.Mouse.X) / 200.0f);

            if (Keyboard[OpenTK.Input.Key.T]) Camera.UpdateMouseCamera(0.3f, false, false, false, false, 0.02f, 0);
            if (Keyboard[OpenTK.Input.Key.G]) Camera.UpdateMouseCamera(0.3f, false, false, false, false,-0.02f, 0);
            if (Keyboard[OpenTK.Input.Key.F]) Camera.UpdateMouseCamera(0.3f, false, false, false, false, 0, 0.02f);
            if (Keyboard[OpenTK.Input.Key.H]) Camera.UpdateMouseCamera(0.3f, false, false, false, false, 0, -0.02f);

            oldMouseX = this.Mouse.X;
            oldMouseY = this.Mouse.Y;

            //if (this.Mouse.X < SCREEN_WIDTH / 2 - 100) OpenTK.Input.Mouse.SetPosition(this.X + SCREEN_WIDTH / 2 + 100,  this.Y + this.Mouse.Y);
            //if (this.Mouse.X > SCREEN_WIDTH / 2 + 100) OpenTK.Input.Mouse.SetPosition(this.X + SCREEN_WIDTH / 2 - 100,  this.Y + this.Mouse.Y);

            if (Keyboard[OpenTK.Input.Key.O]) currentIBL = iblSources[0];
            if (Keyboard[OpenTK.Input.Key.P]) currentIBL = iblSources[1];

            // updateCounter simply increaes
            updateCounter++;
        }


        protected override void OnRenderFrame(FrameEventArgs e)
        {

            updateCounter = 1000;

            // set the light direction to the current IBL light settings
            CascadedShadowMapping.SetLightDirection(currentIBL.lightDirection);

            // set transformatiopn for shadow-casting objects
            monkeyObject.Transformation = Matrix4.CreateRotationX(updateCounter / 60.0f) * Matrix4.CreateRotationZ(updateCounter / 70.0f) *
                                          Matrix4.CreateTranslation((float)Math.Sin(updateCounter / 100.0f) * 10.0f, 7, (float)Math.Cos(updateCounter / 100.0f) * 10.0f);

            sphereObject.Transformation = Matrix4.CreateRotationX(updateCounter / 60.0f) * Matrix4.CreateRotationZ(updateCounter / 70.0f) *
                                          Matrix4.CreateTranslation((float)Math.Sin(updateCounter / 100.0f + (float)Math.PI) * 10.0f, 7, (float)Math.Cos(updateCounter / 100.0f + (float)Math.PI) * 10.0f);

            cubeObject.Transformation = Matrix4.CreateScale(2.5f) * Matrix4.CreateRotationX(updateCounter / 60.0f) * Matrix4.CreateRotationZ(updateCounter / 70.0f) *
                                        Matrix4.CreateTranslation((float)Math.Sin(updateCounter / 100.0f + (float)Math.PI * 0.5f) * 10.0f, 7, (float)Math.Cos(updateCounter / 100.0f + (float)Math.PI * 0.5f) * 10.0f);

            CascadedShadowMapping.StartShadowMapping();
            for (int i = 0; i < 3; i++)
            {
                CascadedShadowMapping.SetDepthTextureTarget(i);

                // render all shadow casting objects here
                (MaterialManager.GetMaterial(Material.CASTSHADOW) as CastShadowMaterial).DrawDirect(monkeyObject);
                (MaterialManager.GetMaterial(Material.CASTSHADOW) as CastShadowMaterial).DrawDirect(sphereObject);
                (MaterialManager.GetMaterial(Material.CASTSHADOW) as CastShadowMaterial).DrawDirect(cubeObject);
            }
            CascadedShadowMapping.EndShadowMapping();

            // initilaize drawing into g-buffers
            DeferredRendering.StartGBufferRendering();

            // draw all objects into the gbuffers
            GL.CullFace(CullFaceMode.Front);

            /*
                private int harshbricksNormalID;
                private int harshbricksColorID;
                private int harshbricksAOID;
                private int harshbricksGlowID;
                private int harshbricksMetalnessID;
                private int harshbricksRoughnessID;
            */

            // draw dynamic objects
            (MaterialManager.GetMaterial(Material.GBUFFERLAYOUT) as GBufferFromTwoTexturesMaterial).DrawDirect(monkeyObject, rustTextureColorID, rustTextureNormalID);
            (MaterialManager.GetMaterial(Material.GBUFFERLAYOUT) as GBufferFromTwoTexturesMaterial).DrawDirect(sphereObject, rustTextureColorID, rustTextureNormalID);
            (MaterialManager.GetMaterial(Material.GBUFFERCOMPONENTS) as GBufferFromComponentsMaterial).DrawDirect(cubeObject, harshbricksColorID, harshbricksNormalID, harshbricksMetalnessID, harshbricksRoughnessID, harshbricksAOID, harshbricksGlowID);

            // draw static octree
            octree.DrawSolidMaterials();

            //(MaterialManager.GetMaterial(Material.GBUFFERLAYOUT) as GBufferLayoutMaterial).DrawDirect(sphereObject, rustTextureColorID, rustTextureNormalID);

            // Set screen as render target and clear it
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.ClearColor(new Color4(0, 0, 0, 0));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Disable(EnableCap.DepthTest);

            if (displaySwitch == 0)
            {
                // render directional light
                DeferredRendering.DrawFullscreenIBL(currentIBL.SpecularCubeTex, currentIBL.IrradianceCubeTex);

                // set to additive blending
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);

                // draw all point light with the now enebaled additive blending
                Vector3 addPos = new Vector3();
                for (int i = 0; i < pointLightPositions.Count; i++)
                {
                    addPos.X = (float)(Math.Sin(updateCounter * 0.01 + i * 20) + Math.Cos(updateCounter * 0.026 + i * 31)) * 4;
                    addPos.Z = (float)(Math.Sin(updateCounter * 0.017 + i * 20) + Math.Cos(updateCounter * 0.023 + i * 30)) * 4;
                    //DeferredRendering.DrawPointLight(pointLightPositions[i] + addPos, lightColors[i % lightColors.Count], lightColors[i % lightColors.Count], new Vector3(1, 1, 1), 3, 5);
                    DeferredRendering.DrawPointLightPBR(pointLightPositions[i] + addPos, lightColors[i % lightColors.Count], 5);
                }
                GL.Disable(EnableCap.Blend);

                // copy depth from deferred gBuffer Framebuffer to main screen
                DeferredRendering.CopyDepthToMainScreen();

                // draw the skybox
                currentIBL.IBLSkyBox.Draw();

                // apply blur to glowing areas and blend-draw it
                DeferredRendering.PingPongBlurGlowAndDraw();

                // draw all transparent objects on screen
                octree.DrawTransparentMaterials();

                
            }
            else
            {
                DeferredRendering.DrawDebugFullscreen(displaySwitch - 1);
            }

            // draw the gui
            containerStage.Draw();

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

