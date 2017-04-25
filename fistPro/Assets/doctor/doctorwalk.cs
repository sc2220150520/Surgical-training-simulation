using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class doctorwalk : MonoBehaviour {
    public GameObject doctor;
	// Use this for initialization
	void Start () {
        Animation ani = gameObject.GetComponent<Animation>();
        // ani.Play("doctor_walk");
        ani.CrossFade("doctor_walk");
    }
	
	// Update is called once per frame
	void Update () {
        
    }
}
