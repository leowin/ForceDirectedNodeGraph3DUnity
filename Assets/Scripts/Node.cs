using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public abstract class Node : MonoBehaviour
{
    [SerializeField]
    protected static bool verbose = true;

    protected static GraphController graphControl;
    private GameCtrlUI gameCtrlUI;
    private string id;
    private string text;
    private string type;

    private InputField NodeTextInput;


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
        NodeTextInput = Resources.FindObjectsOfTypeAll<InputField>().FirstOrDefault(p => p.name == "Input_Text");
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
        if (NodeTextInput!= null && NodeTextInput.isActiveAndEnabled && graphControl != null && graphControl.SelectedNode == this && graphControl.IsRecording())
            return;
        var position = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        GUI.depth = 100000;
       
        var heading = gameObject.transform.position - Camera.main.transform.position;
        var distance = Vector3.Dot(heading, Camera.main.transform.forward);

        //var distance = Vector3.Distance(Camera.main.transform.position, gameObject.transform.position);
        if (distance > Camera.main.nearClipPlane + 20 && distance < 130)
        {
            labelStyle.fontSize = (int)(gameCtrlUI.labelFontSize * ( 1 -  distance / 300));
            labelStyle.alignment = TextAnchor.UpperLeft;
            var textSize = labelStyle.CalcSize(new GUIContent(Text));
            GUI.color = graphControl.IsSelected(this) ? Color.red : new Color(0, 0, 0, 100 / distance / 2);
            GUI.Label(new Rect(position.x - textSize.x / 2.0f, Screen.height - position.y + gameCtrlUI.labelFontSize/2, textSize.x, textSize.y), Text, labelStyle);
        }
        
    }
}