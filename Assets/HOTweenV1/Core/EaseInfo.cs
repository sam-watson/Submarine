//
// EaseInfo.cs
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

using Holoville.HOTween.Core.Easing;

namespace Holoville.HOTween.Core
{
    /// <summary>
    /// Enumeration of ease types.
    /// </summary>
    public class EaseInfo
    {
        // VARS ///////////////////////////////////////////////////

        /// <summary>
        /// Ease function.
        /// </summary>
        public TweenDelegate.EaseFunc ease;

        /// <summary>
        /// Inverse ease function.
        /// </summary>
        public TweenDelegate.EaseFunc inverseEase;


        // ***********************************************************************************
        // CONSTRUCTOR
        // ***********************************************************************************

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="p_ease">
        /// The ease function.
        /// </param>
        /// <param name="p_inverseEase">
        /// Inverse ease function.
        /// </param>
        EaseInfo(TweenDelegate.EaseFunc p_ease, TweenDelegate.EaseFunc p_inverseEase)
        {
            ease = p_ease;
            inverseEase = p_inverseEase;
        }
		
		 static EaseInfo easeInSineInfo = new EaseInfo(Sine.EaseIn, Sine.EaseOut);
		 static EaseInfo easeOutSineInfo = new EaseInfo(Sine.EaseOut, Sine.EaseIn);
		 static EaseInfo easeInOutSineInfo = new EaseInfo(Sine.EaseInOut, null);
		 static EaseInfo easeInQuadInfo = new EaseInfo(Quad.EaseIn, Quad.EaseOut);
		 static EaseInfo easeOutQuadInfo = new EaseInfo(Quad.EaseOut, Quad.EaseIn);
		 static EaseInfo easeInOutQuadInfo = new EaseInfo(Quad.EaseInOut, null);
		 static EaseInfo easeInCubicInfo = new EaseInfo(Cubic.EaseIn, Cubic.EaseOut);
		 static EaseInfo easeOutCubicInfo = new EaseInfo(Cubic.EaseOut, Cubic.EaseIn);
		 static EaseInfo easeInOutCubicInfo = new EaseInfo(Cubic.EaseInOut, null);
		 static EaseInfo easeInQuartInfo = new EaseInfo(Quart.EaseIn, Quart.EaseOut);
		 static EaseInfo easeOutQuartInfo = new EaseInfo(Quart.EaseOut, Quart.EaseIn);
		 static EaseInfo easeInOutQuartInfo = new EaseInfo(Quart.EaseInOut, null);
		 static EaseInfo easeInQuintInfo = new EaseInfo(Quint.EaseIn, Quint.EaseOut);
		 static EaseInfo easeOutQuintInfo = new EaseInfo(Quint.EaseOut, Quint.EaseIn);
		 static EaseInfo easeInOutQuintInfo = new EaseInfo(Quint.EaseInOut, null);
		 static EaseInfo easeInExpoInfo = new EaseInfo(Expo.EaseIn, Expo.EaseOut);
		 static EaseInfo easeOutExpoInfo = new EaseInfo(Expo.EaseOut, Expo.EaseIn);
		 static EaseInfo easeInOutExpoInfo = new EaseInfo(Expo.EaseInOut, null);
		 static EaseInfo easeInCircInfo = new EaseInfo(Circ.EaseIn, Circ.EaseOut);
		 static EaseInfo easeOutCircInfo = new EaseInfo(Circ.EaseOut, Circ.EaseIn);
		 static EaseInfo easeInOutCircInfo = new EaseInfo(Circ.EaseInOut, null);
		 static EaseInfo easeInElasticInfo = new EaseInfo(Elastic.EaseIn, Elastic.EaseOut);
		 static EaseInfo easeOutElasticInfo = new EaseInfo(Elastic.EaseOut, Elastic.EaseIn);
		 static EaseInfo easeInOutElasticInfo = new EaseInfo(Elastic.EaseInOut, null);
		 static EaseInfo easeInBackInfo = new EaseInfo(Back.EaseIn, Back.EaseOut);
		 static EaseInfo easeOutBackInfo = new EaseInfo(Back.EaseOut, Back.EaseIn);
		 static EaseInfo easeInOutBackInfo = new EaseInfo(Back.EaseInOut, null);
		 static EaseInfo easeInBounceInfo = new EaseInfo(Bounce.EaseIn, Bounce.EaseOut);
		 static EaseInfo easeOutBounceInfo = new EaseInfo(Bounce.EaseOut, Bounce.EaseIn);
		 static EaseInfo easeInOutBounceInfo = new EaseInfo(Bounce.EaseInOut, null);
		 static EaseInfo easeInStrongInfo = new EaseInfo(Strong.EaseIn, Strong.EaseOut);
		 static EaseInfo easeOutStrongInfo = new EaseInfo(Strong.EaseOut, Strong.EaseIn);
		 static EaseInfo easeInOutStrongInfo = new EaseInfo(Strong.EaseInOut, null);
		 static EaseInfo defaultEaseInfo = new EaseInfo(Linear.EaseNone, null);

