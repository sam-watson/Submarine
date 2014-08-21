//
// TweenVar.cs
//
// Author: Daniele Giardini
//
// Copyright (c) 2012 Daniele Giardini - Holoville s.a.s. - http://www.holoville.com
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

using Holoville.HOTween.Core;
using Holoville.HOTween.Core.Easing;
using UnityEngine;

namespace Holoville.HOTween
{
    /// <summary>
    /// A special class used to setup a "virtual" tween,
    /// which will not actually be tweened nor updated,
    /// but will just set and return a value when you call Update.
    /// </summary>
    public class TweenVar
    {
        // VARS ///////////////////////////////////////////////////

        /// <summary>
        /// Virtual duration
        /// (you could also set it to 1 or 100 to treat it as a percentage).
        /// </summary>
        public float duration;

        float _value;
        float _startVal;
        float _endVal;
        EaseType _easeType;
        EaseCurve _easeCurve;
        AnimationCurve _easeAnimationCurve;
        float _elapsed;

        float changeVal;
        TweenDelegate.EaseFunc ease;

        // GETS/SETS //////////////////////////////////////////////

        /// <summary>
        /// Start value (FROM).
        /// </summary>
        public float startVal
        {
            get
            {
                return _startVal;
            }
            set
            {
                _startVal = value;
                SetChangeVal();
            }
        }

        /// <summary>
        /// End value (TO).
        /// </summary>
        public float endVal
        {
            get
            {
                return _endVal;
            }
            set
            {
                _endVal = value;
                SetChangeVal();
            }
        }

        /// <summary>
        /// Ease type.
        /// </summary>
        public EaseType easeType
        {
            get
            {
                return _easeType;
            }
            set
            {
                _easeType = value;
                ease = EaseInfo.GetEaseInfo(_easeType).ease;
            }
        }

        /// <summary>
        /// Ease type as AnimationCurve.
        /// </summary>
        public AnimationCurve easeCurve
        {
            get
            {
                return _easeAnimationCurve;
            }
            set
            {
                _easeAnimationCurve = value;
                _easeCurve = new EaseCurve(_easeAnimationCurve);
                ease = _easeCurve.Evaluate;
            }
        }

        // READ-ONLY GETS /////////////////////////////////////////

        /// <summary>
        /// The current value of this <see cref="TweenVar"/>
        /// </summary>
        public float value
        {
            get
            {
                return _value;
            }
        }

        /// <summary>
        /// The current elapsed time.
        /// </summary>
        public float elapsed
        {
            get
            {
                return _elapsed;
            }
        }


        // ***********************************************************************************
        // CONSTRUCTOR
        // ***********************************************************************************

        /// <summary>
        /// Creates a new TweenVar instance using Linear ease.
        /// </summary>
        /// <param name="p_startVal">
        /// Start value (FROM).
        /// </param>
        /// <param name="p_endVal">
        /// End value (TO).
        /// </param>
        /// <param name="p_duration">
        /// Virtual duration.
        /// (you could also set it to 1 or 100 to treat it as a percentage).
        /// </param>
        public TweenVar(float p_startVal, float p_endVal, float p_duration)
            : this(p_startVal, p_endVal, p_duration, EaseType.Linear)
        {
        }

        /// <summary>
        /// Creates a new TweenVar instance.
        /// </summary>
        /// <param name="p_startVal">
        /// Start value (FROM).
        /// </param>
        /// <param name="p_endVal">
        /// End value (TO).
        /// </param>
        /// <param name="p_duration">
        /// Virtual duration.
        /// (you could also set it to 1 or 100 to treat it as a percentage).
        /// </param>
        /// <param name="p_easeType">
        /// Ease type.
        /// </param>
        public TweenVar(float p_startVal, float p_endVal, float p_duration, EaseType p_easeType)
        {
            startVal = _value = p_startVal;
            endVal = p_endVal;
            duration = p_duration;
            easeType = p_easeType;
        }

        /// <summary>
        /// Creates a new TweenVar instance.
        /// </summary>
        /// <param name="p_startVal">
        /// Start value (FROM).
        /// </param>
        /// <param name="p_endVal">
        /// End value (TO).
        /// </param>
        /// <param name="p_duration">
        /// Virtual duration.
        /// (you could also set it to 1 or 100 to treat it as a percentage).
        /// </param>
        /// <param name="p_easeCurve">
        /// AnimationCurve used for ease.
        /// </param>
        public TweenVar(float p_startVal, float p_endVal, float p_duration, AnimationCurve p_easeCurve)
        {
            startVal = _value = p_startVal;
            endVal = p_endVal;
            duration = p_duration;
            easeCurve = p_easeCurve;
        }

        // ===================================================================================
        // METHODS ---------------------------------------------------------------------------

        /// <summary>
        /// Sets and returns the value at which this <see cref="TweenVar"/>
        /// would be after the given absolute time.
        /// </summary>
        /// <param name="p_elapsed">
        /// The elapsed time to calculate.
        /// </param>
        public float Update(float p_elapsed)
        {
            return Update(p_elapsed, false);
        }

        /// <summary>
        /// Sets and returns the value at which this <see cref="TweenVar"/>
        /// would be after the given time.
        /// </summary>
        /// <param name="p_elapsed">
        /// The elapsed time to calculate.
        /// </param>
        /// <param name="p_relative">
        /// If <c>true</c> consideres p_elapsed as relative,
        /// meaning it will be added to the previous elapsed time,
        /// otherwise it is considered absolute.
        /// </param>
        public float Update(float p_elapsed, bool p_relative)
        {
            _elapsed = (p_relative ? _elapsed + p_elapsed : p_elapsed);
            if (_elapsed > duration)
            {
                _elapsed = duration;
            }
            else if (_elapsed < 0)
            {
                _elapsed = 0;
            }
            _value = ease(_elapsed, _startVal, changeVal, duration, HOTween.defEaseOvershootOrAmplitude, HOTween.defEasePeriod);
            return _value;
        }

        // ===================================================================================
        // PRIVATE METHODS -------------------------------------------------------------------

        void SetChangeVal()
        {
            changeVal = endVal - startVal;
        }
    }
}
