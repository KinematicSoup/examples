using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using KS.Reactor;

// Common utility functions
public static class Utils
{
    public static ksVector3 CalculateVelocityFromRotation(ksSweepResult hit, ksVector3 angularVelocity)
    {
        if (angularVelocity == ksVector3.Zero)
        {
            return ksVector3.Zero;
        }
        float dt = 1f / 60f;
        ksVector3 start = hit.Point - hit.Entity.Transform.Position;
        ksQuaternion rotation = ksQuaternion.FromAngularDisplacement(angularVelocity * dt);
        ksVector3 end = start * rotation;
        return (end - start) / dt;
    }

    // Converts the quantized int array aim property to a rotation.
    public static ksQuaternion GetAimRotation(ksMultiType aim)
    {
        return GetAimRotation(Dequantize(aim));
    }

    // Converts the vector2 aim property to a rotation.
    public static ksQuaternion GetAimRotation(ksVector2 aim)
    {
        return ksQuaternion.FromEuler(new ksVector3(-aim.X, aim.Y, 0f));
    }

    // Converts the quantized int array aim property to a direction.
    public static ksVector3 GetAimDirection(ksMultiType aim)
    {
        return GetAimDirection(Dequantize(aim));
    }

    // Converts the vector2 aim property to a direction.
    public static ksVector3 GetAimDirection(ksVector2 aim)
    {
        return ksVector3.Forward * GetAimRotation(aim);
    }

    private static ksVector2 Dequantize(int[] array)
    {
        if (array == null || array.Length < 2)
        {
            return ksVector2.Zero;
        }
        return new ksVector2(array[0], array[1]);
    }
}
