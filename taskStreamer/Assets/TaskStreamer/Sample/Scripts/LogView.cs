using UnityEngine;
using UnityEngine.UI;


namespace TaskStreamer.Runtime
{
    public class LogView : MonoBehaviour
    {
        public ScrollRect scrollRect;
        public Transform content;
        public GameObject logTextPrefab;



        public void AddLog(string log)
        {
            GameObject instant = Instantiate(logTextPrefab, content);
            Text text = instant.GetComponent<Text>();
            text.text = log;
        }



        public void ClearLog()
        {
            for (int i = content.childCount - 1; i >= 0; --i)
            {
                Destroy(content.GetChild(i).gameObject);
            }
        }
    }
}