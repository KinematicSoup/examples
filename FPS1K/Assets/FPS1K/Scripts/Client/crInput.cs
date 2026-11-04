using KS.Reactor.Client.Unity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


public class crInput : ksRoomScript
{
    public InputActionReference Move;
    public InputActionReference Shoot;
    public InputActionReference Jump;
    public InputActionReference Dash;

    private void Update()
    {
        if (Move != null)
        {
            Vector2 move = Move.action.ReadValue<Vector2>();
            ksReactor.InputManager.SetAxis(Axes.X, move.x);
            ksReactor.InputManager.SetAxis(Axes.Z, move.y);
        }
        if (Shoot != null)
        {
            ksReactor.InputManager.SetButton(Buttons.SHOOT, Shoot.action.IsPressed() && ValidateMouseInput());
        }
        if (Jump != null)
        {
            ksReactor.InputManager.SetButton(Buttons.JUMP, Jump.action.IsPressed());
        }
        if (Dash != null)
        {
            ksReactor.InputManager.SetButton(Buttons.DASH, Dash.action.IsPressed());
        }
    }

    // Prevent shooting when the player is clicking on UI.
    private bool ValidateMouseInput()
    {
        return EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject();
    }
}
