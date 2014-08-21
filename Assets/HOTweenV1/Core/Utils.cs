//
// Utils.cs
//
// Author: Daniele Giardini
//
// Copyright (c) 2012 Daniele Giardini - Holoville - http://www.holoville.com
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using UnityEngine;

namespace Holoville.HOTween.Core
{
    /// <summary>
    /// Various utils used by HOTween.
    /// </summary>
    internal static class Utils
    {
        /// <summary>
        /// Converts the given Matrix4x4 to a Quaternion and returns it.
        /// </summary>
        /// <param name="m">
        /// The matrix to convert.
        /// </param>
        /// <returns>
        /// The resulting <see cref="Quaternion"/>.
        /// </returns>
        internal static Quaternion MatrixToQuaternion(Matrix4x4 m)
        {
            Quaternion q = new Quaternion();

            float n = 1 + m[0, 0] + m[1, 1] + m[2, 2];
            if (n < 0)
            {
                n = 0;
            }
            q.w = (float)Math.Sqrt(n) * 0.5f;

            n = 1 + m[0, 0] - m[1, 1] - m[2, 2];
            if (n < 0)
            {
                n = 0;
            }
            q.x = (float)Math.Sqrt(n) * 0.5f;

            n = 1 - m[0, 0] + m[1, 1] - m[2, 2];
            if (n < 0)
            {
                n = 0;
            }
            q.y = (float)Math.Sqrt(n) * 0.5f;

            n = 1 - m[0, 0] - m[1, 1] + m[2, 2];
            if (n < 0)
            {
                n = 0;
            }
            q.z = (float)Math.Sqrt(n) * 0.5f;

            q.x *= Mathf.Sign(q.x*(m[2, 1] - m[1, 2]));
            q.y *= Mathf.Sign(q.y*(m[0, 2] - m[2, 0]));
            q.z *= Mathf.Sign(q.z*(m[1, 0] - m[0, 1]));

            return q;
        }

        /// <summary>
        /// Returns a string representing the given Type without the packages
        /// (like Single instead than System.Single).
        /// </summary>
        internal static string SimpleClassName(Type p_class)
        {
            string s = p_class.ToString();
            return s.Substring(s.LastIndexOf('.') + 1);
        }

        /// <summary>
        /// Returns the 2D angle between two vectors
        /// </summary>
        public static float GetAngle2D(Vector3 p_from, Vector3 p_to)
        {
            Vector2 from = Vector2.right;
            Vector3 to = p_to - p_from;
            float ang = Vector2.Angle(from, to);
            Vector3 cross = Vector3.Cross(from, to);
            if (cross.z > 0) ang = 360 - ang;
            ang *= -1f;
            return ang;
        }
    }
}
