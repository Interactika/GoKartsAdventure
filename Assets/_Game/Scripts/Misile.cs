using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Misile : MonoBehaviour {

	private AudioSource _AudioSource;
	void Start()
	{
		_AudioSource = GetComponent<AudioSource>();
	}
	void OnTriggerEnter(Collider Col)
	{
		if(Col.tag == "Enemy"){
            Col.GetComponent<Enemy>().GetDamage(50);
			_AudioSource.Play();
            Debug.Log(Col.tag);
        }
	}
}
