using System.IO;
using System.Linq;
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

        [Header("FX")]
        [SerializeField] ParticleSystem m_particlePrefab;
        [SerializeField] List<ParticleSystem> m_particlePooling = new List<ParticleSystem>();

        [Header("UI")]
        [SerializeField] Text m_recordTime;
        [SerializeField] Text m_bestTime;
        [SerializeField] RectTransform m_startPanel;
        [SerializeField] RectTransform m_completePanel;

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

            Player.enabled = false;

            LoadRecord();

            m_startPanel.gameObject.SetActive(true);
            m_completePanel.gameObject.SetActive(false);
        }

        #region Game start and end.

        public void GameStart()
        {
            m_startPanel.gameObject.SetActive(false);

            Player.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;

            m_timeRecorder.StartRecording((time) =>
            {
                int second = Mathf.FloorToInt(time);

                m_recordTime.text = string.Format("{0}:{1}", (second / 60).ToString("00"), (second % 60).ToString("00"));
            });

            m_enemyManager.EnemyCountChanged.AddListener((count) =>
            { 
                if (count == 0)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Player.enabled = false;

                    m_timeRecorder.StopRecording();
                    SaveRecord();

                    m_completePanel.gameObject.SetActive(true);
                }
            });
        }

        #endregion

        #region Utilities

        public void ShotCollision(Ray ray, float distance, int damage)
        {
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, distance))
            {
                Debug.Log(hit.transform.tag);

                if (hit.transform.tag == "Player")
                {
                    hit.transform.GetComponent<Health>().OnDamaged(damage);
                }
                else if (hit.transform.tag == "Enemy")
                {
                    m_enemyManager.DamagedEnemy(hit.transform.GetComponent<Enemy>(), hit, damage);
                }
                else
                {
                    Vector3 position = hit.point;
                    Quaternion rotation = Quaternion.LookRotation(hit.normal);

                    PlayVFX(m_particlePrefab, m_particlePooling, transform, position, rotation);
                }
            }
        }

        public void PlayVFX(ParticleSystem origin, List<ParticleSystem> pooling, Transform parent, Vector3 localPosition, Quaternion localRotation)
        {
            if (m_particlePrefab == null) return;

            ParticleSystem currentParticle = null;

            foreach (var particle in pooling)
            {
                if (!particle.isPlaying)
                {
                    currentParticle = particle;

                    break;
                }
            }

            if (currentParticle == null)
            {
                currentParticle = Instantiate(origin, parent);
                pooling.Add(currentParticle);
            }

            currentParticle.transform.localPosition = localPosition;
            currentParticle.transform.localRotation = localRotation;
            currentParticle.Play();
        }

        public void OnClickRestart()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }

        #endregion

        #region Record

        private void LoadRecord()
        {
            string path = Path.Combine(Application.persistentDataPath, "Recording.json");

            if (File.Exists(path))
            {
                Recording recording = JsonUtility.FromJson<Recording>(File.ReadAllText(path));

                int m = recording.second / 60;
                int s = recording.second % 60;

                m_bestTime.text = string.Format("{0}:{1}", m.ToString("00"), s.ToString("00"));
            }
        }

        private void SaveRecord()
        {
            string path = Path.Combine(Application.persistentDataPath, "Recording.json");

            Recording recording = new Recording();
            recording.time = System.DateTime.Now.ToString();
            recording.second = Mathf.FloorToInt(m_timeRecorder.RecordingTime);

            if (File.Exists(path))
            {
                Recording oldRecording = JsonUtility.FromJson<Recording>(File.ReadAllText(path));

                if (oldRecording.second < recording.second)
                {
                    recording = oldRecording;
                }
            }

            File.WriteAllText(path, JsonUtility.ToJson(recording));

            int m = recording.second / 60;
            int s = recording.second % 60;

            m_bestTime.text = string.Format("{0}:{1}", m.ToString("00"), s.ToString("00"));
        }

        #endregion
    }
}
