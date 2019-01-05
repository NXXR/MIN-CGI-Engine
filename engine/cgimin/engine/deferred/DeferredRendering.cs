using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using cgimin.engine.object3d;
using Engine.cgimin.engine.material.local.pointlight;
using Engine.cgimin.engine.material.fullscreensimple;
using Engine.cgimin.engine.material.ibl;
using Engine.cgimin.engine.lighting.local.pointlightPBR;
using Engine.cgimin.engine.postprocessing.blur;

namespace cgimin.engine.deferred
{

    public class DeferredRendering
    {

        // G Buffers
        public static int GColorRoughnessBuffer;
        public static int GPositionBuffer;
        public static int GNormalBuffer;
        public static int GMetalnessShadowBuffer;
        public static int GGlowBuffer;

        private static BaseObject3D fullscreenQuad;
        private static ObjLoaderObject3D pointLightObject;

        private static int GFramebufferName;
        private static int width;
        private static int height;

        // Debug Fullscreen Material
        private static FullscreenSimple fullscreenSimple;

        // Fullscreen Materials
        private static FullscreenIBL fullscreenIBL;

        // Fullscreen Postprocessings
        private static BlurHorizontalMaterial horizontalBlurMaterial;
        private static BlurVerticalMaterial verticalBlurMaterial;

        // Local Lights
        private static PointLight pointLight;
        private static PointLightPBR pointLightPBR;

        // Ping Pong Buffer
        private static int PingPongFBO0;
        private static int PingPongBuffer0;

        private static int PingPongFBO1;
        private static int PingPongBuffer1;

        public static void Init(int screenWidth, int screenHeight)
        {
            width = screenWidth;
            height = screenHeight;

            fullscreenSimple = new FullscreenSimple();
            fullscreenIBL = new FullscreenIBL();
            pointLight = new PointLight();
            pointLightPBR = new PointLightPBR();

            horizontalBlurMaterial = new BlurHorizontalMaterial();
            verticalBlurMaterial = new BlurVerticalMaterial();

            // the fullscreen quad object
            fullscreenQuad = new BaseObject3D();
            fullscreenQuad.addTriangle(new Vector3(1, -1, 0), new Vector3(-1, -1, 0), new Vector3(1, 1, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(1, 1));
            fullscreenQuad.addTriangle(new Vector3(-1, -1, 0), new Vector3(1, 1, 0), new Vector3(-1, 1, 0), new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 1));
            fullscreenQuad.CreateVAO();

            //point light object
            pointLightObject = new ObjLoaderObject3D("data/objects/sphere.obj", 1.0f, true);

            // G Buffers
            GFramebufferName = 0;
            GL.GenFramebuffers(1, out GFramebufferName);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, GFramebufferName);

            GL.GenTextures(1, out GColorRoughnessBuffer);
            GL.BindTexture(TextureTarget.Texture2D, GColorRoughnessBuffer);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, screenWidth, screenHeight, 0, PixelFormat.Rgba, PixelType.UnsignedInt8888, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, GColorRoughnessBuffer, 0);

