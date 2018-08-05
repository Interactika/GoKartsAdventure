using System.Collections;
using System.Collections.Generic;
using UnityEngine.PostProcessing;
using UnityEngine.UI;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Camera Setup")]
    [SerializeField]
    private GameObject[] _Cameras;
    [SerializeField]
    [Header("PostProcessing Setup")]
    private PostProcessingBehaviour _PostProcessingBehaviour;

    [Header("Effects Setup")]

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

    [Header("UI Setup")]

    [SerializeField]
    private Player_UI _PlayerUI;
    private AudioSource _AudioSource;
    private static float increasedVelocity = 0.0f;
    protected float _maxVelocity = 90f;
    private float _Sensitivity = 2f;
    private float _canShoot;
    private float energy = 0f;
    private float maxEnergy = 100.0f;

    private int _maxAmmo = 100;

    private float _canReloadEnergy;

    private bool _reloadingMachineGun = false;
    private bool _isReloadingEnergy = false;
    private GameObject _CurrentCamera;
    private int selection = 0;
    delegate void PowerUp();
    PowerUp _currentPowerUp;


    // Use this for initialization
    void Start()
    {
        Cursor.visible = false;
        energy = maxEnergy;
        Cursor.lockState = CursorLockMode.Locked;
        _AudioSource = GetComponent<AudioSource>();
        _currentAmmo = _maxAmmo;
        _PlayerUI.SetEnergy(energy);
    }

    // Update is called once per frame
    void Update()
    {   
        _PlayerUI.GetData(100,_currentAmmo);
        //Asiganmos las camaras
        rearview();
        SelectPowerUp();
        /*
        si el usuario quiere salir puede presionar la tecla escape
        y volver a esconder el mouse con click
        */
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Actualizar la cuenta de las balas


        //ya sea si presionas el boton izquierdo del raton o el gatillo derecho
        bool shoot = (Input.GetMouseButton(0) || Input.GetAxis("Shoot") > 0);

        //Si shoot es true y las balas son mayores que cero
        if (shoot && _currentAmmo > 0)
        {
            //Dispara activa efectos y sonido
            Shoot();
            _Effects[2].SetActive(true);
            var bulletEject = _Effects[2].GetComponent<ParticleSystem>();
            if (!bulletEject.isPlaying)
            {
                bulletEject.Play();
            }

        }
        else
        //de lo contrario se desactivan 
        {
            _MachineGunLight.SetActive(false);
            _Effects[2].SetActive(false);
            _AudioSource.Stop();


            //si nos quedamos sin municion y no está recargando
            if (_currentAmmo <= 0 && !_reloadingMachineGun)
            {
                _reloadingMachineGun = true;
                StartCoroutine(ReloadMachineGun());
            }

        }

        // se ejecuta la rotación de la cámara
        CameraRotation();



        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.JoystickButton2))
        {
            //disparar powerUps
            _currentPowerUp();
        }

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
    void CameraRotation()
    {
        float MouseX = Input.GetAxis("Horizontal");
        Vector3 newRotation = transform.eulerAngles;
        newRotation.y += MouseX * _Sensitivity;
        transform.localEulerAngles = newRotation;

        //Queremos hacer que la camará cambie su posición en z al acelerar
        //Obtenemos primero los datos de la aceleración
        float temp_a = Input.GetAxis("Acelerate");
        float max = -5f;
        if (temp_a > 0)
        {
            _CurrentCamera.transform.localPosition = new Vector3(0, .8f, max - temp_a);
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
                    _PlayerUI.SetEnergy(energy);
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
            _PlayerUI.SetEnergy(energy);
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
            _PlayerUI.SetEnergy(energy);
            float percent = _maxVelocity / 2;

            increasedVelocity = (_maxVelocity + percent);


            Debug.Log("Usando Boost" + increasedVelocity);
        }
        else
        {
            Debug.Log("No tienes suficientes puntos para usar Boost" + increasedVelocity);
            _maxVelocity = 90f;
        }
    }

    void rearview()
    {
        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.JoystickButton3))
        {
            _Cameras[0].SetActive(false);
            _Cameras[1].SetActive(true);
            _CurrentCamera = _Cameras[1];
        }
        else
        {
            _Cameras[0].SetActive(true);
            _Cameras[1].SetActive(false);
            _CurrentCamera = _Cameras[0];

        }


    }

    void SelectPowerUp()
    {   
        
        bool leftDumper = Input.GetKeyDown(KeyCode.JoystickButton4);
        bool rigthDumper = Input.GetKeyDown(KeyCode.JoystickButton5);

        if (rigthDumper)
        {
            selection++;
        }
        if(leftDumper)
        {
            selection--;
        }

        if (selection > 2 || selection < 0)
        {
            selection = 0;
        }

        switch (selection)
        {
            case 0:
            _currentPowerUp = MisilePowerUp;
            break;
            case 1:
            _currentPowerUp = BoostPowerUp;
            break;
            case 2:
            _currentPowerUp = MinesPowerUp;
            break;
        }
        
        //MisilePowerUp();
        //BoostPowerUp();
        //MinesPowerUp();
    }

    IEnumerator ReloadEnergy()
    {
        yield return new WaitForSeconds(5f);
        energy = maxEnergy;
        _PlayerUI.SetEnergy(energy);
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