        // ===================================================================================
        // STATIC METHODS --------------------------------------------------------------------

        /// <summary>
        /// Returns an <see cref="EaseInfo"/> instance based on the given <see cref="EaseType"/>.
        /// </summary>
        /// <param name="p_easeType">
        /// An <see cref="EaseType"/>.
        /// </param>
        internal static EaseInfo GetEaseInfo(EaseType p_easeType)
        {
            switch (p_easeType)
            {
                case EaseType.EaseInSine:
                    return easeInSineInfo;
                case EaseType.EaseOutSine:
                    return easeOutSineInfo;
                case EaseType.EaseInOutSine:
                    return easeInOutSineInfo;
 
                case EaseType.EaseInQuad:
					return easeInQuadInfo;
                case EaseType.EaseOutQuad:
                    return easeOutQuadInfo;
                case EaseType.EaseInOutQuad:
                    return easeInOutQuadInfo;

                case EaseType.EaseInCubic:
                    return easeInCubicInfo;
                case EaseType.EaseOutCubic:
                    return easeOutCubicInfo;
                case EaseType.EaseInOutCubic:
                    return easeInOutCubicInfo;

                case EaseType.EaseInQuart:
                    return easeInQuartInfo;
                case EaseType.EaseOutQuart:
                    return easeOutQuartInfo;
                case EaseType.EaseInOutQuart:
                    return easeInOutQuartInfo;

                case EaseType.EaseInQuint:
                    return easeInQuintInfo;
                case EaseType.EaseOutQuint:
                    return easeOutQuintInfo;
                case EaseType.EaseInOutQuint:
                    return easeInOutQuintInfo;

                case EaseType.EaseInExpo:
                    return easeInExpoInfo;
                case EaseType.EaseOutExpo:
                    return easeOutExpoInfo;
                case EaseType.EaseInOutExpo:
                    return easeInOutExpoInfo;

                case EaseType.EaseInCirc:
                    return easeInCircInfo;
                case EaseType.EaseOutCirc:
                    return easeOutCircInfo;
                case EaseType.EaseInOutCirc:
                    return easeInOutCircInfo;

                case EaseType.EaseInElastic:
                    return easeInElasticInfo;
                case EaseType.EaseOutElastic:
                    return easeOutElasticInfo;
                case EaseType.EaseInOutElastic:
                    return easeInOutElasticInfo;

                case EaseType.EaseInBack:
                    return easeInBackInfo;
                case EaseType.EaseOutBack:
                    return easeOutBackInfo;
                case EaseType.EaseInOutBack:
                    return easeInOutBackInfo;

                case EaseType.EaseInBounce:
                    return easeInBounceInfo;
                case EaseType.EaseOutBounce:
                    return easeOutBounceInfo;
                case EaseType.EaseInOutBounce:
                    return easeInOutBounceInfo;

                case EaseType.EaseInStrong:
                    return easeInStrongInfo;
                case EaseType.EaseOutStrong:
                    return easeOutStrongInfo;
                case EaseType.EaseInOutStrong:
                    return easeInOutStrongInfo;

                default:
                    return defaultEaseInfo;
            }
        }
    }
}
