using UnityEngine;

public class scr_Rifle : scr_Interactable
{
    [SerializeField]
    private GameObject weapon;

    void Start()
    {
        promptMessage = "Pick up the Rifle";
    }

    protected override void Interact()
    {
        scr_CharacterController characterController = FindObjectOfType<scr_CharacterController>();

        if (characterController != null)
        {
            characterController.ManageWeaponPickup("Rifle");
        }

        Destroy(gameObject);
    }
}