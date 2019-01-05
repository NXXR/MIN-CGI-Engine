using OpenTK;

namespace Engine.cgimin.engine.gui
{
    public class GUIElement
    {
        // private
        private float intX;
        private float intY;
        private float intScaleX;
        private float intScaleY;
        private float intRotPointX;
        private float intRotPointY;
        private float intRotation;
        private float saveAlpha;

        // internal values
        internal Matrix4 intMatrix = new Matrix4();
        internal float intAlpha = 1.0f;

        // private custom matrix for special tranformations
        private Matrix4 customMatrix = Matrix4.Identity;

        // getter / setter properties
        public float X { get { return intX; } set { intX = value; CalcMatrix(); } }
        public float Y { get { return intY; } set { intY = value; CalcMatrix(); } }
        public float ScaleX { get { return intScaleX; } set { intScaleX = value; CalcMatrix(); } }
        public float ScaleY { get { return intScaleY; } set { intScaleY = value; CalcMatrix(); } }

        public bool visible;
        public float alpha { get { return intAlpha; } set { intAlpha = value; saveAlpha = value; } }

        public GUIElement()
        {
            intMatrix = Matrix4.Identity;

            X = 0;
            Y = 0;
            ScaleX = 1.0f;
            ScaleY = 1.0f;

            intRotPointX = 0;
            intRotPointY = 0;
            intRotation = 0;
            intAlpha = 1.0f;
            alpha = 1.0f;

            visible = true;
        }

        public virtual void SetToInitialValues()
        {
            X = 0;
            Y = 0;
            ScaleX = 1.0f;
            ScaleY = 1.0f;

            intRotPointX = 0;
            intRotPointY = 0;
            intRotation = 0;

            intAlpha = 1.0f;
            alpha = 1.0f;

            CalcMatrix();
        }


        public void SetRotation(float rotation, float aroundPointX = 0, float aroundPointY = 0)
        {
            intRotPointX = aroundPointX;
            intRotPointY = aroundPointY;
            intRotation = rotation;
            CalcMatrix();
        }


        private Matrix4 RotateAroundMatrix(float px, float py, float rot)
        {
            return Matrix4.CreateTranslation(-px, py, 0) * Matrix4.CreateRotationZ(rot / 180.0f * MathHelper.Pi) * Matrix4.CreateTranslation(px, -py, 0);
        }


        internal void CalcMatrix()
        {
            intMatrix = Matrix4.CreateScale(intScaleX, intScaleY, 0) * RotateAroundMatrix(intRotPointX, intRotPointY, intRotation) *  Matrix4.CreateTranslation(intX, -intY, 0) * customMatrix;
        }

        internal void SetToSaveAlpha()
        {
            intAlpha = saveAlpha;
        }


        public void SetCustomTransform(Matrix4 matrix)
        {
            customMatrix = matrix;
        }

        public virtual void Update() { }
        public virtual void Draw() { }

    }
}
