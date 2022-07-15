
//Impact Player Controller Developed by John Ellis, 2021.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JTools
{
    public class ImpactController : MonoBehaviour
    {

        /// <summary>
        /// This is only active if there's at least one controller who has "assignCurrent" set to true.
        /// </summary>
        public static ImpactController current; //Just call ImpactController.current to access the current player's variables from any other script!

        public bool assignCurrent = true; //Determines whether or not the "current" controller should be assigned to. Impact controller components don't rely on a singleton to function, but in many games singletons can be helpful. It's up to your discretion.

        [Header("Impact - General")]
        [Tooltip("A direct reference to the player's camera.")] public Camera playerCamera;
        [Tooltip("I strongly advise creating an empty gameobject rooted to the player, and assigning it to this. This is used to manage the player's body in third person.")] public Transform playerArtRoot;
        [Min(0f)] public float playerRadius = 0.3f;
        [Min(0f)] public float playerHeight = 1.64f;
        [Space]
        public ImpactComponent_Input inputComponent;
        public ImpactComponent_Camera cameraComponent;
        public ImpactComponent_Motion motionComponent;
        [Space]
        public List<ImpactComponent_Addon> addonComponents;
        [Space]
        [Tooltip("Whether or not the game should have its framerate locked.")] public bool lockFramerate = true;
        [Tooltip("The framerate the game will be locked at whenever a player is spawned in. Requires lockFramerate to be active first.")] [Range(1, 60)] public int frameRate = 60;


        [HideInInspector]
        public CapsuleCollider capsuleCollider; //Reference to the capsule collider component, cached for performance reasons.
        [HideInInspector]
        public AudioSource soundComponent; //Reference to the player's audiosource component.
        ///

        void Start()
        {

            if (lockFramerate)
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = frameRate;
            }
            else
            {
                Application.targetFrameRate = -1;
            }

            ///Singleton Access
            if (assignCurrent)
                current = this; //The current player controller is assigned so you can access it whenever you need to.

            //The controller's sound component is setup here. This allows it to actually make noises when moving around. You can also just add an audiosource yourself if you need to configure the settings in any special way!
            soundComponent = (GetComponent<AudioSource>() != null) ? GetComponent<AudioSource>() : gameObject.AddComponent<AudioSource>();
            soundComponent.spatialBlend = 0f;
         
            Cursor.lockState = CursorLockMode.Locked; //Cursor is locked.
            Cursor.visible = false; //Cursor is hidden.
            
            //Capsule is added manually, this way the settings can be perfect every time without relying on the user.
            capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
            capsuleCollider.radius = playerRadius;
            capsuleCollider.height = playerHeight;
            capsuleCollider.center = Vector3.up * playerHeight * 0.5f;

            PhysicMaterial m_phy = new PhysicMaterial
            {
                dynamicFriction = 0f,
                staticFriction = 0f,
                frictionCombine = PhysicMaterialCombine.Minimum

            };

            capsuleCollider.material = m_phy;

            if (inputComponent != null)
            {
                if (!inputComponent.initialized)
                {
                    inputComponent.ComponentInitialize(this);
                    inputComponent.initialized = true;
                }
            }


            if (motionComponent != null)
            {
                if (!motionComponent.initialized)
                {
                    motionComponent.ComponentInitialize(this);
                    motionComponent.initialized = true;
                }
            }

            if (cameraComponent != null)
            {
                if (!cameraComponent.initialized)
                {
                    cameraComponent.ComponentInitialize(this);
                    cameraComponent.initialized = true;
                }
            }

            for (int i = 0; i < addonComponents.Count; i++)
            {
                if (addonComponents[i] != null)
                {
                    addonComponents[i].ComponentInitialize(this);
                    addonComponents[i].initialized = true;
                }
            }

        }

        void Update()
        {


            if (inputComponent != null)
            {
                if (!inputComponent.initialized)
                {
                    inputComponent.ComponentInitialize(this);
                    inputComponent.initialized = true;
                }
            }

            if (motionComponent != null)
            {
                if (!motionComponent.initialized)
                {
                    motionComponent.ComponentInitialize(this);
                    motionComponent.initialized = true;
                }
            }

            if (cameraComponent != null)
            {
                if (!cameraComponent.initialized)
                {
                    cameraComponent.ComponentInitialize(this);
                    cameraComponent.initialized = true;
                }
            }

            if (inputComponent != null)
                inputComponent.ComponentEarlyUpdate(this);

            if (motionComponent != null)
                motionComponent.ComponentEarlyUpdate(this);

            if (cameraComponent != null)
                cameraComponent.ComponentEarlyUpdate(this);

            for (int i = 0; i < addonComponents.Count; i++)
            {
                if (addonComponents[i] != null)
                {
                    if (!addonComponents[i].initialized)
                    {
                        addonComponents[i].ComponentInitialize(this);
                        addonComponents[i].initialized = true;
                    }

                    addonComponents[i].ComponentEarlyUpdate(this);
                }
            }

            if (inputComponent != null)
                inputComponent.ComponentUpdate(this);

            if (motionComponent != null)
                motionComponent.ComponentUpdate(this);

            if (cameraComponent != null)
                cameraComponent.ComponentUpdate(this);

            for (int i = 0; i < addonComponents.Count; i++)
            {
                if (addonComponents[i] != null)
                {
                    addonComponents[i].ComponentUpdate(this);
                }
            }

        }

        private void LateUpdate()
        {

            if (inputComponent != null)
                inputComponent.ComponentLateUpdate(this);

            if (motionComponent != null)
                motionComponent.ComponentLateUpdate(this);

            if (cameraComponent != null)
                cameraComponent.ComponentLateUpdate(this);

            for (int i = 0; i < addonComponents.Count; i++)
            {
                if (addonComponents[i] != null)
                {
                    addonComponents[i].ComponentLateUpdate(this);
                }
            }


        }

        void FixedUpdate()
        {

            if (inputComponent != null)
                inputComponent.ComponentFixedUpdate(this);

            if (motionComponent != null)
                motionComponent.ComponentFixedUpdate(this);

            if (cameraComponent != null)
                cameraComponent.ComponentFixedUpdate(this);

            for (int i = 0; i < addonComponents.Count; i++)
            {
                if (addonComponents[i] != null)
                {
                    addonComponents[i].ComponentFixedUpdate(this);
                }
            }

        }

        public static float SmoothUp(float evaluate)
        {
            return (evaluate < 1f) ? Mathf.Log10((Mathf.Clamp01(evaluate) + 0.1f) * 10f) * 0.96f : 1f;
        }

        public static Vector3 RLerp(Vector3 a, Vector3 b, float t)
        {
            t = 1f - Mathf.Pow(1f - t, Time.unscaledDeltaTime * 60f);

            return (Vector3.Distance(a, b) > 0.001f) ? (a + (b - a) * t) : b;
        }

        public static float RLerp(float a, float b, float t)
        {
            t = 1f - Mathf.Pow(1f - t, Time.unscaledDeltaTime * 60f);

            return (Mathf.Abs(a - b) > 0.001f) ? (a + (b - a) * t) : b;
        }

        /*
        public void Teleport(Vector3 position)
        {
            transform.position = position;

            switch (playerMode)
            {
                case (RushMode.thirdPerson):
                    Vector3 m_camPos = Vector3.zero;

                    m_camPos = transform.position + ((m_thirdPersonRotation * -Vector3.forward) * thirdPersonOrbitDistance) + movement * Time.fixedDeltaTime;

                    if (Physics.SphereCast(transform.position, 0.5f, (m_camPos - transform.position).normalized, out m_hit, thirdPersonOrbitDistance, groundingLayers, QueryTriggerInteraction.Ignore))
                    {

                        if (rigidbodyOcclusion)
                        {
                            m_camPos = m_hit.point + m_hit.normal * 0.1f;
                        }
                        else
                        {
                            if (m_hit.collider.GetComponent<Rigidbody>() != null)
                            {
                                if (m_hit.collider.GetComponent<Rigidbody>().isKinematic)
                                    m_camPos = m_hit.point + m_hit.normal * 0.1f;
                            }
                            else
                                m_camPos = m_hit.point + m_hit.normal * 0.1f;
                        }

                    }

                    playerCamera.transform.position = m_camPos;

                    break;
            }
        }*/


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + Vector3.up * playerHeight * 0.5f, new Vector3(playerRadius * 2f, playerHeight, playerRadius * 2f));
            Gizmos.DrawLine(transform.position + Vector3.up * playerHeight * 0.5f, (transform.position + Vector3.up * playerHeight * 0.5f) - Vector3.up * playerHeight * 0.66f);
        }

    }

}
