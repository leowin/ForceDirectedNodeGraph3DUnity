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

    private int _nextId;
    public Int32 NextId { get { return ++_nextId; } internal set { _nextId = value; } }

    public Node SelectedNode
    {
        get { return selected; }
    }



    internal void SelectById(String id)
    {
        var gO = GameObject.Find(id);
        if (gO != null)
        {
            Select((NodePhysX)gO.GetComponent(typeof(NodePhysX)));
        }
        else
            Select(null);
    }
    internal void Select(Node node)
    {
        selected = node;
        gameCtrlUI.SelectedNodeChanged(node);
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
                Destroy(destroyTarget);
                LinkCount -= 1;
                gameCtrlUI.PanelStatusLinkCountTxt.text = "Linkcount: " + LinkCount;
            }
        }
        Destroy(gameObject);
        NodeCount -= 1;
        gameCtrlUI.PanelStatusNodeCountTxt.text = "Nodecount: " + NodeCount;
       

        foreach (GameObject destroyTarget in GameObject.FindGameObjectsWithTag("debug"))
        {
            if (destroyTarget == gameObject.transform.Find("debugRepulseObj").gameObject)
            {
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
            Destroy(destroyTarget);
            LinkCount -= 1;
            gameCtrlUI.PanelStatusLinkCountTxt.text = "Linkcount: " + LinkCount;
        }

        foreach (GameObject destroyTarget in GameObject.FindGameObjectsWithTag("node"))
        {
            Destroy(destroyTarget);
            NodeCount -= 1;
            gameCtrlUI.PanelStatusNodeCountTxt.text = "Nodecount: " + NodeCount;
        }

        foreach (GameObject destroyTarget in GameObject.FindGameObjectsWithTag("debug"))
        {
            Destroy(destroyTarget);
        }

        debugObjects.Clear();
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

    public bool RemoveLink(string id)
    {
        var link = GameObject.Find(id);
        if (link != null)
        {
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
                    if (checkLink.source == source && checkLink.target == target)
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
    }
    public void ReplayActions(int position)
    {
        if (position < timeline.currentPosition)
        {
            ResetWorld();
            timeline.currentPosition = 0;
        }
        for (int p = timeline.currentPosition; p < position; p++)
        {
            PerformAction(timeline.actions[p], false);
            timeline.currentPosition = p+1;
        }
    }
  
    public int GetPosition()
    {
        return timeline.currentPosition;
    }
    public int GetTimelineCount()
    {
        return timeline.actions.Count;
    }
    public void SetPosition(int position)
    {
        ReplayActions(position);
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
            ReplayActions(position);
            return "";
        }
        else
        {
            var ret = PerformAction(action, true);
            if (string.IsNullOrEmpty(ret))
            {
                timeline.actions.RemoveAt(timeline.currentPosition);
            }
            return ret;
        }


    }
    public string DoAction(TimelineAction action)
    {
        var ret = PerformAction(action, false);
        if (string.IsNullOrEmpty(ret))
        {
            //combine rename actions
            if (action is RenameAction && timeline.currentPosition>0 && timeline.actions[timeline.currentPosition-1] is RenameAction)
            {
                if( (action as RenameAction).nodeId ==( timeline.actions[timeline.currentPosition - 1] as RenameAction).nodeId)
                {
                    (timeline.actions[timeline.currentPosition - 1] as RenameAction).name = (action as RenameAction).name;
                    return ret;
                }
            }
            timeline.actions.Insert(timeline.currentPosition, action);
            timeline.currentPosition++;
        }
        return ret;
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
                GenerateNode(action.name, action.nodeId, action.type, new Vector3((float)action.x, (float)action.y, (float)action.z));
            }
            else
            {
                RemoveNode(action.nodeId);
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
        var createAction = new CreateNode() { name = oldNode.Text, x = position.x, y = position.y, z = position.z };
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
        ReplayActions(timeline.currentPosition);
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
    }

}
