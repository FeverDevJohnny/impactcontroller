using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace JTools
{
    public abstract class ImpactComponent_Camera : ImpactComponent
    {
        //OUTPUTS
        [HideInInspector] public Vector3 cameraAngles = Vector3.zero; //The direction the camera is looking in.
        [HideInInspector] public Vector3 cameraOrigin = Vector3.zero; //Where the camera should be normally. Helpful for several effects (such as view bobbing and so on).
        [HideInInspector] public float baseFOV = -1f; //The base FOV used in field of view calculations. Change this to modify the camera's zoom. Defaults to -1f, since camera components are expected to assign this on intiialization.

        [HideInInspector]
        public UnityEvent onStep;

        //INPUTS
        [HideInInspector] public bool restrainCameraRotating = false; //Works in tandem with DisengageCameraRotating. Please be careful not to mix this up with SetLookTarget, which works independently of this system.
        [HideInInspector] public bool restrainCameraPositioning = false; //Works in tandem with DisengageCameraPositioning. Please be careful not to mix this up with SetLookTarget, which works independently of this system.
        public float cameraSensitivity = 1f; //How sensitive the camera is. Organized inside of the camera core.
        public float screenshakeDecayFactor = 0.2f; //Increases the speed at which screenshake stops.
        [Tooltip("The angles at which the player's camera can look up and down. For technical reasons, third person mode completely ignores this and uses its own restraints.")] public Vector2 verticalRestraint = new Vector2(-90f, 90f);

        [HideInInspector]
        public float screenshake = 0f; //SHAKE IT

        [HideInInspector]
        public Transform lookTarget;


        public void Reset()
        {
            if (GetComponent<ImpactController>())
            {
                if (GetComponent<ImpactController>().cameraComponent == null)
                {
                    GetComponent<ImpactController>().cameraComponent = this;
                }
            }
        }

        public virtual void OnStairStep(Vector3 m_smoothDirectionVector)
        {
            //This is used with the motion component to manage how the camera should react to stepping up a stair. The vector3 provided is the direction and distance the player moved while stepping up the stair.
        }

        public override void ComponentUpdate(ImpactController player)
        {
            base.ComponentUpdate(player);

            screenshake = Mathf.Max(screenshake - Time.deltaTime * screenshakeDecayFactor, 0f);
        }


        /// <summary>
        /// Lets you define a look target for the camera to stare at. Pretty handy if you need to call certain things to the player's attention.
        /// </summary>
        /// <param name="target">Set this to null to disable look targeting, set it to an active object if you wanna track something.</param>
        public virtual void SetLookTarget(Transform target)
        {
            lookTarget = target;

            if(lookTarget == null)
            {
                Vector3 m_rot = owner.playerCamera.transform.rotation.eulerAngles;
                m_rot.z = 0f;

                if(m_rot.x < verticalRestraint.x)
                    m_rot.x = 360f + verticalRestraint.x;

                if (m_rot.x > verticalRestraint.y)
                    m_rot.x = 360f - verticalRestraint.x;

                cameraAngles = m_rot;
            }
        }

        /// <summary>
        /// Allows you to stop the camera from responding to mouse input for a bit. Mainly used in case you need to take explicit control of the camera's rotation.
        /// Setting this to false will automatically adjust this component's cameraAngles to match the current camera rotation.
        /// </summary>
        /// <param name="state"></param>
        public virtual void DisengageCameraRotating(bool state)
        {
            restrainCameraRotating = state;

            if(!state)
            { 
                Vector3 m_rot = owner.playerCamera.transform.rotation.eulerAngles;
                m_rot.z = 0f;

                if (m_rot.x < verticalRestraint.x)
                    m_rot.x = -(360f + verticalRestraint.x);

                if (m_rot.x > verticalRestraint.y)
                    m_rot.x = -(360f - verticalRestraint.x);

                cameraAngles = m_rot;
            }
        }

        /// <summary>
        /// Prevents the camera from being moved to its desired position. You are fully responsible for managing its position while this is true. Only use this feature if you need to navigate the camera around during like, a cutscene or something.
        /// </summary>
        /// <param name="state"></param>
        public virtual void DisengageCameraPositioning(bool state)
        {
            restrainCameraPositioning = state;
        }

        /// <summary>
        /// Sets the screenshake value to a given intensity. Not all camera components are guaranteed to utilize this method or the shaking system in general, but for scripting convenience it's here.
        /// </summary>
        /// <param name="intensity">How hard the screen should shake.</param>
        public virtual void ShakeScreen(float intensity)
        {
            screenshake = intensity;
        }
    }
}