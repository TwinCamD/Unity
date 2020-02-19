using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotation_along_chair : MonoBehaviour {

	// Use this for initialization
    public float rotationAngle = 0;
    private float lastRotAngle = 0;
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.Rotate((Vector3.up * (-rotationAngle + lastRotAngle)));
	    lastRotAngle = rotationAngle;
    }
}
