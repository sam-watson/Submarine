//
// HOTween.cs
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

// Created: 2011/12/13
// Last update: 2012/09/20

using System.Collections;
using System.Collections.Generic;
using Holoville.HOTween.Core;
using Holoville.HOTween.Plugins;
using Holoville.HOTween.Plugins.Core;
using UnityEngine;

namespace Holoville.HOTween
{
    /// <summary>
    /// Main tween manager.
    /// Controls all tween types (<see cref="Tweener"/> and <see cref="Sequence"/>),
    /// and is used to directly create Tweeners (to create Sequences, directly create a new <see cref="Sequence"/> instead).
    /// <para>Author: Daniele Giardini (http://www.holoville.com)</para>
    /// </summary>
    public class HOTween : MonoBehaviour
    {
        // SETTINGS ///////////////////////////////////////////////

        /// <summary>
        /// HOTween version.
        /// </summary>
        public static readonly string VERSION = "1.3.355";

#if MICRO
        public static readonly bool IS_MICRO = true;
#else
        public static readonly bool IS_MICRO = false;
#endif

        /// <summary>
        /// HOTween author - me! :P
        /// </summary>
        public const string AUTHOR = "Daniele Giardini - Holoville";

        const string GAMEOBJNAME = "HOTween";

        // DEFAULTS ///////////////////////////////////////////////

        /// <summary>
        /// Default <see cref="UpdateType"/> that will be used by any new Tweener/Sequence that doesn't implement a specific ease
        /// (default = <c>EaseType.easeOutQuad</c>)
        /// </summary>
        public static UpdateType defUpdateType = UpdateType.Update;

        /// <summary>
        /// Default time scale that will be used by any new Tweener/Sequence that doesn't implement a specific timeScale
        /// (default = <c>1</c>).
        /// </summary>
        public static float defTimeScale = 1;

        /// <summary>
        /// Default <see cref="EaseType"/> that will be used by any new Tweener/Sequence that doesn't implement a specific ease
        /// (default = <c>EaseType.easeOutQuad</c>).
        /// </summary>
        public static EaseType defEaseType = EaseType.EaseOutQuad;

        /// <summary>
        /// Default overshoot to use with Back easeTypes.
        /// </summary>
        public static float defEaseOvershootOrAmplitude = 1.70158f;

        /// <summary>
        /// Default period to use with Elastic easeTypes.
        /// </summary>
        public static float defEasePeriod = 0;

        /// <summary>
        /// Default <see cref="LoopType"/> that will be used by any Tweener/Sequence that doesn't implement a specific loopType
        /// (default = <c>LoopType.Restart</c>).
        /// </summary>
        public static LoopType defLoopType = LoopType.Restart;

        // VARS ///////////////////////////////////////////////////

        /// <summary>
        /// If <c>true</c>, shows the eventual paths in use by <see cref="PlugVector3Path"/>
        /// while playing inside Unity's Editor (and if the Editor's Gizmos button is on).
        /// </summary>
        public static bool showPathGizmos;

        /// <summary>
        /// Level of message output in case an error is encountered.
        /// Warnings are logged when HOTween encounters an error, and automatically resolves it without throwing any exception
        /// (like if you try to tween an unexisting property, in which case the tween simply won't be generated,
        /// and an eventual warning will appear in the output window).
        /// </summary>
        public static WarningLevel warningLevel = WarningLevel.Verbose;

        /// <summary>
        /// <c>true</c> if the current player is iOS (iPhone).
        /// Used so simple Reflection instead than unsupported MemberAccessorCacher will be applyed
        /// (iOS doesn't support <c>Reflection.Emit</c>).
        /// </summary>
        internal static bool isIOS;

        /// <summary>
        /// <c>true</c> if the current player is running in the Editor.
        /// </summary>
        internal static bool isEditor;

        /// <summary>
        /// Filled by tweens that are completed, so that their onCompleteDispatch method can be called AFTER HOTween has eventually removed them
        /// (otherwise a Kill + To on the same target won't work).
        /// This field is emptied as soon as all onCompletes are called.
        /// </summary>
        internal static List<ABSTweenComponent> onCompletes = new List<ABSTweenComponent>();

        /// <summary>
        /// TRUE while inside the update loop
        /// </summary>
        internal static bool isUpdateLoop { get; private set; }

        static bool initialized;
        static bool isPermanent; // If TRUE doesn't destroy HOTween when all tweens are killed.
        static bool renameInstToCountTw; // If TRUE renames HOTween's instance to show running tweens.
        static float time;
        static bool isQuitting;
        static List<int> tweensToRemoveIndexes = new List<int>(); // Used for removing tweens that were killed during an update 

        // REFERENCES /////////////////////////////////////////////

        /// <summary>
        /// Reference to overwrite manager (if in use).
        /// </summary>
        internal static OverwriteManager overwriteManager;

        static List<ABSTweenComponent> tweens; // Contains both Tweeners than Sequences
        static GameObject tweenGOInstance;
        static HOTween it;

        // READ-ONLY GETS /////////////////////////////////////////

        /// <summary>
        /// Total number of tweeners/sequences (paused and delayed ones are included).
        /// Tweeners and sequences contained into other sequences don't count:
        /// for example, if there's only one sequence that contains 2 tweeners, <c>totTweens</c> will be 1.
        /// </summary>
        public static int totTweens
        {
            get
            {
                if (tweens == null) {
                    return 0;
                }
                return tweens.Count;
            }
        }


        // ***********************************************************************************
        // INIT
        // ***********************************************************************************

        /// <summary>
        /// Initializes <see cref="HOTween"/> and sets it as non-permanent
        /// (meaning HOTween instance will be destroyed when all tweens are killed,
        /// and re-created when needed).
        /// Call this method once when your application starts up,
        /// to avoid auto-initialization when the first tween is started or created,
        /// and to set options.
        /// </summary>
        public static void Init()
        {
            Init(false, true, false);
        }

        /// <summary>
        /// Initializes <see cref="HOTween"/>.
        /// Call this method once when your application starts up,
        /// to avoid auto-initialization when the first tween is started or created,
        /// and to set options.
        /// </summary>
        /// <param name="p_permanentInstance">
        /// If set to <c>true</c>, doesn't destroy HOTween manager when no tween is present,
        /// otherwise the manager is destroyed when all tweens have been killed,
        /// and re-created when needed.
        /// </param>
        public static void Init(bool p_permanentInstance)
        {
            Init(p_permanentInstance, true, false);
        }

        /// <summary>
        /// Initializes <see cref="HOTween"/>.
        /// Call this method once when your application starts up,
        /// to avoid auto-initialization when the first tween is started or created,
        /// and to set options.
        /// </summary>
        /// <param name="p_permanentInstance">
        /// If set to <c>true</c>, doesn't destroy HOTween manager when no tween is present,
        /// otherwise the manager is destroyed when all tweens have been killed,
        /// and re-created when needed.
        /// </param>
        /// <param name="p_renameInstanceToCountTweens">
        /// If <c>true</c>, renames HOTween's instance to show
        /// the current number of running tweens (only while in the Editor).
        /// </param>
        /// <param name="p_allowOverwriteManager">
        /// If <c>true</c>, allows HOTween's instance to enable or disable
        /// the OverwriteManager to improve performance if it is never needed.
        /// </param>
        public static void Init(bool p_permanentInstance, bool p_renameInstanceToCountTweens, bool p_allowOverwriteManager)
        {
            if (initialized) {
                return;
            }

            initialized = true;

            isIOS = (Application.platform == RuntimePlatform.IPhonePlayer);
            isEditor = Application.isEditor;
            isPermanent = p_permanentInstance;
            renameInstToCountTw = p_renameInstanceToCountTweens;

            if (p_allowOverwriteManager) {
                overwriteManager = new OverwriteManager();
            }

            if (isPermanent && tweenGOInstance == null) {
                NewTweenInstance();
                SetGOName();
            }
        }

        // ===================================================================================
        // UNITY METHODS ---------------------------------------------------------------------

        void OnApplicationQuit()
        {
            isQuitting = true;
        }

        void OnDrawGizmos()
        {
            if (tweens == null || !showPathGizmos) {
                return;
            }

            // Get all existing plugins.
            List<ABSTweenPlugin> plugs = GetPlugins();
            int pluginsCount = plugs.Count;

            // Find path plugins and draw paths.
            for (int i = 0; i < pluginsCount; ++i) {
                PlugVector3Path pathPlug = plugs[i] as PlugVector3Path;
                if (pathPlug != null && pathPlug.path != null) {
                    pathPlug.path.GizmoDraw(pathPlug.pathPerc, false);
                }
            }
        }

        void OnDestroy()
        {
            // Clear everything if this was the currenlty running HOTween.
            // HINT I can use OnDestroy also to check for scene changes, and instantiate another HOTween instance if I need to keep it running.
            // TODO For now HOTween is NOT destroyed when a scene is loaded, - add option to set it as destroyable?
            // (consider also isPermanent option if doing that).
            if (!isQuitting && this == it) Clear();
        }

        // ===================================================================================
        // TWEEN METHODS ---------------------------------------------------------------------

        /// <summary>
        /// Called internally each time a new <see cref="Sequence"/> is created.
        /// Adds the given Sequence to the tween list.
        /// </summary>
        /// <param name="p_sequence">
        /// The <see cref="Sequence"/> to add.
        /// </param>
        internal static void AddSequence(Sequence p_sequence)
        {
            if (!initialized) {
                Init();
            }

            AddTween(p_sequence);
        }

