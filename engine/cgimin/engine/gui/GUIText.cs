using System;


namespace Engine.cgimin.engine.gui
{
    public class GUIText : GUIElement
    {

        private BitmapFont mFont;

        public GUIText(BitmapFont font) : base()
        {
            mFont = font;
            R = 255;
            G = 255;
            B = 255;
        }

        public String Text { get; set; }
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }


        public int CalcTextWidth()
        {
            return mFont.CalcTextWidth(Text);
        }

        public void SetTextColor(int red, int green, int blue)
        {
            R = red;
            G = green;
            B = blue;
        }

        public override void Draw()
        {
            base.Draw();
            mFont.DrawString(Text, intMatrix, R, G, B, (int)(Math.Min(Math.Max(intAlpha, 0.0f), 1.0f) * 255));
        }

    }
}
