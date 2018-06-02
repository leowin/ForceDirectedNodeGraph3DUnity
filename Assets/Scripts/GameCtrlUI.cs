using UnityEngine;
using UnityEngine.UI;
using ProgressBar;
using System;
using Assets.Scripts;

public class GameCtrlUI : MonoBehaviour {

    [SerializeField]
    private bool verbose = true;

    private static GameController gameControl;
    private static GameCtrlInputReader gameCtrlInputReader;
    private static GraphController graphControl;
    private static InputField nodename;
    public GameObject leftPanel;
    public GameObject leftPanelCollapsed;
    public Slider slider;

    // All UI Elements here
    private RectTransform panelrecttrans;
    private RectTransform tlPaneltrans;
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
        Vector3[] corners = new Vector3[4];
        panelrecttrans.GetWorldCorners(corners);
        Vector3[] cornersTlPanel = new Vector3[4];
        tlPaneltrans.GetWorldCorners(cornersTlPanel);
        Rect plRect = new Rect(corners[0], corners[2] - corners[0]);
        Rect tlRect = new Rect(cornersTlPanel[0], cornersTlPanel[2] - cornersTlPanel[0]);
        if ((panelVisible && plRect.Contains(pointerCoords)) || tlRect.Contains(pointerCoords))
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

  

    // Use this for initialization
    void Start()
    {
        gameControl = GetComponent<GameController>();
        gameCtrlInputReader = GetComponent<GameCtrlInputReader>();
        graphControl = GetComponent<GraphController>();

        progressBar = FindObjectOfType<ProgressBarBehaviour>();
        progressBarObj = progressBar.gameObject;
        panelrecttrans = GameObject.Find("PanelLeft").GetComponent<RectTransform>();
        tlPaneltrans = GameObject.Find("TimelinePanel").GetComponent<RectTransform>();
        progressBarObj.SetActive(false);

        nodename = GameObject.Find("Input_Text").GetComponent<InputField>();
        nodename.onValueChanged.AddListener((s) => NodeTextChanged(s));
        ShowPanel(panelVisible);
        slider.onValueChanged.AddListener((pos) => graphControl.SetPosition((int)pos));
    }

    public void NodeTextChanged(string text)
    {
        var selected = graphControl.SelectedNode;
        if (selected != null && (!string.IsNullOrEmpty(text) || !string.IsNullOrEmpty(selected.Text)))
        {
            graphControl.DoAction(new RenameAction() { name = text, oldName = selected.Text, nodeId = selected.name });
        }
    }

    public void OnGUI()
    {
        nodename.enabled = graphControl.SelectedNode != null;
        slider.minValue = 0;
        slider.maxValue = graphControl.GetTimelineCount();
        slider.value = graphControl.GetPosition();
    }
    
    public void OverCollapsed()
    {
        
    }
}
