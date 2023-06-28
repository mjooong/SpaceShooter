using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SideWall
{
    public bool m_IsColl = false;
    public GameObject m_SideWalls = null;
    public Material m_WallMaterial = null;

    public SideWall()
    {
        m_IsColl = false;
        m_SideWalls = null;
        m_WallMaterial = null;
    }
}

public class FollowCam : MonoBehaviour
{
    public Transform targetTr;      //������ Ÿ�� ���ӿ�����Ʈ�� Transform ����
    public float dist = 10.0f;      //ī�޶���� ���� �Ÿ�
    public float height = 3.0f;     //ī�޶��� ���� ����
    public float dampTrace = 20.0f; //�ε巯�� ������ ���� ����

    Vector3 m_PlayerVec = Vector3.zero;
    float rotSpeed = 10.0f;

    //--- Side Wall ����Ʈ ���� ����
    Vector3 a_CacVLen = Vector3.zero;
    Vector3 a_CacDirVec = Vector3.zero;

    GameObject[] a_SideWalls = null;
    [HideInInspector] public LayerMask m_WallLyMask = -1;
    List<SideWall> m_SW_List = new List<SideWall>();
    //--- Side Wall ����Ʈ ���� ����

    //--- ĳ���� ���� ���� ����
    public GameObject[] CharObjects;    //ĳ���� ����
    int    CharType = 0;
    //--- ĳ���� ���� ���� ����

    //--- ī�޶� ��ġ ���� ����
    float m_RotV = 0.0f;        //���콺 ���� ���۰� ���� ����
    float m_DefaltRotV = 25.2f; //���� ������ ȸ�� ����
    float m_MarginRotV = 22.3f; //�ѱ����� ���� ����
    float m_MinLimitV = -17.9f; //�� �Ʒ� ���� ����
    float m_MaxLimitV = 52.9f;  //�� �Ʒ� ���� ����
    float maxDist     = 4.0f;   //���콺 �� �ƿ� �ִ� �Ÿ� ���Ѱ�
    float minDist     = 2.0f;   //���콺 �� �� �ִ� �Ÿ� ���Ѱ�
    float zoomSpeed   = 0.7f;   //���콺 �� ���ۿ� ���� ���ξƿ� ����Ʈ ������

    Quaternion a_BuffRot;       //ī�޶� ȸ�� ���� ����
    Vector3    a_BuffPos;       //ī�޶� ȸ���� ���� ��ġ ��ǥ ���� ����
    Vector3    a_BasicPos = Vector3.zero;  //��ġ ���� ����
    //--- ī�޶� ��ġ ���� ����

    //--- �� ���� ���� ���� ����
    public static Vector3 m_RifleDir = Vector3.zero;    //�� ���� ����
    Quaternion a_RFCacRot;
    Vector3 a_RFCacPos = Vector3.forward;
    //--- �� ���� ���� ���� ����

    // Start is called before the first frame update
    void Start()
    {
        dist = 3.4f;
        height = 2.8f;

        //--- Side Wall ����Ʈ�� �����...
        m_WallLyMask = 1 << LayerMask.NameToLayer("SideWall");
        //"SideWall"�� ���̾ ���������� �����ɽ�Ʈ(����������) üũ ����...

        a_SideWalls = GameObject.FindGameObjectsWithTag("SideWall");
        if(0 < a_SideWalls.Length)
        {
            SideWall a_SdWall = null;
            for(int ii = 0; ii < a_SideWalls.Length; ii++)
            {
                a_SdWall = new SideWall();
                a_SdWall.m_IsColl = false;
                a_SdWall.m_SideWalls = a_SideWalls[ii];
                a_SdWall.m_WallMaterial =
                    a_SideWalls[ii].GetComponent<Renderer>().material;
                WallAlphaOnOff(a_SdWall.m_WallMaterial, false);
                m_SW_List.Add(a_SdWall);
            }//for(int ii = 0; ii < a_SideWalls.Length; ii++)
        }//if(0 < a_SideWalls.Length)
        //--- Side Wall ����Ʈ�� �����...

        //--- ī�޶� ��ġ ���
        m_RotV = m_DefaltRotV;  //���� ������ ȸ�� ����
        //--- ī�޶� ��ġ ���

        if (SceneManager.GetActiveScene().name == "scLevel02")
            m_RotV = 10.2f;

    }//void Start()

