using UnityEngine;

public class scr_Smg : scr_Interactable
{
    [SerializeField]
    private GameObject weapon;
    private bool pickup;

    void Start()
    {
        promptMessage = "Pick up the Smg";
        pickup = true;
    }

    protected override void Interact()
    {
        scr_CharacterController characterController = FindObjectOfType<scr_CharacterController>();

        if (characterController != null && pickup)
        {
            characterController.ManageWeaponPickup("Smg");
            pickup = false;
        }

        foreach (scr_Smg instance in FindObjectsOfType<scr_Smg>())
        {
            instance.pickup = false;
            Destroy(instance.gameObject);
        }
    }
}