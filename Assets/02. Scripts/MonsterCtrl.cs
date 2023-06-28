using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterCtrl : MonoBehaviour
{
    //������ ���� ������ �ִ� Enumerable ���� ����
    public enum MonsterState { idle, trace, attack, die };

    //������ ���� ���� ������ ������ Enum ����
    public MonsterState monsterState = MonsterState.idle;

    private Transform playerTr;
    //private NavMeshAgent nvAgent;
    private Animator animator;

    //���� �����Ÿ�
    public float traceDist = 10.0f;
    //���� �����Ÿ�
    public float attackDist = 2.0f;

    //������ ��� ����
    private bool isDie = false;

    //���� ȿ�� ������
    public GameObject bloodEffect;
    //���� ��Į ȿ�� ������
    public GameObject bloodDecal;

    //���� ���� ����
    private int hp = 100;

    //GameMgr�� �����ϱ� ���� ����
    private GameMgr gameMgr;

    Rigidbody m_Rigid;

    //--- �Ѿ� �߻� ���� ����
    public GameObject bullet;   //�Ѿ� ������
    float m_BLTime = 0.0f;
    LayerMask m_LaserMask = -1;
    //--- �Ѿ� �߻� ���� ����

    //Awake() --> OnEnable() --> Start()

    // Start is called before the first frame update
    void Awake()
    {
        traceDist = 10.0f;  //100.0f;
        attackDist = 1.6f;  //1.8f;

        //���� ����� Player�� Transform �Ҵ�
        playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();
        ////NavMeshAgent ������Ʈ �Ҵ�
        //nvAgent = this.gameObject.GetComponent<NavMeshAgent>();
        //Animator ������Ʈ �Ҵ�
        animator = this.gameObject.GetComponent<Animator>();

        gameMgr = GameObject.Find("GameMgr").GetComponent<GameMgr>();



        //////���� ����� ��ġ�� �����ϸ� �ٷ� ���� ����
        ////nvAgent.destination = playerTr.position;

        ////������ �������� ������ �ൿ ���¸� üũ�ϴ� �ڷ�ƾ �Լ� ����
        //StartCoroutine(this.CheckMonsterState());

        ////������ ���¿� ���� �����ϴ� ��ƾ�� �����ϴ� �ڷ�ƾ �Լ� ����
        //StartCoroutine(this.MonsterAction());

        m_Rigid = GetComponent<Rigidbody>();

    }//void Awake()

    void Start()
    {
        m_LaserMask = 1 << LayerMask.NameToLayer("Default");  //�ǹ���
        m_LaserMask |= 1 << LayerMask.NameToLayer("PLAYER");  //���ΰ�
    }

    ////�̺�Ʈ �߻� �� ������ �Լ� ����
    //void OnEnable() //Active�� ���� �� ȣ��
    //{
    //    //������ �������� ������ �ൿ ���¸� üũ�ϴ� �ڷ�ƾ �Լ� ����
    //    StartCoroutine(this.CheckMonsterState());

    //    //������ ���¿� ���� �����ϴ� ��ƾ�� �����ϴ� �ڷ�ƾ �Լ� ����
    //    StartCoroutine(this.MonsterAction());
    //}


    // Update is called once per frame
    void Update()
    {
        if (GameMgr.s_GameState == GameState.GameEnd)
            return;

        //���� �������̴� ���ΰ� ĳ���� ���׺갡 ���� ������... 
        if (playerTr.gameObject.activeSelf == false) 
            playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();
        //GameObject.FindWithTag("Player") �Լ��� ���� ��Ƽ�갡 ���� �ִ� ���ΰ��� ã����

        CheckMonStateUpdate();
        MonActionUpdate();

        if (isDie == false)
            m_Rigid.AddForce(Vector3.down * 100.0f); //�߷°� ������ �ֱ�

    }//void Update()

    float m_AI_Delay = 0.0f;
    //������ �������� ������ �ൿ ���¸� üũ�ϰ� monsterState �� ����
    void CheckMonStateUpdate()
    {
        if (isDie == true)
            return;

        //0.1�� �ֱ�θ� üũ�ϱ� ���� ������ ��� �κ�
        m_AI_Delay -= Time.deltaTime;
        if (0.0f < m_AI_Delay)
            return;

        m_AI_Delay = 0.1f;

        //���Ϳ� �÷��̾� ������ �Ÿ� ����
        float dist = Vector3.Distance(playerTr.position, transform.position);

        if (dist <= attackDist) //���ݰŸ� ���� �̳��� ���Դ��� Ȯ��
        {
            monsterState = MonsterState.attack;
        }
        else if (dist <= traceDist) //�����Ÿ� ���� �̳��� ���Դ��� Ȯ��
        {
            monsterState = MonsterState.trace;  //������ ���¸� �������� ����
        }
        else
        {
            monsterState = MonsterState.idle;   //������ ���¸� idle ���� ����
        }

    } //void CheckMonStateUpdate()

    //������ ���°��� ���� ������ ������ �����ϴ� �Լ�
    void MonActionUpdate()
    {
        if (isDie == true)
            return;

        switch (monsterState)
        {
            //idle ����
            case MonsterState.idle:
                //���� ����
                //Animator�� IsTrace ������ false�� ����
                animator.SetBool("IsTrace", false);
                break;

            //���� ����
            case MonsterState.trace:
                {
                    //-- �̵� ����
                    float a_MoveVelocity = 2.0f; //��� �ʴ� �̵� �ӵ�...
                    Vector3 a_MoveDir = Vector3.zero;
                    a_MoveDir = playerTr.position - transform.position;
                    float a_RayUDLimit = 3.0f;
                    if (a_MoveDir.y < -a_RayUDLimit || a_RayUDLimit < a_MoveDir.y)
                    { //���̰� �Ѱ�ġ
                        return;
                    }
                    a_MoveDir.y = 0.0f;
                    Vector3 a_StepVec = (a_MoveDir.normalized * Time.deltaTime * a_MoveVelocity);
                    transform.Translate(a_StepVec, Space.World);

                    if(0.1f < a_MoveDir.magnitude) //ĳ���� ȸ��
                    {
                        Quaternion a_TargetRot;
                        float a_RotSpeed = 7.0f;
                        a_TargetRot = Quaternion.LookRotation(a_MoveDir.normalized);
                        transform.rotation =
                                    Quaternion.Slerp(transform.rotation, a_TargetRot,
                                    Time.deltaTime * a_RotSpeed);
                    }//ĳ���� ȸ��
                    //-- �̵� ����

                    //Animator�� IsAttack ������ false�� ����
                    animator.SetBool("IsAttack", false);
                    //Animator�� IsTrace ������ true�� ����
                    animator.SetBool("IsTrace", true);
                }
                break;

            //���� ����
            case MonsterState.attack:
                {
                    //���� ����
                    //IsAttack�� true�� ������ attack State�� ����
                    animator.SetBool("IsAttack", true);

                    //���Ͱ� ���ΰ��� �����ϸ鼭 �ٶ� ������ �ؾ� �Ѵ�.
                    float a_RotSpeed = 6.0f;    //�ʴ� ȸ�� �ӵ�
                    Vector3 a_CacVLen = playerTr.position - transform.position;
                    a_CacVLen.y = 0.0f;
                    Quaternion a_TargetRot = Quaternion.LookRotation(a_CacVLen.normalized);
                    transform.rotation =
                                Quaternion.Slerp(transform.rotation, a_TargetRot,
                                                            Time.deltaTime * a_RotSpeed);
                    //���Ͱ� ���ΰ��� �����ϸ鼭 �ٶ� ������ �ؾ� �Ѵ�.
                }
                break;
        }//switch(monsterState)

        //--- �Ѿ� �߻�
        if(2 < GlobalValue.g_CurBlockNum) //���ʹ� 3������ �Ѿ˹߻�
            FireUpdate();
        //--- �Ѿ� �߻�

    } //void MonActionUpdate()

    //Bullet�� �浹 üũ
    void OnCollisionEnter(Collision coll)
    {
        if(coll.gameObject.tag == "BULLET")
        {
            //���� ȿ�� �Լ� ȣ��
            CreateBloodEffect( coll.transform.position + (coll.transform.forward * -0.3f) );

            //���� �Ѿ��� Damage�� ������ ���� hp ����
            hp -= coll.gameObject.GetComponent<BulletCtrl>().damage;
            if(hp <= 0)
            {
                MonsterDie();
            }

            //Bullet ����
            Destroy(coll.gameObject);

            //IsHit Trigger �� �߻���Ű�� Any State���� gothit�� ���̵�
            animator.SetTrigger("IsHit");
        }
    }

    //���� ��� �� ó�� ��ƾ
    void MonsterDie()
    {
        //����� ������ �±׸� Untagged�� ����
        gameObject.tag = "Untagged";

        //��� �ڷ�ƾ�� ����
        StopAllCoroutines();

        isDie = true;
        monsterState = MonsterState.die;
        animator.SetTrigger("IsDie");

        m_Rigid.useGravity = false;

        //���Ϳ� �߰��� Collider�� ��Ȱ��ȭ
        gameObject.GetComponentInChildren<CapsuleCollider>().enabled = false;

        foreach(Collider coll in gameObject.GetComponentsInChildren<SphereCollider>())
        {
            coll.enabled = false;
        }

        //GameUI�� ���ھ� ������ ���ھ� ǥ�� �Լ� ȣ��
        gameMgr.DispScore(1); //50);

        //���� ������Ʈ Ǯ�� ȯ����Ű�� �ڷ�ƾ �Լ� ȣ��
        StartCoroutine(this.PushObjectPool());

        //--- �������� ���� ������ ���
        GameMgr.Inst.SpawnCoin(this.transform.position);
        //--- �������� ���� ������ ���

    }//void MonsterDie()

    IEnumerator PushObjectPool()
    {
        yield return new WaitForSeconds(3.0f);

        //���� ���� �ʱ�ȭ
        isDie = false;
        hp = 100;
        gameObject.tag = "MONSTER";
        monsterState = MonsterState.idle;

        m_Rigid.useGravity = true;

        //���Ϳ� �߰��� Collider�� �ٽ� Ȱ��ȭ
        gameObject.GetComponentInChildren<CapsuleCollider>().enabled = true;

        foreach(Collider coll in gameObject.GetComponentsInChildren<SphereCollider>())
        {
            coll.enabled = true;
        }

        //���͸� ��Ȱ��ȭ
        gameObject.SetActive(false);
    }//IEnumerator PushObjectPool()

    void CreateBloodEffect(Vector3 pos)
    {
        //���� ȿ�� ����
        GameObject blood1 = (GameObject)Instantiate(bloodEffect, pos, Quaternion.identity);
        blood1.GetComponent<ParticleSystem>().Play();
        Destroy(blood1, 3.0f);

        //��Į ���� ��ġ - �ٴڿ��� ���� �ø� ��ġ ����
        Vector3 decalPos = transform.position + (Vector3.up * 0.05f);
        //��Į�� ȸ������ �������� ����
        Quaternion decalRot = Quaternion.Euler(90, 0, Random.Range(0, 360));

        //��Į ������ ����
        GameObject blood2 = (GameObject)Instantiate(bloodDecal, decalPos, decalRot);
        //��Į�� ũ�⵵ �ұ�Ģ������ ��Ÿ���Բ� ������ ����
        float scale = Random.Range(1.5f, 3.5f);
        blood2.transform.localScale = Vector3.one * scale;

        //5�� �Ŀ� ����ȿ�� �������� ����
        Destroy(blood2, 5.0f);
    }

    //�÷��̾ ������� �� ����Ǵ� �Լ�
    void OnPlayerDie()
    {
        //������ ���¸� üũ�ϴ� �ڷ�ƾ �Լ��� ��� ������Ŵ
        StopAllCoroutines();
        //������ �����ϰ� �ִϸ��̼��� ����
        animator.SetTrigger("IsPlayerDie");
    }

    void FireUpdate()
    {
        Vector3 a_PlayerPos = playerTr.position;
        a_PlayerPos.y = a_PlayerPos.y + 1.5f;
        Vector3 a_MonPos = transform.position;
        a_MonPos.y = a_MonPos.y + 1.5f;
        Vector3 a_CacDist = a_PlayerPos - a_MonPos;
        Vector3 a_H_Dist = a_CacDist;
        float a_RayUDLimit = 3.0f;

        bool isRayOK = false;
        if(Physics.Raycast(a_MonPos, a_H_Dist.normalized,
                            out RaycastHit hit, 100.0f, m_LaserMask) == true)
        {
            if(hit.collider.gameObject.layer == LayerMask.NameToLayer("PLAYER"))
            {
                isRayOK = true;
            }
        }

        if(isRayOK == true && traceDist < a_H_Dist.magnitude)
        {
            if(-a_RayUDLimit <= a_CacDist.y && a_CacDist.y <= a_RayUDLimit)
            { //���̰� �Ѱ�ġ
                //--- ���Ͱ� ���ΰ��� �����ϸ鼭 �ٶ� ������ �ؾ� �Ѵ�.
                Vector3 a_CacVLen = playerTr.position - transform.position;
                a_CacVLen.y = 0.0f;
                Quaternion a_TargetRot =
                                Quaternion.LookRotation(a_CacVLen.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                        a_TargetRot, Time.deltaTime * 6.0f);
            }
        }//if(isRayOK == true && traceDist < a_H_Dist.magnitude)

        m_BLTime = m_BLTime - Time.deltaTime;
        if(m_BLTime <= 0.0f)
        {
            m_BLTime = 0.0f;

            if (a_H_Dist.magnitude <= traceDist) //���� �Ÿ� �����̸� �׳� ����
                return; //���� �Ÿ� �ۿ� ������ �Ѿ��� ��� �ϱ� ���� �ڵ�

            if (a_CacDist.y < -a_RayUDLimit || a_RayUDLimit < a_CacDist.y)
                return; //������ ���� ��, �Ʒ� -3 ~ 3 ���̷� ���� �ɾ���
            //1���� �ִ� ���Ͱ� 2���� �ִ� ���ΰ��� ���������� ���ϰ� �ϱ� ���ؼ�...

            if (isRayOK == false) //���Ϳ� ���ΰ� ���̿� ���� ������ ����
                return;

            Vector3 a_StartPos = a_MonPos + a_CacDist.normalized * 1.5f;
            GameObject a_Bullet = Instantiate(bullet, a_StartPos, Quaternion.identity);
            a_Bullet.layer = LayerMask.NameToLayer("E_BULLET");
            a_Bullet.tag = "E_BULLET";
            a_Bullet.transform.forward = a_CacDist.normalized;

            float a_CacDF = (GlobalValue.g_CurBlockNum - 10) * 0.012f;
            if (a_CacDF < 0.0f)
                a_CacDF = 0.0f;
            if (1.0f < a_CacDF)
                a_CacDF = 1.0f; 

            m_BLTime = 2.0f - a_CacDF;

        }//if(m_BLTime <= 0.0f)

    }//void FireUpdate()

    public void TakeDamage(int a_Value)
    {
        if (hp <= 0.0f)
            return;

        //���� ȿ�� �Լ� ȣ��
        CreateBloodEffect(transform.position);

        hp -= a_Value;
        if (hp <= 0)
        {
            hp = 0;
            MonsterDie();
        }
        animator.SetTrigger("IsHit");
    }

}//public class MonsterCtrl
