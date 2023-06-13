using UnityEngine;

public class scr_Plasma : scr_Interactable
{
    [SerializeField]
    private GameObject weapon;
    private bool pickup;

    void Start()
    {
        promptMessage = "Pick up the Plama";
        pickup = true;
    }

    protected override void Interact()
    {
        scr_CharacterController characterController = FindObjectOfType<scr_CharacterController>();

        if (characterController != null && pickup)
        {
            characterController.ManageWeaponPickup("Plasma");
            pickup = false;
        }

        foreach (scr_Plasma instance in FindObjectsOfType<scr_Plasma>())
        {
            instance.pickup = false;
            Destroy(instance.gameObject);
        }
    }
}