        /// <summary>
        /// Creates a new absolute tween with default values, and returns the <see cref="Tweener"/> representing it,
        /// or <c>null</c> if the tween was invalid (no valid property to tween was given).
        /// </summary>
        /// <param name="p_target">
        /// The tweening target (must be the object containing the properties or fields to tween).
        /// </param>
        /// <param name="p_duration">
        /// The duration in seconds of the tween.
        /// </param>
        /// <param name="p_propName">
        /// The name of the property or field to tween.
        /// </param>
        /// <param name="p_endVal">
        /// The end value the property should reach with the tween.
        /// </param>
        /// <returns>
        /// The newly created <see cref="Tweener"/>,
        /// or <c>null</c> if the parameters were invalid.
        /// </returns>
        public static Tweener To(object p_target, float p_duration, string p_propName, object p_endVal)
        {
            return To(p_target, p_duration, new TweenParms().Prop(p_propName, p_endVal));
        }
        /// <summary>
        /// Creates a new tween with default values, and returns the <see cref="Tweener"/> representing it,
        /// or <c>null</c> if the tween was invalid (no valid property to tween was given).
        /// </summary>
        /// <param name="p_target">
        /// The tweening target (must be the object containing the properties or fields to tween).
        /// </param>
        /// <param name="p_duration">
        /// The duration in seconds of the tween.
        /// </param>
        /// <param name="p_propName">
        /// The name of the property or field to tween.
        /// </param>
        /// <param name="p_endVal">
        /// The end value the property should reach with the tween.
        /// </param>
        /// <param name="p_isRelative">
        /// If <c>true</c> treats the end value as relative (tween BY instead than tween TO), otherwise as absolute.
        /// </param>
        /// <returns>
        /// The newly created <see cref="Tweener"/>,
        /// or <c>null</c> if the parameters were invalid.
        /// </returns>
        public static Tweener To(object p_target, float p_duration, string p_propName, object p_endVal, bool p_isRelative)
        {
            return To(p_target, p_duration, new TweenParms().Prop(p_propName, p_endVal, p_isRelative));
        }
        /// <summary>
        /// Creates a new tween with default values, and returns the <see cref="Tweener"/> representing it,
        /// or <c>null</c> if the tween was invalid (no valid property to tween was given).
        /// </summary>
        /// <param name="p_target">
        /// The tweening target (must be the object containing the properties or fields to tween).
        /// </param>
        /// <param name="p_duration">
        /// The duration in seconds of the tween.
        /// </param>
        /// <param name="p_propName">
        /// The name of the property or field to tween.
        /// </param>
        /// <param name="p_endVal">
        /// The end value the property should reach with the tween.
        /// </param>
        /// <param name="p_isRelative">
        /// If <c>true</c> treats the end value as relative (tween BY instead than tween TO), otherwise as absolute.
        /// </param>
        /// <param name="p_easeType">
        /// The ease to use.
        /// </param>
        /// <param name="p_delay">
        /// The eventual delay to apply.
        /// </param>
        /// <returns>
        /// The newly created <see cref="Tweener"/>,
        /// or <c>null</c> if the parameters were invalid.
        /// </returns>
        public static Tweener To(object p_target, float p_duration, string p_propName, object p_endVal, bool p_isRelative, EaseType p_easeType, float p_delay)
        {
            return To(p_target, p_duration, new TweenParms().Prop(p_propName, p_endVal, p_isRelative).Delay(p_delay).Ease(p_easeType));
        }
        /// <summary>
        /// Creates a new tween and returns the <see cref="Tweener"/> representing it,
        /// or <c>null</c> if the tween was invalid (no valid property to tween was given).
        /// </summary>
        /// <param name="p_target">
        /// The tweening target (must be the object containing the properties or fields to tween).
        /// </param>
        /// <param name="p_duration">
        /// The duration in seconds of the tween.
        /// </param>
        /// <param name="p_parms">
        /// A <see cref="TweenParms"/> representing the tween parameters.
        /// You can pass an existing one, or create a new one inline via method chaining,
        /// like <c>new TweenParms().Prop("x",10).Loops(2).OnComplete(myFunction)</c>
        /// </param>
        /// <returns>
        /// The newly created <see cref="Tweener"/>,
        /// or <c>null</c> if the parameters were invalid.
        /// </returns>
        public static Tweener To(object p_target, float p_duration, TweenParms p_parms)
        {
            if (!initialized) Init();

            Tweener tw = new Tweener(p_target, p_duration, p_parms);

            // Check if tween is valid.
            if (tw.isEmpty) {
                return null;
            }

            AddTween(tw);
            return tw;
        }

        /// <summary>
        /// Creates a new absolute FROM tween with default values, and returns the <see cref="Tweener"/> representing it,
        /// or <c>null</c> if the tween was invalid (no valid property to tween was given).
        /// </summary>
        /// <param name="p_target">
        /// The tweening target (must be the object containing the properties or fields to tween).
        /// </param>
        /// <param name="p_duration">
        /// The duration in seconds of the tween.
        /// </param>
        /// <param name="p_propName">
        /// The name of the property or field to tween.
        /// </param>
        /// <param name="p_fromVal">
        /// The end value the property should reach with the tween.
        /// </param>
        /// <returns>
        /// The newly created <see cref="Tweener"/>,
        /// or <c>null</c> if the parameters were invalid.
        /// </returns>
        public static Tweener From(object p_target, float p_duration, string p_propName, object p_fromVal)
        {
            return From(p_target, p_duration, new TweenParms().Prop(p_propName, p_fromVal));
        }
        /// <summary>
        /// Creates a new FROM tween with default values, and returns the <see cref="Tweener"/> representing it,
        /// or <c>null</c> if the tween was invalid (no valid property to tween was given).
        /// </summary>
        /// <param name="p_target">
        /// The tweening target (must be the object containing the properties or fields to tween).
        /// </param>
        /// <param name="p_duration">
        /// The duration in seconds of the tween.
        /// </param>
        /// <param name="p_propName">
        /// The name of the property or field to tween.
        /// </param>
        /// <param name="p_fromVal">
        /// The end value the property should reach with the tween.
        /// </param>
        /// <param name="p_isRelative">
        /// If <c>true</c> treats the end value as relative (tween BY instead than tween TO), otherwise as absolute.
        /// </param>
        /// <returns>
        /// The newly created <see cref="Tweener"/>,
        /// or <c>null</c> if the parameters were invalid.
        /// </returns>
        public static Tweener From(object p_target, float p_duration, string p_propName, object p_fromVal, bool p_isRelative)
        {
            return From(p_target, p_duration, new TweenParms().Prop(p_propName, p_fromVal, p_isRelative));
        }
        /// <summary>
        /// Creates a new FROM tween with default values, and returns the <see cref="Tweener"/> representing it,
        /// or <c>null</c> if the tween was invalid (no valid property to tween was given).
        /// </summary>
        /// <param name="p_target">
        /// The tweening target (must be the object containing the properties or fields to tween).
        /// </param>
        /// <param name="p_duration">
        /// The duration in seconds of the tween.
        /// </param>
        /// <param name="p_propName">
        /// The name of the property or field to tween.
        /// </param>
        /// <param name="p_fromVal">
        /// The end value the property should reach with the tween.
        /// </param>
        /// <param name="p_isRelative">
        /// If <c>true</c> treats the end value as relative (tween BY instead than tween TO), otherwise as absolute.
        /// </param>
        /// <param name="p_easeType">
        /// The ease to use.
        /// </param>
        /// <param name="p_delay">
        /// The eventual delay to apply.
        /// </param>
        /// <returns>
        /// The newly created <see cref="Tweener"/>,
        /// or <c>null</c> if the parameters were invalid.
        /// </returns>
        public static Tweener From(object p_target, float p_duration, string p_propName, object p_fromVal, bool p_isRelative, EaseType p_easeType, float p_delay)
        {
            return From(p_target, p_duration, new TweenParms().Prop(p_propName, p_fromVal, p_isRelative).Delay(p_delay).Ease(p_easeType));
        }
        /// <summary>
        /// Creates a new FROM tween and returns the <see cref="Tweener"/> representing it,
        /// or <c>null</c> if the tween was invalid (no valid property to tween was given).
        /// </summary>
        /// <param name="p_target">
        /// The tweening target (must be the object containing the properties or fields to tween).
        /// </param>
        /// <param name="p_duration">
        /// The duration in seconds of the tween.
        /// </param>
        /// <param name="p_parms">
        /// A <see cref="TweenParms"/> representing the tween parameters.
        /// You can pass an existing one, or create a new one inline via method chaining,
        /// like <c>new TweenParms().Prop("x",10).Loops(2).OnComplete(myFunction)</c>
        /// </param>
        /// <returns>
        /// The newly created <see cref="Tweener"/>,
        /// or <c>null</c> if the parameters were invalid.
        /// </returns>
        public static Tweener From(object p_target, float p_duration, TweenParms p_parms)
        {
            if (!initialized) Init();

            p_parms = p_parms.IsFrom();
            Tweener tw = new Tweener(p_target, p_duration, p_parms);

            // Check if tween is valid.
            if (tw.isEmpty) {
                return null;
            }

            AddTween(tw);
            // Immediately jump to position 0 to avoid flickering of objects before they're punched to FROM position.
            // p_isStartupIteration is set to FALSE to ignore callbacks.
            if (!tw._isPaused) {
                tw.Update(0, true, true, false, true);
            }
            return tw;
        }

        /// <summary>
        /// Creates a new absolute PUNCH tween, and returns the <see cref="Tweener"/> representing it,
        /// or <c>null</c> if the tween was invalid (no valid property to tween was given).
        /// </summary>
        /// <param name="p_target">
        /// The tweening target (must be the object containing the properties or fields to tween).
        /// </param>
        /// <param name="p_duration">The duration in seconds of the tween.</param>
        /// <param name="p_propName">The name of the property or field to tween.</param>
        /// <param name="p_fromVal">The end value the property should reach with the tween.</param>
        /// <param name="p_punchAmplitude">Default: 0.5f - amplitude of the punch effect</param>
        /// <param name="p_punchPeriod">Default: 0.1f - oscillation period of punch effect</param>
        /// <returns>
        /// The newly created <see cref="Tweener"/>,
        /// or <c>null</c> if the parameters were invalid.
        /// </returns>
        public static Tweener Punch(object p_target, float p_duration, string p_propName, object p_fromVal, float p_punchAmplitude = 0.5f, float p_punchPeriod = 0.1f)
        {
            TweenParms parms = new TweenParms()
                .Prop(p_propName, p_fromVal)
                .Ease(EaseType.EaseOutElastic, p_punchAmplitude, p_punchPeriod);
            return To(p_target, p_duration, parms);
        }

        /// <summary>
        /// Creates a new PUNCH tween, and returns the <see cref="Tweener"/> representing it,
        /// or <c>null</c> if the tween was invalid (no valid property to tween was given).
        /// </summary>
        /// <param name="p_target">
        /// The tweening target (must be the object containing the properties or fields to tween).
        /// </param>
        /// <param name="p_duration">The duration in seconds of the tween.</param>
        /// <param name="p_propName">The name of the property or field to tween.</param>
        /// <param name="p_fromVal">The end value the property should reach with the tween.</param>
        /// <param name="p_isRelative">
        /// If <c>true</c> treats the end value as relative (tween BY instead than tween TO), otherwise as absolute.
        /// </param>
        /// <param name="p_punchAmplitude">Default: 0.5f - amplitude of the punch effect</param>
        /// <param name="p_punchPeriod">Default: 0.1f - oscillation period of punch effect</param>
        /// <returns>
        /// The newly created <see cref="Tweener"/>,
        /// or <c>null</c> if the parameters were invalid.
        /// </returns>
        public static Tweener Punch(object p_target, float p_duration, string p_propName, object p_fromVal, bool p_isRelative, float p_punchAmplitude = 0.5f, float p_punchPeriod = 0.1f)
        {
            TweenParms parms = new TweenParms()
                .Prop(p_propName, p_fromVal, p_isRelative)
                .Ease(EaseType.EaseOutElastic, p_punchAmplitude, p_punchPeriod);
            return To(p_target, p_duration, parms);
        }

