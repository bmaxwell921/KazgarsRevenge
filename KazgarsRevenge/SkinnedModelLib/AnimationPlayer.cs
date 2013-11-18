#region File Description
//-----------------------------------------------------------------------------
// AnimationPlayer.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
#endregion

namespace SkinnedModelLib
{
    /// <summary>
    /// The animation player is in charge of decoding bone position
    /// matrices from an animation clip.
    /// </summary>
    public class AnimationPlayer
    {
        #region Fields


        // Information about the currently playing animation clip.
        AnimationClip currentClipValue;
        TimeSpan currentTimeValue;
        int currentKeyframe;

        bool mixing = false;
        bool playMixedOnce = false;
        AnimationClip secondClipValue = null;
        TimeSpan secondTimeValue;
        int secondKeyframe;
        List<int> bonesToIgnore;

        // Current animation transform matrices.
        Matrix[] boneTransforms;
        Matrix[] worldTransforms;
        Matrix[] skinTransforms;


        // Backlink to the bind pose and skeleton hierarchy data.
        public SkinningData skinningDataValue { get; private set; }


        #endregion


        /// <summary>
        /// Constructs a new animation player.
        /// </summary>
        public AnimationPlayer(SkinningData skinningData)
        {
            if (skinningData == null)
                throw new ArgumentNullException("skinningData");

            skinningDataValue = skinningData;

            boneTransforms = new Matrix[skinningData.BindPose.Count];
            worldTransforms = new Matrix[skinningData.BindPose.Count];
            skinTransforms = new Matrix[skinningData.BindPose.Count];
        }

        public void StopMixing()
        {
            mixing = false;
            secondClipValue = null;
        }

        /// <summary>
        /// Starts decoding the specified animation clip.
        /// </summary>
        public void StartClip(string clipName)
        {
            currentClipValue = skinningDataValue.AnimationClips[clipName];
            currentTimeValue = TimeSpan.Zero;
            currentKeyframe = 0;

            // Initialize bone transforms to the bind pose.
            skinningDataValue.BindPose.CopyTo(boneTransforms, 0);
        }

        /// <summary>
        /// Mixes a clip with the one already playing, but stops after one run
        /// </summary>
        /// <param name="clipName">the clip to play</param>
        /// <param name="boneIndicesToIgnore">a list of bone indices to ignore. just pass in null if you don't care</param>
        public void MixClipOnce(string clipName, List<int> boneIndicesToIgnore)
        {
            mixing = true;
            playMixedOnce = true;
            secondClipValue = skinningDataValue.AnimationClips[clipName];
            secondTimeValue = TimeSpan.Zero;
            secondKeyframe = 0;
            this.bonesToIgnore = boneIndicesToIgnore;
        }

        public void BlendCurrentIntoClip(string clipName)
        {
            playMixedOnce = false;
            mixing = true;
            secondClipValue = skinningDataValue.AnimationClips[clipName];
            secondTimeValue = TimeSpan.Zero;
            secondKeyframe = 0;
        }

        /// <summary>
        /// Advances the current animation position.
        /// </summary>
        public void Update(TimeSpan time, bool relativeToCurrentTime,
                           Matrix rootTransform)
        {
            UpdateBoneTransforms(time, relativeToCurrentTime);
            UpdateWorldTransforms(rootTransform);
            UpdateSkinTransforms();
        }

        private void UpdateCurrentTime(TimeSpan time, bool relativeToCurrentTime)
        {
            // Update the animation position.
            if (relativeToCurrentTime)
            {
                time += currentTimeValue;

                // If we reached the end, loop back to the start.
                while (time >= currentClipValue.Duration)
                    time -= currentClipValue.Duration;
            }

            if ((time < TimeSpan.Zero) || (time >= currentClipValue.Duration))
                throw new ArgumentOutOfRangeException("time");

            // If the position moved backwards, reset the keyframe index.
            if (time < currentTimeValue)
            {
                currentKeyframe = 0;
                skinningDataValue.BindPose.CopyTo(boneTransforms, 0);
            }

            currentTimeValue = time;
        }

