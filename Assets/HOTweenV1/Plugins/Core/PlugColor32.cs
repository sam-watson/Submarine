// Author: Daniele Giardini
// Copyright (c) 2013 Daniele Giardini - Holoville - http://www.holoville.com
// Created: 2013/12/02 20:15
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

namespace Holoville.HOTween.Plugins.Core
{
    /// <summary>
    /// Default plugin for the tweening of Color32 objects.
    /// </summary>
    public class PlugColor32 : ABSTweenPlugin
    {
         // VARS ///////////////////////////////////////////////////

        internal static Type[] validPropTypes = {typeof(Color32)};
        internal static Type[] validValueTypes = {typeof(Color32)};

        Color typedStartVal;
        Color typedEndVal;
        Color diffChangeVal; // Used for incremental loops.

        // GETS/SETS //////////////////////////////////////////////

        /// <summary>
        /// Gets the untyped start value,
        /// sets both the untyped and the typed start value.
        /// </summary>
        protected override object startVal
        {
            get
            {
                return _startVal;
            }
            set
            {
                if (tweenObj.isFrom && isRelative)
                {
                    _startVal = typedStartVal = typedEndVal + (Color)value;
                }
                else
                {
                    _startVal = typedStartVal = (Color32)value;
                }
            }
        }

        /// <summary>
        /// Gets the untyped end value,
        /// sets both the untyped and the typed end value.
        /// </summary>
        protected override object endVal
        {
            get
            {
                return _endVal;
            }
            set
            {
                _endVal = typedEndVal = (Color32)value;
            }
        }


        // ***********************************************************************************
        // CONSTRUCTOR
        // ***********************************************************************************

        /// <summary>
        /// Creates a new instance of this plugin using the main ease type.
        /// </summary>
        /// <param name="p_endVal">
        /// The <see cref="Color32"/> value to tween to.
        /// </param>
        public PlugColor32(Color32 p_endVal)
            : base(p_endVal, false)
        {
        }

        /// <summary>
        /// Creates a new instance of this plugin.
        /// </summary>
        /// <param name="p_endVal">
        /// The <see cref="Color32"/> value to tween to.
        /// </param>
        /// <param name="p_easeType">
        /// The <see cref="EaseType"/> to use.
        /// </param>
        public PlugColor32(Color32 p_endVal, EaseType p_easeType)
            : base(p_endVal, p_easeType, false)
        {
        }

        /// <summary>
        /// Creates a new instance of this plugin using the main ease type.
        /// </summary>
        /// <param name="p_endVal">
        /// The <see cref="Color32"/> value to tween to.
        /// </param>
        /// <param name="p_isRelative">
        /// If <c>true</c>, the given end value is considered relative instead than absolute.
        /// </param>
        public PlugColor32(Color32 p_endVal, bool p_isRelative)
            : base(p_endVal, p_isRelative)
        {
        }

        /// <summary>
        /// Creates a new instance of this plugin.
        /// </summary>
        /// <param name="p_endVal">
        /// The <see cref="Color32"/> value to tween to.
        /// </param>
        /// <param name="p_easeType">
        /// The <see cref="EaseType"/> to use.
        /// </param>
        /// <param name="p_isRelative">
        /// If <c>true</c>, the given end value is considered relative instead than absolute.
        /// </param>
        public PlugColor32(Color32 p_endVal, EaseType p_easeType, bool p_isRelative)
            : base(p_endVal, p_easeType, p_isRelative)
        {
        }

        /// <summary>
        /// Creates a new instance of this plugin.
        /// </summary>
        /// <param name="p_endVal">
        /// The <see cref="Color32"/> value to tween to.
        /// </param>
        /// <param name="p_easeAnimCurve">
        /// The <see cref="AnimationCurve"/> to use for easing.
        /// </param>
        /// <param name="p_isRelative">
        /// If <c>true</c>, the given end value is considered relative instead than absolute.
        /// </param>
        public PlugColor32(Color32 p_endVal, AnimationCurve p_easeAnimCurve, bool p_isRelative)
            : base(p_endVal, p_easeAnimCurve, p_isRelative) {}

        // ===================================================================================
        // METHODS ---------------------------------------------------------------------------

        /// <summary>
        /// Returns the speed-based duration based on the given speed x second.
        /// </summary>
        protected override float GetSpeedBasedDuration(float p_speed)
        {
            float speedDur = 1f/p_speed;
            if (speedDur < 0)
            {
                speedDur = -speedDur;
            }
            return speedDur;
        }

        /// <summary>
        /// Sets the typed changeVal based on the current startVal and endVal.
        /// </summary>
        protected override void SetChangeVal()
        {
            if (isRelative && !tweenObj.isFrom)
            {
                typedEndVal = typedStartVal + typedEndVal;
            }

            diffChangeVal = typedEndVal - typedStartVal;
        }

        /// <summary>
        /// Sets the correct values in case of Incremental loop type.
        /// </summary>
        /// <param name="p_diffIncr">
        /// The difference from the previous loop increment.
        /// </param>
        protected override void SetIncremental(int p_diffIncr)
        {
            typedStartVal += diffChangeVal*p_diffIncr;
            typedEndVal += diffChangeVal*p_diffIncr;
        }

        /// <summary>
        /// Sets the correct values in case of RestartIncremental.
        /// </summary>
        protected override void SetIncrementalRestart()
        {
            Color32 prevStartVal = typedStartVal;
            startVal = (Color32)GetValue();
            Color32 diff = typedStartVal - prevStartVal;
            typedEndVal = typedStartVal + diff;
        }

        /// <summary>
        /// Updates the tween.
        /// </summary>
        /// <param name="p_totElapsed">
        /// The total elapsed time since startup.
        /// </param>
        protected override void DoUpdate(float p_totElapsed)
        {
            float time = ease(p_totElapsed, 0f, 1f, _duration, tweenObj.easeOvershootOrAmplitude, tweenObj.easePeriod);

            SetValue((Color32)(new Color(
                typedStartVal.r + diffChangeVal.r * time,
                typedStartVal.g + diffChangeVal.g * time,
                typedStartVal.b + diffChangeVal.b * time,
                typedStartVal.a + diffChangeVal.a * time
            )));
        }
    }
}