        /// <summary>
        /// Creates a new PUNCH tween and returns the <see cref="Tweener"/> representing it,
        /// or <c>null</c> if the tween was invalid (no valid property to tween was given).
        /// Any ease type passed won't be considered, since punch uses its own one.
        /// </summary>
        /// <param name="p_target">
        /// The tweening target (must be the object containing the properties or fields to tween).
        /// </param>
        /// <param name="p_duration">The duration in seconds of the tween.</param>
        /// <param name="p_parms">
        /// A <see cref="TweenParms"/> representing the tween parameters.
        /// You can pass an existing one, or create a new one inline via method chaining,
        /// like <c>new TweenParms().Prop("x",10).Loops(2).OnComplete(myFunction)</c>
        /// </param>
        /// <param name="p_punchAmplitude">Default: 0.5f - amplitude of the punch effect</param>
        /// <param name="p_punchPeriod">Default: 0.1f - oscillation period of punch effect</param>
        /// <returns>
        /// The newly created <see cref="Tweener"/>,
        /// or <c>null</c> if the parameters were invalid.
        /// </returns>
        public static Tweener Punch(object p_target, float p_duration, TweenParms p_parms, float p_punchAmplitude = 0.5f, float p_punchPeriod = 0.1f)
        {
            if (!initialized) Init();

            p_parms.Ease(EaseType.EaseOutElastic, p_punchAmplitude, p_punchPeriod);
            Tweener tw = new Tweener(p_target, p_duration, p_parms);

            // Check if tween is valid.
            if (tw.isEmpty) return null;

            AddTween(tw);
            return tw;
        }

        /// <summary>
        /// Creates a new absolute SHAKE tween, and returns the <see cref="Tweener"/> representing it,
        /// or <c>null</c> if the tween was invalid (no valid property to tween was given).
        /// </summary>
        /// <param name="p_target">
        /// The tweening target (must be the object containing the properties or fields to tween).
        /// </param>
        /// <param name="p_duration">The duration in seconds of the tween.</param>
        /// <param name="p_propName">The name of the property or field to tween.</param>
        /// <param name="p_fromVal">The amount of shaking to apply.</param>
        /// <param name="p_shakeAmplitude">Default: 0.1f - amplitude of the shake effect</param>
        /// <param name="p_shakePeriod">Default: 0.12f - oscillation period of shake effect</param>
        /// <returns>
        /// The newly created <see cref="Tweener"/>,
        /// or <c>null</c> if the parameters were invalid.
        /// </returns>
        public static Tweener Shake(object p_target, float p_duration, string p_propName, object p_fromVal, float p_shakeAmplitude = 0.1f, float p_shakePeriod = 0.12f)
        {
            TweenParms parms = new TweenParms()
                .Prop(p_propName, p_fromVal)
                .Ease(EaseType.EaseOutElastic, p_shakeAmplitude, p_shakePeriod);
            return From(p_target, p_duration, parms);
        }

        /// <summary>
        /// Creates a new SHAKE tween, and returns the <see cref="Tweener"/> representing it,
        /// or <c>null</c> if the tween was invalid (no valid property to tween was given).
        /// </summary>
        /// <param name="p_target">
        /// The tweening target (must be the object containing the properties or fields to tween).
        /// </param>
        /// <param name="p_duration">The duration in seconds of the tween.</param>
        /// <param name="p_propName">The name of the property or field to tween.</param>
        /// <param name="p_fromVal">The amount of shaking to apply.</param>
        /// <param name="p_isRelative">
        /// If <c>true</c> treats the end value as relative (tween BY instead than tween TO), otherwise as absolute.
        /// </param>
        /// <param name="p_shakeAmplitude">Default: 0.1f - amplitude of the shake effect</param>
        /// <param name="p_shakePeriod">Default: 0.12f - oscillation period of shake effect</param>
        /// <returns>
        /// The newly created <see cref="Tweener"/>,
        /// or <c>null</c> if the parameters were invalid.
        /// </returns>
        public static Tweener Shake(object p_target, float p_duration, string p_propName, object p_fromVal, bool p_isRelative, float p_shakeAmplitude = 0.1f, float p_shakePeriod = 0.12f)
        {
            TweenParms parms = new TweenParms()
                .Prop(p_propName, p_fromVal, p_isRelative)
                .Ease(EaseType.EaseOutElastic, p_shakeAmplitude, p_shakePeriod);
            return From(p_target, p_duration, parms);
        }

        /// <summary>
        /// Creates a new SHAKE tween and returns the <see cref="Tweener"/> representing it,
        /// or <c>null</c> if the tween was invalid (no valid property to tween was given).
        /// Any ease type passed won't be considered, since shake uses its own one.
        /// </summary>
        /// <param name="p_target">
        /// The tweening target (must be the object containing the properties or fields to tween).
        /// </param>
        /// <param name="p_duration">The duration in seconds of the tween.</param>
        /// <param name="p_parms">
        /// A <see cref="TweenParms"/> representing the tween parameters.
        /// You can pass an existing one, or create a new one inline via method chaining,
        /// like <c>new TweenParms().Prop("x",10).Loops(2).OnComplete(myFunction)</c>
        /// </param>
        /// <param name="p_shakeAmplitude">Default: 0.1f - amplitude of the shake effect</param>
        /// <param name="p_shakePeriod">Default: 0.12f - oscillation period of shake effect</param>
        /// <returns>
        /// The newly created <see cref="Tweener"/>,
        /// or <c>null</c> if the parameters were invalid.
        /// </returns>
        public static Tweener Shake(object p_target, float p_duration, TweenParms p_parms, float p_shakeAmplitude = 0.1f, float p_shakePeriod = 0.12f)
        {
            if (!initialized) Init();

            p_parms
                .Ease(EaseType.EaseOutElastic, p_shakeAmplitude, p_shakePeriod)
                .IsFrom();
            Tweener tw = new Tweener(p_target, p_duration, p_parms);

            // Check if tween is valid.
            if (tw.isEmpty) return null;

            AddTween(tw);
            return tw;
        }

        // ===================================================================================
        // UPDATE METHODS --------------------------------------------------------------------

        /// <summary>
        /// Updates normal tweens.
        /// </summary>
        void Update()
        {
            if (tweens == null) return;

            // Update tweens.
            DoUpdate(UpdateType.Update, Time.deltaTime);

            CheckClear();
        }

        /// <summary>
        /// Updates lateUpdate tweens.
        /// </summary>
        void LateUpdate()
        {
            if (tweens == null) return;

            // Update tweens.
            DoUpdate(UpdateType.LateUpdate, Time.deltaTime);

            CheckClear();
        }

        /// <summary>
        /// Updates fixedUpdate tweens.
        /// </summary>
        void FixedUpdate()
        {
            if (tweens == null) return;

            // Update tweens.
            DoUpdate(UpdateType.FixedUpdate, Time.fixedDeltaTime);

            CheckClear();
        }

        /// <summary>
        /// Updates timeScaleIndependent tweens.
        /// </summary>
        static IEnumerator TimeScaleIndependentUpdate()
        {
            while (tweens != null) {
                if (time > Time.realtimeSinceStartup) time = Time.realtimeSinceStartup;
                float elapsed = Time.realtimeSinceStartup - time;
                time = Time.realtimeSinceStartup;

                // Update tweens.
                DoUpdate(UpdateType.TimeScaleIndependentUpdate, elapsed);

                if (CheckClear()) {
                    yield break;
                }

                yield return null;
            }
        }

        // ===================================================================================
        // METHODS ---------------------------------------------------------------------------

        /// <summary>
        /// Enables the overwrite manager (disabled by default).
        /// </summary>
        /// <param name="logWarnings">If TRUE, the overwriteManager will log a warning each time a tween is overwritten</param>
        public static void EnableOverwriteManager(bool logWarnings=true)
        {
            if (overwriteManager != null) {
                overwriteManager.enabled = true;
                overwriteManager.logWarnings = logWarnings;
            }
        }

        /// <summary>
        /// Disables the overwrite manager (disabled by default).
        /// </summary>
        public static void DisableOverwriteManager()
        {
            if (overwriteManager != null) {
                overwriteManager.enabled = false;
            }
        }

        /// <summary>
        /// Pauses all the tweens for the given target, and returns the total number of paused Tweeners.
        /// </summary>
        /// <param name="p_target">
        /// The target whose tweens to pause.
        /// </param>
        /// <returns>
        /// The total number of paused Tweeners.
        /// </returns>
        public static int Pause(object p_target)
        {
            return DoFilteredIteration(p_target, DoFilteredPause, false);
        }

        /// <summary>
        /// Pauses all the Tweeners/Sequences with the given ID, and returns the total number of paused Tweeners/Sequences.
        /// </summary>
        /// <param name="p_id">
        /// The ID of the Tweeners/Sequences to pause.
        /// </param>
        /// <returns>
        /// The total number of paused Tweeners/Sequences.
        /// </returns>
        public static int Pause(string p_id)
        {
            return DoFilteredIteration(p_id, DoFilteredPause, false);
        }

        /// <summary>
        /// Pauses all the Tweeners/Sequences with the given intId, and returns the total number of paused Tweeners/Sequences.
        /// </summary>
        /// <param name="p_intId">
        /// The intId of the Tweeners/Sequences to pause.
        /// </param>
        /// <returns>
        /// The total number of paused Tweeners/Sequences.
        /// </returns>
        public static int Pause(int p_intId)
        {
            return DoFilteredIteration(p_intId, DoFilteredPause, false);
        }

        /// <summary>
        /// Pauses the given Tweener, and returns the total number of paused ones (1 if the Tweener existed, otherwise 0).
        /// </summary>
        /// <param name="p_tweener">
        /// The Tweener to pause.
        /// </param>
        /// <returns>
        /// The total number of paused Tweener (1 if the Tweener existed, otherwise 0).
        /// </returns>
        public static int Pause(Tweener p_tweener)
        {
            return DoFilteredIteration(p_tweener, DoFilteredPause, false);
        }

        /// <summary>
        /// Pauses the given Sequence, and returns the total number of paused ones (1 if the Sequence existed, otherwise 0).
        /// </summary>
        /// <param name="p_sequence">
        /// The Sequence to pause.
        /// </param>
        /// <returns>
        /// The total number of paused Sequence (1 if the sequence existed, otherwise 0).
        /// </returns>
        public static int Pause(Sequence p_sequence)
        {
            return DoFilteredIteration(p_sequence, DoFilteredPause, false);
        }

        /// <summary>
        /// Pauses all Tweeners/Sequences, and returns the total number of paused Tweeners/Sequences.
        /// </summary>
        /// <returns>
        /// The total number of paused Tweeners/Sequences.
        /// </returns>
        public static int Pause()
        {
            return DoFilteredIteration(null, DoFilteredPause, false);
        }

        /// <summary>
        /// Resumes all the tweens (delays included) for the given target, and returns the total number of resumed Tweeners.
        /// </summary>
        /// <param name="p_target">
        /// The target whose tweens to resume.
        /// </param>
        /// <returns>
        /// The total number of resumed Tweeners.
        /// </returns>
        public static int Play(object p_target)
        {
            return Play(p_target, false);
        }

        /// <summary>
        /// Resumes all the tweens for the given target, and returns the total number of resumed Tweeners.
        /// </summary>
        /// <param name="p_target">
        /// The target whose tweens to resume.
        /// </param>
        /// <param name="p_skipDelay">
        /// If <c>true</c> skips any initial delay.
        /// </param>
        /// <returns>
        /// The total number of resumed Tweeners.
        /// </returns>
        public static int Play(object p_target, bool p_skipDelay)
        {
            return DoFilteredIteration(p_target, DoFilteredPlay, false, p_skipDelay);
        }

