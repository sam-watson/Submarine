//
// OverwriteManager.cs
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

using System.Collections.Generic;
using Holoville.HOTween.Plugins.Core;

namespace Holoville.HOTween.Core
{
    /// <summary>
    /// Manager used for automatic control of eventual overwriting of tweens.
    /// It is disabled by default, you need to call <see cref="HOTween.EnableOverwriteManager"/> to enable it.
    /// </summary>
    internal class OverwriteManager
    {
        // VARS ///////////////////////////////////////////////////

        internal bool enabled;
        internal bool logWarnings;

        /// <summary>
        /// List of currently running Tweeners
        /// (meaning all Tweeners whose OnStart has been called, paused or not).
        /// </summary>
        readonly List<Tweener> runningTweens;


        // ***********************************************************************************
        // CONSTRUCTOR
        // ***********************************************************************************

        public OverwriteManager()
        {
            runningTweens = new List<Tweener>();
        }

        // ===================================================================================
        // METHODS ---------------------------------------------------------------------------

        public void AddTween(Tweener p_tween)
        {
            if (enabled) {
                // Check running tweens for eventual overwrite.
                List<ABSTweenPlugin> addPlugs = p_tween.plugins;

                int runningTweensCount = runningTweens.Count - 1;
                int addPlugsCount = addPlugs.Count;

                for (int i = runningTweensCount; i > -1; --i) {
                    Tweener tw = runningTweens[i];
                    List<ABSTweenPlugin> twPlugins = tw.plugins;
                    int twPluginsCount = twPlugins.Count;

                    if (tw.target == p_tween.target) {
                        // Check internal plugins.
                        for (int n = 0; n < addPlugsCount; ++n) {
                            ABSTweenPlugin addPlug = addPlugs[n];
                            for (int c = twPluginsCount - 1; c > -1; --c) {
                                ABSTweenPlugin plug = twPlugins[c];
                                if (plug.propName == addPlug.propName && (addPlug.pluginId == -1 || plug.pluginId == -1 || plug.pluginId == addPlug.pluginId)) {
                                    if (tw.isSequenced && p_tween.isSequenced && tw.contSequence == p_tween.contSequence) {
                                        goto NEXT_TWEEN;
                                    }
                                    if (!tw._isPaused && (!tw.isSequenced || !tw.isComplete)) {
                                        // Overwrite old plugin.
                                        twPlugins.RemoveAt(c);
                                        twPluginsCount--;
                                        if (HOTween.isEditor && HOTween.warningLevel == WarningLevel.Verbose) {
                                            string t0 = addPlug.GetType().ToString();
                                            t0 = t0.Substring(t0.LastIndexOf(".") + 1);
                                            string t1 = plug.GetType().ToString();
                                            t1 = t1.Substring(t1.LastIndexOf(".") + 1);
                                            if (logWarnings) TweenWarning.Log(t0 + " is overwriting " + t1 + " for " + tw.target + "." + plug.propName);
                                        }
                                        // Check if whole tween needs to be removed.
                                        if (twPluginsCount == 0) {
                                            if (tw.isSequenced) {
                                                tw.contSequence.Remove(tw);
                                            }
                                            runningTweens.RemoveAt(i);
                                            tw.Kill(false);
                                        }
                                        // Dispatch eventual pluginOverwritten event
                                        if (tw.onPluginOverwritten != null) {
                                            tw.onPluginOverwritten();
                                        } else if (tw.onPluginOverwrittenWParms != null) {
                                            tw.onPluginOverwrittenWParms(new TweenEvent(tw, tw.onPluginOverwrittenParms));
                                        }
                                        // If whole tween was killed jump to next tween
                                        if (tw.destroyed) goto NEXT_TWEEN;
                                    }
                                }
                            }
                        }
                    NEXT_TWEEN: ;
                    }
                }
            }

            runningTweens.Add(p_tween);
        }

        public void RemoveTween(Tweener p_tween)
        {
            int runningTweensCount = runningTweens.Count;
            for (int i = 0; i < runningTweensCount; ++i) {
                if (runningTweens[i] == p_tween) {
                    runningTweens.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
