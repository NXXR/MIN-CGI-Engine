using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;


namespace cgimin.engine.camera
{
    public class Camera
    {

        // Matrix for the transformation
        private static Matrix4 transformation;

        // ... and the petrspective projection
        private static Matrix4 perspectiveProjection;
        
        // position for the camera is saved
        private static Vector3 position;


        public static void Init()
        {
            perspectiveProjection = Matrix4.Identity;
            transformation = Matrix4.Identity;
        }


        // width, height = size of screen in pixeln, fov = "field of view", der opening-angle for the camera lense
        public static void SetWidthHeightFov(int width, int height, float fov)
        {
            float aspectRatio = width / (float)height;
            Matrix4.CreatePerspectiveFieldOfView((float)(fov * Math.PI / 180.0f), aspectRatio, 0.01f, 500, out perspectiveProjection);
        }


        // generation of the camera-transformation using LookAt
        // position of the camera-"eye", look-at poinmt, "up" direction of camera
        public static void SetLookAt(Vector3 eye, Vector3 target, Vector3 up)
        {
            position = eye;
            transformation = Matrix4.LookAt(eye, target, up);
        }


        // Getter
        public static Vector3 Position
        {
            get { return position; }
        }


        public static Matrix4 Transformation
        {
            get { return transformation; }
        }


        public static Matrix4 PerspectiveProjection
        {
            get { return perspectiveProjection; }
        }

    }
}
