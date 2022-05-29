using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FPS
{
    public class Health : MonoBehaviour
    {
        [Header("Amounts")]
        [SerializeField] int m_maxHp;
        [SerializeField] int m_curHp;

        [Header("Events")]
        [SerializeField] UnityEvent m_onDamagedEvent;
        [SerializeField] UnityEvent m_onRecoverEvent;

        #region Properties

        public int MaxHP => m_maxHp;
        public int curHP => m_curHp;

        public UnityEvent OnDamagedEvent => m_onDamagedEvent;
        public UnityEvent onRecoverEvent => m_onRecoverEvent;

        #endregion

        public void OnDamaged(int damage)
        {
            m_curHp = Mathf.Max(0, m_curHp - damage);

            m_onDamagedEvent.Invoke();
        }

        public void OnRecovered(int recovery)
        {
            m_curHp = Mathf.Min(m_curHp + recovery, m_maxHp);

            m_onRecoverEvent.Invoke();
        }
    }
}
