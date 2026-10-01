using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KS.Reactor.Client.Unity;
using KS.Reactor;

// Controller for camera/player rotation.
public class FirstPersonCamera : MonoBehaviour
{
    public float Sensitivity = 1f;
    // The offset to add to the target object's position to get the camera position. The y value should match the
    // seCharacter GunOffsetY.
    public Vector3 Offset;
    public Vector3 RotatedOffset;
    // Max angle in degrees players can look up or down.
    public float MaxPitch = 75f;

    // The target object to follow.
    public static Transform Target;
    public static FPSController Controller;

    public static float PitchAxis;
    public static float YawAxis;

    public void Update()
    {
        if (Target == null)
        {
            return;
        }
        if (Cursor.lockState != CursorLockMode.Locked || Controller == null)
        {
            transform.position = Target.position + Offset + transform.rotation * RotatedOffset;
            return;
        }
        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        mouseDelta *= Sensitivity * Config.Instance.MouseSensitivity;
        if (Config.Instance.InvertY)
        {
            mouseDelta.y *= -1f;
        }
        if (Config.Instance.InvertX)
        {
            mouseDelta.x *= -1f;
        }
        Vector3 eulers = transform.eulerAngles;
        eulers.y += mouseDelta.x;
        eulers.y = new ksRange(-180, 180).Wrap(eulers.y);
        if (eulers.x > 180f)
        {
            eulers.x -= 360f;
        }
        eulers.x -= mouseDelta.y;
        eulers.x = Math.Max(-Controller.MaxPitch, eulers.x);
        eulers.x = Math.Min(Controller.MaxPitch, eulers.x);
        // Send yaw and pitch to server as input axes. Axis values must be in the range -1 to 1.
        ksReactor.InputManager.SetAxis(Axes.YAW, eulers.y / 180f);
        ksReactor.InputManager.SetAxis(Axes.PITCH, -eulers.x / Controller.MaxPitch);
        YawAxis = eulers.y / 180f;
        PitchAxis = -eulers.x / Controller.MaxPitch;
        if (eulers.x < 0f)
        {
            eulers.x += 360f;
        }
        transform.eulerAngles = eulers;
        transform.position = Target.position + Offset + transform.rotation * RotatedOffset;
    }
}