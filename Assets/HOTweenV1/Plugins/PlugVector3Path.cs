//
// PlugVector3Path.cs
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
using System.Collections.Generic;
using System.Reflection;
using Holoville.HOTween.Core;
using Holoville.HOTween.Plugins.Core;
using UnityEngine;

namespace Holoville.HOTween.Plugins
{
    /// <summary>
    /// Plugin for the tweening of Vector3 objects along a Vector3 path.
    /// </summary>
    public class PlugVector3Path : ABSTweenPlugin
    {
        // ENUMS //////////////////////////////////////////////////

        enum OrientType
        {
            None,
            ToPath,
            LookAtTransform,
            LookAtPosition
        }

        // VARS ///////////////////////////////////////////////////

        internal static Type[] validPropTypes = { typeof(Vector3) };
        internal static Type[] validValueTypes = { typeof(Vector3[]) };

        internal PathType pathType { get; private set; }
        internal Path path; // Internal so that HOTween OnDrawGizmo can find it and draw the paths.
        internal float pathPerc; // Stores the current percentage of the path, so that HOTween's OnDrawGizmo can show its velocity.
        internal bool hasAdditionalStartingP; // True if the path was created with an additional starting point

        const int SUBDIVISIONS_MULTIPLIER = 16;
        const float EPSILON = 0.001f; // Used for floating points comparison
        const float MIN_LOOKAHEAD = 0.0001f;
        const float MAX_LOOKAHED = 0.9999f;
        Vector3 typedStartVal;
        Vector3[] points;
        Vector3 diffChangeVal; // Used for incremental loops.
        internal bool isClosedPath;
        bool is2D; // TRUE in case of Unity 2D path
        bool is2DsideScroller;
        OrientType orientType = OrientType.None;
        float lookAheadVal = MIN_LOOKAHEAD;
        Axis lockPositionAxis = Axis.None;
        Axis lockRotationAxis = Axis.None;
        bool isPartialPath;
        bool usesLocalPosition; // Used to apply mods when calculating orientToPath lookAt
        float startPerc = 0; // Implemented to allow partial paths
        float changePerc = 1; // Implemented to allow partial paths

        // REFERENCES /////////////////////////////////////////////

        Vector3 lookPos;
        Transform lookTrans;
        Transform orientTrans;
        float orZ; // Used to store Z value in case of lock Z, in order to rotate things differently

        // GETS/SETS //////////////////////////////////////////////

        /// <summary>
        /// Gets the untyped start value,
        /// sets both the untyped and the typed start value.
        /// </summary>
        protected override object startVal
        {
            get { return _startVal; }
            set
            {
                if (tweenObj.isFrom) {
                    _endVal = value;
                    Vector3[] ps = (Vector3[])value;
                    points = new Vector3[ps.Length];
                    Array.Copy(ps, points, ps.Length);
                    Array.Reverse(points);
                } else {
                    _startVal = typedStartVal = (Vector3)value;
                }
            }
        }

        /// <summary>
        /// Gets the untyped end value,
        /// sets both the untyped and the typed end value.
        /// </summary>
        protected override object endVal
        {
            get { return _endVal; }
            set
            {
                if (tweenObj.isFrom) {
                    _startVal = typedStartVal = (Vector3)value;
                } else {
                    _endVal = value;
                    Vector3[] ps = (Vector3[])value;
                    points = new Vector3[ps.Length];
                    Array.Copy(ps, points, ps.Length);
                }
            }
        }


        // ***********************************************************************************
        // CONSTRUCTOR
        // ***********************************************************************************

