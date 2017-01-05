using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPS_Counter : MonoBehaviour
{
  [SerializeField]
  public Text m_label;
  private Queue<float> m_queue;
  private float m_timer;
  [SerializeField]
  private float m_refreshPeriod;
  [SerializeField]
  private float m_rollingWindowSize;

    void Awake()
    {
        this.m_queue = new Queue<float>();
    }

  void Update()
  {
    this.m_queue.Enqueue(Time.deltaTime);
    if ((double) this.m_queue.Count > (double) this.m_rollingWindowSize)
    {
      this.m_queue.Dequeue();
    }
    this.m_timer += Time.deltaTime;
    if ((double) this.m_timer < (double) this.m_refreshPeriod)
      return;
    this.m_timer = 0.0f;
    this.m_label.text = string.Format("{0:f0} fps", (object) this.GetFps());
    //this.m_label.color = !MonoSingleton<DwellerPool>.Instance.BatchUpdateEnabled ? Color.get_white() : Color.get_green();
  }

  private float GetFps()
  {
    float num = 0.0f;
    using (Queue<float>.Enumerator enumerator = this.m_queue.GetEnumerator())
    {
      while (enumerator.MoveNext())
      {
        float current = enumerator.Current;
        num += current;
      }
    }
    return (float) this.m_queue.Count / num;
  }
}