            GL.GenTextures(1, out GPositionBuffer);
            GL.BindTexture(TextureTarget.Texture2D, GPositionBuffer);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb32f, screenWidth, screenHeight, 0, PixelFormat.Rgb, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, GPositionBuffer, 0);

            GL.GenTextures(1, out GNormalBuffer);
            GL.BindTexture(TextureTarget.Texture2D, GNormalBuffer);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb16f, screenWidth, screenHeight, 0, PixelFormat.Rgb, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment2, GNormalBuffer, 0);

            GL.GenTextures(1, out GMetalnessShadowBuffer);
            GL.BindTexture(TextureTarget.Texture2D, GMetalnessShadowBuffer);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb16f, screenWidth, screenHeight, 0, PixelFormat.Rg, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment3, GMetalnessShadowBuffer, 0);

            GL.GenTextures(1, out GGlowBuffer);
            GL.BindTexture(TextureTarget.Texture2D, GGlowBuffer);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, screenWidth, screenHeight, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment4, GGlowBuffer, 0);


            DrawBuffersEnum[] drawEnum = { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2, DrawBuffersEnum.ColorAttachment3, DrawBuffersEnum.ColorAttachment4 };
            GL.DrawBuffers(5, drawEnum);

            int depthrenderbuffer;
            GL.GenRenderbuffers(1, out depthrenderbuffer);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthrenderbuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, screenWidth, screenHeight);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, depthrenderbuffer);


            /// ---- Ping Pong buffer for glow

            // Ping-Pong Buffer 0
            GL.GenFramebuffers(1, out PingPongFBO0);
            GL.GenTextures(1, out PingPongBuffer0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, PingPongFBO0);
            GL.BindTexture(TextureTarget.Texture2D, PingPongBuffer0);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, screenWidth, screenHeight, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, PingPongBuffer0, 0);

            // Ping-Pong Buffer 1
            GL.GenFramebuffers(1, out PingPongFBO1);
            GL.GenTextures(1, out PingPongBuffer1);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, PingPongFBO1);
            GL.BindTexture(TextureTarget.Texture2D, PingPongBuffer1);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, screenWidth, screenHeight, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, PingPongBuffer1, 0);


            FramebufferErrorCode eCode = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (eCode != FramebufferErrorCode.FramebufferComplete)
            {
                Console.WriteLine("GBuffer init wrong" + eCode.ToString());
            }
            else
            {
                Console.WriteLine("GBuffer init Correct");
            }

        }

      
        public static void StartGBufferRendering()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, GFramebufferName);
            DrawBuffersEnum[] drawEnum = { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2, DrawBuffersEnum.ColorAttachment3 , DrawBuffersEnum.ColorAttachment4 };
            GL.DrawBuffers(5, drawEnum);

            GL.ClearColor(new Color4(0, 0, 0, 0));
            GL.DepthMask(true);
            GL.Enable(EnableCap.DepthTest);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Viewport(0, 0, width, height);
        }



        public static void DrawFullscreenIBL(int iblSpecularCube, int iblIrradianceCube)
        {
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);

            GL.Viewport(0, 0, width, height);

            fullscreenIBL.Draw(fullscreenQuad, GColorRoughnessBuffer, GNormalBuffer, GPositionBuffer, GMetalnessShadowBuffer, GGlowBuffer, iblSpecularCube, iblIrradianceCube);

            GL.Enable(EnableCap.CullFace);
        }



        public static void CopyDepthToMainScreen()
        {   
            // copy depth to main screen
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, GFramebufferName);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
            GL.BlitFramebuffer(0, 0, width, height, 0, 0, width, height, ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);
            
            // reeset render target to main screen
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            // enable z-buffer again
            GL.Enable(EnableCap.DepthTest);
        }



        public static void DrawDebugFullscreen(int mode)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            GL.ClearColor(new Color4(1, 0, 0, 0));
            GL.Disable(EnableCap.DepthTest);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Disable(EnableCap.CullFace);

            GL.Viewport(0, 0, width, height);

            if (mode == 0) fullscreenSimple.Draw(fullscreenQuad, GColorRoughnessBuffer);
            if (mode == 1) fullscreenSimple.Draw(fullscreenQuad, GNormalBuffer);
            if (mode == 2) fullscreenSimple.Draw(fullscreenQuad, GPositionBuffer);
            if (mode == 3) fullscreenSimple.Draw(fullscreenQuad, GMetalnessShadowBuffer);
            if (mode == 4) fullscreenSimple.Draw(fullscreenQuad, GGlowBuffer);

            GL.Enable(EnableCap.CullFace);
        }


        public static void DrawPointLight(Vector3 pos, Vector3 ambientColor, Vector3 diffuseColor, Vector3 specularColor, float radius, float shininess)
        {
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            pointLight.Draw(pointLightObject, pos, shininess, ambientColor, diffuseColor, specularColor,  radius, GNormalBuffer, GPositionBuffer, width, height);
        }


        public static void DrawPointLightPBR(Vector3 pos, Vector3 color, float radius)
        {
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            pointLightPBR.Draw(pointLightObject, pos, radius, color, width, height);
        }


        public static void PingPongBlurGlowAndDraw()
        {
            GL.Disable(EnableCap.CullFace);

            for (int i = 0; i < 4; i++)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, PingPongFBO0);
                if (i == 0) { horizontalBlurMaterial.Draw(fullscreenQuad, GGlowBuffer); } else { horizontalBlurMaterial.Draw(fullscreenQuad, PingPongBuffer1); }

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, PingPongFBO1);
                verticalBlurMaterial.Draw(fullscreenQuad, PingPongBuffer0);
            }

            
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            horizontalBlurMaterial.Draw(fullscreenQuad, PingPongBuffer1);
            GL.Disable(EnableCap.Blend);

            GL.Enable(EnableCap.CullFace);
        }



    }
}
