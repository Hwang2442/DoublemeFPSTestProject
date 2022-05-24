using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class TimeRecorder : MonoBehaviour
{
    public float RecordingTime { get; private set; }

    Coroutine m_coroutine;

    public void StartRecording(UnityAction<float> onUpdate)
    {
        if (m_coroutine != null)
        {
            Debug.LogWarning("Already recording...");

            return;
        }

        RecordingTime = 0;
        m_coroutine = StartCoroutine(Co_TimeRecording(onUpdate));
    }

    public void StopRecording()
    {
        if (m_coroutine == null)
        {
            Debug.LogWarning("Not recording...");

            return;
        }

        StopCoroutine(m_coroutine);
    }

    private IEnumerator Co_TimeRecording(UnityAction<float> onUpdate)
    {
        while (true)
        {
            yield return null;

            RecordingTime += Time.deltaTime;
            onUpdate?.Invoke(RecordingTime);
        }
    }
}