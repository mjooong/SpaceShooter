using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//�ݵ�� �ʿ��� ������Ʈ�� ����� �ش� ������Ʈ�� �����Ǵ� ���� �����ϴ� Attribute
[RequireComponent(typeof(AudioSource))]
public class FireCtrl : MonoBehaviour
{
    //�Ѿ� ������
    public GameObject bullet;
    //�Ѿ� �߻���ǥ
    public Transform firePos;

    float fireDur = 0.11f;

    //�Ѿ� �߻� ����
    public AudioClip fireSfx;
    //AudioSource ������Ʈ�� ������ ����
    private AudioSource source = null;

    //MuzzleFlash�� MeshRenderer ������Ʈ ���� ����
    public MeshRenderer muzzleFlash;

    //--- ������ ������ ���� ����
    public GameObject LaserPointer = null;
    LayerMask m_LaserMask = -1;
    //--- ������ ������ ���� ����

    //����ź ������
    public GameObject m_Grenade;

    // Start is called before the first frame update
    void Start()
    {
        //AudioSource ������Ʈ�� ������ �� ������ �Ҵ�
        source = GetComponent<AudioSource>();
        //���ʿ� MuzzleFlash MeshRenderer �� ��Ȱ��ȭ
        muzzleFlash.enabled = false;

        m_LaserMask = 1 << LayerMask.NameToLayer("PLAYER");
        m_LaserMask |= 1 << LayerMask.NameToLayer("BULLET");
        m_LaserMask |= 1 << LayerMask.NameToLayer("E_BULLET");
        m_LaserMask = ~m_LaserMask; //Ư�� ���̾ ����
    }

    // Update is called once per frame
    void Update()
    {
        if (GameMgr.s_GameState == GameState.GameEnd)
            return;

        fireDur -= Time.deltaTime;
        if (fireDur <= 0.0f)
        {
            fireDur = 0.0f;

            //���콺 ���� ��ư�� Ŭ������ �� Fire �Լ� ȣ��
            if (Input.GetMouseButton(0))
            {
                if(GameMgr.IsPointerOverUIObject() == false)
                    Fire();

                fireDur = 0.11f;
            }
        }//if (fireDur <= 0.0f)


        //--- LaserPointer ǥ��
        if (Physics.Raycast(firePos.position, FollowCam.m_RifleDir.normalized,
                                    out RaycastHit hit, 100.0f, m_LaserMask) == true)
        {
            LaserPointer.transform.position =
                            hit.point + (firePos.position - hit.point) * 0.07f;
            LaserPointer.transform.rotation =
                             Quaternion.LookRotation(hit.normal, Vector3.up);
            LaserPointer.transform.eulerAngles =
                            new Vector3(90.0f,
                                    LaserPointer.transform.eulerAngles.y,
                                    LaserPointer.transform.eulerAngles.z);
            float a_Dist = Vector3.Distance(firePos.position,
                                        LaserPointer.transform.position);
            a_Dist = a_Dist * 0.3f;
            if (a_Dist < 2.0f)
                a_Dist = 2.0f;
            if (5.0f < a_Dist)
                a_Dist = 5.0f;
            LaserPointer.transform.localScale = new Vector3(a_Dist, a_Dist, a_Dist);
        }
        else
        {
            if (FollowCam.m_RifleDir.magnitude <= 0.0f)
                return;

            LaserPointer.transform.position = firePos.position +
                                        FollowCam.m_RifleDir.normalized * 90.0f;

            LaserPointer.transform.rotation =
                            Quaternion.LookRotation(-FollowCam.m_RifleDir.normalized,
                                        Vector3.up);
            LaserPointer.transform.eulerAngles =
                                    new Vector3(90.0f,
                                    LaserPointer.transform.eulerAngles.y,
                                    LaserPointer.transform.eulerAngles.z);
        }
        //----- LaserPointer ǥ��

    }//void Update()


    void Fire()
    {
        //�������� �Ѿ��� �����ϴ� �Լ�
        CreateBullet();
        //���� �߻� �Լ�
        source.PlayOneShot(fireSfx, 0.2f);
        //GameMgr.Inst.PlaySfx(firePos.position, fireSfx);

        //��� ��ٸ��� ��ƾ�� ���� �ڷ�ƾ �Լ��� ȣ��
        StartCoroutine(this.ShowMuzzleFlash());
    }

    void CreateBullet()
    {
        //Bullet �������� �������� ����
        Instantiate(bullet, firePos.position, firePos.rotation);
    }

    //MuzzleFlash Ȱ�� / ��Ȱ��ȭ�� ª�� �ð� ���� �ݺ�
    IEnumerator ShowMuzzleFlash()
    {
        //MuzzleFlash �������� �ұ�Ģ�ϰ� ����
        float scale = Random.Range(1.0f, 2.0f);
        muzzleFlash.transform.localScale = Vector3.one * scale;

        //MuzzleFlash�� Z���� �������� �ұ�Ģ�ϰ� ȸ����Ŵ
        Quaternion rot = Quaternion.Euler(0, 0, Random.Range(0, 360));
        muzzleFlash.transform.localRotation = rot;

        //Ȱ��ȭ���� ���̰� ��
        muzzleFlash.enabled = true;

        //�ұ�Ģ���� �ð� ���� Delay�� ���� MeshRenderer�� ��Ȱ��ȭ
        yield return new WaitForSeconds(Random.Range(0.01f, 0.03f)); //Random.Range(0.05f, 0.3f));

        //��Ȱ��ȭ�ؼ� ������ �ʰ� ��
        muzzleFlash.enabled = false;
    }

    public void FireGrenade()
    {
        GameObject a_Grenade = Instantiate(m_Grenade,
                                firePos.position, firePos.rotation);
        if(a_Grenade != null)
        {
            GrenadeCtrl a_Grd = a_Grenade.GetComponent<GrenadeCtrl>();
            if (a_Grd != null)
                a_Grd.SetForwardDir(FollowCam.m_RifleDir.normalized);
        }
    }//public void FireGrenade()
}
