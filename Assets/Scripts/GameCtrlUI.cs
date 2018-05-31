using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using ProgressBar;
using System;
using UnityEngine.EventSystems;

public class GameCtrlUI : MonoBehaviour {

    [SerializeField]
    private bool verbose = true;

    private static GameController gameControl;
    private static GameCtrlInputReader gameCtrlInputReader;
    private static GraphController graphControl;
    private static InputField nodename;
    public GameObject leftPanel;
    public GameObject leftPanelCollapsed;

    // All UI Elements here
    private RectTransform panelrecttrans;
    [SerializeField]
    private Text statusText;
    [SerializeField]
    private Text nodeCountTxt;
    [SerializeField]
    private Text linkCountTxt;

    // ProgressBar here, e.g. while loading file
    private ProgressBarBehaviour progressBar;
    private GameObject progressBarObj;

    internal Text PanelStatusText
    {
        get
        {
            return statusText;
        }
        set
        {
            statusText = value;
        }
    }

    internal Text PanelStatusNodeCountTxt
    {
        get
        {
            return nodeCountTxt;
        }
        set
        {
            nodeCountTxt = value;
        }
    }

    public void FontLarger()
    {
        labelFontSize = Math.Min(labelFontSize+2, 40);
    }
    public void FontSmaller()
    {
        labelFontSize = Math.Max(labelFontSize-2, 0);
    }
    internal void SelectedNodeChanged(Node node)
    {
        if (node != null)
        {
            if (!panelVisible)
                ShowPanel(true);
            nodename.text = node.Text;
            nodename.enabled = true;
            nodename.Select();
         }
        else
        {
            nodename.text = "";
            if (panelVisible)
                ShowPanel(false);
        }

    }
    public bool panelVisible = false;

    public void ShowPanel(bool show)
    {
        panelVisible = show;
        leftPanelCollapsed.SetActive(!show);
        leftPanel.gameObject.SetActive(show);

    }
    internal Text PanelStatusLinkCountTxt
    {
        get
        {
            return linkCountTxt;
        }
        set
        {
            linkCountTxt = value;
        }
    }

    [SerializeField]
    public Int32 labelFontSize = 20;

    public void TogglePaintMode(Toggle tgl)
    {
        if (tgl.isOn)
        {
            graphControl.PaintMode = true;
            if (verbose)
                Debug.Log(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": PaintMode active");
        }
        else
        {
            graphControl.PaintMode = false;
            if (verbose)
                Debug.Log(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": PaintMode inactivate");
        }
    }

    public void ToggleAllStatic(Toggle tgl)
    {
        if (tgl.isOn)
        {
            graphControl.AllStatic = true;
            if (verbose)
                Debug.Log(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": AllStatic on");
        }
        else
        {
            graphControl.AllStatic = false;
            if (verbose)
                Debug.Log(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": AllStatic off");
        }
    }

    public void ToggleRepulseActive(Toggle tgl)
    {
        if (tgl.isOn)
        {
            graphControl.RepulseActive = true;
            if (verbose)
                Debug.Log(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": Repulse on");
        }
        else
        {
            graphControl.RepulseActive = false;
            if (verbose)
                Debug.Log(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": Repulse off");
        }
    }

    public void ToggleDebugRepulse(Toggle tgl)
    {
        if (tgl.isOn)
        {
            graphControl.DebugRepulse = true;
            if (verbose)
                Debug.Log(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": DebugRepulse on");
        }
        else
        {
            graphControl.DebugRepulse = false;
            if (verbose)
                Debug.Log(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": DebugRepulse off");
        }
    }

    internal bool PanelIsPointeroverPanel(Vector3 pointerCoords)
    {
        if (panelVisible &&  pointerCoords.x < panelrecttrans.rect.width)
        {
            Debug.Log(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": PointerOverPanel: " + pointerCoords.x);
            return true;
        }
        else
            return false;
    }

    internal void ProgressBarSetActive(bool state)
    {
        progressBarObj.SetActive(state);
    }

    internal void ProgressBarValue(float progressValue)
    {
        progressBar.Value = progressValue;
    }

    internal string OpenFileDialogGetFile()
    {
        string file = EditorUtility.OpenFilePanel("Open Input File", Application.dataPath + "/Data", "");
        return file;
    }

    // Use this for initialization
    void Start()
    {
        gameControl = GetComponent<GameController>();
        gameCtrlInputReader = GetComponent<GameCtrlInputReader>();
        graphControl = GetComponent<GraphController>();

        progressBar = FindObjectOfType<ProgressBarBehaviour>();
        progressBarObj = progressBar.gameObject;
        panelrecttrans = GameObject.Find("PanelLeft").GetComponent<RectTransform>();
        progressBarObj.SetActive(false);

        nodename = GameObject.Find("Input_Text").GetComponent<InputField>();
        nodename.onValueChanged.AddListener((s) => NodeTextChanged(s));
        ShowPanel(panelVisible);
    }

    public void NodeTextChanged(string text)
    {
        var selected = graphControl.SelectedNode;
        if (selected != null)
        {
            graphControl.SelectedNode.Text =  text;
        }
    }

    public void OnGUI()
    {
        nodename.enabled = graphControl.SelectedNode != null;

    }
    
    public void OverCollapsed()
    {
        
    }
}
