using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;


public class Enemy : MonoBehaviour {

	// Use this for initialization
	[SerializeField]
	private Slider _lifeBar;
	private AudioSource _audio;
	protected float _currentLife;
	private float _maxLife = 100f;
	[SerializeField]
	private GameObject _Explosion;
	private bool isExploting = false;
	void Start () {
		_audio = GetComponent<AudioSource>();
		_currentLife = _maxLife;
		_lifeBar.value = _currentLife;
	}
	
	// Update is called once per frame
	void Update () {
		if(_currentLife <= 0 && !isExploting){
			Debug.Log("Explotar");
			isExploting = true;
			Explote();
		}
	}

	public void GetDamage(float damage)
	{
		_currentLife -= damage;		
		_lifeBar.value = _currentLife;
		if(_lifeBar.value < 50)
		{
			_lifeBar.GetComponentInChildren<RectTransform>().GetComponentInChildren<Image>().color = Color.red;
		}
	}

	private void Explote()
	{		
		_audio.Play();
		
		var explosion = Instantiate(_Explosion,transform.position,transform.rotation);
		transform.GetComponent<MeshRenderer>().enabled = false;
		transform.GetComponent<Collider>().enabled = false;
		Destroy(explosion,2f);
		Destroy(this.transform.gameObject,8f);
	}

}
