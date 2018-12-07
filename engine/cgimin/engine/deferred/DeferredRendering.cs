using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using cgimin.engine.object3d;
using cgimin.engine.texture;
using Engine.cgimin.engine.material.fullscreensimple;

namespace cgimin.engine.deferred
{

    public class DeferredRendering
    {

        // G Buffers
        public static int GColorBuffer;
        public static int GPositionBuffer;
        public static int GNormalBuffer;

        private static BaseObject3D fullscreenQuad;

        private static int GFramebufferName;
        private static int width;
        private static int height;

        // Fullscreen Materials
        private static FullscreenSimple fullscreenSimpleMaterial; 

        public static void Init(int screenWidth, int screenHeight)
        {
            width = screenWidth;
            height = screenHeight;

            fullscreenSimpleMaterial = new FullscreenSimple();

            fullscreenQuad = new BaseObject3D();
            fullscreenQuad.addTriangle(new Vector3(1, -1, 0), new Vector3(-1, -1, 0), new Vector3(1, 1, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(1, 1));
            fullscreenQuad.addTriangle(new Vector3(-1, -1, 0), new Vector3(1, 1, 0), new Vector3(-1, 1, 0), new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 1));
            fullscreenQuad.CreateVAO();

            // G Buffers
            GFramebufferName = 0;
            GL.GenFramebuffers(1, out GFramebufferName);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, GFramebufferName);

            GL.GenTextures(1, out GColorBuffer);
            GL.BindTexture(TextureTarget.Texture2D, GColorBuffer);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, screenWidth, screenHeight, 0, PixelFormat.Rgba, PixelType.UnsignedInt8888, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, GColorBuffer, 0);

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

            DrawBuffersEnum[] drawEnum = { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2};
            GL.DrawBuffers(3, drawEnum);

            int depthrenderbuffer;
            GL.GenRenderbuffers(1, out depthrenderbuffer);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthrenderbuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, screenWidth, screenHeight);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, depthrenderbuffer);

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
            DrawBuffersEnum[] drawEnum = { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2 };
            GL.DrawBuffers(3, drawEnum);

            GL.ClearColor(new Color4(0, 0, 0, 0));
            GL.DepthMask(true);
            GL.Enable(EnableCap.DepthTest);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Viewport(0, 0, width, height);
        }



        public static void DebugFullscreen()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
 
            GL.ClearColor(new Color4(1, 0, 0, 0));
            GL.Disable(EnableCap.DepthTest);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Disable(EnableCap.CullFace);

            GL.Viewport(0, 0, width, height);


            fullscreenSimpleMaterial.DrawDirect(fullscreenQuad, GNormalBuffer);


            GL.Enable(EnableCap.CullFace);

        }



    }
}
