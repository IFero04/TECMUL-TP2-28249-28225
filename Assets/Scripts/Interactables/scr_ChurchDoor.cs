using UnityEngine;

public class scr_ChurchDoor : scr_Interactable
{
    [SerializeField]
    private GameObject door;

    void Start()
    {
        promptMessage = "Open the door";
    }

    protected override void Interact()
    {
        door.GetComponent<Animator>().SetBool("IsOpen", true);

        foreach (scr_ChurchDoor doorInstance in FindObjectsOfType<scr_ChurchDoor>())
        {
            doorInstance.promptMessage = string.Empty;
            doorInstance.gameObject.layer = LayerMask.NameToLayer("Default");
        }
    }
}
