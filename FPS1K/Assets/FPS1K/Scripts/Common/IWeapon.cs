using System.Collections;
using System.Collections.Generic;
using KS.Reactor;

// Common interface for weapons that can be updated with player controller inputs.
public interface IWeapon
{
    void OnUpdate(ksInput input);
}
