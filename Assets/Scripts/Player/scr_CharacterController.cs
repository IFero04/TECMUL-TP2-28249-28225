using UnityEngine;
using System.Collections;
using static scr_Models;

public class scr_CharacterController : MonoBehaviour
{
    #region - Variabels -
    private CharacterController characterController;
    private PlayerInput playerInput;
    public PlayerInput.CharacterActions characterInput;

    [HideInInspector]
    public Vector2 input_Movement;
    [HideInInspector]
    public Vector2 input_View;

    private Vector3 newCameraRotation;
    private Vector3 newCharacterRotation;

    [HideInInspector]
    public bool isGrounded;
    [HideInInspector]
    public bool isFalling;
    [HideInInspector]
    public bool isSwaping;

    [Header("References")]
    public Transform cameraHolder;
    public Transform camera;
    public Transform feetTransform;

    [Header("Settings")]
    public PlayerSettingsModel playerSettings;
    private float viewClampYMin = -70;
    private float viewClampYMax = 80;
    public LayerMask playerMask;
    public LayerMask groundMask;

    [Header("Gravity")]
    public float gravityAmount;
    public float gravityMin;
    private float playerGravity;

    public Vector3 jumpingForce;
    private Vector3 jumpingForceVelocity;

    [Header("Stance")]
    public PlayerStance playerStance;
    public float playerSranceSmoothing;
    public CharacterStance playerStandStance;
    public CharacterStance playerCrouchStance;
    public CharacterStance playerProneStance;
    private float stanceCheckErrorMargin = 0.05f;
    private float cameraHeight;
    private float cameraHeightVelocity;

    private Vector3 stanceCapsuleCenterVelocity;
    private float stanceCapsuleHeightVelocity;

    [HideInInspector]
    public bool sprinting;

    private Vector3 newMovementSpeed;
    private Vector3 newMovementSpeedVelocity;

    [Header("Weapon")]
    public scr_WeaponController[] storageWeapons;
    private int storageWeaponIndex = 2;
    public scr_WeaponController[] currentWeapons;
    private int currentWeaponIndex = 0;
    public float weaponAnimationSpeed;

    [Header("Leaning")]
    public Transform leanPivot;
    public float leanAngle;
    public float leanSmoothing;
    private float currentLean;
    private float targetLean;
    private float leanVelocity;

    private bool isLeaningLeft;
    private bool isLeaningRight;

    [Header("Aiming In")]
    public bool isAimingIn;

    #endregion

    #region - Awake - 
    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerInput = new PlayerInput();
        characterInput = playerInput.Character;

        characterInput.Movement.performed += e => input_Movement = e.ReadValue<Vector2>();
        characterInput.View.performed += e => input_View = e.ReadValue<Vector2>();
        characterInput.Jump.performed += e => Jump();
        characterInput.Sprint.performed += e => Sprint();
        characterInput.SprintReleased.performed += e => StopSprint();
        characterInput.Crouch.performed += e => Crouch();
        characterInput.Prone.performed += e => Prone();
        characterInput.LeanLeft.performed += e => isLeaningLeft = true;
        characterInput.LeanLeftReleased.performed += e => isLeaningLeft = false;
        characterInput.LeanRight.performed += e => isLeaningRight = true;
        characterInput.LeanRightReleased.performed += e => isLeaningRight = false;

        playerInput.Weapon.Fire2Pressed.performed += e => AimingInPressed();
        playerInput.Weapon.Fire2Released.performed += e => AimingInReleased();
        playerInput.Weapon.Fire1Pressed.performed += e => currentWeapons[currentWeaponIndex].isShooting = true;
        playerInput.Weapon.Fire1Released.performed += e => currentWeapons[currentWeaponIndex].isShooting = false;
        playerInput.Weapon.Reload.performed += e => StartCoroutine(currentWeapons[currentWeaponIndex].Reload());

        playerInput.Enable();

        newCameraRotation = cameraHolder.localRotation.eulerAngles;
        newCharacterRotation = transform.localRotation.eulerAngles;

