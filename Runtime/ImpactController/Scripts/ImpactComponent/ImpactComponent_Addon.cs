using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JTools
{
    public abstract class ImpactComponent_Addon : ImpactComponent
    {
        //Addons have the unique property of being the only Impact controller components a player can have multiple of.
        //Other components for movement and camera controls are restricted for a variety of reasons, mainly it helps to prevent conflicts, and it also makes it easier to work using singleton access, since you only have to expect one type of a given component.

        //Addons include behavior such as menu control, pausing, view bobbing and more.

        public void Reset()
        {
            if (GetComponent<ImpactController>())
            {
                if (!GetComponent<ImpactController>().addonComponents.Contains(this))
                {
                    GetComponent<ImpactController>().addonComponents.Add(this);
                }
            }
        }

    }
}