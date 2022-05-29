using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FPS
{
    public class GameManager : MonoBehaviour
    {
        static GameManager m_instance;

        [SerializeField] TimeRecorder m_timeRecorder;
        [SerializeField] PlayerController m_player;
        [SerializeField] EnemyManager m_enemyManager;

        [Header("UI")]
        [SerializeField] Text m_recordTime;
        [SerializeField] Text m_bestTime;

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

            m_timeRecorder.StartRecording((time) =>
            {
                int second = Mathf.FloorToInt(time);

                m_recordTime.text = string.Format("{0}:{1}", (second / 60).ToString("00"), (second % 60).ToString("00"));
            });
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
