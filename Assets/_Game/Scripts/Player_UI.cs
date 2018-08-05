using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Player_UI : MonoBehaviour
{

    [SerializeField]
    private Slider _HealthSlider, _EnergySlider, _BulletSlider;

    // Use this for initialization
    void Start()
    {
        _HealthSlider.minValue = 0;
        _EnergySlider.minValue = 0;
        _BulletSlider.minValue = 0;

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GetData(int healt, int bullets)
    {
        _HealthSlider.value = healt;
        _BulletSlider.value = bullets;
    }

    public void SetEnergy(float energy)
    {		
		_EnergySlider.value = energy;
    }
}
