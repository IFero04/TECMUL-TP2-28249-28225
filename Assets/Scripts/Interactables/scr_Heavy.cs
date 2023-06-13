using UnityEngine;

public class scr_Heavy : scr_Interactable
{
    [SerializeField]
    private GameObject weapon;

    void Start()
    {
        promptMessage = "Pick up the HeavyGun";
    }

    protected override void Interact()
    {
        scr_CharacterController characterController = FindObjectOfType<scr_CharacterController>();

        if (characterController != null)
        {
            characterController.ManageWeaponPickup("Heavy");
        }

        Destroy(gameObject);
    }
}