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
    }
}
