using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace NimbleGames.BackgroundAssets
{
    public class SwitchCameraButton : MonoBehaviour
    {
        [SerializeField] private Camera m_FixedCam;

        public void ToggleFixedCam()
        {
            m_FixedCam.enabled = !m_FixedCam.enabled;
        }
    }
}