        /// <summary>
        /// Creates a new instance of this plugin using the main ease type and an absolute path.
        /// </summary>
        /// <param name="p_path">
        /// The <see cref="Vector3"/> path to tween through.
        /// </param>
        /// <param name="p_type">Type of path</param>
        public PlugVector3Path(Vector3[] p_path, PathType p_type = PathType.Curved)
            : base(p_path, false) { pathType = p_type; }
        /// <summary>
        /// Creates a new instance of this plugin using an absolute path.
        /// </summary>
        /// <param name="p_path">
        /// The <see cref="Vector3"/> path to tween through.
        /// </param>
        /// <param name="p_easeType">
        /// The <see cref="EaseType"/> to use.
        /// </param>
        /// <param name="p_type">Type of path</param>
        public PlugVector3Path(Vector3[] p_path, EaseType p_easeType, PathType p_type = PathType.Curved)
            : base(p_path, p_easeType, false) { pathType = p_type; }
        /// <summary>
        /// Creates a new instance of this plugin using the main ease type.
        /// </summary>
        /// <param name="p_path">
        /// The <see cref="Vector3"/> path to tween through.
        /// </param>
        /// <param name="p_isRelative">
        /// If <c>true</c>, the path is considered relative to the starting value of the property, instead than absolute.
        /// Not compatible with <c>HOTween.From</c>.
        /// </param>
        /// <param name="p_type">Type of path</param>
        public PlugVector3Path(Vector3[] p_path, bool p_isRelative, PathType p_type = PathType.Curved)
            : base(p_path, p_isRelative) { pathType = p_type; }
        /// <summary>
        /// Creates a new instance of this plugin.
        /// </summary>
        /// <param name="p_path">
        /// The <see cref="Vector3"/> path to tween through.
        /// </param>
        /// <param name="p_easeType">
        /// The <see cref="EaseType"/> to use.
        /// </param>
        /// <param name="p_isRelative">
        /// If <c>true</c>, the path is considered relative to the starting value of the property, instead than absolute.
        /// Not compatible with <c>HOTween.From</c>.
        /// </param>
        /// <param name="p_type">Type of path</param>
        public PlugVector3Path(Vector3[] p_path, EaseType p_easeType, bool p_isRelative, PathType p_type = PathType.Curved)
            : base(p_path, p_easeType, p_isRelative) { pathType = p_type; }

        /// <summary>
        /// Creates a new instance of this plugin.
        /// </summary>
        /// <param name="p_path">
        /// The <see cref="Vector3"/> path to tween through.
        /// </param>
        /// <param name="p_easeAnimCurve">
        /// The <see cref="AnimationCurve"/> to use for easing.
        /// </param>
        /// <param name="p_isRelative">
        /// If <c>true</c>, the given end value is considered relative instead than absolute.
        /// </param>
        /// <param name="p_type">Type of path</param>
        public PlugVector3Path(Vector3[] p_path, AnimationCurve p_easeAnimCurve, bool p_isRelative, PathType p_type = PathType.Curved)
            : base(p_path, p_easeAnimCurve, p_isRelative) { pathType = p_type; }

        /// <summary>
        /// Init override.
        /// Used to check that isRelative is FALSE,
        /// and otherwise use the given parameters to send a decent warning message.
        /// </summary>
        internal override void Init(Tweener p_tweenObj, string p_propertyName, EaseType p_easeType, Type p_targetType, PropertyInfo p_propertyInfo, FieldInfo p_fieldInfo)
        {
            if (isRelative && p_tweenObj.isFrom) {
                isRelative = false;
                TweenWarning.Log("\"" + p_tweenObj.target + "." + p_propertyName + "\": PlugVector3Path \"isRelative\" parameter is incompatible with HOTween.From. The tween will be treated as absolute.");
            }
            usesLocalPosition = p_propertyName == "localPosition";

            base.Init(p_tweenObj, p_propertyName, p_easeType, p_targetType, p_propertyInfo, p_fieldInfo);
        }

        // ===================================================================================
        // PARAMETERS ------------------------------------------------------------------------

        /// <summary>
        /// Parameter > Smoothly closes the path, so that it can be used for cycling loops.
        /// </summary>
        /// <returns>
        /// A <see cref="PlugVector3Path"/>
        /// </returns>
        public PlugVector3Path ClosePath()
        {
            return ClosePath(true);
        }

        /// <summary>
        /// Parameter > Choose whether to smoothly close the path, so that it can be used for cycling loops.
        /// </summary>
        /// <param name="p_close">
        /// Set to <c>true</c> to close the path.
        /// </param>
        public PlugVector3Path ClosePath(bool p_close)
        {
            isClosedPath = p_close;
            return this;
        }

        /// <summary>
        /// Parameter > If the tween target is a <see cref="Transform"/>, orients the tween target to the path.
        /// </summary>
        /// <returns>
        /// A <see cref="PlugVector3Path"/>
        /// </returns>
        public PlugVector3Path OrientToPath()
        {
            return OrientToPath(true);
        }

        /// <summary>
        /// Parameter > Choose whether to orient the tween target to the path (only if it's a <see cref="Transform"/>).
        /// </summary>
        /// <param name="p_orient">
        /// Set to <c>true</c> to orient the tween target to the path.
        /// </param>
        public PlugVector3Path OrientToPath(bool p_orient)
        {
            return OrientToPath(p_orient, MIN_LOOKAHEAD, Axis.None);
        }

