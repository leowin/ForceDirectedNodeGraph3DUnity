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

    private static GameObject btnPlay;
    private static GameObject btnPause; 
    private static GameObject btnStop;
    private static GameObject btnRecord;
    private static GameObject btnNext;
    private static GameObject btnPrev;

    public GameObject leftPanel;
    public GameObject leftPanelCollapsed;
    public Slider slider;
    public Text playerPosText;

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
    internal void SelectedNodeChanged(Node node, bool showProps)
    {
        if (node != null)
        {
            var position = Camera.main.WorldToScreenPoint(node.transform.position);
            position += new Vector3(0, labelFontSize*-2, position.z * -1);
            nodename.transform.position = position;
        }
        if (node != null && showProps && graphControl.IsRecording())
        {
            nodename.enabled = true;
            nodename.Select();
            nodename.text = node.Text;
            nodename.ActivateInputField();
        }
        else
        {
            nodename.DeactivateInputField();
            nodename.transform.position = new Vector3(-200, -200, -100); //invisible
            //nodename.gameObject.SetActive(false);
            
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
    public void SetPlayState(bool play = false, bool record = false)
    {
        btnPlay.SetActive(!play && !record);
        btnPause.SetActive(play || record);
        btnRecord.SetActive(!record && !play);
        btnStop.SetActive(play || record);
    }
    public void OnPlayerPlay()
    {
        graphControl.Play();
    }
    public void OnPlayerPause()
    {
        graphControl.Pause();
    }
    public void OnPlayerRecord()
    {
        graphControl.BeginRecord();
    }
    public void OnPlayerStart()
    {
        graphControl.SetPosition(0);
    }
    public void OnPlayerEnd()
    {
        graphControl.SetPosition(graphControl.GetTimelineCount());
    }
    public void OnPlayerStop()
    {
        graphControl.Stop();
    }
    public void OnPlayerNext()
    {
        graphControl.SetPosition(Math.Min(graphControl.GetPosition() + 1, graphControl.GetTimelineCount()));
    }
    public void OnPlayerPrev()
    {
        graphControl.SetPosition(Math.Max(graphControl.GetPosition() - 1, 0));
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
        slider.onValueChanged.AddListener((pos) => graphControl.SetTimePosition(pos));

        graphControl.PlayerPosition += GraphControl_PlayerPosition;
        btnPlay = GameObject.Find("BtnPlay");
        btnPause = GameObject.Find("BtnPause");
        btnStop = GameObject.Find("BtnStop");
        btnRecord = GameObject.Find("BtnRecord");
        btnNext = GameObject.Find("BtnNext");
        btnPrev = GameObject.Find("BtnPrev");

        SetPlayState();

    }


    private void GraphControl_PlayerPosition(object sender, bool playing, bool recording, float framePosition, int frame, int frames)
    {
        SetPlayState(playing, recording);
        playerPosText.text = TimeSpan.FromSeconds(framePosition).ToString().Split('.')[0] + "(" +  frame + "/" + frames + ")";
    }

    public void NodeTextChanged(string text)
    {
        var selected = graphControl.SelectedNode;
        if (selected != null && (!string.IsNullOrEmpty(text) || !string.IsNullOrEmpty(selected.Text)))
        {
            graphControl.DoAction(new RenameAction() { name = text, oldName = selected.Text, nodeId = selected.name });
        }
    }
    public void Update()
    {
        //button controls:
        if (Input.GetButtonUp("Play") && !graphControl.IsPlaying() && !graphControl.IsRecording())
        {
            OnPlayerPlay();
        }
        if (Input.GetButtonUp("Stop") && (graphControl.IsPlaying() || graphControl.IsRecording()))
        {
            OnPlayerStop();
        }
        if (Input.GetButtonUp("Load") && !graphControl.IsPlaying() && !graphControl.IsRecording())
        {
            graphControl.Load();
        }
        if (Input.GetButtonUp("Record") && !graphControl.IsPlaying() && !graphControl.IsRecording())
        {
            OnPlayerRecord();
        }
        if (Input.GetKeyUp(KeyCode.RightArrow) && !graphControl.IsRecording())
        {
            OnPlayerNext();
        }
        if (Input.GetKeyUp(KeyCode.LeftArrow) && !graphControl.IsRecording())
        {
            OnPlayerPrev();
        }
        if (Input.GetButtonUp("First") && !graphControl.IsRecording())
        {
            OnPlayerStart();
        }
        if (Input.GetButtonUp("Last") && !graphControl.IsRecording())
        {
            OnPlayerEnd();
        }

        if (Input.GetKeyUp("escape"))
            Application.Quit();

    }
    public void OnGUI()
    {
        nodename.enabled = graphControl.SelectedNode != null;
        slider.minValue = 0;
        slider.maxValue = graphControl.GetTimelineTime();
        slider.value = graphControl.GetTimePosition();
    }
    
    public void OverCollapsed()
    {
        
    }
}