    void Update()
    {
        if (GameMgr.s_GameState == GameState.GameEnd)
            return;

        if (Input.GetMouseButton(0) == true || Input.GetMouseButton(1) == true)
        {
            if (GameMgr.IsPointerOverUIObject() == true)
                return;

            ////--- ī�޶� ���Ʒ� �ٶ󺸴� ���� ������ ���� �ڵ�
            //height -= (rotSpeed * Time.deltaTime * Input.GetAxis("Mouse Y"));
            //if (height < 0.1f)
            //    height = 0.1f;

            //if (5.7f < height)
            //    height = 5.7f;
            ////--- ī�޶� ���Ʒ� �ٶ󺸴� ���� ������ ���� �ڵ�

            //--- (����ǥ�踦 ������ǥ��� ȯ���ϴ� �κ�)
            rotSpeed = 235.0f;  //ī�޶� ���Ʒ� ȸ�� �ӵ�
            m_RotV -= (rotSpeed * Time.deltaTime * Input.GetAxisRaw("Mouse Y"));
            //���콺�� ���Ʒ��� �������� �� ��
            if (m_RotV < m_MinLimitV)
                m_RotV = m_MinLimitV;
            if (m_MaxLimitV < m_RotV)
                m_RotV = m_MaxLimitV;
            //--- (����ǥ�踦 ������ǥ��� ȯ���ϴ� �κ�)
        }//if (Input.GetMouseButton(0) == true || Input.GetMouseButton(1) == true)

        //--- ī�޶� ���� �ܾƿ�
        if(Input.GetAxis("Mouse ScrollWheel") < 0 && dist < maxDist)
        {
            dist += zoomSpeed;
        }

        if(Input.GetAxis("Mouse ScrollWheel") > 0 && dist > minDist)
        {
            dist -= zoomSpeed;
        }
        //--- ī�޶� ���� �ܾƿ�

        //--- Rifle ���� ���
        a_RFCacRot = Quaternion.Euler(
                        Camera.main.transform.eulerAngles.x - m_MarginRotV,
                        targetTr.eulerAngles.y,
                        0.0f);
        m_RifleDir = a_RFCacRot * a_RFCacPos;
        //--- Rifle ���� ���

    }//void Update()