        /// <summary>
        /// Resumes all the Tweeners (delays included) and Sequences with the given ID, and returns the total number of resumed Tweeners/Sequences.
        /// </summary>
        /// <param name="p_id">
        /// The ID of the Tweeners/Sequences to resume.
        /// </param>
        /// <returns>
        /// The total number of resumed Tweeners/Sequences.
        /// </returns>
        public static int Play(string p_id)
        {
            return Play(p_id, false);
        }

        /// <summary>
        /// Resumes all the Tweeners/Sequences with the given ID, and returns the total number of resumed Tweeners/Sequences.
        /// </summary>
        /// <param name="p_id">
        /// The ID of the Tweeners/Sequences to resume.
        /// </param>
        /// <param name="p_skipDelay">
        /// If <c>true</c> skips any initial tween delay.
        /// </param>
        /// <returns>
        /// The total number of resumed Tweeners/Sequences.
        /// </returns>
        public static int Play(string p_id, bool p_skipDelay)
        {
            return DoFilteredIteration(p_id, DoFilteredPlay, false, p_skipDelay);
        }

        /// <summary>
        /// Resumes all the Tweeners (delays included) and Sequences with the given intId, and returns the total number of resumed Tweeners/Sequences.
        /// </summary>
        /// <param name="p_intId">
        /// The intId of the Tweeners/Sequences to resume.
        /// </param>
        /// <returns>
        /// The total number of resumed Tweeners/Sequences.
        /// </returns>
        public static int Play(int p_intId)
        {
            return Play(p_intId, false);
        }

        /// <summary>
        /// Resumes all the Tweeners/Sequences with the given intId, and returns the total number of resumed Tweeners/Sequences.
        /// </summary>
        /// <param name="p_intId">
        /// The intId of the Tweeners/Sequences to resume.
        /// </param>
        /// <param name="p_skipDelay">
        /// If <c>true</c> skips any initial tween delay.
        /// </param>
        /// <returns>
        /// The total number of resumed Tweeners/Sequences.
        /// </returns>
        public static int Play(int p_intId, bool p_skipDelay)
        {
            return DoFilteredIteration(p_intId, DoFilteredPlay, false, p_skipDelay);
        }

        /// <summary>
        /// Resumes the given Tweener (delays included), and returns the total number of resumed ones (1 if the Tweener existed, otherwise 0).
        /// </summary>
        /// <param name="p_tweener">
        /// The Tweener to resume.
        /// </param>
        /// <returns>
        /// The total number of resumed Tweeners (1 if the Tweener existed, otherwise 0).
        /// </returns>
        public static int Play(Tweener p_tweener)
        {
            return Play(p_tweener, false);
        }

        /// <summary>
        /// Resumes the given Tweener, and returns the total number of resumed ones (1 if the Tweener existed, otherwise 0).
        /// </summary>
        /// <param name="p_tweener">
        /// The Tweener to resume.
        /// </param>
        /// <param name="p_skipDelay">
        /// If <c>true</c> skips any initial delay.
        /// </param>
        /// <returns>
        /// The total number of resumed Tweeners (1 if the Tweener existed, otherwise 0).
        /// </returns>
        public static int Play(Tweener p_tweener, bool p_skipDelay)
        {
            return DoFilteredIteration(p_tweener, DoFilteredPlay, false, p_skipDelay);
        }

        /// <summary>
        /// Resumes the given Sequence, and returns the total number of resumed ones (1 if the Sequence existed, otherwise 0).
        /// </summary>
        /// <param name="p_sequence">
        /// The Sequence to resume.
        /// </param>
        /// <returns>
        /// The total number of resumed Sequences (1 if the Sequence existed, otherwise 0).
        /// </returns>
        public static int Play(Sequence p_sequence)
        {
            return DoFilteredIteration(p_sequence, DoFilteredPlay, false);
        }

        /// <summary>
        /// Resumes all Tweeners (delays included) and Sequences, and returns the total number of resumed Tweeners/Sequences.
        /// </summary>
        /// <returns>
        /// The total number of resumed Tweeners/Sequences.
        /// </returns>
        public static int Play()
        {
            return Play(false);
        }

        /// <summary>
        /// Resumes all Tweeners/Sequences, and returns the total number of resumed Tweeners/Sequences.
        /// </summary>
        /// <param name="p_skipDelay">
        /// If <c>true</c> skips any initial tween delay.
        /// </param>
        /// <returns>
        /// The total number of resumed Tweeners/Sequences.
        /// </returns>
        public static int Play(bool p_skipDelay)
        {
            return DoFilteredIteration(null, DoFilteredPlay, false, p_skipDelay);
        }

        /// <summary>
        /// Resumes all the tweens (delays included) for the given target,
        /// sets the tweens so that they move forward and not backwards,
        /// and returns the total number of resumed Tweeners.
        /// </summary>
        /// <param name="p_target">
        /// The target whose tweens to resume.
        /// </param>
        /// <returns>
        /// The total number of resumed Tweeners.
        /// </returns>
        public static int PlayForward(object p_target)
        {
            return PlayForward(p_target, false);
        }

        /// <summary>
        /// Resumes all the tweens for the given target,
        /// sets the tweens so that they move forward and not backwards,
        /// and returns the total number of resumed Tweeners.
        /// </summary>
        /// <param name="p_target">
        /// The target whose tweens to resume.
        /// </param>
        /// <param name="p_skipDelay">
        /// If <c>true</c> skips any initial delay.
        /// </param>
        /// <returns>
        /// The total number of resumed Tweeners.
        /// </returns>
        public static int PlayForward(object p_target, bool p_skipDelay)
        {
            return DoFilteredIteration(p_target, DoFilteredPlayForward, false, p_skipDelay);
        }

        /// <summary>
        /// Resumes all the Tweeners (delays included) and Sequences with the given ID,
        /// sets the tweens so that they move forward and not backwards,
        /// and returns the total number of resumed Tweeners/Sequences.
        /// </summary>
        /// <param name="p_id">
        /// The ID of the Tweeners/Sequences to resume.
        /// </param>
        /// <returns>
        /// The total number of resumed Tweeners/Sequences.
        /// </returns>
        public static int PlayForward(string p_id)
        {
            return PlayForward(p_id, false);
        }

        /// <summary>
        /// Resumes all the Tweeners/Sequences with the given ID,
        /// sets the tweens so that they move forward and not backwards,
        /// and returns the total number of resumed Tweeners/Sequences.
        /// </summary>
        /// <param name="p_id">
        /// The ID of the Tweeners/Sequences to resume.
        /// </param>
        /// <param name="p_skipDelay">
        /// If <c>true</c> skips any initial tween delay.
        /// </param>
        /// <returns>
        /// The total number of resumed Tweeners/Sequences.
        /// </returns>
        public static int PlayForward(string p_id, bool p_skipDelay)
        {
            return DoFilteredIteration(p_id, DoFilteredPlayForward, false, p_skipDelay);
        }

        /// <summary>
        /// Resumes all the Tweeners (delays included) and Sequences with the given intId,
        /// sets the tweens so that they move forward and not backwards,
        /// and returns the total number of resumed Tweeners/Sequences.
        /// </summary>
        /// <param name="p_intId">
        /// The intId of the Tweeners/Sequences to resume.
        /// </param>
        /// <returns>
        /// The total number of resumed Tweeners/Sequences.
        /// </returns>
        public static int PlayForward(int p_intId)
        {
            return PlayForward(p_intId, false);
        }

        /// <summary>
        /// Resumes all the Tweeners/Sequences with the given intId,
        /// sets the tweens so that they move forward and not backwards,
        /// and returns the total number of resumed Tweeners/Sequences.
        /// </summary>
        /// <param name="p_intId">
        /// The intId of the Tweeners/Sequences to resume.
        /// </param>
        /// <param name="p_skipDelay">
        /// If <c>true</c> skips any initial tween delay.
        /// </param>
        /// <returns>
        /// The total number of resumed Tweeners/Sequences.
        /// </returns>
        public static int PlayForward(int p_intId, bool p_skipDelay)
        {
            return DoFilteredIteration(p_intId, DoFilteredPlayForward, false, p_skipDelay);
        }

        /// <summary>
        /// Resumes the given Tweener (delays included),
        /// sets it so that it moves forward and not backwards,
        /// and returns the total number of resumed ones (1 if the Tweener existed, otherwise 0).
        /// </summary>
        /// <param name="p_tweener">
        /// The Tweener to resume.
        /// </param>
        /// <returns>
        /// The total number of resumed Tweeners (1 if the Tweener existed, otherwise 0).
        /// </returns>
        public static int PlayForward(Tweener p_tweener)
        {
            return PlayForward(p_tweener, false);
        }

        /// <summary>
        /// Resumes the given Tweener,
        /// sets it so that it moves forward and not backwards,
        /// and returns the total number of resumed ones (1 if the Tweener existed, otherwise 0).
        /// </summary>
        /// <param name="p_tweener">
        /// The Tweener to resume.
        /// </param>
        /// <param name="p_skipDelay">
        /// If <c>true</c> skips any initial delay.
        /// </param>
        /// <returns>
        /// The total number of resumed Tweeners (1 if the Tweener existed, otherwise 0).
        /// </returns>
        public static int PlayForward(Tweener p_tweener, bool p_skipDelay)
        {
            return DoFilteredIteration(p_tweener, DoFilteredPlayForward, false, p_skipDelay);
        }

        /// <summary>
        /// Resumes the given Sequence,
        /// sets it so that it moves forward and not backwards,
        /// and returns the total number of resumed ones (1 if the Sequence existed, otherwise 0).
        /// </summary>
        /// <param name="p_sequence">
        /// The Sequence to resume.
        /// </param>
        /// <returns>
        /// The total number of resumed Sequences (1 if the Sequence existed, otherwise 0).
        /// </returns>
        public static int PlayForward(Sequence p_sequence)
        {
            return DoFilteredIteration(p_sequence, DoFilteredPlayForward, false);
        }

        /// <summary>
        /// Resumes all Tweeners (delays included) and Sequences,
        /// sets the tweens so that they move forward and not backwards,
        /// and returns the total number of resumed Tweeners/Sequences.
        /// </summary>
        /// <returns>
        /// The total number of resumed Tweeners/Sequences.
        /// </returns>
        public static int PlayForward()
        {
            return PlayForward(false);
        }

        /// <summary>
        /// Resumes all Tweeners/Sequences,
        /// sets the tweens so that they move forward and not backwards,
        /// and returns the total number of resumed Tweeners/Sequences.
        /// </summary>
        /// <param name="p_skipDelay">
        /// If <c>true</c> skips any initial tween delay.
        /// </param>
        /// <returns>
        /// The total number of resumed Tweeners/Sequences.
        /// </returns>
        public static int PlayForward(bool p_skipDelay)
        {
            return DoFilteredIteration(null, DoFilteredPlayForward, false, p_skipDelay);
        }

