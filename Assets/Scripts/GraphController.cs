using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Assets.Scripts;
using Crosstales.FB;
using System.Linq;
//using BulletUnity;

public class GraphController : MonoBehaviour {

    [SerializeField]
    private static bool verbose = true;

    private static GameController gameControl;
    private static GameCtrlUI gameCtrlUI;
    private static GameCtrlHelper gameCtrlHelper;
    private Timeline timeline = new Timeline();
    
    [SerializeField]
    private bool allStatic = false;
    [SerializeField]
    private bool paintMode = true;
    [SerializeField]
    private bool repulseActive = true;
    [SerializeField]
    private bool debugRepulse = false;

    [SerializeField]
    private GameObject nodePrefabBullet;
    [SerializeField]
    private GameObject nodePrefabPhysX;
    [SerializeField]
    private Link linkPrefab;
    [SerializeField]
    private float nodeVectorGenRange = 7F;

    public Int32 NextId { get { return timeline.NextId(); } }

    public event OnCameraMoved MoveCamera;
    public event OnPlayerPosition PlayerPosition;

    public delegate void OnCameraMoved(object sender, Vector3 position, Vector3 rotation, float duration);
    public delegate void OnPlayerPosition(object sender, bool playing, bool recording, float framePosition, int frame, int frames);


    public Node SelectedNode
    {
        get { return selected; }
    }



    internal void SelectById(String id, bool showProps = false)
    {
        var gO = GameObject.Find(id);
        if (gO != null)
        {
            Select((NodePhysX)gO.GetComponent(typeof(NodePhysX)), showProps);
        }
        else
            Select(null);
    }
    internal void Select(Node node, bool showProps = false)
    {
        selected = node;
        gameCtrlUI.SelectedNodeChanged(node, showProps);
    }

    [SerializeField]
    private float globalGravityBullet = 0.1f;
    [SerializeField]
    private float globalGravityPhysX = 10f;
    [SerializeField]
    private float repulseForceStrength = 0.1f;

    private Node selected;

    internal void RemoveNode(string id)
    {
        var gameObject = GameObject.Find(id);
        if (SelectedNode != null && SelectedNode.gameObject == gameObject)
            Select(null);
        foreach (GameObject destroyTarget in GameObject.FindGameObjectsWithTag("link"))
        {
            var link = destroyTarget.GetComponent<Link>();
            if (link.target == gameObject || link.source == gameObject)
            {
                destroyTarget.name = "DELETED";
                Destroy(destroyTarget);
                LinkCount -= 1;
                gameCtrlUI.PanelStatusLinkCountTxt.text = "Linkcount: " + LinkCount;
            }
        }
        gameObject.name = "DELETED";
        Destroy(gameObject);
        NodeCount -= 1;
        gameCtrlUI.PanelStatusNodeCountTxt.text = "Nodecount: " + NodeCount;
       

        foreach (GameObject destroyTarget in GameObject.FindGameObjectsWithTag("debug"))
        {
            if (destroyTarget == gameObject.transform.Find("debugRepulseObj").gameObject)
            {
                destroyTarget.name = "DELETED";
                Destroy(destroyTarget);
            }
        }
    }

    internal bool IsSelected(Node node)
    {
        return node == selected;
    }


    [SerializeField]
    private float nodePhysXForceSphereRadius = 50F;                         // only works in PhysX; in BulletUnity CollisionObjects are used, which would need removing and readding to the world. Todo: Could implement it somewhen.
    [SerializeField]
    private float linkForceStrength = 6F;
    [SerializeField]
    private float linkIntendedLinkLength = 5F;

    private static int nodeCount;
    private static int linkCount;
    private List<GameObject> debugObjects = new List<GameObject>();
    private bool playerPlaying;
    private bool recording;
    private float recordTime;
    private float playTime = 0;
    private float firstActionTime;

    public bool AllStatic
    {
        get
        {
            return allStatic;
        }
        set
        {
            allStatic = value;
        }
    }

    public bool PaintMode
    {
        get
        {
            return paintMode;
        }
        set
        {
            paintMode = value;
        }
    }

    public bool RepulseActive
    {
        get
        {
            return repulseActive;
        }
        set
        {
            repulseActive = value;
        }
    }

