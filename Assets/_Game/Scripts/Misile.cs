using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Misile : MonoBehaviour {

	private AudioSource _AudioSource;

	[SerializeField]
	private GameObject _ExplosionEffect;
	void Start()
	{
		_AudioSource = GetComponent<AudioSource>();
	}
	void OnTriggerEnter(Collider Col)
	{
		if(Col.tag == "Enemy"){
            Col.GetComponent<Enemy>().GetDamage(50);
			_ExplosionEffect.GetComponent<ParticleSystem>().Play();
			_AudioSource.Play();
            var explosion = Instantiate(_ExplosionEffect,transform.position,transform.rotation);
			StartCoroutine(Deactive(explosion));
        }
	}

	IEnumerator Deactive(GameObject effect){
		Destroy(effect,2f);
		yield return new WaitForSeconds(2);
	}
}