        /// <summary>
        /// Resumes all the tweens for the given target,
        /// sets the tweens so that they move backwards instead than forward,
        /// and returns the total number of resumed Tweeners.
        /// </summary>
        /// <param name="p_target">
        /// The target whose tweens to resume.
        /// </param>
        /// <returns>
        /// The total number of resumed Tweeners.
        /// </returns>
        public static int PlayBackwards(object p_target)
        {
            return DoFilteredIteration(p_target, DoFilteredPlayBackwards, false);
        }

        /// <summary>
        /// Resumes all the Tweeners/Sequences with the given ID,
        /// sets the tweens so that they move backwards instead than forward,
        /// and returns the total number of resumed Tweeners/Sequences.
        /// </summary>
        /// <param name="p_id">
        /// The ID of the Tweeners/Sequences to resume.
        /// </param>
        /// <returns>
        /// The total number of resumed Tweeners/Sequences.
        /// </returns>
        public static int PlayBackwards(string p_id)
        {
            return DoFilteredIteration(p_id, DoFilteredPlayBackwards, false);
        }

        /// <summary>
        /// Resumes all the Tweeners/Sequences with the given intId,
        /// sets the tweens so that they move backwards instead than forward,
        /// and returns the total number of resumed Tweeners/Sequences.
        /// </summary>
        /// <param name="p_intId">
        /// The intId of the Tweeners/Sequences to resume.
        /// </param>
        /// <returns>
        /// The total number of resumed Tweeners/Sequences.
        /// </returns>
        public static int PlayBackwards(int p_intId)
        {
            return DoFilteredIteration(p_intId, DoFilteredPlayBackwards, false);
        }

        /// <summary>
        /// Resumes the given Tweener,
        /// sets it so that it moves backwards instead than forward,
        /// and returns the total number of resumed ones (1 if the Tweener existed, otherwise 0).
        /// </summary>
        /// <param name="p_tweener">
        /// The Tweener to resume.
        /// </param>
        /// <returns>
        /// The total number of resumed Tweeners (1 if the Tweener existed, otherwise 0).
        /// </returns>
        public static int PlayBackwards(Tweener p_tweener)
        {
            return DoFilteredIteration(p_tweener, DoFilteredPlayBackwards, false);
        }

        /// <summary>
        /// Resumes the given Sequence,
        /// sets it so that it moves backwards instead than forward,
        /// and returns the total number of resumed ones (1 if the Sequence existed, otherwise 0).
        /// </summary>
        /// <param name="p_sequence">
        /// The Sequence to resume.
        /// </param>
        /// <returns>
        /// The total number of resumed Sequences (1 if the Sequence existed, otherwise 0).
        /// </returns>
        public static int PlayBackwards(Sequence p_sequence)
        {
            return DoFilteredIteration(p_sequence, DoFilteredPlayBackwards, false);
        }

        /// <summary>
        /// Resumes all Tweeners/Sequences,
        /// sets the tweens so that they move backwards instead than forward,
        /// and returns the total number of resumed Tweeners/Sequences.
        /// </summary>
        /// <returns>
        /// The total number of resumed Tweeners/Sequences.
        /// </returns>
        public static int PlayBackwards()
        {
            return DoFilteredIteration(null, DoFilteredPlayBackwards, false);
        }

        /// <summary>
        /// Rewinds all the tweens (delays included) for the given target, and returns the total number of rewinded Tweeners.
        /// </summary>
        /// <param name="p_target">
        /// The target whose tweens to rewind.
        /// </param>
        /// <returns>
        /// The total number of rewinded Tweeners.
        /// </returns>
        public static int Rewind(object p_target)
        {
            return Rewind(p_target, false);
        }

        /// <summary>
        /// Rewinds all the tweens for the given target, and returns the total number of rewinded Tweeners.
        /// </summary>
        /// <param name="p_target">
        /// The target whose tweens to rewind.
        /// </param>
        /// <param name="p_skipDelay">
        /// If <c>true</c> skips any initial delay.
        /// </param>
        /// <returns>
        /// The total number of rewinded Tweeners.
        /// </returns>
        public static int Rewind(object p_target, bool p_skipDelay)
        {
            return DoFilteredIteration(p_target, DoFilteredRewind, false, p_skipDelay);
        }

        /// <summary>
        /// Rewinds all the Tweeners (delays included) and Sequences with the given ID, and returns the total number of rewinded Tweeners/Sequences.
        /// </summary>
        /// <param name="p_id">
        /// The ID of the Tweeners/Sequences to rewind.
        /// </param>
        /// <returns>
        /// The total number of rewinded Tweeners/Sequences.
        /// </returns>
        public static int Rewind(string p_id)
        {
            return Rewind(p_id, false);
        }

        /// <summary>
        /// Rewinds all the Tweeners/Sequences with the given ID, and returns the total number of rewinded Tweeners/Sequences.
        /// </summary>
        /// <param name="p_id">
        /// The ID of the Tweeners/Sequences to rewind.
        /// </param>
        /// <param name="p_skipDelay">
        /// If <c>true</c> skips any initial tween delay.
        /// </param>
        /// <returns>
        /// The total number of rewinded Tweeners/Sequences.
        /// </returns>
        public static int Rewind(string p_id, bool p_skipDelay)
        {
            return DoFilteredIteration(p_id, DoFilteredRewind, false, p_skipDelay);
        }

        /// <summary>
        /// Rewinds all the Tweeners (delays included) and Sequences with the given intId, and returns the total number of rewinded Tweeners/Sequences.
        /// </summary>
        /// <param name="p_intId">
        /// The intId of the Tweeners/Sequences to rewind.
        /// </param>
        /// <returns>
        /// The total number of rewinded Tweeners/Sequences.
        /// </returns>
        public static int Rewind(int p_intId)
        {
            return Rewind(p_intId, false);
        }

        /// <summary>
        /// Rewinds all the Tweeners/Sequences with the given intId, and returns the total number of rewinded Tweeners/Sequences.
        /// </summary>
        /// <param name="p_intId">
        /// The intId of the Tweeners/Sequences to rewind.
        /// </param>
        /// <param name="p_skipDelay">
        /// If <c>true</c> skips any initial tween delay.
        /// </param>
        /// <returns>
        /// The total number of rewinded Tweeners/Sequences.
        /// </returns>
        public static int Rewind(int p_intId, bool p_skipDelay)
        {
            return DoFilteredIteration(p_intId, DoFilteredRewind, false, p_skipDelay);
        }

        /// <summary>
        /// Rewinds the given Tweener (delays included), and returns the total number of rewinded ones (1 if the Tweener existed, otherwise 0).
        /// </summary>
        /// <param name="p_tweener">
        /// The Tweener to rewind.
        /// </param>
        /// <returns>
        /// The total number of rewinded Tweeners (1 if the Tweener existed, otherwise 0).
        /// </returns>
        public static int Rewind(Tweener p_tweener)
        {
            return Rewind(p_tweener, false);
        }

        /// <summary>
        /// Rewinds the given Tweener, and returns the total number of rewinded ones (1 if the Tweener existed, otherwise 0).
        /// </summary>
        /// <param name="p_tweener">
        /// The Tweener to rewind.
        /// </param>
        /// <param name="p_skipDelay">
        /// If <c>true</c> skips any initial delay.
        /// </param>
        /// <returns>
        /// The total number of rewinded Tweeners (1 if the Tweener existed, otherwise 0).
        /// </returns>
        public static int Rewind(Tweener p_tweener, bool p_skipDelay)
        {
            return DoFilteredIteration(p_tweener, DoFilteredRewind, false, p_skipDelay);
        }

        /// <summary>
        /// Rewinds the given Sequence, and returns the total number of rewinded ones (1 if the Sequence existed, otherwise 0).
        /// </summary>
        /// <param name="p_sequence">
        /// The Sequence to rewind.
        /// </param>
        /// <returns>
        /// The total number of rewinded Sequences (1 if the Sequence existed, otherwise 0).
        /// </returns>
        public static int Rewind(Sequence p_sequence)
        {
            return DoFilteredIteration(p_sequence, DoFilteredRewind, false);
        }

        /// <summary>
        /// Rewinds all Tweeners (delay included) and Sequences, and returns the total number of rewinded Tweeners/Sequences.
        /// </summary>
        /// <returns>
        /// The total number of rewinded Tweeners/Sequences.
        /// </returns>
        public static int Rewind()
        {
            return Rewind(false);
        }

        /// <summary>
        /// Rewinds all Tweeners/Sequences, and returns the total number of rewinded Tweeners/Sequences.
        /// </summary>
        /// <param name="p_skipDelay">
        /// If <c>true</c> skips any initial tween delay.
        /// </param>
        /// <returns>
        /// The total number of rewinded Tweeners/Sequences.
        /// </returns>
        public static int Rewind(bool p_skipDelay)
        {
            return DoFilteredIteration(null, DoFilteredRewind, false, p_skipDelay);
        }

        /// <summary>
        /// Restarts all the tweens (delays included) for the given target, and returns the total number of restarted Tweeners.
        /// </summary>
        /// <param name="p_target">
        /// The target whose tweens to restart.
        /// </param>
        /// <returns>
        /// The total number of restarted Tweeners.
        /// </returns>
        public static int Restart(object p_target)
        {
            return Restart(p_target, false);
        }

        /// <summary>
        /// Restarts all the tweens for the given target, and returns the total number of restarted Tweeners.
        /// </summary>
        /// <param name="p_target">
        /// The target whose tweens to restart.
        /// </param>
        /// <param name="p_skipDelay">
        /// If <c>true</c> skips any initial delay.
        /// </param>
        /// <returns>
        /// The total number of restarted Tweeners.
        /// </returns>
        public static int Restart(object p_target, bool p_skipDelay)
        {
            return DoFilteredIteration(p_target, DoFilteredRestart, false, p_skipDelay);
        }

        /// <summary>
        /// Restarts all the Tweeners (delays included) and Sequences with the given ID, and returns the total number of restarted Tweeners/Sequences.
        /// </summary>
        /// <param name="p_id">
        /// The ID of the Tweeners/Sequences to restart.
        /// </param>
        /// <returns>
        /// The total number of restarted Tweeners/Sequences.
        /// </returns>
        public static int Restart(string p_id)
        {
            return Restart(p_id, false);
        }

        /// <summary>
        /// Restarts all the Tweeners/Sequences with the given ID, and returns the total number of restarted Tweeners/Sequences.
        /// </summary>
        /// <param name="p_id">
        /// The ID of the Tweeners/Sequences to restart.
        /// </param>
        /// <param name="p_skipDelay">
        /// If <c>true</c> skips any initial tween delay.
        /// </param>
        /// <returns>
        /// The total number of restarted Tweeners/Sequences.
        /// </returns>
        public static int Restart(string p_id, bool p_skipDelay)
        {
            return DoFilteredIteration(p_id, DoFilteredRestart, false, p_skipDelay);
        }

        /// <summary>
        /// Restarts all the Tweeners (delays included) and Sequences with the given intId, and returns the total number of restarted Tweeners/Sequences.
        /// </summary>
        /// <param name="p_intId">
        /// The intId of the Tweeners/Sequences to restart.
        /// </param>
        /// <returns>
        /// The total number of restarted Tweeners/Sequences.
        /// </returns>
        public static int Restart(int p_intId)
        {
            return Restart(p_intId, false);
        }