        /// <summary>
        /// Parameter > If the tween target is a <see cref="Transform"/>, orients the tween target to the path,
        /// using the given lookAhead percentage.
        /// </summary>
        /// <param name="p_lookAhead">
        /// The look ahead percentage (0 to 1).
        /// </param>
        public PlugVector3Path OrientToPath(float p_lookAhead)
        {
            return OrientToPath(true, p_lookAhead, Axis.None);
        }

        /// <summary>
        /// Parameter > If the tween target is a <see cref="Transform"/>, orients the tween target to the path,
        /// locking its rotation on the given axis.
        /// </summary>
        /// <param name="p_lockRotationAxis">
        /// Sets one or more axis to lock while rotating.
        /// To lock more than one axis, use the bitwise OR operator (ex: <c>Axis.X | Axis.Y</c>).
        /// </param>
        public PlugVector3Path OrientToPath(Axis p_lockRotationAxis)
        {
            return OrientToPath(true, MIN_LOOKAHEAD, p_lockRotationAxis);
        }

        /// <summary>
        /// Parameter > If the tween target is a <see cref="Transform"/>, orients the tween target to the path,
        /// using the given lookAhead percentage and locking its rotation on the given axis.
        /// </summary>
        /// <param name="p_lookAhead">The look ahead percentage (0 to 1)</param>
        /// <param name="p_lockRotationAxis">
        /// Sets one or more axis to lock while rotating.
        /// To lock more than one axis, use the bitwise OR operator (ex: <c>Axis.X | Axis.Y</c>).
        /// </param>
        public PlugVector3Path OrientToPath(float p_lookAhead, Axis p_lockRotationAxis)
        {
            return OrientToPath(true, p_lookAhead, p_lockRotationAxis);
        }

        /// <summary>
        /// Parameter > Choose whether to orient the tween target to the path (only if it's a <see cref="Transform"/>),
        /// and which lookAhead percentage ad lockRotation to use.
        /// </summary>
        /// <param name="p_orient">
        /// Set to <c>true</c> to orient the tween target to the path.
        /// </param>
        /// <param name="p_lookAhead">
        /// The look ahead percentage (0 to 1).
        /// </param>
        /// <param name="p_lockRotationAxis">
        /// Sets one or more axis to lock while rotating.
        /// To lock more than one axis, use the bitwise OR operator (ex: <c>Axis.X | Axis.Y</c>).
        /// </param>
        public PlugVector3Path OrientToPath(bool p_orient, float p_lookAhead, Axis p_lockRotationAxis)
        {
            if (p_orient) {
                orientType = OrientType.ToPath;
            }
            lookAheadVal = p_lookAhead;
            if (lookAheadVal < MIN_LOOKAHEAD) {
                lookAheadVal = MIN_LOOKAHEAD;
            } else if (lookAheadVal > MAX_LOOKAHED) {
                lookAheadVal = MAX_LOOKAHED;
            }
            lockRotationAxis = p_lockRotationAxis;
            return this;
        }

        /// <summary>
        /// Parameter > If the tween target is a <see cref="Transform"/>, sets the tween so that the target will always look at the given transform.
        /// </summary>
        /// <param name="p_transform">
        /// The <see cref="Transform"/> to look at.
        /// </param>
        public PlugVector3Path LookAt(Transform p_transform)
        {
            if (p_transform != null) {
                orientType = OrientType.LookAtTransform;
                lookTrans = p_transform;
            }
            return this;
        }

        /// <summary>
        /// Parameter > If the tween target is a <see cref="Transform"/>, sets the tween so that the target will always look at the given position.
        /// </summary>
        /// <param name="p_position">
        /// The <see cref="Vector3"/> to look at.
        /// </param>
        public PlugVector3Path LookAt(Vector3 p_position)
        {
            orientType = OrientType.LookAtPosition;
            lookPos = p_position;
            lookTrans = null;
            return this;
        }

        /// <summary>
        /// Parameter > locks the given position axis.
        /// </summary>
        /// <param name="p_lockAxis">Sets one or more axis to lock.
        /// To lock more than one axis, use the bitwise OR operator (ex: <c>Axis.X | Axis.Y</c>)</param>
        /// <returns></returns>
        public PlugVector3Path LockPosition(Axis p_lockAxis)
        {
            lockPositionAxis = p_lockAxis;
            return this;
        }

