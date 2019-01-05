using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using cgimin.engine.object3d;
using Engine.cgimin.engine.material.simpletexture;
using cgimin.engine.texture;
using cgimin.engine.camera;

namespace Engine.cgimin.engine.skybox
{
    public class SkyBox
    {

        private int frontID;
        private int backID;
        private int leftID;
        private int rightID;
        private int upID;
        private int downID;

        private BaseObject3D frontSide;
        private BaseObject3D backSide;
        private BaseObject3D leftSide;
        private BaseObject3D rightSide;
        private BaseObject3D upSide;
        private BaseObject3D downSide;

        private static SimpleTextureMaterial skyboxTextureMaterial;

        public SkyBox(String front, String back, String left, String right, String up, String down)
        {
            skyboxTextureMaterial = new SimpleTextureMaterial();

            frontID = TextureManager.LoadTexture(front, true);
            backID = TextureManager.LoadTexture(back, true);
            leftID = TextureManager.LoadTexture(left, true);
            rightID = TextureManager.LoadTexture(right, true);
            upID = TextureManager.LoadTexture(up, true);
            downID = TextureManager.LoadTexture(down, true);

            float s = 200.0f;

            frontSide = new BaseObject3D();
            frontSide.addTriangle(new Vector3(-s, -s, -s), new Vector3(s, -s, -s), new Vector3(s, s, -s), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0));
            frontSide.addTriangle(new Vector3(-s, -s, -s), new Vector3(s, s, -s), new Vector3(-s, s, -s), new Vector2(0, 1), new Vector2(1, 0), new Vector2(0, 0));
            frontSide.CreateVAO();

            backSide = new BaseObject3D();
            backSide.addTriangle(new Vector3(-s, -s, s), new Vector3(s, -s, s), new Vector3(s, s, s), new Vector2(1, 1), new Vector2(0, 1), new Vector2(0, 0));
            backSide.addTriangle(new Vector3(-s, -s, s), new Vector3(s, s, s), new Vector3(-s, s, s), new Vector2(1, 1), new Vector2(0, 0), new Vector2(1, 0));
            backSide.CreateVAO();

            rightSide = new BaseObject3D();
            rightSide.addTriangle(new Vector3(-s, -s, -s), new Vector3(-s, s, -s), new Vector3(-s, s, s), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0));
            rightSide.addTriangle(new Vector3(-s, -s, -s), new Vector3(-s, s, s), new Vector3(-s, -s, s), new Vector2(1, 1), new Vector2(0, 0), new Vector2(0, 1));
            rightSide.CreateVAO();

            leftSide = new BaseObject3D();
            leftSide.addTriangle(new Vector3(s, -s, -s), new Vector3(s, s, -s), new Vector3(s, s, s), new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0));
            leftSide.addTriangle(new Vector3(s, -s, -s), new Vector3(s, s, s), new Vector3(s, -s, s), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1));
            leftSide.CreateVAO();

            upSide = new BaseObject3D();
            upSide.addTriangle(new Vector3(-s, s, -s), new Vector3(s, s, -s), new Vector3(s, s, s), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 1));
            upSide.addTriangle(new Vector3(-s, s, -s), new Vector3(s, s, s), new Vector3(-s, s, s), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1));
            upSide.CreateVAO();

            downSide = new BaseObject3D();
            downSide.addTriangle(new Vector3(-s, -s, -s), new Vector3(s, -s, -s), new Vector3(s, -s, s), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0));
            downSide.addTriangle(new Vector3(-s, -s, -s), new Vector3(s, -s, s), new Vector3(-s, -s, s), new Vector2(0, 1), new Vector2(1, 0), new Vector2(0, 0));
            downSide.CreateVAO();

        }

        public void Draw()
        {
            Matrix4 saveTrasform = Camera.Transformation;
            Matrix4 cameraTransform = Matrix4.CreateTranslation(Camera.Position.X, Camera.Position.Y, Camera.Position.Z) * saveTrasform;
            Camera.SetTransformMatrix(cameraTransform);

            GL.Disable(EnableCap.CullFace);
            GL.DepthMask(false);

            GL.ActiveTexture(TextureUnit.Texture0);

            skyboxTextureMaterial.DrawDirect(frontSide, frontID);
            skyboxTextureMaterial.DrawDirect(backSide, backID);
            skyboxTextureMaterial.DrawDirect(leftSide, leftID);
            skyboxTextureMaterial.DrawDirect(rightSide, rightID);
            skyboxTextureMaterial.DrawDirect(upSide, upID);
            skyboxTextureMaterial.DrawDirect(downSide, downID);

            GL.Enable(EnableCap.CullFace);
            GL.DepthMask(true);

            Camera.SetTransformMatrix(saveTrasform);
        }

    }
}
