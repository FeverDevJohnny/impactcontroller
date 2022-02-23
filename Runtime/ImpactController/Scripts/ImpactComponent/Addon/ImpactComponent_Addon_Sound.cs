using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JTools
{
    public class ImpactComponent_Addon_Sound : ImpactComponent_Addon
    {
        [Tooltip("Whether or not sounds will play from the player.")] public bool enableSounds = true;
        [Tooltip("How loud the player's sounds are.")] [Range(0f, 1f)] public float soundVolume = 1f;
        [Space]
        [Tooltip("The sound that plays whenever the player walks. The rate this plays at is scaled based on speed.")] public AudioClip[] walkSounds;
        [Tooltip("The sound that plays whenever the player jumps.")] public AudioClip jumpingSound;
        [Tooltip("The sound that plays whenever the player lands on the ground.")] public AudioClip landingSound;
      
        [HideInInspector]
        public bool overrideFootsteps = false; //This can be used by other scripts to tell a sound component that it doesn't need to run the OnPlayerStep events.

        public bool validStepping => owner.motionComponent.isGrounded || owner.motionComponent.isSliding;

        public override void ComponentInitialize(ImpactController player)
        {
            base.ComponentInitialize(player);

            player.motionComponent.onJump.AddListener(OnPlayerJump);
            player.motionComponent.onLanding.AddListener(OnPlayerLanding);
            player.cameraComponent.onStep.AddListener(OnPlayerStep);
        }

        public void OnPlayerLanding(float intensity)
        {
            if (enableSounds)
            {
                owner.soundComponent.PlayOneShot(landingSound, soundVolume); //If we're allowed to play sounds on landing, we do it here. The timer is reset to prevent spamming.
            }

        }

        public void OnPlayerStep()
        {
            if (enableSounds && !overrideFootsteps)
            {
                PlayStepSound();
            }
        }

        public void PlayStepSound()
        {
            if (validStepping)
            owner.soundComponent.PlayOneShot(walkSounds[Random.Range(0, walkSounds.Length)], soundVolume);
        }

        public void OnPlayerJump()
        {
            if (enableSounds)
                owner.soundComponent.PlayOneShot(jumpingSound, soundVolume);
        }

    }
}