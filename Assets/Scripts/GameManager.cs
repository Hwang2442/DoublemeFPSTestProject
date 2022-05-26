using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPS
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] PlayerController m_player;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
