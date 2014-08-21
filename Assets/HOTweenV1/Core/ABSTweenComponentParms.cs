//
// ABSTweenComponentParms.cs
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

using UnityEngine;

namespace Holoville.HOTween.Core
{
    /// <summary>
    /// Base class for all HOTParms.
    /// </summary>
    public abstract class ABSTweenComponentParms
    {
        // VARS ///////////////////////////////////////////////////

        /// <summary>
        /// ID.
        /// </summary>
        protected string id = "";

        /// <summary>
        /// Int ID.
        /// </summary>
        protected int intId = -1;

        /// <summary>
        /// Auto kill on complete.
        /// </summary>
        protected bool autoKillOnComplete = true;

        /// <summary>
        /// Update type.
        /// </summary>
        protected UpdateType updateType = HOTween.defUpdateType;

        /// <summary>
        /// Time scale.
        /// </summary>
        protected float timeScale = HOTween.defTimeScale;

        /// <summary>
        /// Loops
        /// </summary>
        protected int loops = 1;

        /// <summary>
        /// Loop type.
        /// </summary>
        protected LoopType loopType = HOTween.defLoopType;

        /// <summary>
        /// Paused.
        /// </summary>
        protected bool isPaused;

        /// <summary>
        /// On start.
        /// </summary>
        protected TweenDelegate.TweenCallback onStart;

        /// <summary>
        /// On start with parms.
        /// </summary>
        protected TweenDelegate.TweenCallbackWParms onStartWParms;

        /// <summary>
        /// On start parameters.
        /// </summary>
        protected object[] onStartParms;

        /// <summary>
        /// On update.
        /// </summary>
        protected TweenDelegate.TweenCallback onUpdate;

        /// <summary>
        /// On update with parms.
        /// </summary>
        protected TweenDelegate.TweenCallbackWParms onUpdateWParms;

        /// <summary>
        /// On update parameters.
        /// </summary>
        protected object[] onUpdateParms;

        /// <summary>
        /// On plugin results.
        /// </summary>
        protected TweenDelegate.TweenCallback onPluginUpdated;

        /// <summary>
        /// On plugin results with parms.
        /// </summary>
        protected TweenDelegate.TweenCallbackWParms onPluginUpdatedWParms;

        /// <summary>
        /// On plugin results parameters.
        /// </summary>
        protected object[] onPluginUpdatedParms;

        /// <summary>
        /// On pause.
        /// </summary>
        protected TweenDelegate.TweenCallback onPause;

        /// <summary>
        /// On pause with parms.
        /// </summary>
        protected TweenDelegate.TweenCallbackWParms onPauseWParms;

        /// <summary>
        /// On pause parameters.
        /// </summary>
        protected object[] onPauseParms;

        /// <summary>
        /// On play.
        /// </summary>
        protected TweenDelegate.TweenCallback onPlay;

        /// <summary>
        /// On play with parms.
        /// </summary>
        protected TweenDelegate.TweenCallbackWParms onPlayWParms;

        /// <summary>
        /// On play parameters.
        /// </summary>
        protected object[] onPlayParms;

        /// <summary>
        /// On rewinded.
        /// </summary>
        protected TweenDelegate.TweenCallback onRewinded;

        /// <summary>
        /// On rewinded with parms.
        /// </summary>
        protected TweenDelegate.TweenCallbackWParms onRewindedWParms;

        /// <summary>
        /// On rewinded parameters.
        /// </summary>
        protected object[] onRewindedParms;

        /// <summary>
        /// On step complete.
        /// </summary>
        protected TweenDelegate.TweenCallback onStepComplete;

        /// <summary>
        /// On step complete with parms.
        /// </summary>
        protected TweenDelegate.TweenCallbackWParms onStepCompleteWParms;

        /// <summary>
        /// On step complete parameters.
        /// </summary>
        protected object[] onStepCompleteParms;

        /// <summary>
        /// On complete.
        /// </summary>
        protected TweenDelegate.TweenCallback onComplete;

