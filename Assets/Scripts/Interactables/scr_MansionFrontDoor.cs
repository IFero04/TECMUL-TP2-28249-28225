using UnityEngine;

public class scr_MansionFrontDoor : scr_Interactable
{
    [SerializeField]
    private GameObject door;

    void Start()
    {
        promptMessage = "Open the door";
    }

    protected override void Interact()
    {
        door.GetComponent<Animator>().SetBool("IsOpenMF", true);
        promptMessage = string.Empty;
        gameObject.layer = LayerMask.NameToLayer("Default");
    }
}
