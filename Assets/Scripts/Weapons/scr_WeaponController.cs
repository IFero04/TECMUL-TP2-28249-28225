using System.Collections;
using UnityEngine;
using static scr_Models;
using UnityEngine.UI;

public class scr_WeaponController : MonoBehaviour
{
    private scr_CharacterController characterController;
    private float lerpTimer;
    public float chipSpeed = 2f;

    [Header("References")]
    public Animator weaponAnimator;
    public Transform weaponSwayObject;
    public Transform sightTarget;
    public AudioSource shootingAudioSource;
    public AudioClip shootingSound;
    public AudioClip reloadingSound;
    public AudioClip handlingSound;
    public AudioClip hitMarker;
    public Image frontAmmo;
    public Image backAmmo;

    [Header("Settings")]
    public WeaponSettingsModel settings;

    bool isInit;

    Vector3 newWeaponRotation;
    Vector3 newWeaponRotationVelocity;

    Vector3 targetWeaponRotation;
    Vector3 targetWeaponRotationVelocity;

    Vector3 newWeaponMovementRotation;
    Vector3 newWeaponMovementRotationVelocity;

    Vector3 targetWeaponMovementRotation;
    Vector3 targetWeaponMovementRotationVelocity;

    private bool isGroundedTrigger;
    private float fallingDelay;

    [Header("Weapon Sway")]
    public float swayAmountA = 1;
    public float swayAmountB = 2;
    public float swayScale = 600;
    public float swayLerpSpeed = 14;

    private float swayTime;
    private Vector3 swayPosition;

    [Header("Sights")]
    public float sightOffset;
    public float aimingInTime;
    private Vector3 weaponSwayPosition;
    private Vector3 weaponSwayPositionVelocity;
    [HideInInspector]
    public bool isAimingIn;
    [HideInInspector]
    public bool isShooting;
    [HideInInspector]
    public bool isRealoading = false;

    private void Start()
    {
        newWeaponRotation = transform.localRotation.eulerAngles;
        settings.currentAmmo = settings.maxAmmo;
        StartCoroutine(FakeReload());
    }

    private void Update()
    {
        UpdateAmmoUi();
    }

    private void OnEnable()
    {
        isRealoading = false;
        weaponAnimator.SetBool("Relaod", false);
    }

    public void Init(scr_CharacterController CharacterController)
    {
        characterController = CharacterController;
        isInit = true;
    }

    private void FixedUpdate()
    {
        if (!isInit)
        {
            return;
        }

        CalculateWeaponRotation();
        SetWeaponAnimations();
        CalculateWeaponSway();
        CalculateAimingIn();
        CalculateShoot();
    }

    private void CalculateAimingIn()
    {
        var targetPosition = transform.position;

        if (isAimingIn)
        {
            targetPosition = characterController.camera.transform.position + (weaponSwayObject.transform.position - sightTarget.position) + (characterController.camera.transform.forward * sightOffset);
        }

        weaponSwayPosition = weaponSwayObject.transform.position;
        weaponSwayPosition = Vector3.SmoothDamp(weaponSwayPosition, targetPosition, ref weaponSwayPositionVelocity, aimingInTime);
        weaponSwayObject.transform.position = weaponSwayPosition + swayPosition;
    }

    public void TriggerJump()
    {
        isGroundedTrigger = false;
        weaponAnimator.SetTrigger("Jump");
    }

    private void CalculateWeaponRotation()
    {
        targetWeaponRotation.y += (isAimingIn ? settings.SwayAmount / 3 : settings.SwayAmount) * (settings.SwayXInverted ? -characterController.input_View.x : characterController.input_View.x) * Time.deltaTime;
        targetWeaponRotation.x += (isAimingIn ? settings.SwayAmount / 3 : settings.SwayAmount) * (settings.SwayYInverted ? characterController.input_View.y : -characterController.input_View.y) * Time.deltaTime;

        targetWeaponRotation.x = Mathf.Clamp(targetWeaponRotation.x, -settings.SwayClampX, settings.SwayClampX);
        targetWeaponRotation.y = Mathf.Clamp(targetWeaponRotation.y, -settings.SwayClampY, settings.SwayClampY);
        targetWeaponRotation.z = isAimingIn ? 0 : targetWeaponRotation.y;

        targetWeaponRotation = Vector3.SmoothDamp(targetWeaponRotation, Vector3.zero, ref targetWeaponRotationVelocity, settings.SwayResetSmoothing);
        newWeaponRotation = Vector3.SmoothDamp(newWeaponRotation, targetWeaponRotation, ref newWeaponRotationVelocity, settings.SwaySmoothing);


        targetWeaponMovementRotation.z = (isAimingIn ? settings.MovementSwayX / 2 : settings.MovementSwayX) * (settings.MovementSwayXInverted ? -characterController.input_Movement.x : characterController.input_Movement.x);
        targetWeaponMovementRotation.x = (isAimingIn ? settings.MovementSwayY / 2 : settings.MovementSwayY) * (settings.MovementSwayYInverted ? characterController.input_Movement.y : -characterController.input_Movement.y);

        targetWeaponMovementRotation = Vector3.SmoothDamp(targetWeaponMovementRotation, Vector3.zero, ref targetWeaponMovementRotationVelocity, settings.MovementSwaySmoothing);
        newWeaponMovementRotation = Vector3.SmoothDamp(newWeaponMovementRotation, targetWeaponMovementRotation, ref newWeaponMovementRotationVelocity, settings.MovementSwaySmoothing);

        transform.localRotation = Quaternion.Euler(newWeaponRotation + newWeaponMovementRotation);
    }

