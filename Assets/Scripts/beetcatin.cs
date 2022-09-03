using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Cinemachine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Mirror;
using TMPro;

public class beetcatin : PlayerController
{
    [Header("Beat System")]

    [SerializeField] float bpm; 
    private int beat = 1;
    private bool Onbeat;
    Vector3 NoteDir;

    [Header("String Attack")]
    RaycastHit hitpoint;
    public List<Bonder> Bonds = new List<Bonder>();
    public List<GameObject> StringPoints = new List<GameObject>();
    [SerializeField] Material StringMat;
    [SerializeField] Vector3 Startpos;
    [SerializeField] Vector3 EndPos;
    [SerializeField] int StringHookCount = 12;
    bool canboost = true;

    [Header("UI")]

    [SerializeField] TextMeshProUGUI BeatCounter;
    [SerializeField] TextMeshProUGUI Speed;
    [SerializeField] TextMeshProUGUI Shots;

    [Header("ObjRef & TransRef")]

    [SerializeField] GameObject MusicalNoteBlock;   
    [SerializeField] Transform OutRayPos;
    [SerializeField] GameObject Hook;

    [Header("Other")]

    [SerializeField] UnityEvent OnStart, OnEnd;
    void Start()
    {        
        StartCoroutine(StartMetronome());
        ability0 = new Ability()
        {
            ability = delegate
            {
                if (Onbeat && movementSpeed <20)
                {
                    movementSpeed += 2.5f;
                }
                else if(!Onbeat) movementSpeed = GetOriginalSpeeed();
                if (!isGroundeed())
                {
                    GameObject TempBlock = Instantiate(MusicalNoteBlock, transform.position - Vector3.up + moveDirection * movementSpeed * 0.2f, Quaternion.identity, null);
                    TempBlock.transform.forward = moveDirection;
                    TempBlock.transform.localScale = new Vector3(TempBlock.transform.localScale.x * movementSpeed * 0.07f, TempBlock.transform.localScale.y, TempBlock.transform.localScale.z * movementSpeed * 0.2f);
                    Destroy(TempBlock, 6f);
                }
                ability0.End.Invoke();                
            }
            , coolDown = 0.2f
            ,events = new UnityEvent[2] { OnStart, OnEnd }
        };
        ability1 = new Ability()
        {
            ability = delegate
            {
                if (StringHookCount >0)
                {
                    StartCoroutine(HookSequence());
                    IEnumerator HookSequence()
                    {
                        StringHookCount--;
                        Vector3 startpos = OutRayPos.position;
                        Vector3 hitpoint_point = GetHitPoint();
                        GameObject HookObj = Instantiate(Hook, OutRayPos.position, Quaternion.identity, null);
                        HookObj.name = Random.Range(0, 12).ToString();
                        Vector3 ThrowDirection = new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1));
                        for (float i = 0; Vector3.Distance(HookObj.transform.position, hitpoint_point) > 0; i += Time.deltaTime)
                        {
                            HookObj.transform.position = Bezier2(startpos, (startpos + hitpoint_point) / 2 + ThrowDirection * 2, hitpoint_point, i * 2f);
                            if (Vector3.Distance(HookObj.transform.position, hitpoint_point) <= 0)
                            {
                                StringPoints.Add(HookObj);
                                ChkNeighbors();
                            }
                            yield return null;
                        }
                        ability1.End.Invoke();
                        yield return null;
                    }
                }                             
            },
            coolDown = 0.2f
            ,
            events = new UnityEvent[2] { OnStart, OnEnd }
        };
    }
    
    // Update is called once per frame
    new void Update()
    {
        base.Update();
        NoteDir = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        BeatCounter.text = (beat + "/4");
        Speed.text = (Mathf.RoundToInt(movementSpeed).ToString());
        Shots.text = StringHookCount.ToString();
        if (movementSpeed > 7f)
        {
            movementSpeed -= Time.deltaTime;
        }
        if (Input.GetMouseButton(2))
        {
            for (int i = 0; i < StringPoints.Count; i++)
            {                
                if (Vector3.Distance(StringPoints[i].transform.position,this.transform.position) < 1f)
                {
                    GameObject stringtemp = StringPoints[i];
                    Bonder JamesBond =  GetBondOf(stringtemp);
                    StringPoints.Remove(stringtemp);
                    JamesBond.DestroyFromLibrary(stringtemp);
                    JamesBond.UpdateMesh();
                    StringHookCount++;
                    return;
                }
            }
        }
    }
    Bonder GetBondOf(GameObject haga)
    {
        foreach (var item in Bonds)
        {
            if (item.EndObj == haga || item.StartObj == haga)
            {
                return item;
            }
        }
        return null;
    }
    public static Vector3 Bezier2(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        var clamped_t = Mathf.Clamp(t, 0, 1);

        float u = 1 - clamped_t;
        float tt = clamped_t * clamped_t;
        float uu = u * u;
        Vector3 p = uu * p0;
        p += 2 * u * clamped_t * p1;
        p += tt * p2;

        return p;
    }
    private Vector3 GetHitPoint()
    {       
        Vector3 hitPoint = Vector3.zero;
        if (Physics.Raycast(OutRayPos.position,OutRayPos.forward,out hitpoint))
        {
            hitPoint = hitpoint.point;
        }
        return hitPoint;
    }
    public void ChkNeighbors()
    {
        foreach (var item in StringPoints)
        {
            if (!_IsConnected(item))
            {
                for (int i = 0; i < StringPoints.Count; i++)
                {
                    if (Physics.Raycast(item.transform.position, StringPoints[i].transform.position - item.transform.position, out RaycastHit hit) && item != StringPoints[i] && !_IsConnected(StringPoints[i]) && StringPoints[i].transform.position.x != item.transform.position.x)
                    {

                        if (Vector3.Distance(hit.point, StringPoints[i].transform.position) < 0.5f)
                        {
                            Bonder Bond69 = new Bonder();
                            Bond69.StartObj = item;
                            Bond69.EndObj = StringPoints[i];
                            Bond69.MeshMat = StringMat;
                            Bonds.Add(Bond69);
                            Bond69.UpdateMesh();

                            // StringPoints.Remove(item);
                            //  StringPoints.Remove(Bond69.EndObj);
                            return;
                        }
                    }

                }
            }

        }
    }    
    IEnumerator StartMetronome()
    {
        if (beat == 4 || beat == 2) Onbeat = true;
        else Onbeat = false;
        yield return new WaitForSeconds(60/bpm);        
        if (beat > 3)
        {
            beat = 1;
        }
        else
        {
            beat++;
        }
        StartCoroutine(StartMetronome());
    }
    public bool _IsConnected(GameObject Hook)
    {
        foreach (var item in Bonds)
        {
            if (item.EndObj == Hook || item.StartObj == Hook)
            {
                return item.IsConnected();
            }
        }
        return false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "String" && canboost == true)
        {
            StartCoroutine(BoostCooldown());
            if (movementSpeed < 20f)
            {
                movementSpeed += 5f;
            }
            Vector3 movedir = Vector3.up + Vector3.right;
            if (moveDirection != Vector3.zero)
            {
                movedir = moveDirection;
            }
            AddImpact(movedir,75f,false);
        }
    }
    IEnumerator BoostCooldown()
    {
        canboost = false;
        yield return new WaitForSeconds(5f);
        canboost = true;
    }
}
[System.Serializable]
public class Bonder
{
    public GameObject StartObj;
    public GameObject EndObj;
    public GameObject _Mesh;
    public Material MeshMat;
    public bool IsConnected()
    {
        return (StartObj != null && EndObj != null);
    }
    public void DestroyFromLibrary(GameObject hagatanya)
    {
        if (hagatanya == StartObj)
        {
            GameObject.Destroy(StartObj);
            StartObj = null;
        }
        if (hagatanya == EndObj)
        {
            GameObject.Destroy(EndObj);
            EndObj = null;
        }       
    }
    public void UpdateMesh()
    {
        if (_Mesh != null)
        {
            GameObject.Destroy(_Mesh);
            _Mesh = null;
        }      
        if (StartObj != null && EndObj != null)
        {
           _Mesh = CreateMesh(StartObj.transform.position, StartObj.transform.position + Vector3.up * 2f, EndObj.transform.position, EndObj.transform.position + Vector3.up * 2f,MeshMat);
        }       
    }
    GameObject CreateMesh(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4,Material StringMat)
    {
        Vector3[] verticies = new Vector3[4];
        Vector2[] uv = new Vector2[4];
        int[] triangles = new int[6];
        verticies[0] = p1 += Vector3.one * 0.00001f;
        verticies[1] = p2;
        verticies[2] = p3;
        verticies[3] = p4;
        uv[0] = p1;
        uv[1] = p2;
        uv[2] = p3;
        uv[3] = p4;
        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;
        triangles[3] = 2;
        triangles[4] = 1;
        triangles[5] = 3;
        Mesh mesh = new Mesh();
        mesh.vertices = verticies;
        mesh.triangles = triangles;
        mesh.uv = uv;
        GameObject StringMesh = new GameObject("StringWall", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
        StringMesh.transform.tag = "String";
        StringMesh.GetComponent<MeshFilter>().mesh = mesh;
        StringMesh.GetComponent<MeshRenderer>().material = StringMat;
        StringMesh.GetComponent<MeshCollider>().sharedMesh = mesh;
        StringMesh.GetComponent<MeshCollider>().convex = true;
        StringMesh.GetComponent<MeshCollider>().isTrigger = true;
        verticies[0] = p1;
        mesh.vertices = verticies;
        return StringMesh;
    }
}
