//
// TweenEvent.cs
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
    using Holoville.HOTween.Plugins.Core;

    /// <summary>
    /// This object is passed as the only parameter of all HOTween's callbacks.
    /// </summary>
    public class TweenEvent
    {
        // VARS ///////////////////////////////////////////////////

        readonly IHOTweenComponent _tween;
        readonly object[] _parms;
        readonly ABSTweenPlugin _plugin;

        // READ-ONLY GETS /////////////////////////////////////////

        /// <summary>
        /// A reference to the IHOTweenComponent that invoked the callback method.
        /// </summary>
        public IHOTweenComponent tween
        {
            get {
                return _tween;
            }
        }

        /// <summary>
        /// An array of eventual parameters that were passed to the callback.
        /// </summary>
        public object[] parms
        {
            get {
                return _parms;
            }
        }

        /// <summary>
        /// The plugin (if any) that triggered the callback.
        /// </summary>
        public ABSTweenPlugin plugin
        {
            get {
                return _plugin;
            }
        }


        // ***********************************************************************************
        // CONSTRUCTOR
        // ***********************************************************************************

        internal TweenEvent(IHOTweenComponent p_tween, object[] p_parms)
        {
            _tween = p_tween;
            _parms = p_parms;
            _plugin = null;
        }

        internal TweenEvent(IHOTweenComponent p_tween, object[] p_parms, ABSTweenPlugin p_plugin)
        {
            _tween = p_tween;
            _parms = p_parms;
            _plugin = p_plugin;
        }
    }
}