    //Update �Լ� ȣ�� ���� �� ���� ȣ��Ǵ� �Լ��� LateUpdate ���
    //������ Ÿ���� �̵��� ����� ���Ŀ� ī�޶� �����ϱ� ���� LateUpdate ���
    // Update is called once per frame
    void LateUpdate()
    {
        m_PlayerVec = targetTr.position;
        m_PlayerVec.y = m_PlayerVec.y + 1.2f;

        ////--- ī�޶� ��ġ ��� �ִ� ���밭�� �ҽ� 
        ////ī�޶��� ��ġ�� ��������� dist ������ŭ �������� ��ġ�ϰ�
        ////height ������ŭ ���� �ø�
        //transform.position = Vector3.Lerp(transform.position,
        //                                    targetTr.position
        //                                    - (targetTr.forward * dist)
        //                                    + (Vector3.up * height),
        //                                    Time.deltaTime * dampTrace);
        ////--- ī�޶� ��ġ ��� �ִ� ���밭�� �ҽ� 

        //-- (����ǥ�踦 ������ǥ��� ȯ���ϴ� �κ�)
        a_BuffRot = Quaternion.Euler(m_RotV, targetTr.eulerAngles.y, 0.0f);
        a_BasicPos.x = 0.0f;
        a_BasicPos.y = 0.0f;
        a_BasicPos.z = -dist;
        a_BuffPos = m_PlayerVec + (a_BuffRot * a_BasicPos);
        transform.position = Vector3.Lerp(transform.position, a_BuffPos,
                                                Time.deltaTime * dampTrace);
        //-- (����ǥ�踦 ������ǥ��� ȯ���ϴ� �κ�)

        //ī�޶� Ÿ�� ���ӿ�����Ʈ�� �ٶ󺸰� ����
        transform.LookAt(m_PlayerVec);  //targetTr.position);

        //--- Wall ī�޶� ������ �浹 ó�� �κ�
        a_CacVLen = this.transform.position - targetTr.position;
        a_CacDirVec = a_CacVLen.normalized;
        GameObject a_FindObj = null;
        RaycastHit a_hitInfo;
        if(Physics.Raycast(targetTr.position + (-a_CacDirVec * 1.0f),
                a_CacDirVec, out a_hitInfo, a_CacVLen.magnitude + 4.0f,
                m_WallLyMask.value))
        {
            a_FindObj = a_hitInfo.collider.gameObject;
        }

        for (int ii = 0; ii < m_SW_List.Count; ii++)
        {
            if (m_SW_List[ii].m_SideWalls == null)
                continue;

            if (m_SW_List[ii].m_SideWalls == a_FindObj) //������Ѿ� �� ��
            {
                if (m_SW_List[ii].m_IsColl == false)
                {
                    WallAlphaOnOff(m_SW_List[ii].m_WallMaterial, true);
                    m_SW_List[ii].m_IsColl = true;
                }
            }//if(m_SW_List[ii].m_SideWalls == a_FindObj)
            else  //����ȭ ��Ű�� ���ƾ� �� ��
            {
                if (m_SW_List[ii].m_IsColl == true)
                {
                    WallAlphaOnOff(m_SW_List[ii].m_WallMaterial, false);
                    m_SW_List[ii].m_IsColl = false;
                }
            }
        }//for(int ii = 0; ii < m_SW_List.Count; ii++)
         //----- Wall ī�޶� ������ �浹 ó�� �κ�

        //if(Input.GetKeyDown(KeyCode.C) == true)
        //{
        //    CharacterChange();
        //}//if(Input.GetKeyDown(KeyCode.C) == true)

        //--- Rifle ���� ���
        a_RFCacRot = Quaternion.Euler(
                        Camera.main.transform.eulerAngles.x - m_MarginRotV,
                        targetTr.eulerAngles.y,
                        0.0f);
        m_RifleDir = a_RFCacRot * a_RFCacPos;
        //--- Rifle ���� ���

    }//void LateUpdate()

    void WallAlphaOnOff(Material mtrl, bool isOn = true)
    {
        if(isOn == true)
        {
            mtrl.SetFloat("_Mode", 3);  //Transparent
            mtrl.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mtrl.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mtrl.SetInt("_ZWrite", 0);
            mtrl.DisableKeyword("_ALPHATEST_ON");
            mtrl.DisableKeyword("_ALPHABLEND_ON");
            mtrl.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            mtrl.renderQueue = 3000;
            mtrl.color = new Color(1, 1, 1, 0.3f);
        }
        else
        {
            mtrl.SetFloat("_Mode", 0);  //Opaque
            mtrl.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mtrl.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            mtrl.SetInt("_ZWrite", 1);
            mtrl.DisableKeyword("_ALPHATEST_ON");
            mtrl.DisableKeyword("_ALPHABLEND_ON");
            mtrl.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mtrl.renderQueue = -1;
            mtrl.color = new Color(1, 1, 1, 1);
        }

    }//void WallAlphaOnOff(Material mtrl, bool isOn = true)

    void CharacterChange()
    {
        Vector3 a_Pos    = CharObjects[CharType].transform.position;
        Quaternion a_Rot = CharObjects[CharType].transform.rotation;
        CharObjects[CharType].SetActive(false);
        CharType++;
        if (1 < CharType)
            CharType = 0;
        CharObjects[CharType].SetActive(true);
        CharObjects[CharType].transform.position = a_Pos;
        CharObjects[CharType].transform.rotation = a_Rot;
        targetTr = CharObjects[CharType].transform;
    }
}
