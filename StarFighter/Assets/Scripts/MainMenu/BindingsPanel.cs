using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/*
 * Displays a control binding in the main menu
 * p button represents the pc controls display, while j is the joystick control
 */
public class BindingsPanel : MonoBehaviour 
{
    private Transform m_buttonP;
    private Transform m_buttonJ;
    private InputButton m_button;
    private InputAxis m_axis;
    private MainMenu m_menu;

    void Awake()
    {
        m_buttonP = transform.Find("Button_PC");
        m_buttonJ = transform.Find("Button_Joystick");
    }

    public Button GetPButton()
    {
        return m_buttonP.GetComponent<Button>();
    }

    public Button GetJButton()
    {
        return m_buttonJ.GetComponent<Button>();
    }

    /*
     * builds the navigation mappings between the selectable elements
     */
    public void SetNavigation(BindingsPanel panelAbove, BindingsPanel panelBelow, Scrollbar scrollbar, Button reloadDefaults, MainMenu menu)
    {
        m_menu = menu;

        Navigation pNav = new Navigation();
        Navigation jNav = new Navigation();

        pNav.mode = Navigation.Mode.Explicit;
        jNav.mode = Navigation.Mode.Explicit;

        if (panelAbove)
        {
            pNav.selectOnUp = panelAbove.GetPButton();
            jNav.selectOnUp = panelAbove.GetJButton();
        }
        if (panelBelow)
        {
            pNav.selectOnDown = panelBelow.GetPButton();
            jNav.selectOnDown = panelBelow.GetJButton();
        }
        else
        {
            pNav.selectOnDown = reloadDefaults;
            jNav.selectOnDown = reloadDefaults;
        }

        pNav.selectOnRight = GetJButton();
        jNav.selectOnLeft = GetPButton();
        jNav.selectOnRight = scrollbar;

        m_buttonP.GetComponent<Button>().navigation = pNav;
        m_buttonJ.GetComponent<Button>().navigation = jNav;
    }

    public void Initialize(InputButton button)
    {
        GetComponentInChildren<Text>().text = button.Name;
        m_buttonP.GetComponentInChildren<Text>().text = button.PcButton.ToString();
        m_buttonJ.GetComponentInChildren<Text>().text = button.JoystickButton.Name;
        m_button = button;
    }

    public void Initialize(InputAxis axis)
    {
        GetComponentInChildren<Text>().text = axis.Name;
        m_buttonP.GetComponentInChildren<Text>().text = axis.MouseAxis.Name;
        m_buttonJ.GetComponentInChildren<Text>().text = axis.JoystickAxis.Name;
        m_axis = axis;
    }

    public void Button_Binding()
    {
        m_menu.OpenRebindMenu(m_button, m_axis);
    }
}