    public bool DebugRepulse
    {
        get
        {
            return debugRepulse;
        }
        set
        {
            if (debugRepulse != value)
            {
                debugRepulse = value;
                DebugAllNodes();
            }
        }
    }

    public float GlobalGravityBullet
    {
        get
        {
            return globalGravityBullet;
        }
        private set
        {
            globalGravityBullet = value;
        }
    }

    public float GlobalGravityPhysX
    {
        get
        {
            return globalGravityPhysX;
        }
        set
        {
            globalGravityPhysX = value;
        }
    }

    public float RepulseForceStrength
    {
        get
        {
            return repulseForceStrength;
        }
        private set
        {
            repulseForceStrength = value;
        }
    }

    public float NodePhysXForceSphereRadius
    {
        get
        {
            return nodePhysXForceSphereRadius;
        }
        set
        {
            nodePhysXForceSphereRadius = value;
        }
    }

    public float LinkForceStrength
    {
        get
        {
            return linkForceStrength;
        }
        private set
        {
            linkForceStrength = value;
        }
    }

    public float LinkIntendedLinkLength
    {
        get
        {
            return linkIntendedLinkLength;
        }
        set
        {
            linkIntendedLinkLength = value;
        }
    }

    public int NodeCount
    {
        get
        {
            return nodeCount;
        }
        set
        {
            nodeCount = value;
        }
    }

    internal void Pause()
    {
        playerPlaying = false;
    }

    internal void BeginRecord()
    {
        if (recording)
            return;
        recording = true;
        recordTime = Time.time;
        firstActionTime = Time.time - (GetPrevAction(timeline.currentPosition) ?? new TimelineAction()).time;
        EmitPosition();
    }

    public int LinkCount
    {
        get
        {
            return linkCount;
        }
        set
        {
            linkCount = value;
        }
    }
    
    void DebugAllNodes()
    {
        if (DebugRepulse)
        {
            foreach (GameObject debugObj in debugObjects)
            {
                debugObj.SetActive(true);
                if (debugObj.name == "debugRepulseObj")
                {
                    float sphereDiam = gameCtrlHelper.GetRepulseSphereDiam();
                    debugObj.transform.localScale = new Vector3(sphereDiam, sphereDiam, sphereDiam);
                }
            }
        }
        else
        {
            foreach (GameObject debugObj in debugObjects)
            {
                debugObj.SetActive(false);
            }
        }
    }

    public void ResetWorld()
    {
        foreach (GameObject destroyTarget in GameObject.FindGameObjectsWithTag("link"))
        {
            destroyTarget.name = "DELETED";
            Destroy(destroyTarget);
            LinkCount -= 1;
            gameCtrlUI.PanelStatusLinkCountTxt.text = "Linkcount: " + LinkCount;
        }

        foreach (GameObject destroyTarget in GameObject.FindGameObjectsWithTag("node"))
        {
            destroyTarget.name = "DELETED";
            Destroy(destroyTarget);
            NodeCount -= 1;
            gameCtrlUI.PanelStatusNodeCountTxt.text = "Nodecount: " + NodeCount;
        }

        foreach (GameObject destroyTarget in GameObject.FindGameObjectsWithTag("debug"))
        {
            destroyTarget.name = "DELETED";
            Destroy(destroyTarget);
        }

        debugObjects.Clear();
        EmitPosition();
        gameCtrlUI.SelectedNodeChanged(null, false);
    }

    private GameObject InstObj(Vector3 createPos)
    {
        if (gameControl.EngineBulletUnity)
        {
            return Instantiate(nodePrefabBullet, createPos, Quaternion.identity) as GameObject;
        }
        else
        {
            return Instantiate(nodePrefabPhysX, createPos, Quaternion.identity) as GameObject;
        }
    }

  