        /// <summary>
        /// Indicates that the path works must be calculated in 2D
        /// </summary>
        /// <param name="p_isSideScroller">If TRUE the target will be considered as moving from a side-scrolling perspective,
        /// if FALSE (default) the target will be considered as moving from a top-down perspective</param>
        /// <returns></returns>
        public PlugVector3Path Is2D(bool p_isSideScroller = false)
        {
            is2D = true;
            is2DsideScroller = p_isSideScroller;
            return this;
        }

        // ===================================================================================
        // PRIVATE METHODS -------------------------------------------------------------------

        /// <summary>
        /// Returns the speed-based duration based on the given speed x second.
        /// </summary>
        protected override float GetSpeedBasedDuration(float p_speed)
        {
            return path.pathLength / p_speed;
        }

        /// <summary>
        /// Adds the correct starting and ending point so the path can be reached from the property's actual position.
        /// </summary>
        protected override void SetChangeVal()
        {
            if (orientType != OrientType.None) {
                // Store orient transform.
                if (orientTrans == null) {
                    orientTrans = tweenObj.target as Transform;
                    orZ = orientTrans.eulerAngles.z;
                }
            }

            // Create path.
            Vector3[] pts;
            int indMod = 1;
            int pAdd = (isClosedPath ? 1 : 0);
            int pointsLength = points.Length;

            if (isRelative) {
                hasAdditionalStartingP = false;
                Vector3 diff = points[0] - typedStartVal;
                // Path length is the same (plus control points).
                pts = new Vector3[pointsLength + 2 + pAdd];
                for (int i = 0; i < pointsLength; ++i) pts[i + indMod] = points[i] - diff;
            } else {
                Vector3 currVal = (Vector3)GetValue();
                // Calculate if currVal and start point are equal,
                // managing floating point imprecision.
                Vector3 diff = currVal - points[0];
                if (diff.x < 0) diff.x = -diff.x;
                if (diff.y < 0) diff.y = -diff.y;
                if (diff.z < 0) diff.z = -diff.z;
                if (diff.x < EPSILON && diff.y < EPSILON && diff.z < EPSILON) {
                    // Path length is the same (plus control points).
                    hasAdditionalStartingP = false;
                    pts = new Vector3[pointsLength + 2 + pAdd];
                } else {
                    // Path needs additional point for current value as starting point (plus control points).
                    hasAdditionalStartingP = true;
                    pts = new Vector3[pointsLength + 3 + pAdd];
                    if (tweenObj.isFrom) {
                        pts[pts.Length - 2] = currVal;
                    } else {
                        pts[1] = currVal;
                        indMod = 2;
                    }
                }
                for (int i = 0; i < pointsLength; ++i) {
                    pts[i + indMod] = points[i];
                }
            }

            pointsLength = pts.Length;

            if (isClosedPath) {
                // Close path.
                pts[pointsLength - 2] = pts[1];
            }

            // Add control points.
            if (isClosedPath) {
                pts[0] = pts[pointsLength - 3];
                pts[pointsLength - 1] = pts[2];
            } else {
                pts[0] = pts[1];
                Vector3 lastP = pts[pointsLength - 2];
                Vector3 diffV = lastP - pts[pointsLength - 3];
                pts[pointsLength - 1] = lastP + diffV;
            }

            // Manage eventual lockPositionAxis.
            if (lockPositionAxis != Axis.None) {
                bool lockX = ((lockPositionAxis & Axis.X) == Axis.X);
                bool lockY = ((lockPositionAxis & Axis.Y) == Axis.Y);
                bool lockZ = ((lockPositionAxis & Axis.Z) == Axis.Z);
                Vector3 orPos = typedStartVal;
                for (int i = 0; i < pointsLength; ++i) {
                    Vector3 pt = pts[i];
                    pts[i] = new Vector3(
                        lockX ? orPos.x : pt.x,
                        lockY ? orPos.y : pt.y,
                        lockZ ? orPos.z : pt.z
                    );
                }
            }

            // Create the path.
            path = new Path(pathType, pts);

            // Store arc lengths tables for constant speed.
            path.StoreTimeToLenTables(path.path.Length * SUBDIVISIONS_MULTIPLIER);

            if (!isClosedPath) {
                // Store the changeVal used for Incremental loops
                diffChangeVal = pts[pointsLength - 2] - pts[1];
            }
        }

