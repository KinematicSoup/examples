using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using KS.Reactor.Client.Unity;
using KS.Reactor.Client;
using KS.Reactor;

// Attach this to one entity in each tile prefab (usually the ground) to assign a color for that tile in the minimap.
public class ceMinimapColor : ksEntityScript
{
    public Color Color;

    // Called after properties are initialized.
    public override void Initialize()
    {
        // The platform is positioned in the corner of the grid, so offset position by rotation to get in the cell.
        Hud.Instance.Minimap.SetColor(transform.position + transform.rotation * new Vector3(1f, 0f, 1f), Color);   
    }
}