    public GameObject GenerateNode(string name, string id, string type, Vector3 createPos)
    {
        // Method for creating a Node on random coordinates, but with defined labels. E.g. when loaded from a file which contains these label.

        GameObject nodeCreated = null;

  
        //nodeCreated = Instantiate(nodePrefabBullet, createPos, Quaternion.identity) as Node;
        nodeCreated = InstObj(createPos);

        if (nodeCreated != null){
            Node nodeNode = nodeCreated.GetComponent<Node>();
            nodeNode.name = id;
            nodeNode.Text = name;
            nodeNode.Type = type;

            nodeCount++;
            gameCtrlUI.PanelStatusNodeCountTxt.text = "Nodecount: " + NodeCount;

            GameObject debugObj = nodeCreated.transform.Find("debugRepulseObj").gameObject;
            debugObjects.Add(debugObj);
            debugObj.SetActive(false);

            if (verbose)
                Debug.Log(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": Node created: " + nodeCreated.gameObject.name);
        }
        else
        {
            if (verbose)
                Debug.Log(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": Something went wrong, no node created.");
        }

        return nodeCreated.gameObject;
    }

    internal bool IsPlaying()
    {
        return playerPlaying;
    }

    internal float GetTimelineTime()
    {
        var tlTime = 0f;
        if (timeline.actions.Count > 0)
        {
            tlTime = timeline.actions.Last().time;
        }
        if (recording)
            tlTime += Time.time - recordTime;
        return tlTime;
    }

    public bool RemoveLink(string id)
    {
        var link = GameObject.Find(id);
       
        if (link != null)
        {
            var lO = link.GetComponent<Link>();
            lO.source = null;
            lO.target = null;
            link.name = "DELETED";
            Destroy(link);
            return true;
        }
        return false;
    }
    public bool CreateLink(string id, GameObject source, GameObject target)
    {
        if (source == null || target == null)
        {
            if (verbose)
            {
                Debug.Log(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": source or target does not exist. Link not created.");
            }
            return false;
        }
        else
        {
            if (source != target)
            {
                bool alreadyExists = false;
                foreach (GameObject checkObj in GameObject.FindGameObjectsWithTag("link"))
                {
                    Link checkLink = checkObj.GetComponent<Link>();
                    if (checkObj != null && (checkLink.source == source && checkLink.target == target))
                    {
                        alreadyExists = true;
                        break;
                    }
                }

                if (!alreadyExists)
                {
                    Link linkObject = Instantiate(linkPrefab, new Vector3(0, 0, 0), Quaternion.identity) as Link;
                    linkObject.name = id;
                    linkObject.source = source;
                    linkObject.target = target;
                    linkCount++;
                    gameCtrlUI.PanelStatusLinkCountTxt.text = "Linkcount: " + LinkCount;

                    return true;
                }
                else
                {
                    if (verbose)
                    {
                        Debug.Log(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": Link between source " + source.name + " and target " + target.name + " already exists. Link not created.");
                    }
                    return false;
                }
            }
            else
            {
                if (verbose)
                {
                    Debug.Log(this.GetType().Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + ": source " + source.name + " and target " + target.name + " are the same. Link not created.");
                }
                return false;
            }
        }
    }

   


