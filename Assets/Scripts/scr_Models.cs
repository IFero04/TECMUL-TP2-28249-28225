using System;
using UnityEngine;

public static class scr_Models
{
    #region - Player -
    public enum PlayerStance
    {
        Stand,
        Crouch,
        Prone
    }

    [Serializable]
    public class PlayerSettingsModel
    {
        [Header("View Settings")]
        public float ViewXSensitivity;
        public float ViewYSensitivity;

        public float AimingSensitivityEffector;

        public bool ViewXInverted;
        public bool ViewYInverted;

        [Header("Movement - Settings")]
        public bool SprintingHold;
        public float MovementSmoothing;

        [Header("Movement - Running")]
        public float RunningForwardSpeed;
        public float RunningStrafeSpeed;

        [Header("Movement - Walking")]
        public float WalkingForwardSpeed;
        public float WalkingBackwardSpeed;
        public float WalkingStrafeSpeed;

        [Header("Jumping")]
        public float JumpingHeight;
        public float JumpingFalloff;
        public float FallingSmoothing;

        [Header("Speed Effectors")]
        public float SpeedEffector = 1;
        public float CrouchSpeedEffector;
        public float ProneSpeedEffector;
        public float FallingSpeedEffector;
        public float AimingSpeedEffector;

        [Header("Is Grounded / Falling")]
        public float isGroundedRadius;
        public float isFallingSpeed;
    }

    [Serializable]
    public class CharacterStance
    {
        public float CameraHeight;
        public CapsuleCollider StanceCollider;
    }


    #endregion

    #region - Weapons -

    [Serializable]
    public class WeaponSettingsModel
    {
        [Header("Weapon - Sway")]
        public float SwayAmount;
        public bool SwayXInverted;
        public bool SwayYInverted;
        public float SwaySmoothing;
        public float SwayResetSmoothing;
        public float SwayClampX;
        public float SwayClampY;

        [Header("Weapon - Movement Sway")]
        public float MovementSwayX;
        public float MovementSwayY;
        public bool MovementSwayXInverted;
        public bool MovementSwayYInverted;
        public float MovementSwaySmoothing;

        [Header("Shooting")]
        public float damage;
        public float range;
        public float fireRate;
        public int maxAmmo;
        public int ammoStorage;
       
        public int currentAmmo;
        public float reloadTime;
        [HideInInspector]
        public float nexTimeToFire;
        public ParticleSystem muzzleFlash;
        public GameObject hitFlesh;
        public GameObject impactEffect;
    }

    #endregion

    #region - Enemy -

    [Serializable]
    public class EnemySettingsModel
    {
        public float health;
        public float damageMin;
        public float damageMax;
        public float attackCoolDown;
    }

    #endregion

}