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

    [Header("String Attack")]
    RaycastHit hitpoint;
    public List<Bonder> Bonds = new List<Bonder>();
    public List<GameObject> StringPoints = new List<GameObject>();
    [SerializeField] int StringHookCount = 12;

    [Header("ObjRef & TransRef")]

    [SerializeField] GameObject MusicalNoteBlock;   
    [SerializeField] Transform OutRayPos;
    [SerializeField] GameObject Hook;

    [Header("Other")]

    [SerializeField] UnityEvent OnStart, OnEnd;

    [Header("RingHolo")]

    int HoloLv = 1;
    [SerializeField] GameObject HoloRingObj;
    [SerializeField] float lerpduration = 500f;
    [SerializeField] float ScaleMulti = 1f;
    [SerializeField] UnityEvent TagPing;
    void Start()
    {
        StartCoroutine(StartMetronome());
        ability0 = new Ability()
        {
            ability = delegate
            {
                if (Onbeat && movementSpeed < 20)
                {
                    movementSpeed += 2.5f;
                }
                else if(!Onbeat)
                {
                    animator.SetTrigger("FailedBeat");
                    movementSpeed = GetOriginalSpeeed();
                }

                if (!isGroundeed())
                {
                    animator.SetTrigger("IsPlatform_Attack");

                    PlaceAirPlatform(transform.position, moveDirection, movementSpeed);
                    CmdAirPlatform(GetComponent<NetworkIdentity>() , transform.position, moveDirection, movementSpeed);
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
                if (StringHookCount == 0 || !isLocalPlayer)
                {
                    ability1.End.Invoke();
                    return;
                }

                Vector3 startpos = OutRayPos.position;
                Vector3 hitpoint_point = Vector3.zero;

                if (Physics.Raycast(OutRayPos.position, OutRayPos.forward, out hitpoint))
                {
                    hitpoint_point = hitpoint.point;
                }
                else
                {
                    ability1.End.Invoke();
                    return;
                }

                StringHookCount--;
                StartCoroutine(PlaceHook(hitpoint_point , startpos));
                CmdHookPlacePosition(GetComponent<NetworkIdentity>(), new Vector3[2] { hitpoint_point, startpos });
                
            },
            coolDown = 0.2f
            ,
            events = new UnityEvent[2] { OnStart, OnEnd }
        };
        ability_tag = new Ability()
        {
            ability = delegate
            {
                if (!isLocalPlayer) { ability_tag.End.Invoke(); return; } // to stop the other clients from creating a mess (or keeping them from replication the event twice)

                if (Onbeat)
                {
                    HoloLv++;
                }
                else
                { 
                    HoloLv = 1;
                    animator.SetTrigger("FailedBeat");                   
                }

                if (HoloLv == 3)
                {
                    TagPing.Invoke();
                    HoloLv = 1;
                }

                animator.SetInteger("HoloLv_Attack", HoloLv);
                StartCoroutine(HoloRing(HoloLv));

                CmdStartTagAttack(GetComponent<NetworkIdentity>(), HoloLv);

                ability_tag.End.Invoke();
            }
            ,coolDown = 0.2f
            ,events = new UnityEvent[2] { GetComponent<TagLogic>().StartTag, GetComponent<TagLogic>().EndTag}
            
        };

        if (isLocalPlayer)
        {
            Instantiate(Resources.Load("PointerCanvas"));
        }
    }

    new void Update()
    {
        base.Update();

        if (!isLocalPlayer) return;

        bool isRunning = moveDirection != Vector3.zero;
        animator.SetBool("IsWalk", isRunning);
        animator.SetBool("IsJump", !isGroundeed());
        animator.SetFloat("runSpeed", movementSpeed / 7);

        if (moveDirection != Vector3.zero)
        {
            playerModel.transform.forward = moveDirection;
        }

        HoloRingObj.transform.Rotate(Vector3.up * 50 * Time.deltaTime * HoloLv, Space.World);

        if (movementSpeed > 7f)
        {
            movementSpeed -= Time.deltaTime;
        }

        if (Input.GetMouseButton(2) && !GetDisableInput())
        {
            for (int i = 0; i < StringPoints.Count; i++)
            {
                CmdHookRemoval(GetComponent<NetworkIdentity>(), i , transform.position);
                RemoveAnyHooksInRange(i , transform.position);
            }
        }
    }

    IEnumerator HoloRing(int level)
    {
        if (level >= 2)
        {
            ScaleMulti += 2;
        }
        else if (level == 1)
        {
            ScaleMulti = 1f;
        }

        for (float i = 0; i < lerpduration; i += Time.deltaTime)
        {
            HoloRingObj.transform.localScale = Vector3.Lerp(HoloRingObj.transform.localScale, Vector3.one * ScaleMulti, 5 * Time.deltaTime );
            yield return null;
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
    public void ChkNeighbors()
    {
        foreach (var item in StringPoints)
        {
            if (!_IsConnected(item))
            {
                for (int i = 0; i < StringPoints.Count; i++)
                {

                    if (Physics.Raycast(item.transform.position, StringPoints[i].transform.position - item.transform.position, out RaycastHit hit) && item != StringPoints[i] && !_IsConnected(StringPoints[i]))
                    {
                        if (Vector3.Distance(hit.point, StringPoints[i].transform.position) < 0.5f)
                        {
                            Bonder Bond69 = new Bonder();
                            Bond69.StartObj = item;
                            Bond69.EndObj = StringPoints[i];
                            Bonds.Add(Bond69);
                            Bond69.UpdateMesh();
                            return;
                        }
                    }
                }
            }

        }
    }    
    IEnumerator StartMetronome()
    {
        if (beat == 4 || beat == 2) { Onbeat = true;  AudioManager.instance.Play("HiHat", transform.position, transform); }
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

        if (other.transform.tag == "String" && GetStringBond(other.gameObject).Boostable == true)
        {
            StartCoroutine(GetStringBond(other.gameObject).BoostCooldown());
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
            animator.SetTrigger("IsBoosted");
        }
    }
    Bonder GetStringBond(GameObject String)
    {
        for (int i = 0; i < Bonds.Count; i++)
        {
            if (Bonds[i]._Mesh == String)
            {
                return Bonds[i];
            }           
        }
        return null;
    }  
    void DestroyAccorodingly(GameObject HookPoint,Bonder bond)
    {       
        if (!_IsConnected(HookPoint))
        {
            Destroy(HookPoint);                      
        }
        else
        {
            bond.DestroyFromLibrary(bond.StartObj);
            bond.DestroyFromLibrary(bond.EndObj);
            Bonds.Remove(bond);          
        }
    }

    #region TagAttackReplication

    [Command]
    void CmdStartTagAttack(NetworkIdentity ntd , int i)
    {
        RpcStartTagAttack(ntd, i);
    }

    [ClientRpc]
    void RpcStartTagAttack(NetworkIdentity ntd, int i)
    {
        if (ntd.isLocalPlayer) return;

        StartCoroutine(HoloRing(i));

    }

    #endregion

    #region HooksAbilityReplication

    [Command]
    void CmdHookPlacePosition(NetworkIdentity ntd, Vector3[] vectors)
    {
        RpcHookPlacePosition(ntd, vectors);
    }

    [ClientRpc]
    void RpcHookPlacePosition(NetworkIdentity ntd, Vector3[] vectors)
    {
        if (ntd.isLocalPlayer) return;

        StartCoroutine(PlaceHook(vectors[0], vectors[1]));
    }

    IEnumerator PlaceHook(Vector3 hitpoint_point , Vector3 startpos)
    {

        animator.SetTrigger("IsThrowing");
        GameObject HookObj = Instantiate(Hook, OutRayPos.position, Quaternion.identity, null);
        HookObj.name = Random.Range(0, 12).ToString();

        Vector3 ThrowDirection = new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1));
        for (float i = 0; Vector3.Distance(HookObj.transform.position, hitpoint_point) > 0; i += Time.deltaTime)
        {
            HookObj.transform.Rotate(i,i,i);
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

    [Command]
    void CmdHookRemoval(NetworkIdentity ntd ,int i , Vector3 myPos)
    {
        RpcHookRemoval(ntd , i , myPos);
    }

    [ClientRpc]
    void RpcHookRemoval(NetworkIdentity ntd , int i , Vector3 myPos)
    {
        if (ntd.isLocalPlayer) return;

        ntd.GetComponent<beetcatin>().RemoveAnyHooksInRange(i , myPos);
    }

    void RemoveAnyHooksInRange(int i , Vector3 myPos)
    {
        if (Vector3.Distance(StringPoints[i].transform.position, myPos) < 2f)
        {
            animator.SetTrigger("IsPickingUp");
            GameObject stringtemp = StringPoints[i];
            Bonder JamesBond = GetBondOf(stringtemp);

            if (JamesBond != null)
            {
                StringPoints.Remove(JamesBond.StartObj);
                StringPoints.Remove(JamesBond.EndObj);

                DestroyAccorodingly(stringtemp, JamesBond);
                if (JamesBond != null)
                {
                    JamesBond.UpdateMesh();
                }
                StringHookCount += 2;
            }
            else
            {
                StringPoints.Remove(stringtemp);
                Destroy(stringtemp);
                StringHookCount += 1;
            }

            return;
        }
    }

    #endregion

    #region AirPlatformsReplication

    [Command]
    void CmdAirPlatform(NetworkIdentity ntd, Vector3 PlatformPos , Vector3 _moveDirection, float _movementSpeed)
    {
        RpcAirPlatform(ntd, PlatformPos , _moveDirection , _movementSpeed);
    }

    [ClientRpc]
    void RpcAirPlatform(NetworkIdentity ntd, Vector3 PlatformPos, Vector3 _moveDirection, float _movementSpeed)
    {
        if (ntd.isLocalPlayer) return;
        ntd.GetComponent<beetcatin>().PlaceAirPlatform(PlatformPos , _moveDirection, _movementSpeed);
    }

    void PlaceAirPlatform(Vector3 pos ,Vector3 _moveDirection ,float _movementSpeed )
    {
        GameObject TempBlock = Instantiate(MusicalNoteBlock, pos - Vector3.up + _moveDirection * _movementSpeed * 0.2f, Quaternion.identity, null);
        if (_moveDirection == Vector3.zero)
        {
            TempBlock.transform.forward = playerModel.transform.forward;
        }
        else
        {
            TempBlock.transform.forward = _moveDirection;
        }
        TempBlock.transform.GetComponent<MusicCatPlatFormManager>().UpdateCloudPlatform(_movementSpeed * 0.2f);
        //new Vector3(TempBlock.transform.localScale.x * movementSpeed * 0.07f, TempBlock.transform.localScale.y, TempBlock.transform.localScale.z * movementSpeed * 0.2f);
        Destroy(TempBlock, 2f);
    }

    #endregion
}
[System.Serializable]
public class Bonder
{
    public GameObject StartObj;
    public GameObject EndObj;
    public GameObject _Mesh;
    public bool Boostable = true;
    float cooldown = 5f;
    public bool IsConnected()
    {
        return (StartObj != null && EndObj != null);
    }
    public void DestroyFromLibrary(GameObject hagatanya)
    {
        if (hagatanya == StartObj)
        {
            StartObj = null;
        }
        if (hagatanya == EndObj)
        {            
            EndObj = null;
        }
        GameObject.Destroy(hagatanya);
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
            //_Mesh = CreateMesh(StartObj.transform.position, StartObj.transform.position + Vector3.up * 2f, EndObj.transform.position, EndObj.transform.position + Vector3.up * 2f,MeshMat);
            _Mesh = GameObject.Instantiate((GameObject)Resources.Load("strings") , (StartObj.transform.position + EndObj.transform.position) * 0.5f , Quaternion.LookRotation(StartObj.transform.position - EndObj.transform.position));
            _Mesh.transform.localScale = new Vector3(1, 1, Vector3.Distance(StartObj.transform.position , EndObj.transform.position) / 2);
            _Mesh.AddComponent<BoxCollider>();
            _Mesh.GetComponent<BoxCollider>().isTrigger = true;
            _Mesh.gameObject.tag = "String";

            StartObj.transform.LookAt(_Mesh.transform);
            EndObj.transform.LookAt(_Mesh.transform);
        }
    }
    public IEnumerator BoostCooldown()
    {
        Boostable = false;
        yield return new WaitForSeconds(cooldown);
        Boostable = true;
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
        StringMesh.GetComponent<MeshFilter>().mesh = mesh;
        StringMesh.GetComponent<MeshRenderer>().material = StringMat;
        StringMesh.GetComponent<MeshCollider>().sharedMesh = mesh;
        StringMesh.GetComponent<MeshCollider>().convex = true;
        StringMesh.GetComponent<MeshCollider>().isTrigger = true;
        StringMesh.transform.tag = "String";
        verticies[0] = p1;
        mesh.vertices = verticies;
        return StringMesh;
    }
}