        cameraHeight = cameraHolder.localPosition.y;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (currentWeapons[currentWeaponIndex])
        {
            currentWeapons[currentWeaponIndex].Init(this);
        }
    }
    #endregion

    #region - Update -
    private void Update()
    {
        SetIsGrounded();
        SetIsFalling();

        CalculateView();
        CalculateMovement();
        CalculateJump();
        CalculateStance();
        CalculateAimingIn();
        CalculateLeaning();
        CalculateWeapon();
    }
    #endregion

    #region - Aiming In -
    private void AimingInPressed()
    {
        if (currentWeapons[currentWeaponIndex].isRealoading)
            return;

        isAimingIn = true;
    }
    private void AimingInReleased()
    {
        isAimingIn = false;
    }

    private void CalculateAimingIn()
    {
        if (!currentWeapons[currentWeaponIndex])
            return;

        currentWeapons[currentWeaponIndex].isAimingIn = isAimingIn;
    }
    #endregion

    #region - IsFalling / IsGrounded -

    private void SetIsGrounded()
    {
        isGrounded = Physics.CheckSphere(feetTransform.position, playerSettings.isGroundedRadius, groundMask);
    }

    private void SetIsFalling()
    {
        isFalling = (!isGrounded && characterController.velocity.magnitude >= playerSettings.isFallingSpeed);
    }

    #endregion

    #region - View / Movement -
    private void CalculateView()
    {
        newCharacterRotation.y += (isAimingIn ? playerSettings.ViewXSensitivity * playerSettings.AimingSensitivityEffector: playerSettings.ViewXSensitivity) * (playerSettings.ViewXInverted ? -input_View.x : input_View.x) * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(newCharacterRotation);

        newCameraRotation.x += (isAimingIn ? playerSettings.ViewYSensitivity * playerSettings.AimingSensitivityEffector : playerSettings.ViewXSensitivity) * (playerSettings.ViewYInverted ? input_View.y : -input_View.y) * Time.deltaTime;
        newCameraRotation.x = Mathf.Clamp(newCameraRotation.x, viewClampYMin, viewClampYMax);

        cameraHolder.localRotation = Quaternion.Euler(newCameraRotation);
    }

    private void CalculateMovement()
    {
        if (input_Movement.y <= 0.2f)
        {
            sprinting = false;
        }

        var verticalSpeed = playerSettings.WalkingForwardSpeed;
        var horizontalSpeed = playerSettings.WalkingStrafeSpeed;

        if (sprinting)
        {
            verticalSpeed = playerSettings.RunningForwardSpeed;
            horizontalSpeed = playerSettings.RunningStrafeSpeed;
        }

        if (!isGrounded)
        {
            playerSettings.SpeedEffector = playerSettings.FallingSpeedEffector;
        }
        else if (playerStance == PlayerStance.Crouch)
        {
            playerSettings.SpeedEffector = playerSettings.CrouchSpeedEffector;
        }
        else if (playerStance == PlayerStance.Prone)
        {
            playerSettings.SpeedEffector = playerSettings.ProneSpeedEffector;
        }
        else if (isAimingIn)
        {
            playerSettings.SpeedEffector = playerSettings.AimingSpeedEffector;
        }
        else
        {
            playerSettings.SpeedEffector = 1;
        }

        weaponAnimationSpeed = characterController.velocity.magnitude / (playerSettings.WalkingForwardSpeed * playerSettings.SpeedEffector);

        if (weaponAnimationSpeed > 1)
        {
            weaponAnimationSpeed = 1;
        }

        verticalSpeed *= playerSettings.SpeedEffector;
        horizontalSpeed *= playerSettings.SpeedEffector;

        newMovementSpeed = Vector3.SmoothDamp(newMovementSpeed, new Vector3(horizontalSpeed * input_Movement.x * Time.deltaTime, 0, verticalSpeed * input_Movement.y * Time.deltaTime), ref newMovementSpeedVelocity, isGrounded ? playerSettings.MovementSmoothing : playerSettings.FallingSmoothing);
        var movementSpeed = transform.TransformDirection(newMovementSpeed);

        if (playerGravity > gravityMin)
        {
            playerGravity -= gravityAmount * Time.deltaTime;
        }

        if (playerGravity < -0.1f && isGrounded)
        {
            playerGravity = -0.1f;
        }

        movementSpeed.y += playerGravity;
        movementSpeed += jumpingForce * Time.deltaTime;

        characterController.Move(movementSpeed);
    }
    #endregion

    #region - Leaning -
    private void CalculateLeaning()
    {
        if (isLeaningLeft)
        {
            targetLean = leanAngle;
        }
        else if (isLeaningRight)
        {
            targetLean = -leanAngle;
        } 
        else
        {
            targetLean = 0;
        }

        currentLean = Mathf.SmoothDamp(currentLean, targetLean, ref leanVelocity, leanSmoothing);

        leanPivot.localRotation = Quaternion.Euler(new Vector3(0, 0, currentLean));
    }
    #endregion

    #region - Jumping -
    private void CalculateJump()
    {
        jumpingForce = Vector3.SmoothDamp(jumpingForce, Vector3.zero, ref jumpingForceVelocity, playerSettings.JumpingFalloff);
    }

    private void Jump()
    {
        if (currentWeapons[currentWeaponIndex].isRealoading)
            return;

        if (!isGrounded || playerStance == PlayerStance.Prone)
            return;
        if (playerStance == PlayerStance.Crouch)
        {
            if (StanceCheck(playerStandStance.StanceCollider.height))
            {
                return;
            }

            playerStance = PlayerStance.Stand;
            return;
        }

        // Jump
        jumpingForce = Vector3.up * playerSettings.JumpingHeight;
        playerGravity = 0;
        sprinting = false;
        currentWeapons[currentWeaponIndex].TriggerJump();
    }
    #endregion

    #region - Stance -
    private void CalculateStance()
    {
        var currentStance = playerStandStance;

        if (playerStance == PlayerStance.Crouch)
        {
            currentStance = playerCrouchStance;
        }
        else if(playerStance == PlayerStance.Prone)
        {
            currentStance = playerProneStance;
        }

        cameraHeight = Mathf.SmoothDamp(cameraHolder.localPosition.y, currentStance.CameraHeight, ref cameraHeightVelocity, playerSranceSmoothing);
        cameraHolder.localPosition = new Vector3(cameraHolder.localPosition.x, cameraHeight, cameraHolder.localPosition.z);

        characterController.height = Mathf.SmoothDamp(characterController.height, currentStance.StanceCollider.height, ref stanceCapsuleHeightVelocity, playerSranceSmoothing);
        characterController.center = Vector3.SmoothDamp(characterController.center, currentStance.StanceCollider.center, ref stanceCapsuleCenterVelocity, playerSranceSmoothing);
    }

    private void Crouch()
    {
        if (playerStance == PlayerStance.Crouch)
        {
            if (StanceCheck(playerStandStance.StanceCollider.height))
            {
                return;
            }

            playerStance = PlayerStance.Stand;
            return;
        }

        if (StanceCheck(playerCrouchStance.StanceCollider.height))
        {
            return;
        }

        playerStance = PlayerStance.Crouch;
    }

    private void Prone()
    {
        if (playerStance == PlayerStance.Prone)
        {
            if (StanceCheck(playerStandStance.StanceCollider.height))
            {
                return;
            }

            playerStance = PlayerStance.Stand;
            return;
        }

        if (StanceCheck(playerProneStance.StanceCollider.height))
        {
            return;
        }

        playerStance = PlayerStance.Prone;
    }

    private bool StanceCheck(float stanceCheckHeight)
    {
        var start = new Vector3(feetTransform.position.x, feetTransform.position.y + characterController.radius + stanceCheckErrorMargin, feetTransform.position.z);
        var end = new Vector3(feetTransform.position.x, feetTransform.position.y - characterController.radius - stanceCheckErrorMargin + stanceCheckHeight, feetTransform.position.z);

        return Physics.CheckCapsule(start, end, characterController.radius, playerMask);
    }
    #endregion

    #region - Sprinting -
    private void Sprint()
    {
        if (currentWeapons[currentWeaponIndex].isRealoading)
            return;

        if (input_Movement.y <= 0.2f || playerStance == PlayerStance.Prone)
        {
            sprinting = false;
            return;
        }
        if (playerStance == PlayerStance.Crouch)
        {
            playerStance = PlayerStance.Stand;
        }

        sprinting = true;
    }

    private void StopSprint()
    {
        if (playerSettings.SprintingHold)
        {
            sprinting = false;
        }
    }
    #endregion

    #region - Gizmos -
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(feetTransform.position, playerSettings.isGroundedRadius);   
    }
    #endregion

    #region - Weapons Inventory -
    private scr_WeaponController GetWeaponByName(string name)
    {
        foreach (Transform weapon in leanPivot)
        {
            if (weapon.name == name)
            {
                scr_WeaponController weaponController = weapon.GetComponent<scr_WeaponController>();
                return weaponController;
            }
        }

        return null;
    }
    public void ManageWeaponPickup(string name)
    {
        scr_WeaponController weapon = GetWeaponByName(name);
        if (weapon)
        {
            weapon.Init(this);
        }

        for (int i = 0; i < storageWeapons.Length; i++)
        {
            if (storageWeapons[i] == null)
            {
                storageWeapons[i] = weapon;
                break;
            }
        }

        if (currentWeapons[1] == null)
        {
            currentWeapons[1] = weapon;
            currentWeaponIndex = 1;
            SelectWeapon();
            return;
        }

        for (int i =0; i < currentWeapons.Length; i++)
        {
            if (currentWeapons[i].settings.currentAmmo <= 0 && currentWeapons[i].settings.ammoStorage <= 0)
            {
                SwapWeapon(currentWeapons[i].name);
                return;
            }
        }
    }
    private void CalculateWeapon()
    {
        if (isAimingIn)
            return;
        if (currentWeapons[currentWeaponIndex].isShooting)
            return;

        int previousSelectedWeapon = currentWeaponIndex;

        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            if (currentWeaponIndex >= currentWeapons.Length - 1)
                currentWeaponIndex = 0;
            else
                currentWeaponIndex++;

            if (currentWeapons[currentWeaponIndex] == null)
            {
                currentWeaponIndex = previousSelectedWeapon;
                return;
            }
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            if (currentWeaponIndex <= 0)
                currentWeaponIndex = currentWeapons.Length - 1;
            else
                currentWeaponIndex--;

            if (currentWeapons[currentWeaponIndex] == null)
            {
                currentWeaponIndex = previousSelectedWeapon;
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (currentWeapons[0] != null)
            {
                currentWeaponIndex = 0;
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (currentWeapons[1] != null)
            {
                currentWeaponIndex = 1;
            }
        }

        if (previousSelectedWeapon != currentWeaponIndex)
        {
            SelectWeapon();
        }
    }
    private void SelectWeapon()
    {
        if (currentWeaponIndex >= 0 && currentWeaponIndex < currentWeapons.Length)
        {
            for (int i = 0; i < storageWeapons.Length; i++)
            {
                if (storageWeapons[i] != null)
                {
                    storageWeapons[i].gameObject.SetActive(false);
                }
                
            }
            currentWeapons[currentWeaponIndex].gameObject.SetActive(true);
        }
    }

    public void SwapWeapon(string name)
    {
        isSwaping = true;

        int cont = 0;
        for (int i = 0; i < storageWeapons.Length; i++)
        {
            if (storageWeapons[i] != null)
            {
                cont ++;
            }
        }

        if (cont < 3)
        {
            isSwaping = false;
            return;
        }
            

        for (int i = 0; i < currentWeapons.Length; i++)
        {
            if (currentWeapons[i].name == name)
            {
                scr_WeaponController swapWeapon = VerifySwapWeapon();
                currentWeapons[i].settings.ammoStorage = currentWeapons[i].settings.maxAmmo;
                currentWeapons[i].settings.currentAmmo = currentWeapons[i].settings.maxAmmo;
                currentWeapons[i] = swapWeapon;
                storageWeaponIndex ++;
                if(storageWeaponIndex >= 5)
                {
                    storageWeaponIndex = 0;
                }
                currentWeaponIndex = i;
                StartCoroutine(GiveTime(1.75f));
                SelectWeapon();
                break;
            }
        }

        isSwaping = false;
    }

    public scr_WeaponController VerifySwapWeapon()
    {
        var canSwap = true;
        if(storageWeapons[storageWeaponIndex] != null)
        {
            for (int i = 0; i < currentWeapons.Length; i++)
            {
                if(currentWeapons[i].name == storageWeapons[storageWeaponIndex].name){
                    canSwap = false;
                }
            }
            if (canSwap)
            {
                scr_WeaponController weaponController = storageWeapons[storageWeaponIndex].GetComponent<scr_WeaponController>();
                return weaponController;
            }
        }
        storageWeaponIndex++;
        if(storageWeaponIndex >= 5)
        {
            storageWeaponIndex = 0;
        }
        return VerifySwapWeapon();

    }
    #endregion

    IEnumerator GiveTime(float time)
    {
        yield return new WaitForSeconds(time);
    }

}
