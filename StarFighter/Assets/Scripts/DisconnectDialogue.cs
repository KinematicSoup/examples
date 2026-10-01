using UnityEngine;
using System.Collections;

public class DisconnectDialogue : MonoBehaviour 
{
	public void Button_OK() 
    {
        LevelManager.Load(LevelManager.Level.MAIN_MENU);
	}
}
