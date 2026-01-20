using UnityEngine;

public class FPSDebugger : MonoBehaviour
{
    public static FPSDebugger Instance { get; private set; }

    private float deltaTime = 0.0f;
    private bool showFPS = true;

    private void Awake()
    {
        Application.targetFrameRate = 120;
        QualitySettings.vSyncCount = 0;
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        if (Input.GetKeyDown(KeyCode.F))
        {
            showFPS = !showFPS;
        }
    }

    private void OnGUI()
    {
        if (!showFPS) return;

        int w = Screen.width, h = Screen.height;
        
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.UpperRight;
        style.fontSize = h * 2 / 100;
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;

        if (fps < 90)
            style.normal.textColor = Color.red;
        else if (fps < 120)
            style.normal.textColor = Color.yellow;
        else
            style.normal.textColor = Color.green;

        string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);

        Rect rect = new Rect((w/40) - 200f, h/40, w, h * 2 / 100);
        GUI.Label(rect, text, style);
    }
}