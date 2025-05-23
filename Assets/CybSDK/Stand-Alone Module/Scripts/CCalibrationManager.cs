using UnityEngine;

namespace CybSDK
{
    public class CCalibrationManager : MonoBehaviour
    {
        public enum CalibrationDataResult
        {
            Success = 1,
            Exception = 0,
            NoCalibrationApp = -1,
            NoCalibrationData = -2,
        }

        // Calibration data
        private float calibratedPositionX;
        private float calibratedPositionZ;
        private float calibratedRotationY;
        private float boundary0X;
        private float boundary0Z;
        private float boundary1X;
        private float boundary1Z;

        private float rotationOffset = 0.0f;

        private CharacterController characterController = null;

        private void Awake()
        {
            characterController = GetComponentInParent<CharacterController>();

            if (characterController == null)
            {
                CLogger.LogError("The script 'CCalibrationManager' needs to be attached to a gameobject that has a CharacterController or to a child of it.");
                enabled = false;
                return;
            }
        }

        public void CalculateNewTransform(Vector3[] playArea)
        {
            CLogger.Log("Calculating new calibrated position and orientation");
            float[,] H = GetHMatrix(playArea);

            // Calculate new x,z position based on the rotation and translation of the play area (H).
            characterController.center = new Vector3(H[0, 0] * calibratedPositionX + H[0, 1] * calibratedPositionZ + H[0, 2],
                characterController.center.y,
                H[1, 0] * calibratedPositionX + H[1, 1] * calibratedPositionZ + H[1, 2]);

            if (Mathf.Asin(H[1, 0]) < 0)
                rotationOffset = -(Mathf.Acos(H[0, 0]) * 180f / Mathf.PI) + calibratedRotationY;
            else
                rotationOffset = (Mathf.Acos(H[0, 0]) * 180f / Mathf.PI) + calibratedRotationY;
        }

        private float[,] GetHMatrix(Vector3[] playArea)
        {
            float[,] R = GetRotationMatrix(playArea);
            Vector2 t = GetTranslation(R, playArea);

            // H =  [ R    t  ]
            //          [ 0    1 ]
            return new float[2, 3] { 
                { R[0, 0], R[0, 1], t.x }, 
                { R[1, 0], R[1, 1], t.y }
            };
        }

        private float[,] GetRotationMatrix(Vector3[] playArea)
        {
            // Vector corner 0 to corner 1
            Vector2 calCorner0_1 = new Vector2(boundary1X - boundary0X, boundary1Z - boundary0Z);
            // Create unit vector with x1 and y1
            calCorner0_1.Normalize();

            Vector2 corner0_1 = new Vector2(playArea[1].x - playArea[0].x, playArea[1].z - playArea[0].z);
            corner0_1.Normalize();

            // [ x1x2 + y1y2      - (x1y2 - x2y1) ]
            // [ x1y2 - x2y1            x1x2 + y1y2 ]
            return new float[2, 2] { 
                { (calCorner0_1.x * corner0_1.x) + (calCorner0_1.y * corner0_1.y), -1 * ((calCorner0_1.x * corner0_1.y) - (corner0_1.x * calCorner0_1.y)) } , 
                { (calCorner0_1.x * corner0_1.y) - (corner0_1.x * calCorner0_1.y), (calCorner0_1.x * corner0_1.x) + (calCorner0_1.y * corner0_1.y) } 
            };
        }

        private Vector2 GetTranslation(float[,] R, Vector3[] playArea)
        {
            // t = p2 - (R * p1)
            return new Vector2(
                playArea[0].x - (R[0, 0] * boundary0X + R[0, 1] * boundary0Z), 
                playArea[0].z - (R[1, 0] * boundary0X + R[1, 1] * boundary0Z)
                );
        }

        public CalibrationDataResult ReadCalibrationData()
        {
            AndroidJavaClass javaClass = new AndroidJavaClass("com.cyberith.unityandroidblevirtdevicelib.CalibrationDataManager");
            float[] calibrationDataArray = javaClass.CallStatic<float[]>("getCalibrationData");

            CalibrationDataResult result = (CalibrationDataResult)calibrationDataArray[0];

            if (result == CalibrationDataResult.Success)
            {
                calibratedPositionX = calibrationDataArray[1];
                calibratedPositionZ = calibrationDataArray[2];
                calibratedRotationY = calibrationDataArray[3];
                boundary0X = calibrationDataArray[4];
                boundary0Z = calibrationDataArray[5];
                boundary1X = calibrationDataArray[6];
                boundary1Z = calibrationDataArray[7];
                rotationOffset = calibratedRotationY;
            }
            else
            {
                CLogger.LogError("Error reading calibration data: " + result.ToString());
            }
            return result;
        }

        public float GetRotationOffset()
        {
            return rotationOffset;
        }
    }
}
