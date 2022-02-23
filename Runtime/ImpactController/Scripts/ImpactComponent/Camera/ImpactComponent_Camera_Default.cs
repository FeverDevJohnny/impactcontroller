using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JTools
{
    public class ImpactComponent_Camera_Default : ImpactComponent_Camera
    {
        [Header("Default - General")]
        [Tooltip("Makes it possible to smoothly interpolate between first and third person cameras. 0 means first person, 1 means third person.")] [Range(0f, 1f)] public float perspective = 0f;
        [Space]
        [Tooltip("Whether or not the player camera should tilt when you move. Used commonly in fancy-shmancy FPS games. Camera tilting won't take effect in third person mode for obvious reasons.")] public bool cameraTilting = false;
        [Tooltip("How much tilt we should apply when the player is moving. This variable affects how much the camera rolls.")] public float cameraTiltRollPower = 5f;
        [Tooltip("How much tilt we should apply when the player is moving. This variable affects how much the camera pitches. Commonly disabled because most camera tilting is based on strafe movement.")] public float cameraTiltPitchPower = 5f;
        [Space]
        [Tooltip("If this is enabled, then mouse movement will be accounted for when tilting the player's camera.")] public bool cameraMouseTilting = false;
        [Tooltip("How much tilt we should apply when the player is turning around. This variable affects how much the camera rolls.")] public float cameraMouseTiltRollPower = 2f;
        [Space]
        [Tooltip("How quickly the camera lerps to different tilting angles. 0.01 means it's incredibly slow, while 1 makes it instant.")] [Range(0.01f, 1f)] public float cameraTiltSpeed = 0.1f;
        [Range(0.1f, 1f)]
        [Tooltip("The lower this value is, the further down towards the ground the camera will drop when crouching. This specifically affects the player's camera height, and isn't related to collider height.")] public float cameraCrouchDrop = 1f;
        [Range(0.1f, 1f)]
        [Tooltip("Changes how quickly the camera orients itself to face look targets. For more details, check out ImpactComponent_Camera's SetLookTarget method.")] public float lookTargetTrackingSpeed = 0.2f;

        [Header("Default - Viewbob")]
        public float viewBobIntensity = 0.2f;
        [Range(0f, 1f)]
        [Tooltip("Defines when the view bob will run onStep, which is used in various components")] public float viewBobStep = 0.5f; //Defines when the onStep event is run by the script. This can vary since your viewbob curve isn't always guaranteed to "step down" at a consistent time.
        public AnimationCurve viewBobX = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));
        public AnimationCurve viewBobY = new AnimationCurve(new Keyframe[] { new Keyframe(0f, 0.5f), new Keyframe(0.5f, -0.5f), new Keyframe(1f, 0.5f) });
        public AnimationCurve viewBobZ = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

        [Header("Default - Third Person")]
        [Tooltip("Defines which layers will occlude the camera. Defaults to everything.")] public LayerMask cameraOccluders = ~0;
        [Tooltip("If true, the system will always bring the camera close to the player if something is blocking it.\n\nIf false, the system won't bring the camera in if a non-kinematic rigidbody is between the player and the camera.")] public bool rigidbodyOcclusion = true;
        [Tooltip("How far the third person camera should be from the player while in third person mode.")] public float thirdPersonOrbitDistance = 5f;

        [Header("Default - Zoom")]
        [Tooltip("How much the player's FOV changes while sprinting.")] public float sprintIntensity = 15f;
        [Tooltip("This value represents how many degrees the zoom changes. The higher this value is, the further in the camera will zoom.")] public float zoomIntensity = 30f;

        private Vector3 m_thirdPersonCamOutput; //The direction the camera faces in.
        private Vector3 m_camPosTracer; //Internal vector3 to lerp the camera's position with. Designed to smooth landing effects.
        private Vector3 m_camPosTracerSecondary; //For additional smoothing.

        private float m_zoomAdditive; //Added onto the current FOV value to decide how far the camera is zoomed in/out.
        private float m_zoomGoal; //The total amount of zoom m_zoomAdditive is trying to reach.

        float m_zoomLerp;

        private float m_cameraOriginBaseHeight;

        Vector2 m_cameraTilt;

        bool m_stepped = false;

        Vector2 thirdPersonClamp = new Vector2(-50f, 85f);

        RaycastHit m_hit; //A cached raycast for performance. The script recycles this often to avoid allocation issues.

        Vector3 m_stairSmoothing;

        public override void ComponentInitialize(ImpactController player)
        {
            base.ComponentInitialize(player);

            if (baseFOV < 0f)
                baseFOV = owner.playerCamera.fieldOfView;

            cameraAngles = new Vector3(owner.playerCamera.transform.rotation.eulerAngles.x, owner.playerCamera.transform.rotation.eulerAngles.y, 0f); //Goal angles are based on the player camera's rotation.
            cameraOrigin = owner.playerCamera.transform.localPosition; //The camera's origin is cached so bobbing and landing effects can be applied without the camera losing its default position.

            m_camPosTracer = cameraOrigin; //This is where the camera truly lies in a given frame.
            m_camPosTracerSecondary = m_camPosTracer;

            m_cameraOriginBaseHeight = cameraOrigin.y;

            owner.playerCamera.transform.SetParent(null);

            player.motionComponent.onLanding.AddListener((float value) => OnPlayerLanding(value));
        }

        public override void OnStairStep(Vector3 m_smoothDirectionVector)
        {
            base.OnStairStep(m_smoothDirectionVector);

            m_stairSmoothing += m_smoothDirectionVector;
        }

        public override void ComponentUpdate(ImpactController player)
        {
            base.ComponentUpdate(player);

            m_zoomLerp = ImpactController.RLerp(m_zoomLerp, (!player.inputComponent.inputData.holdingSecondary) ? 1f : 0f, 0.1f);

            if (lookTarget == null && !restrainCameraRotating)
                cameraAngles += new Vector3(-player.inputComponent.inputData.mouseInput.y * cameraSensitivity, player.inputComponent.inputData.mouseInput.x * cameraSensitivity);


            Vector2 clampGoal = Vector2.Lerp(verticalRestraint, thirdPersonClamp, perspective);

            cameraAngles.x = Mathf.Clamp(cameraAngles.x, clampGoal.x, clampGoal.y);

            Vector2 m_goalTilt;
            m_goalTilt = new Vector2(((cameraTilting) ? player.inputComponent.inputData.motionInput.x * cameraTiltRollPower : 0f) + ((cameraMouseTilting && lookTarget == null) ? player.inputComponent.inputData.mouseInput.x * cameraMouseTiltRollPower : 0f)
                , ((cameraTilting) ? player.inputComponent.inputData.motionInput.z * cameraTiltPitchPower : 0f));

            if (cameraTilting || cameraMouseTilting)
                m_cameraTilt = ImpactController.RLerp(m_cameraTilt, m_goalTilt, cameraTiltSpeed);
            else
                m_cameraTilt = ImpactController.RLerp(m_cameraTilt, Vector2.zero, cameraTiltSpeed);



            if (lookTarget == null)
            {
                if (!restrainCameraRotating)
                    player.playerCamera.transform.rotation = Quaternion.Euler(cameraAngles) * Quaternion.Euler(m_cameraTilt.y, 0f, -m_cameraTilt.x);
            }
            else
                player.playerCamera.transform.rotation = Quaternion.Slerp(player.playerCamera.transform.rotation, Quaternion.LookRotation((lookTarget.position - player.playerCamera.transform.position).normalized) * Quaternion.Euler(m_cameraTilt.y, 0f, -m_cameraTilt.x), lookTargetTrackingSpeed * 60f * Time.deltaTime);

            player.motionComponent.orientation = Quaternion.Euler(0f, player.playerCamera.transform.rotation.eulerAngles.y, 0f);



            m_stairSmoothing = ImpactController.RLerp(m_stairSmoothing, Vector3.zero, 0.1f);

            ///STAIR STEPPING


            // CAMERA MANAGEMENT

            cameraOrigin.y = m_cameraOriginBaseHeight * (player.capsuleCollider.height / player.playerHeight) * ((player.motionComponent.isCrouching) ? cameraCrouchDrop : 1f);


            if (!player.inputComponent.lockInput)
            {


                /// Dedicated to controlling viewbobbing and walkSounds.
                if (player.motionComponent.isGrounded)
                {
                    if (new Vector3(player.inputComponent.inputData.motionInput.x, 0f, player.inputComponent.inputData.motionInput.z).magnitude > 0.3f)
                    {
                        m_camPosTracer = ImpactController.RLerp(m_camPosTracer, cameraOrigin + Quaternion.Euler(0f, cameraAngles.y, 0f) * new Vector3(viewBobX.Evaluate(player.motionComponent.walkTime), viewBobY.Evaluate(player.motionComponent.walkTime), viewBobZ.Evaluate(player.motionComponent.walkTime)) * viewBobIntensity, 0.4f); //Bobbing effects.

                        if (player.motionComponent.walkTime > viewBobStep)
                        {
                            if (!m_stepped)
                            {
                                onStep.Invoke();
                                m_stepped = true;
                            }
                        }
                        else
                        {
                            m_stepped = false;
                        }

                    }
                    else
                        m_camPosTracer = ImpactController.RLerp(m_camPosTracer, cameraOrigin, 0.4f); //If the player isn't moving fast enough to be constituted as "walking", the camera returns to normal.
                }
                else
                    m_camPosTracer = ImpactController.RLerp(m_camPosTracer, cameraOrigin, 0.4f); //If the player isn't on the ground, the camera goes back to its default position.

            }

            if (player.playerArtRoot != null && player.motionComponent.smoothPlayerBodyStepping)
                player.playerArtRoot.localPosition = m_stairSmoothing;

            /// Zoom Feature. Fairly straightforward. Controlled with a lil' recursive linear interpolation formula.

            m_zoomGoal = 0f; //This is reset per-frame.

            if (!player.inputComponent.lockInput) //We first determine if we're zooming with the zoom button.
                if (player.inputComponent.inputData.holdingSecondary && zoomIntensity != 0f)
                    m_zoomGoal -= zoomIntensity;

            if (player.motionComponent.isSprinting && new Vector3(player.motionComponent.movement.x, player.motionComponent.movement.z).magnitude > 0.05f)
                m_zoomGoal += sprintIntensity;

            m_zoomAdditive = ImpactController.RLerp(m_zoomAdditive, m_zoomGoal, 0.2f);

            owner.playerCamera.fieldOfView = ImpactController.RLerp(owner.playerCamera.fieldOfView, (baseFOV + m_zoomAdditive), 0.2f);
        }

        public void OnPlayerLanding(float intensity)
        {
            m_camPosTracer -= Vector3.up * intensity * 0.2f; //If effects are enabled, the camera is bowed down for the landing.
            m_camPosTracer.y = Mathf.Clamp(m_camPosTracer.y, -4f, 4f); //This effect is clamped to prevent the camera from bowing too low.
        }

        public void OnDestroy()
        {
            if (owner.motionComponent != null)
                owner.motionComponent.onLanding.RemoveListener(OnPlayerLanding);
        }


        /// <summary>
        /// Lets you define a look target for the camera to stare at. Pretty handy if you need to call certain things to the player's attention.
        /// </summary>
        /// <param name="target">Set this to null to disable look targeting, set it to an active object if you wanna track something.</param>
        public override void SetLookTarget(Transform target)
        {
            lookTarget = target;

            if (lookTarget == null)
            {
                Vector3 m_rot = owner.playerCamera.transform.localRotation.eulerAngles;
                m_rot.z = 0f;

                Debug.Log(m_rot);

                if (m_rot.x < Vector2.Lerp(verticalRestraint, thirdPersonClamp, perspective).x)
                    m_rot.x = -(360f + m_rot.x);

                if (m_rot.x > Vector2.Lerp(verticalRestraint, thirdPersonClamp, perspective).y)
                    m_rot.x = -(360f - m_rot.x);

                cameraAngles = m_rot;
            }
        }


        public override void ComponentLateUpdate(ImpactController player)
        {
            base.ComponentLateUpdate(player);

            Vector3 m_camPos = Vector3.zero;

            if (lookTarget == null && !restrainCameraRotating)
            {
                m_camPos = (transform.position + Vector3.up * player.playerHeight * 0.5f) + (Quaternion.Euler(cameraAngles.x, cameraAngles.y, 0f) * Vector3.Lerp(new Vector3(0.5f, 0.5f, -thirdPersonOrbitDistance * 0.5f), new Vector3(0f, 0f, -thirdPersonOrbitDistance), m_zoomLerp));

                if (Physics.SphereCast(transform.position + Vector3.up * player.capsuleCollider.height * 0.5f, 0.15f, Quaternion.Euler(cameraAngles.x, cameraAngles.y, 0f) * Vector3.Lerp(new Vector3(0.5f, 0.5f, -thirdPersonOrbitDistance * 0.5f), new Vector3(0f, 0f, -thirdPersonOrbitDistance), m_zoomLerp).normalized, out m_hit, Mathf.Lerp(new Vector3(0.5f, 0.5f, -thirdPersonOrbitDistance * 0.5f).magnitude, thirdPersonOrbitDistance, m_zoomLerp), cameraOccluders, QueryTriggerInteraction.Ignore))
                {

                    if (rigidbodyOcclusion)
                    {
                        m_camPos = m_hit.point + m_hit.normal * 0.2f;
                    }
                    else
                    {
                        if (m_hit.collider.GetComponent<Rigidbody>() != null)
                        {
                            if (m_hit.collider.GetComponent<Rigidbody>().isKinematic)
                                m_camPos = m_hit.point + m_hit.normal * 0.2f;
                        }
                        else
                            m_camPos = m_hit.point + m_hit.normal * 0.2f;
                    }

                }
            }
            else
            {
                m_camPos = (transform.position + Vector3.up * player.playerHeight * 0.5f) + (player.playerCamera.transform.rotation * new Vector3(0f, 0f, -thirdPersonOrbitDistance));

                if (Physics.SphereCast(transform.position + Vector3.up * player.capsuleCollider.height * 0.5f, 0.15f, player.playerCamera.transform.rotation * -Vector3.forward, out m_hit, thirdPersonOrbitDistance, cameraOccluders, QueryTriggerInteraction.Ignore))
                {

                    if (rigidbodyOcclusion)
                    {
                        m_camPos = m_hit.point + m_hit.normal * 0.2f;
                    }
                    else
                    {
                        if (m_hit.collider.GetComponent<Rigidbody>() != null)
                        {
                            if (m_hit.collider.GetComponent<Rigidbody>().isKinematic)
                                m_camPos = m_hit.point + m_hit.normal * 0.2f;
                        }
                        else
                            m_camPos = m_hit.point + m_hit.normal * 0.2f;
                    }

                }
            }

            m_camPosTracerSecondary = ImpactController.RLerp(m_camPosTracerSecondary, m_camPosTracer, 0.1f);

            if (!restrainCameraPositioning)
                owner.playerCamera.transform.position = Vector3.Lerp(transform.position + m_camPosTracerSecondary, m_camPos, perspective) + m_stairSmoothing + Random.insideUnitSphere * screenshake;
        }
    }
}
