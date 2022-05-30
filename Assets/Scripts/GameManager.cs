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

            if (Player == null)
            {
                m_player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
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

            // 시간 기록 시작 및 UI 업데이트
            m_timeRecorder.StartRecording((time) =>
            {
                int second = Mathf.FloorToInt(time);

                m_recordTime.text = string.Format("{0}:{1}", (second / 60).ToString("00"), (second % 60).ToString("00"));
            });

            // 에너미 숫자 변동 시
            m_enemyManager.EnemyCountChanged.AddListener((count) =>
            { 
                // 에너미 모두 사망
                if (count == 0)
                {
                    GameComplete(true);
                }
            });

            // 플레이어가 죽을 경우
            Health playerHealth = Player.GetComponent<Health>();
            playerHealth.OnDamagedEvent.AddListener(() =>
            {
                if (playerHealth.curHP <= 0)
                { 
                    GameComplete(false);
                }
            });
        }

        #endregion

        private void GameComplete(bool clear)
        {
            Cursor.lockState = CursorLockMode.None;
            Player.enabled = false;

            m_timeRecorder.StopRecording();

            if (clear)
            {
                SaveRecord();
            }

            m_completePanel.gameObject.SetActive(true);
        }

        #region Utilities

        /// <summary>
        /// 플레이어 및 에너미 공격 관리
        /// </summary>
        /// <param name="ray">발사된 레이</param>
        /// <param name="distance">사정거리</param>
        /// <param name="damage">데미지</param>
        public void ShotCollision(Ray ray, float distance, int damage)
        {
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, distance))
            {
                Debug.Log(hit.transform.tag);

                // 에너미 공격 적중 => 플레이어
                if (hit.transform.tag == "Player")
                {
                    hit.transform.GetComponent<Health>().OnDamaged(damage);
                }
                // 플레이어 공격 적중 => 에너미
                else if (hit.transform.tag == "Enemy")
                {
                    m_enemyManager.DamagedEnemy(hit.transform.GetComponent<Enemy>(), hit, damage);
                }
                // 플레이어 공격 적중 => 사물
                else
                {
                    Vector3 position = hit.point;
                    Quaternion rotation = Quaternion.LookRotation(hit.normal);

                    PlayVFX(m_particlePrefab, m_particlePooling, transform, position, rotation);
                }
            }
        }

        /// <summary>
        /// 이펙트 재생 함수
        /// </summary>
        /// <param name="origin">프리팹 원본</param>
        /// <param name="pooling">풀링용 리스트</param>
        /// <param name="parent">인스펙터 정리용 상위오브젝트</param>
        /// <param name="localPosition"></param>
        /// <param name="localRotation"></param>
        public void PlayVFX(ParticleSystem origin, List<ParticleSystem> pooling, Transform parent, Vector3 localPosition, Quaternion localRotation)
        {
            // 이펙트가 없으면 재생 X.
            if (m_particlePrefab == null) return;

            ParticleSystem currentParticle = null;

            foreach (var particle in pooling)
            {
                // 풀링 내에서 현재 진행중이지 않은 이펙트 검색
                if (!particle.isPlaying)
                {
                    currentParticle = particle;

                    break;
                }
            }

            // 이펙트가 부족하면 생성한 뒤 풀링 추가
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