        /// <summary>
        /// Restarts all the Tweeners/Sequences with the given intId, and returns the total number of restarted Tweeners/Sequences.
        /// </summary>
        /// <param name="p_intId">
        /// The intId of the Tweeners/Sequences to restart.
        /// </param>
        /// <param name="p_skipDelay">
        /// If <c>true</c> skips any initial tween delay.
        /// </param>
        /// <returns>
        /// The total number of restarted Tweeners/Sequences.
        /// </returns>
        public static int Restart(int p_intId, bool p_skipDelay)
        {
            return DoFilteredIteration(p_intId, DoFilteredRestart, false, p_skipDelay);
        }

        /// <summary>
        /// Restarts the given Tweener (delays included), and returns the total number of restarted ones (1 if the Tweener existed, otherwise 0).
        /// </summary>
        /// <param name="p_tweener">
        /// The Tweener to restart.
        /// </param>
        /// <returns>
        /// The total number of restarted Tweeners (1 if the Tweener existed, otherwise 0).
        /// </returns>
        public static int Restart(Tweener p_tweener)
        {
            return Restart(p_tweener, false);
        }

        /// <summary>
        /// Restarts the given Tweener, and returns the total number of restarted ones (1 if the Tweener existed, otherwise 0).
        /// </summary>
        /// <param name="p_tweener">
        /// The Tweener to restart.
        /// </param>
        /// <param name="p_skipDelay">
        /// If <c>true</c> skips any initial delay.
        /// </param>
        /// <returns>
        /// The total number of restarted Tweeners (1 if the Tweener existed, otherwise 0).
        /// </returns>
        public static int Restart(Tweener p_tweener, bool p_skipDelay)
        {
            return DoFilteredIteration(p_tweener, DoFilteredRestart, false, p_skipDelay);
        }

        /// <summary>
        /// Restarts the given Sequence, and returns the total number of restarted ones (1 if the Sequence existed, otherwise 0).
        /// </summary>
        /// <param name="p_sequence">
        /// The Sequence to restart.
        /// </param>
        /// <returns>
        /// The total number of restarted Sequences (1 if the Sequence existed, otherwise 0).
        /// </returns>
        public static int Restart(Sequence p_sequence)
        {
            return DoFilteredIteration(p_sequence, DoFilteredRestart, false);
        }

        /// <summary>
        /// Restarts all Tweeners/Sequences (delay included) and Sequences, and returns the total number of restarted Tweeners/Sequences.
        /// </summary>
        /// <returns>
        /// The total number of restarted Tweeners/Sequences.
        /// </returns>
        public static int Restart()
        {
            return Restart(false);
        }

        /// <summary>
        /// Restarts all Tweeners/Sequences and returns the total number of restarted Tweeners/Sequences.
        /// </summary>
        /// <param name="p_skipDelay">
        /// If <c>true</c> skips any initial tween delay.
        /// </param>
        /// <returns>
        /// The total number of restarted Tweeners/Sequences.
        /// </returns>
        public static int Restart(bool p_skipDelay)
        {
            return DoFilteredIteration(null, DoFilteredRestart, false, p_skipDelay);
        }

        /// <summary>
        /// Restarts all tweens from position 0, but recalculating them by taking their current targets values as start values,
        /// and the currently changed value to determine the end values.
        /// </summary>
        /// <param name="p_target">
        /// The target whose tweens to restart.
        /// </param>
        /// <returns>
        /// The total number of restarted Tweeners.
        /// </returns>
        public static int RestartIncremental(object p_target)
        {
            return DoFilteredIteration(p_target, DoFilteredRestartIncremental, false);
        }

        /// <summary>
        /// Restarts all the Tweeners from position 0, but recalculating them by taking their current targets values as start values,
        /// and the currently changed value to determine the end values.
        /// </summary>
        /// <param name="p_id">
        /// The ID of the Tweeners/Sequences to restart.
        /// </param>
        /// <returns>
        /// The total number of restarted Tweeners/Sequences.
        /// </returns>
        public static int RestartIncremental(string p_id)
        {
            return DoFilteredIteration(p_id, DoFilteredRestartIncremental, false);
        }

        /// <summary>
        /// Restarts all the Tweeners from position 0, but recalculating them by taking their current targets values as start values,
        /// and the currently changed value to determine the end values.
        /// </summary>
        /// <param name="p_intId">
        /// The intId of the Tweeners/Sequences to restart.
        /// </param>
        /// <returns>
        /// The total number of restarted Tweeners/Sequences.
        /// </returns>
        public static int RestartIncremental(int p_intId)
        {
            return DoFilteredIteration(p_intId, DoFilteredRestartIncremental, false);
        }

        /// <summary>
        /// Restarts the given Tweener from position 0, but recalculating them by taking their current targets values as start values,
        /// and the currently changed value to determine the end values.
        /// </summary>
        /// <param name="p_tweener">
        /// The Tweener to restart.
        /// </param>
        /// <returns>
        /// The total number of restarted Tweeners (1 if the Tweener existed, otherwise 0).
        /// </returns>
        public static int RestartIncremental(Tweener p_tweener)
        {
            return DoFilteredIteration(p_tweener, DoFilteredRestartIncremental, false);
        }

        /// <summary>
        /// Restarts all Tweeners/Sequences from position 0, but recalculating them by taking their current targets values as start values,
        /// and the currently changed value to determine the end values.
        /// </summary>
        /// <returns>
        /// The total number of restarted Tweeners/Sequences.
        /// </returns>
        public static int RestartIncremental()
        {
            return DoFilteredIteration(null, DoFilteredRestartIncremental, false);
        }

        /// <summary>
        /// Reverses all the tweens for the given target,
        /// animating them from their current value back to the starting one,
        /// and returns the total number of reversed Tweeners.
        /// </summary>
        /// <param name="p_target">
        /// The target whose tweens to reverse.
        /// </param>
        /// <param name="p_forcePlay">
        /// If TRUE, the tween will also start playing in case it was paused,
        /// otherwise it will maintain its current play/pause state (default).
        /// </param>
        /// <returns>
        /// The total number of reversed Tweeners.
        /// </returns>
        public static int Reverse(object p_target, bool p_forcePlay = false)
        {
            return DoFilteredIteration(p_target, DoFilteredReverse, p_forcePlay);
        }

        /// <summary>
        /// Reverses all the Tweeners/Sequences with the given ID,
        /// animating them from their current value back to the starting one,
        /// and returns the total number of reversed Tweeners/Sequences.
        /// </summary>
        /// <param name="p_id">
        /// The ID of the Tweeners/Sequences to reverse.
        /// </param>
        /// <param name="p_forcePlay">
        /// If TRUE, the tween will also start playing in case it was paused,
        /// otherwise it will maintain its current play/pause state (default).
        /// </param>
        /// <returns>
        /// The total number of reversed Tweeners/Sequences.
        /// </returns>
        public static int Reverse(string p_id, bool p_forcePlay = false)
        {
            return DoFilteredIteration(p_id, DoFilteredReverse, p_forcePlay);
        }

        /// <summary>
        /// Reverses all the Tweeners/Sequences with the given intId,
        /// animating them from their current value back to the starting one,
        /// and returns the total number of reversed Tweeners/Sequences.
        /// </summary>
        /// <param name="p_intId">
        /// The intId of the Tweeners/Sequences to reverse.
        /// </param>
        /// <param name="p_forcePlay">
        /// If TRUE, the tween will also start playing in case it was paused,
        /// otherwise it will maintain its current play/pause state (default).
        /// </param>
        /// <returns>
        /// The total number of reversed Tweeners/Sequences.
        /// </returns>
        public static int Reverse(int p_intId, bool p_forcePlay = false)
        {
            return DoFilteredIteration(p_intId, DoFilteredReverse, p_forcePlay);
        }

        /// <summary>
        /// Reverses the given Tweener,
        /// animating it from its current value back to the starting one,
        /// and returns the total number of reversed Tweeners (1 if the Tweener existed, otherwise 0).
        /// </summary>
        /// <param name="p_tweener">
        /// The Tweener to reverse.
        /// </param>
        /// <param name="p_forcePlay">
        /// If TRUE, the tween will also start playing in case it was paused,
        /// otherwise it will maintain its current play/pause state (default).
        /// </param>
        /// <returns>
        /// The total number of reversed Tweeners (1 if the Tweener existed, otherwise 0).
        /// </returns>
        public static int Reverse(Tweener p_tweener, bool p_forcePlay = false)
        {
            return DoFilteredIteration(p_tweener, DoFilteredReverse, p_forcePlay);
        }

        /// <summary>
        /// Reverses the given Sequence, and returns the total number of reversed ones (1 if the Sequence existed, otherwise 0).
        /// </summary>
        /// <param name="p_sequence">
        /// The Sequence to reverse.
        /// </param>
        /// <param name="p_forcePlay">
        /// If TRUE, the tween will also start playing in case it was paused,
        /// otherwise it will maintain its current play/pause state (default).
        /// </param>
        /// <returns>
        /// The total number of reversed Sequences (1 if the Sequence existed, otherwise 0).
        /// </returns>
        public static int Reverse(Sequence p_sequence, bool p_forcePlay = false)
        {
            return DoFilteredIteration(p_sequence, DoFilteredReverse, p_forcePlay);
        }

        /// <summary>
        /// Reverses all Tweeners/Sequences,
        /// animating them from their current value back to the starting one,
        /// and returns the total number of reversed Tweeners/Sequences.
        /// </summary>
        /// <param name="p_forcePlay">
        /// If TRUE, the tween will also start playing in case it was paused,
        /// otherwise it will maintain its current play/pause state (default).
        /// </param>
        /// <returns>
        /// The total number of reversed Tweeners/Sequences.
        /// </returns>
        public static int Reverse(bool p_forcePlay = false)
        {
            return DoFilteredIteration(null, DoFilteredReverse, p_forcePlay);
        }

        /// <summary>
        /// Completes all the tweens for the given target, and returns the total number of completed Tweeners.
        /// Where a loop was involved and not infinite, the relative tween completes at the position where it would actually be after the set number of loops.
        /// If there were infinite loops, this method will have no effect.
        /// </summary>
        /// <param name="p_target">
        /// The target whose tweens to complete.
        /// </param>
        /// <returns>
        /// The total number of completed Tweeners.
        /// </returns>
        public static int Complete(object p_target)
        {
            return DoFilteredIteration(p_target, DoFilteredComplete, true);
        }

        /// <summary>
        /// Completes all the Tweeners/Sequences with the given ID, and returns the total number of completed Tweeners/Sequences.
        /// Where a loop was involved and not infinite, the relative Tweener/Sequence completes at the position where it would actually be after the set number of loops.
        /// If there were infinite loops, this method will have no effect.
        /// </summary>
        /// <param name="p_id">
        /// The ID of the Tweeners/Sequences to complete.
        /// </param>
        /// <returns>
        /// The total number of completed Tweeners/Sequences.
        /// </returns>
        public static int Complete(string p_id)
        {
            return DoFilteredIteration(p_id, DoFilteredComplete, true);
        }

