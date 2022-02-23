using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JTools
{
    public abstract class ImpactComponent : MonoBehaviour
    {
        [HideInInspector]
        public ImpactController owner; //This is implemented to allow components to have individual players associated with them. It mainly allows developers to create local multiplayer games without worrying about component conflicts.

        [HideInInspector]
        public bool initialized = false; //Determines whether or not Rush has already initialized a component. Makes it possible to swap components out at runtime without needing any special methods to account for change.

        public virtual void ComponentInitialize(ImpactController player)
        {
            owner = player;

            //Impact runs this method whenever a component begins receiving updates from the controller for the first time.
            //There's no need to adjust the initialized variable here, since Impact manages initialization of components from its own script.
            //Nontheless, intialized components NEED to organize their owner when activated, that way everything works as expected.
        }

        public virtual void ComponentEarlyUpdate(ImpactController player)
        {
            //This makes it possible for some components to have behavior that's guaranteed to run before other components.
        }

        public virtual void ComponentUpdate(ImpactController player)
        {
            //This is run every tick by Impact.
        }

        public virtual void ComponentLateUpdate(ImpactController player)
        {
            //Primarily used for components that need to wait to analyze others.
        }

        public virtual void ComponentFixedUpdate(ImpactController player)
        {
            //Typically important if a component needs to manipulate data related to physics.
            //This can be seen in Impact's default motion component.
        }
    }
}