        /// <summary>
        /// Sets the correct values in case of Incremental loop type.
        /// </summary>
        /// <param name="p_diffIncr">
        /// The difference from the previous loop increment.
        /// </param>
        protected override void SetIncremental(int p_diffIncr)
        {
            if (isClosedPath) {
                return;
            }

            Vector3[] pathPs = path.path;
            int pathPsLength = pathPs.Length;
            for (int i = 0; i < pathPsLength; ++i) {
                pathPs[i] += (diffChangeVal * p_diffIncr);
            }
            path.changed = true;
        }

        /// <summary>
        /// Sets the correct values in case of RestartIncremental.
        /// </summary>
        protected override void SetIncrementalRestart()
        {
            Vector3 prevStartVal = typedStartVal;
            startVal = GetValue();
            Vector3 diff = typedStartVal - prevStartVal;
            Vector3[] pathPs = path.path;
            int pathPsLength = pathPs.Length;
            for (int i = 0; i < pathPsLength; ++i) {
                pathPs[i] += diff;
            }
            path.changed = true;
        }

        /// <summary>
        /// Updates the tween.
        /// </summary>
        /// <param name="p_totElapsed">
        /// The total elapsed time since startup.
        /// </param>
        protected override void DoUpdate(float p_totElapsed)
        {
            int linearWaypointIndex;
            pathPerc = ease(p_totElapsed, startPerc, changePerc, _duration, tweenObj.easeOvershootOrAmplitude, tweenObj.easePeriod);
            Vector3 newPos = GetConstPointOnPath(pathPerc, true, path, out linearWaypointIndex);
            SetValue(newPos);

            if (orientType != OrientType.None && orientTrans != null && !orientTrans.Equals(null)) {
                Transform parentTrans = usesLocalPosition ? orientTrans.parent : null;
                switch (orientType) {
                case OrientType.LookAtPosition:
                    orientTrans.LookAt(lookPos, Vector3.up);
                    break;
                case OrientType.LookAtTransform:
                    if (orientTrans != null && !orientTrans.Equals(null)) {
                        orientTrans.LookAt(lookTrans.position, Vector3.up);
                    }
                    break;
                case OrientType.ToPath:
                    Vector3 lookAtP;
                    if (pathType == PathType.Linear && lookAheadVal <= MIN_LOOKAHEAD) {
                        // Calculate lookAhead so that it doesn't turn until it starts moving on next waypoint
                        lookAtP = newPos + path.path[linearWaypointIndex] - path.path[linearWaypointIndex - 1];
                    } else {
                        float nextT = pathPerc + lookAheadVal;
                        if (nextT > 1) nextT = (isClosedPath ? nextT - 1 : 1.000001f);
                        lookAtP = path.GetPoint(nextT);
                    }
                    Vector3 transUp = orientTrans.up;
                    // Apply basic modification for local position movement
                    if (usesLocalPosition && parentTrans != null) lookAtP = parentTrans.TransformPoint(lookAtP);
                    
                    if (lockRotationAxis != Axis.None && orientTrans != null) {
                        if ((lockRotationAxis & Axis.X) == Axis.X) {
                            Vector3 v0 = orientTrans.InverseTransformPoint(lookAtP);
                            v0.y = 0;
                            lookAtP = orientTrans.TransformPoint(v0);
                            transUp = usesLocalPosition && parentTrans != null ? parentTrans.up : Vector3.up;
                        }
                        if ((lockRotationAxis & Axis.Y) == Axis.Y) {
                            Vector3 v0 = orientTrans.InverseTransformPoint(lookAtP);
                            if (v0.z < 0) v0.z = -v0.z;
                            v0.x = 0;
                            lookAtP = orientTrans.TransformPoint(v0);
                        }
                        if ((lockRotationAxis & Axis.Z) == Axis.Z) {
//                            transUp = usesLocalPosition && parentTrans != null ? parentTrans.up : Vector3.up;
                            // Fix to allow racing loops to keep cars straight and not flip it
                            if (usesLocalPosition && parentTrans != null) transUp = parentTrans.TransformDirection(Vector3.up);
                            else transUp = orientTrans.TransformDirection(Vector3.up);
                            transUp.z = orZ;
                        }
                    }
                    if (is2D) {
                        float rotY = 0;
                        float rotZ = Utils.GetAngle2D(orientTrans.position, lookAtP);
                        if (rotZ < 0) rotZ = 360 + rotZ;
                        if (is2DsideScroller) {
                            // Manage Y and modified Z rotation
                            rotY = lookAtP.x < orientTrans.position.x ? 180 : 0;
                            if (rotZ > 90 && rotZ < 270) rotZ = 180 - rotZ;
                        }
                        // Top-down, rotate Z axis
                        orientTrans.rotation = Quaternion.Euler(0, rotY, rotZ);
                    } else {
                        orientTrans.LookAt(lookAtP, transUp);
                    }
                    break;
                }
            }
        }

