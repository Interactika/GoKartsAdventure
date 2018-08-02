using System.Collections;
using System.Collections.Generic;
using UnityEngine.PostProcessing;
using UnityEngine.UI;
using UnityEngine;

public class Player : MonoBehaviour
{

    [SerializeField]
    private PostProcessingBehaviour _PostProcessingBehaviour;
    [SerializeField]
    private Transform _ShootOutPos;
    [SerializeField]
    private GameObject[] _Effects;
    [SerializeField]
    private GameObject[] _Projectiles;
    [SerializeField]
    private AudioClip[] _AudioEffects;
    [SerializeField]
    private GameObject _MachineGunLight;

    [SerializeField]
    int _currentAmmo;
    [SerializeField]
    private Slider _AmmoSlider;
    [SerializeField]
    private Transform Wheels;
    private CharacterController _Controller;
    private AudioSource _AudioSource;
    private static float decreasedValue = 0.0f;
    private static float increasedValue = 0.0f;
    private static float EnergyIncrement = 0.0f;
    protected float _maxVelocity = 90f;
    private float _Sensitivity = 2f;
    private float _Gravity = 9.81f;
    private float _canShoot;
    private float energy = 0f;
    private float maxEnergy = 100.0f;

    private int _maxAmmo = 100;

    private float _canReloadEnergy;
    private float _delay = 3.0f;

    private bool _reloadingMachineGun = false;
    private bool _isReloadingEnergy = false;
    private bool _HasExploded = false;
    private Vector3 _velocity;
    private List<Transform> _Wheels = new List<Transform>();



