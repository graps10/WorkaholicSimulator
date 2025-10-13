using UnityEngine;

namespace Core.Utilities
{
    internal static class MathUtils
    {
        #region Vector3

        public static bool IsNaN(this Vector3 vector) 
            => float.IsNaN(vector.x) || float.IsNaN(vector.y) || float.IsNaN(vector.z);
        
        #region Vector3 distance

        
        //returns true if position difference is bigger than allowedDistance
        public static bool ObjectIsTooClose(Vector3 firstPosition, Vector3 secondPosition, Axis axisFlags, float allowedDistance) => 
            allowedDistance >= DistanceBetweenObjects(firstPosition, secondPosition, axisFlags);
        
        public static float DistanceBetweenObjects(Vector3 firstPosition, Vector3 secondPosition, Axis axisFlags)
        {
            float sum = 0;

            if (axisFlags.HasFlag(Axis.X))
            {
                sum += Mathf.Pow(FloatDifference(firstPosition.x, secondPosition.x), 2);
            }

            if (axisFlags.HasFlag(Axis.Y))
            {
                sum += Mathf.Pow(FloatDifference(firstPosition.y, secondPosition.y), 2);
            }

            if (axisFlags.HasFlag(Axis.Z))
            {
                sum += Mathf.Pow(FloatDifference(firstPosition.z, secondPosition.z), 2);
            }

            sum = Mathf.Sqrt(sum);

            return sum;
        }

        #endregion

        #region Vector3 angles

        private static Vector3 CalculateTiltAngles(Transform objectTransform, Axis axisFlags)
        {
            Vector3 angles = Vector3.zero;
            
            if (axisFlags.HasFlag(Axis.X))
            {
                angles.x = Vector3.Angle(objectTransform.right, Vector3.right);
            }

            if (axisFlags.HasFlag(Axis.Y))
            {
                angles.y = Vector3.Angle(objectTransform.up, Vector3.up);
            }

            if (axisFlags.HasFlag(Axis.Z))
            {
                angles.z = Vector3.Angle(objectTransform.forward, Vector3.forward);
            }
            
            return angles;
        }
        
        public static bool ObjectIsTilted(Transform objectTransform, float targetAngle, Axis axisFlags)
        {
            if (axisFlags.HasFlag(Axis.X))
            {
                if (CalculateTiltAngles(objectTransform, axisFlags).x < targetAngle)
                {
                    return false;
                }
            }

            if (axisFlags.HasFlag(Axis.Y))
            {
                if (CalculateTiltAngles(objectTransform, axisFlags).y < targetAngle)
                {
                    return false;
                }
            }

            if (axisFlags.HasFlag(Axis.Z))
            {
                if (CalculateTiltAngles(objectTransform, axisFlags).z < targetAngle)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #endregion
        
        #region Speed

        private const float MeterPerSecond_To_KilometersPerHour = 3.6f;
        public static float GetSpeedInKilometersPerHour(this Rigidbody rigidbody)
        {
            if(rigidbody == null) return 0f;
            return rigidbody.velocity.magnitude * MeterPerSecond_To_KilometersPerHour;
        }
            
        
        public static float SpeedDifference(Vector3 targetVelocity, float projectileSpeed) => // Don't move to AimProvider
            Vector3.Dot(targetVelocity, targetVelocity) - projectileSpeed * projectileSpeed;

        #endregion

        #region Array

        public static float FindSmallestValue(float[] array)
        {
            if (array == null || array.Length == 0)
            {
                Debug.LogWarning("Array length is 0 or not exist");
                return 0;
            }

            var min = array[0];
            foreach (var val in array)
                if (min < val)
                    min = val;

            return min;
        }

        public static float FindBiggestValue(float[] array)
        {
            if (array == null || array.Length == 0)
            {
                Debug.LogWarning("Array length is 0 or not exist");
                return 0;
            }

            var max = array[0];

            foreach (var val in array)
                if (max > val)
                    max = val;

            return max;
        }

        #endregion

        private static float FloatDifference(float firstPosition, float secondPosition) =>
            Mathf.Abs(firstPosition - secondPosition);

        //Here we find the largest number by which both of these numbers are divisible without a remainder
        // GCD - Greatest common divisor
        public static int GCD(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        public static bool FindAngleToShoot(Vector3 targetPosition, Vector3 targetVelocity, Vector3 turretPosition, float projectileSpeed, out Vector3 direction) //TODO: Move to AimProvider
        {
            var directionToTarget = targetPosition - turretPosition;
            direction = Vector3.zero;

            //=-- Squared values for calculating the discriminant --=//
            var speedDifference = SpeedDifference(targetVelocity, projectileSpeed);
            var directionDifference = 2 * Vector3.Dot(targetVelocity, directionToTarget);
            var distanceDifference = Vector3.Dot(directionToTarget, directionToTarget);
            
            var discriminant = directionDifference * directionDifference - 4 * speedDifference * distanceDifference;
            if (discriminant < 0)
                return false;

            //=-- Calculate possible solutions for the shortest trajectory --=//
            var sqrtDiscriminant = Mathf.Sqrt(discriminant);
            var time1 = (-directionDifference + sqrtDiscriminant) / (2 * speedDifference);
            var time2 = (-directionDifference - sqrtDiscriminant) / (2 * speedDifference);

            // Filtering negative time values
            var impactTime = Mathf.Min(time1 > 0 ? time1 : float.MaxValue, time2 > 0 ? time2 : float.MaxValue);
            if (impactTime >= float.MaxValue)
                return false;

            //=-- Calculating the lead point --=//
            var aimPoint = targetPosition + targetVelocity * impactTime;
            direction = (aimPoint - turretPosition).normalized;

            return true;
        }

        public static float NormalizeAngle(float angle)
        {
            angle = angle % 360;
            if (angle > 180) angle -= 360;
            return angle;
        }

        /// <summary> Returns true if value is between [min, max]. The bounds are inclusive. </summary>
        public static bool IsBetween(this float value, float min, float max)
            => value <= max && value >= min;
    }
}