        internal override void Rewind()
        {
            if (isPartialPath) {
                DoUpdate(0);
            } else {
                base.Rewind();
            }
        }

        internal override void Complete()
        {
            if (isPartialPath) {
                DoUpdate(_duration);
            } else {
                base.Complete();
            }
        }

        /// <summary>
        /// Returns the point at the given percentage (0 to 1),
        /// considering the path at constant speed.
        /// Used by DoUpdate and by Tweener.GetPointOnPath.
        /// </summary>
        /// <param name="t">
        /// The percentage (0 to 1) at which to get the point.
        /// </param>
        internal Vector3 GetConstPointOnPath(float t)
        {
            int tmp;
            return GetConstPointOnPath(t, false, null, out tmp);
        }
        /// <summary>
        /// Returns the point at the given percentage (0 to 1),
        /// considering the path at constant speed.
        /// Used by DoUpdate and by Tweener.GetPointOnPath.
        /// </summary>
        /// <param name="t">
        /// The percentage (0 to 1) at which to get the point.
        /// </param>
        /// <param name="p_updatePathPerc">
        /// IF <c>true</c> updates also <see cref="pathPerc"/> value
        /// (necessary if this method is called for an update).
        /// </param>
        /// <param name="p_path">
        /// IF not NULL uses the given path instead than the default one.
        /// </param>
        /// <param name="out_waypointIndex">
        /// Index of waypoint we're moving to (or where we are). Only used for Linear paths.
        /// </param>
        internal Vector3 GetConstPointOnPath(float t, bool p_updatePathPerc, Path p_path, out int out_waypointIndex)
        {
            if (p_updatePathPerc) return p_path.GetConstPoint(t, out pathPerc, out out_waypointIndex);
            out_waypointIndex = -1;
            return path.GetConstPoint(t);
        }

        /// <summary>
        /// Returns the percentage of the path length occupied by the given path waypoints interval.
        /// </summary>
        internal float GetWaypointsLengthPercentage(int p_pathWaypointId0, int p_pathWaypointId1)
        {
            switch (pathType) {
            case PathType.Linear:
                if (path.waypointsLength == null) path.StoreWaypointsLengths(SUBDIVISIONS_MULTIPLIER);
                return path.timesTable[p_pathWaypointId1] - path.timesTable[p_pathWaypointId0];
            default: // Curved
                if (path.waypointsLength == null) path.StoreWaypointsLengths(SUBDIVISIONS_MULTIPLIER);
                float partialLen = 0;
                for (int i = p_pathWaypointId0; i < p_pathWaypointId1; ++i) {
                    partialLen += path.waypointsLength[i];
                }
                float perc = partialLen / path.pathLength;
                if (perc > 1) perc = 1; // Limit in case of near errors (because full path length is calculated differently then sum of waypoints)
                return perc;
            }
        }

        bool IsWaypoint(Vector3 position, out int waypointIndex)
        {
            int pathLen = path.path.Length;
            for (int i = 0; i < pathLen; ++i) {
                float diffX = path.path[i].x - position.x;
                float diffY = path.path[i].y - position.y;
                float diffZ = path.path[i].z - position.z;
                if (diffX < 0) diffX = 0;
                if (diffY < 0) diffY = 0;
                if (diffZ < 0) diffZ = 0;
                if (diffX < EPSILON && diffY < EPSILON && diffZ < EPSILON) {
                    waypointIndex = i;
                    return true;
                }
            }
            waypointIndex = -1;
            return false;
        }

        // ===================================================================================
        // HELPERS ---------------------------------------------------------------------------

        internal void SwitchToPartialPath(float p_duration, EaseType p_easeType, float p_partialStartPerc, float p_partialChangePerc)
        {
            isPartialPath = true;
            _duration = p_duration;
            SetEase(p_easeType);
            startPerc = p_partialStartPerc;
            changePerc = p_partialChangePerc;
        }

        internal void ResetToFullPath(float p_duration, EaseType p_easeType)
        {
            isPartialPath = false;
            _duration = p_duration;
            SetEase(p_easeType);
            startPerc = 0;
            changePerc = 1;
        }
    }
}