    // Use this for initialization
    void Start()
    {
        //Cursor.visible = false;   
        energy = maxEnergy;
        Cursor.lockState = CursorLockMode.Locked;
        _Controller = GetComponent<CharacterController>();
        _AudioSource = GetComponent<AudioSource>();
        //_Animator = GetComponent<Animator>();
        _currentAmmo = _maxAmmo;
        _AmmoSlider.value = _currentAmmo;
        foreach (Transform item in Wheels)
        {
            _Wheels.Add(item);
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        _AmmoSlider.value = _currentAmmo;
        if (Input.GetMouseButton(0) && _currentAmmo > 0)
        {
            Shoot();
            _Effects[2].SetActive(true);
            var bulletEject = _Effects[2].GetComponent<ParticleSystem>();
            if (!bulletEject.isPlaying)
            {
                bulletEject.Play();
            }

        }
        else
        {
            _MachineGunLight.SetActive(false);
            _Effects[2].SetActive(false);

            _AudioSource.Stop();
            if (_currentAmmo <= 0 && !_reloadingMachineGun)
            {
                _reloadingMachineGun = true;
                StartCoroutine(ReloadMachineGun());
            }

        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            MisilePowerUp();
        }

        if (Input.GetKey(KeyCode.Q))
        {
            BoostPowerUp();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            MinesPowerUp();
        }

        CameraRotation();
        CalculateMovement();


        // cada 5 segundos checar si la energía está en 100

        if (Time.time > _canReloadEnergy)
        {
            if (energy < 100f && !_isReloadingEnergy)
            {
                // this._isReloadingEnergy = false;
                Debug.Log("Puedes recargar, tienes: " + energy);
                StartCoroutine(ReloadEnergy());
                _isReloadingEnergy = true;
            }
            else
            {
                Debug.Log("No hacer nada");
                //this._isReloadingEnergy = false;
            }
            _canReloadEnergy = Time.time + 5f;
        }
    }

    void CalculateMovement()
    {
        Vector3 direction = new Vector3(0, 0, Input.GetAxis("Vertical"));
        Mathf.Lerp(_maxVelocity, 0, increasedValue);
        _velocity = direction * increasedValue;

        if (direction.z > 0)
        {
            ChromaticAberrationModel.Settings chaSettings = _PostProcessingBehaviour.profile.chromaticAberration.settings;
            chaSettings.intensity = direction.z;

            _PostProcessingBehaviour.profile.chromaticAberration.settings = chaSettings;


            increasedValue += 5f * Time.deltaTime;
            if (increasedValue > _maxVelocity)
            {
                increasedValue = _maxVelocity;
            }

            if (Input.GetKey(KeyCode.F))
            {

                //_Animator.SetBool("Frenado", true);
                _velocity = direction * Mathf.Lerp(_velocity.z, 0, decreasedValue);
                decreasedValue += .5f * Time.deltaTime;
            }
            else
            {
                //_Animator.SetBool("Frenado", false);
                decreasedValue = 0.0f;
            }
        }
        else
        {

            increasedValue = 0.0f;
        }


        //Debug.Log(increasedValue);
        _velocity.y -= _Gravity;

        _velocity = transform.transform.TransformDirection(_velocity);

        _Controller.Move(_velocity * Time.deltaTime);
        // foreach (Transform wheel in _Wheels)
        // {
        //    wheel.rotation = Quaternion.FromToRotation(Vector3.up, transform.forward);
        // }
    }

    void CameraRotation()
    {
        float MouseX = Input.GetAxis("Mouse X");
        Vector3 newRotation = transform.eulerAngles;
        newRotation.y += MouseX * _Sensitivity;
        transform.localEulerAngles = newRotation;

        //Queremos hacer que la camará cambie su posición en z al acelerar
        //Obtenemos primero los datos de la aceleración
        float temp_a = Input.GetAxis("Vertical");
        float max = -3.5f;
        if (temp_a > 0)
        {
            Camera.main.transform.localPosition = new Vector3(0, 1.5f, max - temp_a);
        }

    }

    void Shoot()
    {
        _AudioSource.clip = _AudioEffects[0];
        _AudioSource.loop = true;

        _MachineGunLight.SetActive(false);
        if (Time.time > _canShoot)
        {
            _MachineGunLight.SetActive(true);

            _canShoot = Time.time + .02f;
        }



        if (!_AudioSource.isPlaying)
        {
            _AudioSource.Play();
        }

        _currentAmmo--;
        Vector3 fwd = _ShootOutPos.TransformDirection(Vector3.forward);
        RaycastHit hitInfo;
        if (Physics.Raycast(_ShootOutPos.position, fwd, out hitInfo, 50))
        {
            if (hitInfo.transform.tag == "Enemy")
            {
                hitInfo.transform.GetComponent<Enemy>().GetDamage(3.4f);
                GameObject hitmarker = Instantiate(_Effects[0], hitInfo.point, Quaternion.LookRotation(hitInfo.normal)) as GameObject;
                Destroy(hitmarker, 1f);
            }
        }

    }

    void MisilePowerUp()
    {
        //80 de energia
        if (energy > 0 && energy > 80)
        {
            Transform enemyPos;
            Collider[] colliders = Physics.OverlapSphere(transform.position, 15);
            foreach (Collider item in colliders)
            {
                // recorremos el arreglo de colliders
                // el misil se disparará
                // al primero que encontremos con el tag de enemigo
                if (item.tag == "Enemy")
                {
                    energy -= 80;
                    Debug.Log("Se ha encontrado un enemigo");
                    var misile = Instantiate(_Projectiles[0], _ShootOutPos.position, _ShootOutPos.rotation);
                    //misile.GetComponent<Rigidbody>().velocity = transform.up * 50;

                    enemyPos = item.transform;
                    StartCoroutine(SeekAndDestroy(enemyPos, misile));
                    return;
                }
                else
                {
                    print("No se encontró objetivo");
                }
            }

            Debug.Log("Usando misil" + energy);
        }
        else
        {
            Debug.Log("No tienes suficientes puntos para usar Misil");
        }
    }

    void MinesPowerUp()
    {
        //30 de energia

        if (energy > 0 && energy > 30)
        {
            energy -= 30;

            Debug.Log("Usando Minas");
        }
        else
        {
            Debug.Log("No tienes suficientes puntos para usar Minas");
        }
    }

    void BoostPowerUp()
    {
        //15 de energia por segundo
        if (energy > 0 && energy > 15)
        {
            energy -= 15 * Time.deltaTime;

            float percent = _maxVelocity / 2;

            increasedValue = (_maxVelocity + percent);


            Debug.Log("Usando Boost" + increasedValue);
        }
        else
        {
            Debug.Log("No tienes suficientes puntos para usar Boost" + increasedValue);
            _maxVelocity = 90f;
        }
    }

    // void ReloadEnergy()
    // {
    //         Mathf.Lerp(energy, maxEnergy, EnergyIncrement);
    //         EnergyIncrement += 0.5f;//* Time.deltaTime;

    //         if (EnergyIncrement >= maxEnergy)
    //         {
    //             EnergyIncrement = maxEnergy;
    //         }
    //         Debug.Log(EnergyIncrement + " " + _isReloadingEnergy);
    // }

    IEnumerator ReloadEnergy()
    {
        yield return new WaitForSeconds(5f);
        energy = maxEnergy;
        _isReloadingEnergy = false;
    }

    IEnumerator SeekAndDestroy(Transform position, GameObject misile)
    {
        while (Vector3.Distance(misile.transform.position, position.position) > 0.05f)
        {
            misile.transform.position = Vector3.Lerp(misile.transform.position, position.position, 7 * Time.deltaTime);

            yield return null;
        }
        var explosion = Instantiate(_Effects[1], misile.transform.position, misile.transform.rotation);
        Destroy(misile, 3f);
        Destroy(explosion, 3f);
        print("Se ha alcanzado el objetivo.");

        yield return new WaitForSeconds(3f);

        print("Mi corutina ha terminado.");
    }
    IEnumerator ReloadMachineGun()
    {
        yield return new WaitForSeconds(1f);
        _currentAmmo = _maxAmmo;
        _reloadingMachineGun = false;
    }


}