        /// <summary>
        /// Completes all the Tweeners/Sequences with the given intId, and returns the total number of completed Tweeners/Sequences.
        /// Where a loop was involved and not infinite, the relative Tweener/Sequence completes at the position where it would actually be after the set number of loops.
        /// If there were infinite loops, this method will have no effect.
        /// </summary>
        /// <param name="p_intId">
        /// The intId of the Tweeners/Sequences to complete.
        /// </param>
        /// <returns>
        /// The total number of completed Tweeners/Sequences.
        /// </returns>
        public static int Complete(int p_intId)
        {
            return DoFilteredIteration(p_intId, DoFilteredComplete, true);
        }

        /// <summary>
        /// Completes the given Tweener, and returns the total number of completed ones (1 if the Tweener existed, otherwise 0).
        /// Where a loop was involved and not infinite, the relative Tweener completes at the position where it would actually be after the set number of loops.
        /// If there were infinite loops, this method will have no effect.
        /// </summary>
        /// <param name="p_tweener">
        /// The Tweener to complete.
        /// </param>
        /// <returns>
        /// The total number of completed Tweeners (1 if the Tweener existed, otherwise 0).
        /// </returns>
        public static int Complete(Tweener p_tweener)
        {
            return DoFilteredIteration(p_tweener, DoFilteredComplete, true);
        }

        /// <summary>
        /// Completes the given Sequence, and returns the total number of completed ones (1 if the Sequence existed, otherwise 0).
        /// Where a loop was involved and not infinite, the relative Sequence completes at the position where it would actually be after the set number of loops.
        /// If there were infinite loops, this method will have no effect.
        /// </summary>
        /// <param name="p_sequence">
        /// The Sequence to complete.
        /// </param>
        /// <returns>
        /// The total number of completed Sequences (1 if the Sequence existed, otherwise 0).
        /// </returns>
        public static int Complete(Sequence p_sequence)
        {
            return DoFilteredIteration(p_sequence, DoFilteredComplete, true);
        }

        /// <summary>
        /// Completes all Tweeners/Sequences, and returns the total number of completed Tweeners/Sequences.
        /// Where a loop was involved and not infinite, the relative Tweener/Sequence completes at the position where it would actually be after the set number of loops.
        /// If there were infinite loops, this method will have no effect.
        /// </summary>
        /// <returns>
        /// The total number of completed Tweeners/Sequences.
        /// </returns>
        public static int Complete()
        {
            return DoFilteredIteration(null, DoFilteredComplete, true);
        }

        /// <summary>
        /// Kills all the tweens for the given target (unless they're were created inside a <see cref="Sequence"/>),
        /// and returns the total number of killed Tweeners.
        /// </summary>
        /// <param name="p_target">
        /// The target whose Tweeners to kill.
        /// </param>
        /// <returns>
        /// The total number of killed Tweeners.
        /// </returns>
        public static int Kill(object p_target)
        {
            return DoFilteredIteration(p_target, DoFilteredKill, true);
        }

        /// <summary>
        /// Kills all the Tweeners/Sequences with the given ID, and returns the total number of killed Tweeners/Sequences.
        /// </summary>
        /// <param name="p_id">
        /// The ID of the Tweeners/Sequences to kill.
        /// </param>
        /// <returns>
        /// The total number of killed Tweeners/Sequences.
        /// </returns>
        public static int Kill(string p_id)
        {
            return DoFilteredIteration(p_id, DoFilteredKill, true);
        }

        /// <summary>
        /// Kills all the Tweeners/Sequences with the given intId, and returns the total number of killed Tweeners/Sequences.
        /// </summary>
        /// <param name="p_intId">
        /// The intId of the Tweeners/Sequences to kill.
        /// </param>
        /// <returns>
        /// The total number of killed Tweeners/Sequences.
        /// </returns>
        public static int Kill(int p_intId)
        {
            return DoFilteredIteration(p_intId, DoFilteredKill, true);
        }

        /// <summary>
        /// Kills the given Tweener, and returns the total number of killed ones (1 if the Tweener existed, otherwise 0).
        /// </summary>
        /// <param name="p_tweener">
        /// The Tweener to kill.
        /// </param>
        /// <returns>
        /// The total number of killed Tweeners (1 if the Tweener existed, otherwise 0).
        /// </returns>
        public static int Kill(Tweener p_tweener)
        {
            return DoFilteredIteration(p_tweener, DoFilteredKill, true);
        }

        /// <summary>
        /// Kills the given Sequence, and returns the total number of killed ones (1 if the Sequence existed, otherwise 0).
        /// </summary>
        /// <param name="p_sequence">
        /// The Sequence to kill.
        /// </param>
        /// <returns>
        /// The total number of killed Sequences (1 if the Sequence existed, otherwise 0).
        /// </returns>
        public static int Kill(Sequence p_sequence)
        {
            return DoFilteredIteration(p_sequence, DoFilteredKill, true);
        }

        /// <summary>
        /// Kills all Tweeners/Sequences, and returns the total number of killed Tweeners/Sequences.
        /// </summary>
        /// <returns>
        /// The total number of killed Tweeners/Sequences.
        /// </returns>
        public static int Kill()
        {
            return DoFilteredIteration(null, DoFilteredKill, true);
        }

