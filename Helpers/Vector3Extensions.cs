using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DontDropTheSoap.Helpers
{
    public static class Vector3Extensions
    {
        private const float MapBoundXMin = -98.0f;
        private const float MapBoundXMax = 98.0f;
        private const float MapBoundYMin = 0.025f;
        private const float MapBoundYMax = 50.0f;
        private const float MapBoundZMin = -98.0f;
        private const float MapBoundZMax = 98.0f;

        /// <summary>
        /// Given a float value, multiplies each vector entry (X/Y/Z) by the given value
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="value"></param>
        /// <returns>The vector with multiplication applied</returns>
        public static Vector3 UniformMultiply(this Vector3 vector, float value)
        {
            vector.X *= value;
            vector.Y *= value;
            vector.Z *= value;

            return vector;
        }

        /// <summary>
        /// Clamps a vector 3 given a clamp axis, and optionally a degrees
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="field"></param>
        /// <param name="minDegrees"></param>
        /// <param name="maxDegrees"></param>
        /// <returns></returns>
        public static Vector3 Clamp(this Vector3 vector, Vector3Fields field, float minDegrees = 90, float maxDegrees = 90)
        {
            var x = vector.X;
            var y = vector.Y;
            var z = vector.Z;
            var minRadian = Mathf.DegToRad(minDegrees);
            var maxRadian = Mathf.DegToRad(maxDegrees);

            switch (field)
            {
                case Vector3Fields.X:
                    {
                        x = Mathf.Clamp(vector.X, -minRadian, maxRadian);
                        break;
                    }
                case Vector3Fields.Y:
                    {
                        y = Mathf.Clamp(vector.Y, -minRadian, maxRadian);
                        break;
                    }
                case Vector3Fields.Z:
                    {
                        x = Mathf.Clamp(vector.Z, -minRadian, maxRadian);
                        break;
                    }
            }

            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Ensures the player character cannot exceed the map bounds
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector3 PositionClampToMapBounds(this Vector3 vector)
        {
            var x = vector.X;
            var y = vector.Y;
            var z = vector.Z;
            var debugFactor = 1f;

            if (x > (MapBoundXMax + debugFactor))
            {
                x = MapBoundXMax;
            }

            if (x < (MapBoundXMin - debugFactor))
            {
                x = MapBoundXMin;
            }

            if (y > (MapBoundYMax + debugFactor))
            {
                y = MapBoundYMax;
            }

            if (y < (MapBoundYMin - debugFactor))
            {
                y = MapBoundYMin;
            }

            if (z > (MapBoundZMax + debugFactor))
            {
                z = MapBoundZMax;
            }

            if (z < (MapBoundZMin - debugFactor))
            {
                z = MapBoundZMin;
            }

            return new Vector3(x, y, z);
        }
    }

    public enum Vector3Fields
    {
        X = 0,
        Y = 1,
        Z = 2
    }
}
