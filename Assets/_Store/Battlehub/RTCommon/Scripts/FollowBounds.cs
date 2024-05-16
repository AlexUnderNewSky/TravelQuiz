using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowBounds : MonoBehaviour {

[SerializeField]
private Collider followWhich;


	// Use this for initialization
	void Start () {
		if(followWhich == null)
		followWhich = transform.parent.GetComponent<Collider>();	
		
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = followWhich.bounds.center;
		transform.localScale = followWhich.bounds.extents*2;
	}
}