    public void GenNodes(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Create a node on random Coordinates
            Vector3 createPos = new Vector3(UnityEngine.Random.Range(0, nodeVectorGenRange), UnityEngine.Random.Range(0, nodeVectorGenRange), UnityEngine.Random.Range(0, nodeVectorGenRange));
            GenerateNode("", "node_" + NextId, "", createPos);
        }
    }

    

    void Start()
    {
        gameControl = GetComponent<GameController>();
        gameCtrlUI = GetComponent<GameCtrlUI>();
        gameCtrlHelper = GetComponent<GameCtrlHelper>();

        nodeCount = 0;
        linkCount = 0;
        debugObjects.Clear();
        firstActionTime = Time.time;

        foreach (GameObject debugObj in GameObject.FindGameObjectsWithTag("debug"))
        {
            debugObjects.Add(debugObj);
            debugObj.SetActive(false);
        }

        // prepare stuff
        if (gameControl.EngineBulletUnity)
        {
            RepulseForceStrength = .1f;
            GlobalGravityBullet = 1f;
            LinkForceStrength = .1f;
            LinkIntendedLinkLength = 3f;
        } else
        {
            RepulseForceStrength = 5f;
            GlobalGravityPhysX = 10f;
            NodePhysXForceSphereRadius = 35f;
            LinkForceStrength = 5f;
            LinkIntendedLinkLength = 3f;
        }
        EmitPosition();
        Invoke("DelayedStart", 0);
    }

    public void DelayedStart()
    {
        gameCtrlUI.SelectedNodeChanged(null, false);
    }
    public void ReplayToPosition(int position)
    {
        if (position < timeline.currentPosition)
        {
            ResetWorld();
            timeline.currentPosition = 0;
            playTime = 0;
        }
        while( timeline.currentPosition < position)
        {
            PerformAction(timeline.actions[timeline.currentPosition], false);
            playTime = timeline.actions[timeline.currentPosition].time;
            timeline.currentPosition++;
        }
    }
  
    public float GetTimePosition()
    {
        return playTime;
    }
    public int GetPosition()
    {
        return timeline.currentPosition;
    }
    public int GetTimelineCount()
    {
        return timeline.actions.Count;
    }
    public void SetTimePosition(float time)
    {
        var newPos = timeline.actions.Count;
        for (var pos = 0; pos < timeline.actions.Count; pos++)
        {
            if (timeline.actions[pos].time > time)
            {
                newPos = pos;
                break;
            }
        }
        if( newPos != timeline.currentPosition)
        {
            SetPosition(newPos);
        }
       
    }
    public TimelineAction GetPrevAction(int position)
    {
        if (position == 0 || position > timeline.actions.Count )
            return null;
        return  timeline.actions[position-1];
    }
    public void EmitPosition()
    {
        if(PlayerPosition != null) PlayerPosition(this, playerPlaying && !recording, recording, playTime, timeline.currentPosition, timeline.actions.Count);
    }
    public void SetPosition(int position)
    {
        ReplayToPosition(position);
        EmitPosition();
    }
    public string UndoAction(int? pos = null)
    {
        var position = pos.GetValueOrDefault(timeline.currentPosition - 1);
        if (timeline.actions.Count <= position)
            return "Nothing to remove";
        timeline.currentPosition = position;

        var action = timeline.actions[position];
        if (action is DeleteNode)
        {
            timeline.actions.RemoveAt(position);
            //replay Whole Timeline
            ReplayToPosition(position);
            EmitPosition();
            return "";
        }
        else
        {
            var ret = PerformAction(action, true);
            if (string.IsNullOrEmpty(ret))
            {
                timeline.actions.RemoveAt(timeline.currentPosition);
                EmitPosition();
            }
            return ret;
        }

    }
    public bool IsRecording()
    {
        return recording;
    }
    public string DoAction(TimelineAction action)
    {
        if (!IsRecording())
            return "";
        action.time = Time.time - firstActionTime;
        var ret = PerformAction(action, false);
        if (string.IsNullOrEmpty(ret))
        {
            if (CombineTimeline(action, timeline.currentPosition))
                return ret;
            timeline.actions.Insert(timeline.currentPosition, action);
            timeline.currentPosition++;
        
        }
        return ret;
    }
    public bool CombineTimeline(TimelineAction action, int position)
    {
        const float cameraCombineTime = 5.0f;

        if (position == 0 || timeline.actions.Count < position)
            return false;
        var p = timeline.actions[position - 1];
        if (timeline.actions[position-1].GetType() != action.GetType())
            return false;

        if (action is RenameAction)
        {
            //combine rename actions
            var a = action as RenameAction;
            var prev =  p as RenameAction;
            if (a.nodeId == prev.nodeId)
            {
                prev.name =a.name;
                return true;
            }
        }
        if (action is MoveCamera && (action.time - p.time) < cameraCombineTime )
        {
            //combine rename actions
            var a = action as MoveCamera;
            var prev = p  as MoveCamera;
            prev.newPos = a.newPos;
            prev.newRot = a.newRot;
            prev.duration = a.time - prev.time;
            return true;
        }
        return false;

    }

    public void Play()
    {
        if (playerPlaying)
            return;
        if (timeline.currentPosition >= timeline.actions.Count)
        {
            Stop();
            return;
        }

        var action = timeline.actions[timeline.currentPosition];
        playTime = action.time;
        playerPlaying = true;
    }
    public void Stop()
    {
        playerPlaying = false;
        if( recording)
        {
            //correct time of subsequent elements when recording parts
            for (var i = timeline.currentPosition; i < timeline.actions.Count; i++)
            {
                timeline.actions[i].time += Time.time - recordTime;
            }
        }
        recording = false;
        EmitPosition();
    }
    public string PerformAction(TimelineAction a, bool undo)
    {
        if( a is RenameAction )
        {
            var action = a as RenameAction;
            var obj = GameObject.Find(action.nodeId);
            if (obj != null)
            {
                (obj.GetComponent(typeof(NodePhysX)) as NodePhysX).Text = undo ? action.oldName : action.name;
            }
        }
        else if( a is DeleteNode)
        {
            var action = a as DeleteNode;
            if (!undo)
            {
                var obj = GameObject.Find(action.nodeId);
                if (obj)
                    RemoveNode(action.nodeId);
            }
            else throw new NotImplementedException();

        }
        else if( a is CreateNode)
        {
            var action = a as CreateNode;
            if (string.IsNullOrEmpty(action.nodeId))
                action.nodeId = "node_" + NextId;
            if (!undo)
            {
                var n = GenerateNode(action.name, action.nodeId, action.type, action.position.ToVector3());
                Select(n.GetComponent<NodePhysX>());
            }
            else
            {
                RemoveNode(action.nodeId);
            }
        }
        else if (a is MoveCamera)
        {
            var action = a as MoveCamera;

            if (!undo)
            {
                gameCtrlUI.SelectedNodeChanged(SelectedNode, false);
                if (MoveCamera != null)
                    MoveCamera(this, action.newPos.ToVector3(), action.newRot.ToVector3(), action.duration);
            }
            else
            {
                gameCtrlUI.SelectedNodeChanged(SelectedNode, false);
                if (MoveCamera != null)
                    MoveCamera(this, action.oldPos.ToVector3(), action.oldPos.ToVector3(), action.duration);
            }
        }
        else if(a is CreateLink)
        {
            var action = a as CreateLink;
            var sourceObj = GameObject.Find(action.SourceId);
            var targetObj = GameObject.Find(action.TargetId);
            if (string.IsNullOrEmpty(action.LinkId))
                action.LinkId = "link_" + NextId;
            if ( !undo)
            {
                if(sourceObj != null && targetObj != null)
                {
                    CreateLink(action.LinkId, sourceObj, targetObj);
                }
            }
            else
            {
                RemoveLink(action.LinkId);
            }
            
        }
        return "";
    }
    internal void DoDuplicateNode(string id, Vector3 position)
    {
        var old = GameObject.Find(id);
        var oldNode = (old.GetComponent(typeof(NodePhysX)) as NodePhysX);
        var links = GameObject.FindGameObjectsWithTag("link").Select(p => p.GetComponent<Link>());
        links = links.Where(p => p.source == old || p.target == old).ToArray();
        var createAction = new CreateNode() { name = oldNode.Text, position=SVector3.FromVector3(position)};
        DoAction(createAction);
        foreach(var l in links)
        {
            DoAction(new CreateLink() { SourceId = l.target == old ? l.source.name : createAction.nodeId, TargetId = l.target == old ? createAction.nodeId  : l.source.name });
        }
        DoAction(new CreateLink() { SourceId = old.name, TargetId = createAction.nodeId });
    }
    public void Load()
    {
        string path = FileBrowser.OpenSingleFile("Open File", Application.dataPath + "/Data", "xml");
        ResetWorld();
        timeline = TimeLineIO.Load(path);
        firstActionTime = Time.time;
        SetPosition(0);
        Stop();
    }
    public void Save()
    {
        string path = FileBrowser.SaveFile("Save File", Application.dataPath + "/Data", "Model", "xml");
        TimeLineIO.Save(timeline,path);
    }
    void Update()
    {
        Link.intendedLinkLength = linkIntendedLinkLength;
        Link.forceStrength = linkForceStrength;

        if( playerPlaying)
        {
            //do next timeline actions
            playTime += Time.deltaTime;
            for( int index = timeline.currentPosition; index < timeline.actions.Count; index++)
            {
                var action = timeline.actions[index];
                if (action.time < playTime)
                {
                    ReplayToPosition(index+1);
                }
                else
                    break;
            }
            EmitPosition();

            if (timeline.currentPosition>= timeline.actions.Count )
            {
                Stop();
            }
            

        }
    }

}
