using System;
using System.Collections.Generic;

namespace Engine.cgimin.engine.gui
{
    public class GUIContainer : GUIElement
    {
        public List<GUIElement> children;


        public GUIContainer() : base()
        {
            children = new List<GUIElement>();
        }


        public override void Draw()
        {
            base.Draw();

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i].visible)
                {
                    children[i].CalcMatrix();
                    children[i].SetToSaveAlpha();
                    children[i].intMatrix *= this.intMatrix;
                    children[i].intAlpha *= this.intAlpha;
                    children[i].Draw();
                }
            }
        }


        public override void Update()
        {
            foreach (GUIElement element in children) element.Update();
        }


        public override void SetToInitialValues()
        {
            foreach (GUIElement element in children) element.SetToInitialValues();
            base.SetToInitialValues();
        }


        public void AddChild(GUIElement element)
        {
            children.Add(element);
        }


        public void RemoveChild(GUIElement element)
        {
            if (children.Contains(element))
            {
                children.Remove(element);
            }
            else
            {
                Console.WriteLine("RemoveChild - Container does NOT contain element");
            }
        }


        public void RemoveAllChildren()
        {
            children.Clear();
        }


        public bool Contains(GUIElement element)
        {
            return children.Contains(element);
        }


        
    }
}
