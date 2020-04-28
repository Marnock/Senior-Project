using UnityEngine;
using System.Collections;

namespace ImmixKit
{
  
    public class LockCursor
    {
        public static bool lockCursor
        {
            get
            {
                if (Application.isMobilePlatform || Application.isConsolePlatform)
                {
                    return !Kit_IngameMain.isPauseMenuOpen;
                }
                else
                {
                    if (Cursor.lockState == CursorLockMode.Locked)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            set
            {
                if (value)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
        }
    }
}