        /// <summary>
        /// On complete with parms.
        /// </summary>
        protected TweenDelegate.TweenCallbackWParms onCompleteWParms;

        /// <summary>
        /// On complete parameters.
        /// </summary>
        protected object[] onCompleteParms;

        // BEHAVIOURS/GAMEOBJECT MANAGEMENT PARMS

        /// <summary>
        /// True if there are behaviours to manage
        /// </summary>
        protected bool manageBehaviours;
        /// <summary>
        /// True if there are gameObject to manage
        /// </summary>
        protected bool manageGameObjects;
        /// <summary>
        /// Behaviours to activate
        /// </summary>
        protected Behaviour[] managedBehavioursOn;
        /// <summary>
        /// Behaviours to deactivate
        /// </summary>
        protected Behaviour[] managedBehavioursOff;
        /// <summary>
        /// GameObjects to activate
        /// </summary>
        protected GameObject[] managedGameObjectsOn;
        /// <summary>
        /// GameObejcts to deactivate
        /// </summary>
        protected GameObject[] managedGameObjectsOff;


        // ***********************************************************************************
        // INIT
        // ***********************************************************************************

        /// <summary>
        /// Initializes the given owner with the stored parameters.
        /// </summary>
        /// <param name="p_owner">
        /// The <see cref="ABSTweenComponent"/> to initialize.
        /// </param>
        protected void InitializeOwner(ABSTweenComponent p_owner)
        {
            p_owner._id = id;
            p_owner._intId = intId;
            p_owner._autoKillOnComplete = autoKillOnComplete;
            p_owner._updateType = updateType;
            p_owner._timeScale = timeScale;
            p_owner._loops = loops;
            p_owner._loopType = loopType;
            p_owner._isPaused = isPaused;

            p_owner.onStart = onStart;
            p_owner.onStartWParms = onStartWParms;
            p_owner.onStartParms = onStartParms;
            p_owner.onUpdate = onUpdate;
            p_owner.onUpdateWParms = onUpdateWParms;
            p_owner.onUpdateParms = onUpdateParms;
            p_owner.onPluginUpdated = onPluginUpdated;
            p_owner.onPluginUpdatedWParms = onPluginUpdatedWParms;
            p_owner.onPluginUpdatedParms = onPluginUpdatedParms;
            p_owner.onPause = onPause;
            p_owner.onPauseWParms = onPauseWParms;
            p_owner.onPauseParms = onPauseParms;
            p_owner.onPlay = onPlay;
            p_owner.onPlayWParms = onPlayWParms;
            p_owner.onPlayParms = onPlayParms;
            p_owner.onRewinded = onRewinded;
            p_owner.onRewindedWParms = onRewindedWParms;
            p_owner.onRewindedParms = onRewindedParms;
            p_owner.onStepComplete = onStepComplete;
            p_owner.onStepCompleteWParms = onStepCompleteWParms;
            p_owner.onStepCompleteParms = onStepCompleteParms;
            p_owner.onComplete = onComplete;
            p_owner.onCompleteWParms = onCompleteWParms;
            p_owner.onCompleteParms = onCompleteParms;

            p_owner.manageBehaviours = manageBehaviours;
            p_owner.manageGameObjects = manageGameObjects;
            p_owner.managedBehavioursOn = managedBehavioursOn;
            p_owner.managedBehavioursOff = managedBehavioursOff;
            p_owner.managedGameObjectsOn = managedGameObjectsOn;
            p_owner.managedGameObjectsOff = managedGameObjectsOff;
            if (manageBehaviours) {
                int len = (managedBehavioursOn != null ? managedBehavioursOn.Length : 0) + (managedBehavioursOff != null ? managedBehavioursOff.Length : 0);
                p_owner.managedBehavioursOriginalState = new bool[len];
            }
            if (manageGameObjects) {
                int len = (managedGameObjectsOn != null ? managedGameObjectsOn.Length : 0) + (managedGameObjectsOff != null ? managedGameObjectsOff.Length : 0);
                p_owner.managedGameObjectsOriginalState = new bool[len];
            }
        }
    }
}
