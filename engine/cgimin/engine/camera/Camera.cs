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
        // enumeration for 6 clipping-planes
        enum planeEnum : int
        {
            NEAR_PLANE = 0,
            FAR_PLANE = 1,
            LEFT_PLANE = 2,
            RIGHT_PLANE = 3,
            TOP_PLANE = 4,
            BOTTOM_PLANE = 5
        };

        // Struct for a plane (hnf)
        struct Plane
        {
            public float d;
            public Vector3 normal;
        }

        // frustum clipping-planes
        private static List<Plane> planes;

        // Matrix for the transformation
        private static Matrix4 transformation;

        // ... and the petrspective projection
        private static Matrix4 perspectiveProjection;

        // position for the camera is saved
        private static Vector3 position;

        // for the control of the fly-cam
        private static float xRotation;
        private static float yRotation;

        // gui projection matrix
        public static Matrix4 GuiProjection { get; private set; }

        public static void Init()
        {
            planes = new List<Plane>();
            for (int i = 0; i < 6; i++) planes.Add(new Plane());

            perspectiveProjection = Matrix4.Identity;
            transformation = Matrix4.Identity;
            xRotation = 0;
            yRotation = 0;
            position = Vector3.Zero;

            // create default orthographic
            Matrix4 ddProjection = new Matrix4();
            Matrix4.CreateOrthographic(1920, 1080, -1, 1, out ddProjection);
            GuiProjection = ddProjection;

        }


        // width, height = size of screen in pixeln, fov = "field of view", der opening-angle for the camera lense
        public static void SetWidthHeightFov(int width, int height, float fov)
        {
            float aspectRatio = width / (float)height;
            Matrix4.CreatePerspectiveFieldOfView((float)(fov * Math.PI / 180.0f), aspectRatio, 0.01f, 500, out perspectiveProjection);

            // Set orthographic projection that width of screen stays 1920, but height depends on aspect-ratio
            Matrix4 ddProjection = new Matrix4();
            Matrix4.CreateOrthographic(1920, 1920.0f * height / width, -1, 1, out ddProjection);
            GuiProjection = ddProjection;
        }


        // generation of the camera-transformation using LookAt
        // position of the camera-"eye", look-at poinmt, "up" direction of camera
        public static void SetLookAt(Vector3 eye, Vector3 target, Vector3 up)
        {
            position = eye;
            transformation = Matrix4.LookAt(eye, target, up);
            CreateViewFrustumPlanes(transformation * perspectiveProjection);
        }


        // Steering the fly-cam
        public static void UpdateFlyCamera(bool rotLeft, bool rotRight, bool moveForward, bool moveBack, bool moveUp = false, bool moveDown = false, bool tiltFoward = false, bool tiltBackward = false)
        {
            if (rotLeft) yRotation -= 0.025f;
            if (rotRight) yRotation += 0.025f;

            if (moveForward) position -= new Vector3(transformation.Column2.X, transformation.Column2.Y, transformation.Column2.Z) * 0.08f;
            if (moveBack) position += new Vector3(transformation.Column2.X, transformation.Column2.Y, transformation.Column2.Z) * 0.08f;

            if (moveUp) position += new Vector3(transformation.Column1.X, transformation.Column1.Y, transformation.Column1.Z) * 0.08f;
            if (moveDown) position -= new Vector3(transformation.Column1.X, transformation.Column1.Y, transformation.Column1.Z) * 0.08f;

            if (tiltFoward) xRotation += 0.02f;
            if (tiltBackward) xRotation -= 0.02f;

            transformation = Matrix4.Identity;
            transformation *= Matrix4.CreateTranslation(-position.X, -position.Y, -position.Z);
            transformation *= Matrix4.CreateRotationX(xRotation);
            transformation *= Matrix4.CreateRotationY(yRotation);

            CreateViewFrustumPlanes(transformation * perspectiveProjection);
        }


        // calculate 6 clipping planes of the view frustum
        private static void CreateViewFrustumPlanes(Matrix4 mat)
        {
            // left
            Plane plane = new Plane();
            plane.normal.X = mat.M14 + mat.M11;
            plane.normal.Y = mat.M24 + mat.M21;
            plane.normal.Z = mat.M34 + mat.M31;
            plane.d = mat.M44 + mat.M41;
            planes[(int)planeEnum.LEFT_PLANE] = plane;

            // right
            plane = new Plane();
            plane.normal.X = mat.M14 - mat.M11;
            plane.normal.Y = mat.M24 - mat.M21;
            plane.normal.Z = mat.M34 - mat.M31;
            plane.d = mat.M44 - mat.M41;
            planes[(int)planeEnum.RIGHT_PLANE] = plane;

            // bottom
            plane = new Plane();
            plane.normal.X = mat.M14 + mat.M12;
            plane.normal.Y = mat.M24 + mat.M22;
            plane.normal.Z = mat.M34 + mat.M32;
            plane.d = mat.M44 + mat.M42;
            planes[(int)planeEnum.BOTTOM_PLANE] = plane;

            // top
            plane = new Plane();
            plane.normal.X = mat.M14 - mat.M12;
            plane.normal.Y = mat.M24 - mat.M22;
            plane.normal.Z = mat.M34 - mat.M32;
            plane.d = mat.M44 - mat.M42;
            planes[(int)planeEnum.TOP_PLANE] = plane;

            // near
            plane = new Plane();
            plane.normal.X = mat.M14 + mat.M13;
            plane.normal.Y = mat.M24 + mat.M23;
            plane.normal.Z = mat.M34 + mat.M33;
            plane.d = mat.M44 + mat.M43;
            planes[(int)planeEnum.NEAR_PLANE] = plane;

            // far
            plane = new Plane();
            plane.normal.X = mat.M14 - mat.M13;
            plane.normal.Y = mat.M24 - mat.M23;
            plane.normal.Z = mat.M34 - mat.M33;
            plane.d = mat.M44 - mat.M43;
            planes[(int)planeEnum.FAR_PLANE] = plane;

            // normalize
            for (int i = 0; i < 6; i++)
            {
                plane = planes[i];

                float length = plane.normal.Length;
                plane.normal.X = plane.normal.X / length;
                plane.normal.Y = plane.normal.Y / length;
                plane.normal.Z = plane.normal.Z / length;
                plane.d = plane.d / length;

                planes[i] = plane;
            }
        }


        // returns the signed distance froma point to frustum clipping plane
        private static float signedDistanceToPoint(int planeID, Vector3 pt)
        {
            return Vector3.Dot(planes[planeID].normal, pt) + planes[planeID].d;
        }


        // is sphere inside or overlapping the view frustum?
        public static bool SphereIsInFrustum(Vector3 center, float radius)
        {
            for (int i = 0; i < 6; i++)
            {
                if (signedDistanceToPoint(i, center) < -radius)
                {
                    return false;
                }
            }
            return true;
        }


        // set transformation Matrix manually from outside
        public static void SetTransformMatrix(Matrix4 transform)
        {
            transformation = transform;
        }


        // getter
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
