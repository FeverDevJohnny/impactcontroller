using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JTools
{
    public class ImpactComponent_Addon_Animation : ImpactComponent_Addon
    {

        [Tooltip("^ Leave this unassigned to ignore animations. ^\n\nExplanation of used animator parameters below:")] public Animator playerAnimator; //This is important, please read the tooltip for a comprehensive list of the animator parameters used.
        [Space]
        [Tooltip("^ Leave this empty to ignore parameter.^\n\nWalking: Boolean, whether or not the player is moving.")] public string walkingParameter = string.Empty;
        [Tooltip("^ Leave this empty to ignore parameter.^\n\nSprinting: Boolean, whether or not sprinting is active.")] public string sprintingParameter = string.Empty;
        [Tooltip("^ Leave this empty to ignore parameter.^\n\nCrouching: Boolean, whether or not the player is crouching.")] public string crouchingParameter = string.Empty;
        [Tooltip("^ Leave this empty to ignore parameter.^\n\nGrounded: Boolean, whether or not the player is on the ground.")] public string groundedParamter = string.Empty;
        [Tooltip("^ Leave this empty to ignore parameter.^\n\nRelativeSpeed: Float, a percentage comparing the player's current speed to the normal walk speed. Used either for smooth animation blending or directly adjusting the speed of a walking animation.")] public string relativeSpeedParameter = string.Empty;
        [Space]
        [Tooltip("^ Leave this empty to ignore weight.^\nQuick explanation: This uses a layer name instead of a parameter. Please be careful!\nCrouching Percent: Float, scales from 0 to 1, representing how crouched the player is.")] public string crouchingWeight = string.Empty;
        [Space]
        [Tooltip("In circumstances where you would rather have your animator manage step sounds via animation events, you can mark this true.")] public bool overrideFootsteps = false;

        public override void ComponentLateUpdate(ImpactController player)
        {
            base.ComponentLateUpdate(player);

            ImpactComponent_Addon_Sound m_snd = player.GetComponent<ImpactComponent_Addon_Sound>();
            if (m_snd != null)
                m_snd.overrideFootsteps = overrideFootsteps;

            if (playerAnimator != null)
            {
                if (player.motionComponent.crouchMode != ImpactMotion_CrouchSetting.none && playerAnimator != null && crouchingWeight != string.Empty)
                    playerAnimator.SetLayerWeight(playerAnimator.GetLayerIndex(crouchingWeight), player.motionComponent.crouchTime);
                if (walkingParameter != string.Empty)
                    playerAnimator.SetBool(walkingParameter, Vector3.ProjectOnPlane(player.inputComponent.inputData.motionInput, Vector3.up).magnitude > 0.2f);
                if (sprintingParameter != string.Empty)
                    playerAnimator.SetBool(sprintingParameter, player.motionComponent.isSprinting);
                if (crouchingParameter != string.Empty)
                    playerAnimator.SetBool(crouchingParameter, player.motionComponent.isCrouching);
                if (groundedParamter != string.Empty)
                    playerAnimator.SetBool(groundedParamter, player.motionComponent.isGrounded);
                if (relativeSpeedParameter != string.Empty && player.motionComponent.moveSpeed != 0f)
                    playerAnimator.SetFloat(relativeSpeedParameter, player.motionComponent.topSpeed / player.motionComponent.moveSpeed);
            }
        }

    }
}