    private void SetWeaponAnimations()
    {
        if (isGroundedTrigger)
        {
            fallingDelay = 0;
        }
        else
        {
            fallingDelay += Time.deltaTime;
        }

        if (characterController.isGrounded && !isGroundedTrigger && fallingDelay > 0.1f)
        {
            weaponAnimator.SetTrigger("Land");
            isGroundedTrigger = true;
        }
        else if (!characterController.isGrounded && isGroundedTrigger)
        {
            weaponAnimator.SetTrigger("Falling");
            isGroundedTrigger = false;
        }

        weaponAnimator.SetBool("IsSprinting", characterController.sprinting);
        weaponAnimator.SetFloat("WeaponAnimationSpeed", characterController.weaponAnimationSpeed);
    }

    private void CalculateWeaponSway()
    {
        var targetPosition = LissajousCurve(swayTime, swayAmountA, swayAmountB) / (isAimingIn ? swayScale * 10 : swayScale);

        swayPosition = Vector3.Lerp(swayPosition, targetPosition, Time.smoothDeltaTime * swayLerpSpeed);
        swayTime += Time.deltaTime;

        if(swayTime > 6.3f)
        {
            swayTime = 0;
        }
    }

    private Vector3 LissajousCurve(float Time, float A, float B)
    {
        return new Vector3(Mathf.Sin(Time), A * Mathf.Sin(B * Time + Mathf.PI));
    }

    #region - Shooting -
    public void CalculateShoot()
    {
        if (isRealoading)
            return;

        if (characterController.isFalling)
            return;

        if (characterController.sprinting)
            return;

        if (characterController.isSwaping)
            return;

        if (settings.currentAmmo <= 0)
        {
            if (settings.ammoStorage > 0)
            {
                StartCoroutine(Reload());
                return;
            }
            shootingAudioSource.PlayOneShot(handlingSound);
            characterController.SwapWeapon(gameObject.name);
            return;
        }

        if (isShooting && Time.time >= settings.nexTimeToFire)
        {
            settings.nexTimeToFire = Time.time + 1f / settings.fireRate;
            lerpTimer = 0;
            Shoot();
        }
    }
    public void Shoot()
    {
        settings.muzzleFlash.Play();
        shootingAudioSource.PlayOneShot(shootingSound);

        settings.currentAmmo--;

        int layerMask = ~(LayerMask.GetMask("Weapon") | LayerMask.GetMask("Player"));
        RaycastHit hit;
        if(Physics.Raycast(characterController.camera.transform.position, characterController.camera.transform.forward, out hit, settings.range, layerMask))
        {
            scr_Enemy enemy = hit.transform.GetComponent<scr_Enemy>();
            if (enemy != null)
            {
                shootingAudioSource.PlayOneShot(hitMarker);
                enemy.TakeDamege(settings.damage);
                if (gameObject.name == "Plasma")
                {
                    characterController.transform.GetComponent<scr_PlayerHealth>().RestoreHealh(Random.Range(0.5f, 1.5f));
                }
                GameObject impactGO = Instantiate(settings.hitFlesh, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactGO, 2f);
            }
            else
            {
                GameObject impactGO = Instantiate(settings.impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactGO, 2f);
            }
        }
    }
    public IEnumerator Reload()
    {
        if (characterController.sprinting)
            yield break;
        if (characterController.isFalling)
            yield break;
        if (characterController.isAimingIn)
            characterController.isAimingIn = false;

        if (settings.currentAmmo < settings.maxAmmo && settings.ammoStorage > 0)
        {
            isRealoading = true;

            weaponAnimator.SetBool("Reload", true);
            shootingAudioSource.PlayOneShot(reloadingSound);
            yield return new WaitForSeconds(settings.reloadTime - 0.25f);
            weaponAnimator.SetBool("Reload", false);
            yield return new WaitForSeconds(0.25f);

            if (gameObject.activeSelf)
            {
                int reloadAmmo = settings.maxAmmo - settings.currentAmmo;

                if (settings.ammoStorage >= reloadAmmo)
                {
                    settings.currentAmmo += reloadAmmo;
                    settings.ammoStorage -= reloadAmmo;
                }
                else if (settings.ammoStorage < reloadAmmo)
                {
                    settings.currentAmmo += settings.ammoStorage;
                    settings.ammoStorage = 0;
                }
            }

            lerpTimer = 0;
            isRealoading = false;         
        }
    }

    public IEnumerator FakeReload()
    {
        isRealoading = true;

        weaponAnimator.SetBool("Reload", true);
        shootingAudioSource.PlayOneShot(reloadingSound);
        yield return new WaitForSeconds(settings.reloadTime - 0.25f);
        weaponAnimator.SetBool("Reload", false);
        yield return new WaitForSeconds(0.25f);

        isRealoading = false;
    }
    #endregion

  
    public void UpdateAmmoUi()
    {
        float fillF = frontAmmo.fillAmount;
        float fillB = backAmmo.fillAmount;
        float hFraction = (float)settings.currentAmmo / (float)settings.maxAmmo;
        if (fillB > hFraction)
        {
            frontAmmo.fillAmount = hFraction;
            backAmmo.color = Color.red;
            lerpTimer += Time.deltaTime;
            float percentComplete = lerpTimer / chipSpeed;
            percentComplete = percentComplete * percentComplete;
            backAmmo.fillAmount = Mathf.Lerp(fillB, hFraction, percentComplete);
        }
        if (fillF < hFraction)
        {
            backAmmo.color = Color.yellow;
            backAmmo.fillAmount = hFraction;
            lerpTimer += Time.deltaTime;
            float percentComplete = lerpTimer / chipSpeed;
            percentComplete = percentComplete * percentComplete;
            frontAmmo.fillAmount = Mathf.Lerp(fillF, backAmmo.fillAmount, percentComplete);
        }
    }
}
