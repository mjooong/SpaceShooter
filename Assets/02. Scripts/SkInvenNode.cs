using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkInvenNode : MonoBehaviour
{
    [HideInInspector] public SkillType m_SkType;
    [HideInInspector] public Text m_SkCountText;      //��ų ī��Ʈ �ؽ�Ʈ

    private void Awake()
    {
        m_SkCountText = GetComponentInChildren<Text>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Button a_BtnCom = this.GetComponent<Button>();
        if (a_BtnCom != null)
            a_BtnCom.onClick.AddListener(() =>
            {
                if (GlobalValue.g_SkillCount[(int)m_SkType] <= 0)
                    return; //��ų �������� ����� �� ����

                PlayerCtrl a_Palyer = GameObject.FindObjectOfType<PlayerCtrl>();
                if (a_Palyer != null)
                    a_Palyer.UseSkill_Item(m_SkType);

                int a_SkCount = GlobalValue.g_SkillCount[(int)m_SkType];
                if (m_SkCountText != null)
                    m_SkCountText.text = a_SkCount.ToString();
            });
    }

    //// Update is called once per frame
    //void Update()
    //{
        
    //}
}
