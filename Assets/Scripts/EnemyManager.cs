using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPS
{
    public class EnemyManager : MonoBehaviour
    {
        [SerializeField] Enemy m_enemyPrefab;
        [SerializeField] List<Enemy> m_enemies;

        #region Properties

        PlayerController Player => GameManager.Instance.Player;

        public Enemy[] Enemies => m_enemies.ToArray();

        #endregion

        private void Start()
        {
            Enemy.Manager = this;
        }

        public void AddEnemy(Vector3 position, Quaternion rotation)
        {
            Enemy enemy = Instantiate(m_enemyPrefab, position, rotation, transform);
            m_enemies.Add(enemy);
        }

        public void DestroyEnemy(Enemy enemy)
        {
            for (int i = 0; i < m_enemies.Count; i++)
            {
                if (enemy == m_enemies[i])
                {
                    Destroy(enemy.gameObject);

                    break;
                }
            }
        }
    }
}
