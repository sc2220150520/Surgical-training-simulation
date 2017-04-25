using UnityEngine;
using UnityEngine.UI;

public class SCButtonEvent : MonoBehaviour {
    Animator m_animator;
    GameObject player;
    bool m_director;
    int i;
    // Use this for initialization
    void Start () {

        player = GameObject.FindWithTag("Player");
        m_animator = player.GetComponent<Animator>();
        m_director = false;

        //获取按钮游戏对象
        GameObject btnObj = gameObject.transform.FindChild("Button").gameObject;
       //获取按钮脚本组件
        Button btn = (Button)btnObj.GetComponent<Button>();
        //添加点击侦听
        btn.onClick.AddListener(this.onClick);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void onClick()
    {
        GameObject gametemp = GameObject.FindWithTag("Patient");
        if (player)
        {
            player.transform.LookAt(new Vector3(gametemp.transform.position.x,player.transform.position.y,gametemp.transform.position.z));
            m_director = true;
        }
        else
        {
            Debug.Log("big");
        }
    }

    private void FixedUpdate()
    {
        if(m_director)
        {
            if (m_animator != null)
            {
                m_animator.SetFloat("Forward", 0.8f, 0.1f, Time.deltaTime);
            }
        }
       
    }
}
