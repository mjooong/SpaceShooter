using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeCtrl : MonoBehaviour
{
    //���� ȿ�� ��ƼŬ ���� ����
    public GameObject expEffect;

    //���� ���� Ÿ�̸�
    float timer = 2.0f;

    //������ ������ �ؽ��� �迭
    public Texture[] textures;

    //����ź �߻� �ӵ�
    float speed = 100.0f;
    Vector3 m_ForwardDir = Vector3.zero;

    bool isRot = true;

    // Start is called before the first frame update
    void Start()
    {
        int idx = Random.Range(0, textures.Length);
        GetComponentInChildren<MeshRenderer>().material.mainTexture = textures[idx];

        speed = 500.0f;

        //-- ���ư��� ���� ������
        transform.forward = m_ForwardDir;

        transform.eulerAngles =
                    new Vector3(20.0f, transform.eulerAngles.y, transform.eulerAngles.z);

        GetComponent<Rigidbody>().AddForce(m_ForwardDir * speed);
    }

    // Update is called once per frame
    void Update()
    {
        if(0.0f < timer)
        {
            timer -= Time.deltaTime;
            if(timer <= 0.0f)
            {
                ExpGrenade();
            }
        }//if(0.0f < timer)

        if(isRot == true)
        {
            transform.Rotate(new Vector3(Time.deltaTime * 190.0f, 0.0f, 0.0f), Space.Self);
        }

    }//void Update()

    void OnCollisionEnter(Collision coll)
    {
        isRot = false;
    }

    //����ź ���߽�ų �Լ�
    void ExpGrenade()
    {
        //���� ȿ�� ��ƼŬ ����
        GameObject explosion = Instantiate(expEffect,
                                    transform.position, Quaternion.identity);

        Destroy(explosion, 
            explosion.GetComponentInChildren<ParticleSystem>().main.duration + 2.0f);

        //������ ������ �߽����� 10.0f �ݰ� ���� ���� �ִ� Collider ��ü ����
        Collider[] colls = Physics.OverlapSphere(transform.position, 10.0f);

        //������ Collider ��ü�� ���߷� ����
        MonsterCtrl a_MonCtrl = null;
        foreach(Collider coll in colls)
        {
            a_MonCtrl = coll.GetComponent<MonsterCtrl>();
            if (a_MonCtrl == null)
                continue;

            a_MonCtrl.TakeDamage(150);
        }

        //��� ����
        Destroy(gameObject);
    }

    public void SetForwardDir(Vector3 a_Dir)
    {
        m_ForwardDir = new Vector3(a_Dir.x, a_Dir.y + 0.5f, a_Dir.z);
    }
}
