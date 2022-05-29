using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPS
{
    public class GameManager : MonoBehaviour
    {
        static GameManager m_instance;

        [SerializeField] PlayerController m_player;
        [SerializeField] EnemyManager m_enemyManager;

        #region Properties

        public static GameManager Instance => m_instance;

        public PlayerController Player => m_player;

        #endregion



        private void Awake()
        {
            if (m_instance == null)
            {
                m_instance = this;
            }
            else
            {
                if (m_instance != this)
                {
                    Destroy(gameObject);
                }
            }
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
