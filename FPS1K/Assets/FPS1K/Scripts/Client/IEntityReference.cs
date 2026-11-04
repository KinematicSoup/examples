using System.Collections;
using System.Collections.Generic;
using KS.Reactor.Client.Unity;

// Interface that holds a reference to an entity.
public interface IEntityReference
{
    ksEntity Entity { get; set; }
}
