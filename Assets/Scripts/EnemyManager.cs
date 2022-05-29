using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPS
{
    public class EnemyManager : MonoBehaviour
    {
        [System.Serializable]
        public class OnEnemyCountChanged : UnityEngine.Events.UnityEvent<int> { }

        [SerializeField] Enemy m_enemyPrefab;
        [SerializeField] List<Enemy> m_enemies;

        [Header("VFX")]
        [SerializeField] ParticleSystem m_particlePrefab;
        [SerializeField] List<ParticleSystem> m_particlePooling = new List<ParticleSystem>();

        [Space]
        [SerializeField] OnEnemyCountChanged m_onEnemyCountChanged = new OnEnemyCountChanged();

        #region Properties

        PlayerController Player => GameManager.Instance.Player;

        public int EnemyCount => m_enemies.Count;

        public OnEnemyCountChanged EnemyCountChanged => m_onEnemyCountChanged;

        #endregion

        private void Start()
        {
            Enemy.Manager = this;

            if (m_enemies.Count == 0)
            {
                foreach (Transform child in transform)
                {
                    m_enemies.Add(child.GetComponent<Enemy>());
                }
            }
        }

        public void AddEnemy(Vector3 position, Quaternion rotation)
        {
            Enemy enemy = Instantiate(m_enemyPrefab, position, rotation, transform);
            m_enemies.Add(enemy);

            m_onEnemyCountChanged.Invoke(m_enemies.Count);
        }

        public void DestroyEnemy(Enemy enemy)
        {
            m_enemies.Remove(enemy);
            Destroy(enemy.gameObject);

            m_onEnemyCountChanged.Invoke(m_enemies.Count);
        }

        public void DamagedEnemy(Enemy enemy, RaycastHit hit, int damamge)
        {
            GameManager.Instance.PlayVFX(m_particlePrefab, m_particlePooling, transform, hit.point, Quaternion.LookRotation(hit.normal));
            enemy.Health.OnDamaged(damamge);
        }
    }
}