        private void UpdateSecondTime(TimeSpan time, bool relativeToCurrentTime)
        {
            // Update the animation position.
            if (relativeToCurrentTime)
            {
                time += secondTimeValue;

                // If we reached the end, stop mixing
                if(time >= secondClipValue.Duration)
                {
                    StopMixing();
                    return;
                }
            }

            if ((time < TimeSpan.Zero) || (time >= secondClipValue.Duration))
                throw new ArgumentOutOfRangeException("time");

            // If the position moved backwards, reset the keyframe index.
            if (time < secondTimeValue)
            {
                secondKeyframe = 0;
                skinningDataValue.BindPose.CopyTo(boneTransforms, 0);
            }

            secondTimeValue = time;
        }

        /// <summary>
        /// Helper used by the Update method to refresh the BoneTransforms data.
        /// </summary>
        public void UpdateBoneTransforms(TimeSpan time, bool relativeToCurrentTime)
        {
            if (currentClipValue == null)
            {
                throw new InvalidOperationException("AnimationPlayer.Update was called before StartClip");
            }

            if (mixing && secondClipValue == null)
            {
                throw new InvalidOperationException("AnimationPlayer.Update was supposed to mix two animations, but secondClipValue is null");
            }

            TimeSpan secondTime = time;

            UpdateCurrentTime(time, relativeToCurrentTime);

            if (mixing)
            {
                UpdateSecondTime(secondTime, relativeToCurrentTime);
            }

            // Read keyframe matrices.
            IList<Keyframe> keyframes = currentClipValue.Keyframes;


            while (currentKeyframe < keyframes.Count)
            {
                Keyframe keyframe = keyframes[currentKeyframe];

                // Stop when we've read up to the current time position.
                if (keyframe.Time > currentTimeValue)
                    break;

                // Use this keyframe.
                Matrix transform = keyframe.Transform;
                boneTransforms[keyframe.Bone] = transform;
                currentKeyframe++;
            }

            if (mixing)
            {
                IList<Keyframe> secondKeyframes = secondClipValue.Keyframes;
                while (secondKeyframe < secondKeyframes.Count)
                {
                    Keyframe keyframe = secondKeyframes[secondKeyframe];

                    // Stop when we've read up to the current time position.
                    if (keyframe.Time > secondTimeValue)
                        break;

                    // Use this keyframe.
                    Matrix transform = keyframe.Transform;
                    float dur=(float)(secondClipValue.Duration.TotalMilliseconds);
                    float cur=(float)(secondTimeValue.TotalMilliseconds);
                    bool lerp = bonesToIgnore == null;
                    if (lerp || !bonesToIgnore.Contains(keyframe.Bone))
                    {
                        boneTransforms[keyframe.Bone] = Matrix.Lerp(boneTransforms[keyframe.Bone], transform, (float)((dur - cur) / dur));
                    }
                    secondKeyframe++;
                }
            }
        }


        /// <summary>
        /// Helper used by the Update method to refresh the WorldTransforms data.
        /// </summary>
        public void UpdateWorldTransforms(Matrix rootTransform)
        {
            // Root bone.
            worldTransforms[0] = boneTransforms[0] * rootTransform;

            // Child bones.
            for (int bone = 1; bone < worldTransforms.Length; bone++)
            {
                int parentBone = skinningDataValue.SkeletonHierarchy[bone];

                worldTransforms[bone] = boneTransforms[bone] *
                                             worldTransforms[parentBone];
            }
        }


        /// <summary>
        /// Helper used by the Update method to refresh the SkinTransforms data.
        /// </summary>
        public void UpdateSkinTransforms()
        {
            for (int bone = 0; bone < skinTransforms.Length; bone++)
            {
                skinTransforms[bone] = skinningDataValue.InverseBindPose[bone] *
                                            worldTransforms[bone];
            }
        }


        /// <summary>
        /// Gets the current bone transform matrices, relative to their parent bones.
        /// </summary>
        public Matrix[] GetBoneTransforms()
        {
            return boneTransforms;
        }


        /// <summary>
        /// Gets the current bone transform matrices, in absolute format.
        /// </summary>
        public Matrix[] GetWorldTransforms()
        {
            return worldTransforms;
        }


        /// <summary>
        /// Gets the current bone transform matrices,
        /// relative to the skinning bind pose.
        /// </summary>
        public Matrix[] GetSkinTransforms()
        {
            return skinTransforms;
        }


        /// <summary>
        /// Gets the clip currently being decoded.
        /// </summary>
        public AnimationClip CurrentClip
        {
            get { return currentClipValue; }
        }


        /// <summary>
        /// Gets the current play position.
        /// </summary>
        public TimeSpan CurrentTime
        {
            get { return currentTimeValue; }
        }
    }
}
