using UnityEngine;

public class scr_MansionBackDoor1 : scr_Interactable
{
    [SerializeField]
    private GameObject door;

    void Start()
    {
        promptMessage = "Open the door";
    }

    protected override void Interact()
    {
        door.GetComponent<Animator>().SetBool("IsOpenMB1", true);
        promptMessage = string.Empty;
        gameObject.layer = LayerMask.NameToLayer("Default");
    }
}
