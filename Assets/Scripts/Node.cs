using UnityEngine;

public abstract class Node : MonoBehaviour
{
    [SerializeField]
    protected static bool verbose = true;

    protected static GraphController graphControl;
    private GameCtrlUI gameCtrlUI;
    private string id;
    private string text;
    private string type;

    public string Id
    {
        get
        {
            return id;
        }
        set
        {
            id = value;
        }
    }

    public string Text
    {
        get
        {
            return text;
        }
        set
        {
            text = value;
        }
    }

    public string Type
    {
        get
        {
            return type;
        }
        set
        {
            type = value;
        }
    }
    public void Select()
    {
        if (graphControl == null)
            return;

        graphControl.Select(this);
    }

    public void DeSelect()
    {
        if (graphControl == null)
            return;

        graphControl.Select(null);
    }

    public bool IsSelected()
    {
        if (graphControl == null)
            return false;

       return graphControl.IsSelected(this);
    }
    protected abstract void doGravity();

    protected abstract void doRepulse();

    protected virtual void Start()
    {
        graphControl = FindObjectOfType<GraphController>();
        gameCtrlUI = FindObjectOfType<GameCtrlUI>();
    }

    void FixedUpdate()
    {
        if (!graphControl.AllStatic && graphControl.RepulseActive)
            doRepulse();

        if (!graphControl.AllStatic)
            doGravity();
    }

    private void OnGUI()
    {
        var position = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = gameCtrlUI.labelFontSize;
        var textSize = labelStyle.CalcSize(new GUIContent(Text));
        GUI.color = graphControl.IsSelected(this) ? Color.yellow : Color.black;
        GUI.Label(new Rect(position.x - textSize.x / 2.0f, Screen.height - position.y + textSize.y / 2.0f, textSize.x, textSize.y), Text, labelStyle);
    }
}