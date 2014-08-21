//
// EaseType.cs
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

namespace Holoville.HOTween
{
    /// <summary>
    /// Enumeration of ease types.
    /// </summary>
    public enum EaseType
    {
        /// <summary>
        /// Linear.
        /// </summary>
        Linear,

        /// <summary>
        /// Ease in sine.
        /// </summary>
        EaseInSine,

        /// <summary>
        /// Ease out sine.
        /// </summary>
        EaseOutSine,

        /// <summary>
        /// Ease in out sine.
        /// </summary>
        EaseInOutSine,

        /// <summary>
        /// Ease in quad.
        /// </summary>
        EaseInQuad,

        /// <summary>
        /// Ease out quad.
        /// </summary>
        EaseOutQuad,

        /// <summary>
        /// Ease in out quad.
        /// </summary>
        EaseInOutQuad,

        /// <summary>
        /// Ease in cubic.
        /// </summary>
        EaseInCubic,

        /// <summary>
        /// Ease out cubic.
        /// </summary>
        EaseOutCubic,

        /// <summary>
        /// Ease in out cubic.
        /// </summary>
        EaseInOutCubic,

        /// <summary>
        /// Ease in quart.
        /// </summary>
        EaseInQuart,

        /// <summary>
        /// Ease out quart.
        /// </summary>
        EaseOutQuart,

        /// <summary>
        /// Ease in out quart.
        /// </summary>
        EaseInOutQuart,

        /// <summary>
        /// Ease in quint.
        /// </summary>
        EaseInQuint,

        /// <summary>
        /// Ease out quint.
        /// </summary>
        EaseOutQuint,

        /// <summary>
        /// Ease in out quint.
        /// </summary>
        EaseInOutQuint,

        /// <summary>
        /// Ease in expo.
        /// </summary>
        EaseInExpo,

        /// <summary>
        /// Ease out expo.
        /// </summary>
        EaseOutExpo,

        /// <summary>
        /// Ease in out expo.
        /// </summary>
        EaseInOutExpo,

        /// <summary>
        /// Ease in circ.
        /// </summary>
        EaseInCirc,

        /// <summary>
        /// Ease out circ.
        /// </summary>
        EaseOutCirc,

        /// <summary>
        /// Ease in out circ.
        /// </summary>
        EaseInOutCirc,

        /// <summary>
        /// Ease in elastic.
        /// </summary>
        EaseInElastic,

        /// <summary>
        /// Ease out elastic.
        /// </summary>
        EaseOutElastic,

        /// <summary>
        /// Ease in out elastic.
        /// </summary>
        EaseInOutElastic,

        /// <summary>
        /// Ease in back.
        /// </summary>
        EaseInBack,

        /// <summary>
        /// Ease out back.
        /// </summary>
        EaseOutBack,

        /// <summary>
        /// Ease in out back.
        /// </summary>
        EaseInOutBack,

        /// <summary>
        /// Ease in bounce.
        /// </summary>
        EaseInBounce,

        /// <summary>
        /// Ease out bounce.
        /// </summary>
        EaseOutBounce,

        /// <summary>
        /// Ease in out bounce.
        /// </summary>
        EaseInOutBounce,

        /// <summary>
        /// Don't assign this! It's assigned internally when setting the ease to an AnimationCurve
        /// </summary>
        AnimationCurve,

        /// <summary>
        /// Ease in strong.
        /// OBSOLETE: use EaseInQuint instead.
        /// </summary>
        [System.ObsoleteAttribute("Use EaseInQuint instead.")]
        EaseInStrong,
        
        /// <summary>
        /// OBSOLETE: use EaseOutQuint instead.
        /// Ease out strong.
        /// </summary>
        [System.ObsoleteAttribute("Use EaseOutQuint instead.")]
        EaseOutStrong,
        
        /// <summary>
        /// OBSOLETE: use EaseInOutQuint instead.
        /// Ease in out strong.
        /// </summary>
        [System.ObsoleteAttribute("Use EaseInOutQuint instead.")]
        EaseInOutStrong
    }
}
