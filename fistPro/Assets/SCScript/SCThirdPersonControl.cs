using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCThirdPersonControl : MonoBehaviour
 {

        public Animator m_Animator;
        Rigidbody m_Rigidbody;
    // Use this for initialization
    void Start()
        {
            m_Animator = GetComponent<Animator>();
            m_Rigidbody = GetComponent<Rigidbody>();
        //冻结刚体的x轴旋转，y轴旋转，z轴旋转
            m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void FixedUpdate()
        {
            m_Animator.SetFloat("Forward", 0.5f, 0.1f, Time.deltaTime);
            m_Animator.speed = 1;
        }

    //public void OnAnimatorMove()
    //{
    //    // we implement this function to override the default root motion.
    //    // this allows us to modify the positional speed before it's applied.

    //    Vector3 v = (m_Animator.deltaPosition * 1.0f) / Time.deltaTime;
    //    // we preserve the existing y part of the current velocity.
    //    v.y = m_Rigidbody.velocity.y;
    //    m_Rigidbody.velocity = v;

    //}
}