        /// <summary>
        /// Used by Sequences to remove added tweens from main tweens list.
        /// </summary>
        /// <param name="p_tween"></param>
        internal static void RemoveFromTweens(IHOTweenComponent p_tween)
        {
            if (tweens == null) return;

            int tweensCount = tweens.Count;
            for (int i = 0; i < tweensCount; ++i) {
                if (tweens[i] == p_tween) {
                    tweens.RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// Returns all existing Tweeners (excluding nested ones) and Sequences, paused or not.
        /// </summary>
        public static List<IHOTweenComponent> GetAllTweens()
        {
            if (tweens == null) return new List<IHOTweenComponent>(1);

            List<IHOTweenComponent> tws = new List<IHOTweenComponent>(tweens.Count);
            foreach (ABSTweenComponent tween in tweens) tws.Add(tween);
            return tws;
        }
        /// <summary>
        /// Returns all existing Tweeners (excluding nested ones) and Sequences that are currently playing.
        /// </summary>
        public static List<IHOTweenComponent> GetAllPlayingTweens()
        {
            if (tweens == null) return new List<IHOTweenComponent>(1);

            List<IHOTweenComponent> tws = new List<IHOTweenComponent>(tweens.Count);
            foreach (ABSTweenComponent tween in tweens) {
                if (!tween.isPaused) tws.Add(tween);
            }
            return tws;
        }
        /// <summary>
        /// Returns all existing Tweeners (excluding nested ones) and Sequences that are currently paused.
        /// </summary>
        public static List<IHOTweenComponent> GetAllPausedTweens()
        {
            if (tweens == null) return new List<IHOTweenComponent>(1);

            List<IHOTweenComponent> tws = new List<IHOTweenComponent>(tweens.Count);
            foreach (ABSTweenComponent tween in tweens) {
                if (tween.isPaused) tws.Add(tween);
            }
            return tws;
        }

        /// <summary>
        /// Returns a list of the eventual existing tweens with the given Id,
        /// (empty if no Tweener/Sequence was found).
        /// </summary>
        /// <param name="p_id">Id to look for</param>
        /// <param name="p_includeNestedTweens">If TRUE also searches inside nested tweens</param>
        /// <returns></returns>
        public static List<IHOTweenComponent> GetTweensById(string p_id, bool p_includeNestedTweens)
        {
            List<IHOTweenComponent> res = new List<IHOTweenComponent>();
            if (tweens == null) return res;
            int tweensCount = tweens.Count;
            for (int i = 0; i < tweensCount; ++i) {
                ABSTweenComponent tw = tweens[i];
                if (p_includeNestedTweens) {
                    res.AddRange(tw.GetTweensById(p_id));
                } else {
                    if (tw.id == p_id) res.Add(tw);
                }
            }
            return res;
        }

        /// <summary>
        /// Returns a list of the eventual existing tweens with the given intId,
        /// (empty if no Tweener/Sequence was found).
        /// </summary>
        /// <param name="p_intId">IntId to look for</param>
        /// <param name="p_includeNestedTweens">If TRUE also searches inside nested tweens</param>
        /// <returns></returns>
        public static List<IHOTweenComponent> GetTweensByIntId(int p_intId, bool p_includeNestedTweens)
        {
            List<IHOTweenComponent> res = new List<IHOTweenComponent>();
            if (tweens == null) return res;
            int tweensCount = tweens.Count;
            for (int i = 0; i < tweensCount; ++i) {
                ABSTweenComponent tw = tweens[i];
                if (p_includeNestedTweens) {
                    res.AddRange(tw.GetTweensByIntId(p_intId));
                } else {
                    if (tw.intId == p_intId) res.Add(tw);
                }
            }
            return res;
        }

        /// <summary>
        /// Returns a list with all the existing <see cref="Tweener"/> objects whose target is the given one,
        /// or an empty list if none was found.
        /// </summary>
        /// <param name="p_target">Target to look for</param>
        /// <param name="p_includeNestedTweens">If TRUE also searches inside nested Tweeners</param>
        /// <returns></returns>
        public static List<Tweener> GetTweenersByTarget(object p_target, bool p_includeNestedTweens)
        {
            List<Tweener> res = new List<Tweener>();
            if (tweens == null) return res;
            int tweensCount = tweens.Count;
            for (int i = 0; i < tweensCount; ++i) {
                ABSTweenComponent tw = tweens[i];
                Tweener tweener = tw as Tweener;
                if (tweener != null) {
                    // Tweener
                    if (tweener.target == p_target) res.Add(tweener);
                } else {
                    // Sequence
                    if (p_includeNestedTweens) res.AddRange(((Sequence)tw).GetTweenersByTarget(p_target));
                }
            }
            return res;
        }

        /// <summary>
        /// Returns <c>true</c> if the given target is currently involved in any running Tweener or Sequence (taking into account also nested tweens).
        /// Returns <c>false</c> both if the given target is not inside a Tweener, than if the relative Tweener is paused.
        /// To simply check if the target is attached to a Tweener or Sequence use <see cref="IsLinkedTo"/> instead.
        /// </summary>
        /// <param name="p_target">
        /// The target to check.
        /// </param>
        /// <returns>
        /// A value of <c>true</c> if the given target is currently involved in any running Tweener or Sequence (taking into account also nested tweens).
        /// </returns>
        public static bool IsTweening(object p_target)
        {
            if (tweens == null) return false;

            int tweensCount = tweens.Count;
            for (int i = 0; i < tweensCount; ++i) {
                if (tweens[i].IsTweening(p_target)) return true;
            }

            return false;
        }
        /// <summary>
        /// Returns <c>true</c> if the given id is involved in any running Tweener or Sequence (taking into account also nested tweens).
        /// </summary>
        /// <param name="p_id">
        /// The target to check.
        /// </param>
        public static bool IsTweening(string p_id)
        {
            if (tweens == null) return false;

            int tweensCount = tweens.Count;
            for (int i = 0; i < tweensCount; ++i) {
                if (tweens[i].IsTweening(p_id)) return true;
            }

            return false;
        }
        /// <summary>
        /// Returns <c>true</c> if the given id is involved in any running Tweener or Sequence (taking into account also nested tweens).
        /// </summary>
        /// <param name="p_id">
        /// The target to check.
        /// </param>
        public static bool IsTweening(int p_id)
        {
            if (tweens == null) return false;

            int tweensCount = tweens.Count;
            for (int i = 0; i < tweensCount; ++i) {
                if (tweens[i].IsTweening(p_id)) return true;
            }

            return false;
        }

        /// <summary>
        /// Returns <c>true</c> if the given target is linked to any Tweener or Sequence (running or not, taking into account also nested tweens).
        /// </summary>
        /// <param name="p_target">
        /// The target to check.
        /// </param>
        /// <returns>
        /// A value of <c>true</c> if the given target is linked to any Tweener or Sequence (running or not, taking into account also nested tweens).
        /// </returns>
        public static bool IsLinkedTo(object p_target)
        {
            if (tweens == null) return false;

            int tweensCount = tweens.Count;

            for (int i = 0; i < tweensCount; ++i) {
                ABSTweenComponent tw = tweens[i];
                if (tw.IsLinkedTo(p_target)) return true;
            }

            return false;
        }

        /// <summary>
        /// Returns a <see cref="TweenInfo"/> list of the current tweens (paused and delayed included),
        /// or null if there are no tweens.
        /// </summary>
        /// <returns></returns>
        public static TweenInfo[] GetTweenInfos()
        {
            if (totTweens <= 0) return null;
            int tweensCount = tweens.Count;
            TweenInfo[] twInfos = new TweenInfo[tweensCount];
            for (int i = 0; i < tweensCount; ++i) twInfos[i] = new TweenInfo(tweens[i]);
            return twInfos;
        }

        // ===================================================================================
        // PRIVATE METHODS -------------------------------------------------------------------

        static void DoUpdate(UpdateType p_updateType, float p_elapsed)
        {
            tweensToRemoveIndexes.Clear();
            isUpdateLoop = true;
            int tweensCount = tweens.Count;
            for (int i = 0; i < tweensCount; ++i) {
                ABSTweenComponent tw = tweens[i];
                if (tw.updateType == p_updateType && !tw._isPaused && tw.Update(p_elapsed * tw.timeScale)) {
                    // Tween complete...
                    if (tw.destroyed || tw.autoKillOnComplete) {
                        // ...autoKill: store for out-of-loop removal
                        tw.Kill(false);
                        if (tweensToRemoveIndexes.IndexOf(i) == -1) tweensToRemoveIndexes.Add(i);
                    }
                }
            }
            isUpdateLoop = false;

            // Remove eventual killed tweens
            int tweensToRemoveCount = tweensToRemoveIndexes.Count;
            if (tweensToRemoveCount > 0) {
                tweensToRemoveIndexes.Sort();
                for (int i = 0; i < tweensToRemoveCount; ++i) {
                    tweens.RemoveAt(tweensToRemoveIndexes[i] - i);
                }
            }

            int onCompletesCount = onCompletes.Count;

            // Dispatch eventual onCompletes.
            if (onCompletesCount > 0) {
                for (int i = 0; i < onCompletesCount; ++i) {
                    onCompletes[i].OnCompleteDispatch();
                }
                onCompletes = new List<ABSTweenComponent>();
            }
        }

        static void DoFilteredKill(int p_index, bool p_optionalBool)
        {
            tweens[p_index].Kill(false);
            if (isUpdateLoop) {
                // We're inside the DoUpdate loop:
                // add index to tweens to remove and let DoUpdate manage it in the correct order
                if (tweensToRemoveIndexes.IndexOf(p_index) == -1) tweensToRemoveIndexes.Add(p_index);
            } else {
                tweens.RemoveAt(p_index);
            }
        }

        static void DoFilteredPause(int p_index, bool p_optionalBool)
        {
            tweens[p_index].Pause();
        }

        static void DoFilteredPlay(int p_index, bool p_skipDelay)
        {
            ABSTweenComponent tw = tweens[p_index];
            Tweener tweener = tw as Tweener;
            if (tweener != null) {
                tweener.Play(p_skipDelay);
            } else {
                tw.Play();
            }
        }

        static void DoFilteredPlayForward(int p_index, bool p_skipDelay)
        {
            ABSTweenComponent tw = tweens[p_index];
            Tweener tweener = tw as Tweener;
            if (tweener != null) {
                tweener.PlayForward(p_skipDelay);
            } else {
                tw.PlayForward();
            }
        }

        static void DoFilteredPlayBackwards(int p_index, bool p_optionalBool)
        {
            ABSTweenComponent tw = tweens[p_index];
            Tweener tweener = tw as Tweener;
            if (tweener != null) {
                tweener.PlayBackwards();
            } else {
                tw.PlayBackwards();
            }
        }

        static void DoFilteredRewind(int p_index, bool p_skipDelay)
        {
            ABSTweenComponent tw = tweens[p_index];
            Tweener tweener = tw as Tweener;
            if (tweener != null) {
                tweener.Rewind(p_skipDelay);
            } else {
                tw.Rewind();
            }
        }

        static void DoFilteredRestart(int p_index, bool p_skipDelay)
        {
            ABSTweenComponent tw = tweens[p_index];
            Tweener tweener = tw as Tweener;
            if (tweener != null) {
                tweener.Restart(p_skipDelay);
            } else {
                tw.Restart();
            }
        }

        static void DoFilteredRestartIncremental(int p_index, bool p_skipDelay)
        {
            ABSTweenComponent tw = tweens[p_index];
            Tweener tweener = tweens[p_index] as Tweener;
            if (tweener != null) {
                tweener.RestartIncremental();
            } else {
                tw.Restart();
            }
        }

        static void DoFilteredReverse(int p_index, bool p_forcePlay = false)
        {
            tweens[p_index].Reverse(p_forcePlay);
        }

        static void DoFilteredComplete(int p_index, bool p_optionalBool)
        {
            tweens[p_index].Complete(false);
        }

        /// <summary>
        /// Used by callbacks that are wired to sendMessage.
        /// </summary>
        static internal void DoSendMessage(TweenEvent e)
        {
            GameObject target = e.parms[0] as GameObject;
            if (target == null) return;

            string methodName = e.parms[1] as string;
            object value = e.parms[2];
            SendMessageOptions options = (SendMessageOptions)e.parms[3];
            if (value != null) {
                target.SendMessage(methodName, e.parms[2], options);
            } else {
                target.SendMessage(methodName, options);
            }
        }

        static void AddTween(ABSTweenComponent p_tween)
        {
            if (tweenGOInstance == null) {
                NewTweenInstance();
            }
            if (tweens == null) {
                tweens = new List<ABSTweenComponent>();
                it.StartCoroutines();
            }
            tweens.Add(p_tween);
            SetGOName();
        }

        static void NewTweenInstance()
        {
            tweenGOInstance = new GameObject(GAMEOBJNAME);
            it = tweenGOInstance.AddComponent<HOTween>();
            DontDestroyOnLoad(tweenGOInstance);
        }

        void StartCoroutines()
        {
            time = Time.realtimeSinceStartup;
            StartCoroutine(StartCoroutines_StartTimeScaleIndependentUpdate());
        }

        IEnumerator StartCoroutines_StartTimeScaleIndependentUpdate()
        {
            yield return null;

            StartCoroutine(TimeScaleIndependentUpdate());

            yield break;
        }

        static void SetGOName()
        {
            if (!isEditor || !renameInstToCountTw || isQuitting) return;
            if (tweenGOInstance != null) tweenGOInstance.name = GAMEOBJNAME + " : " + totTweens;
        }

        static bool CheckClear()
        {
            if (tweens == null || tweens.Count == 0) {
                Clear();
                if (isPermanent) {
                    SetGOName();
                }
                return true;
            }

            SetGOName();
            return false;
        }

        static void Clear()
        {
            if (it != null) it.StopAllCoroutines();

            tweens = null;

            if (!isPermanent) {
                if (tweenGOInstance != null) Destroy(tweenGOInstance);
                tweenGOInstance = null;
                it = null;
            }
        }

        // ===================================================================================
        // HELPERS ---------------------------------------------------------------------------

        /// <summary>
        /// Filter filters for:
        /// - ID if <see cref="string"/>
        /// - Tweener if <see cref="Tweener"/>
        /// - Sequence if <see cref="Sequence"/>
        /// - Tweener target if <see cref="object"/> (doesn't look inside sequence tweens)
        /// - Everything if null
        /// </summary>
        static int DoFilteredIteration(object p_filter, TweenDelegate.FilterFunc p_operation, bool p_collectionChanger)
        {
            return DoFilteredIteration(p_filter, p_operation, p_collectionChanger, false);
        }

        static int DoFilteredIteration(object p_filter, TweenDelegate.FilterFunc p_operation, bool p_collectionChanger, bool p_optionalBool)
        {
            if (tweens == null) {
                return 0;
            }

            int opCount = 0;
            int tweensCount = tweens.Count - 1;

            if (p_filter == null) {
                // All
                for (int i = tweensCount; i > -1; --i) {
                    p_operation(i, p_optionalBool);
                    ++opCount;
                }
            } else if (p_filter is int) {
                // Int ID
                int f = (int)p_filter;
                for (int i = tweensCount; i > -1; --i) {
                    if (tweens[i].intId == f) {
                        p_operation(i, p_optionalBool);
                        ++opCount;
                    }
                }
            } else if (p_filter is string) {
                // ID
                string f = (string)p_filter;
                for (int i = tweensCount; i > -1; --i) {
                    if (tweens[i].id == f) {
                        p_operation(i, p_optionalBool);
                        ++opCount;
                    }
                }
            } else if (p_filter is Tweener) {
                // Tweener
                Tweener f = p_filter as Tweener;
                for (int i = tweensCount; i > -1; --i) {
                    if (tweens[i] == f) {
                        p_operation(i, p_optionalBool);
                        ++opCount;
                    }
                }
            } else if (p_filter is Sequence) {
                // Sequence
                Sequence f = p_filter as Sequence;
                for (int i = tweensCount; i > -1; --i) {
                    if (tweens[i] == f) {
                        p_operation(i, p_optionalBool);
                        ++opCount;
                    }
                }
            } else {
                // Target
                for (int i = tweensCount; i > -1; --i) {
                    Tweener tw = tweens[i] as Tweener;
                    if (tw != null && tw.target == p_filter) {
                        p_operation(i, p_optionalBool);
                        ++opCount;
                    }
                }
            }

            if (p_collectionChanger) {
                CheckClear();
            }

            return opCount;
        }

        /// <summary>
        /// Returns all the currently existing plugins involved in any tween, even if nested or paused,
        /// or <c>null</c> if there are none.
        /// </summary>
        static List<ABSTweenPlugin> GetPlugins()
        {
            if (tweens == null) {
                return null;
            }

            List<ABSTweenPlugin> plugs = new List<ABSTweenPlugin>();
            int tweensCount = tweens.Count;
            for (int i = 0; i < tweensCount; ++i) {
                tweens[i].FillPluginsList(plugs);
            }

            return plugs;
        }
    }
}
