using UnityEngine;

public class scr_PlayerInteract : MonoBehaviour
{
    [SerializeField]
    private float distance = 3f;
    [SerializeField]
    public LayerMask mask;
    private scr_PlayerUI playerUI;
    private scr_CharacterController characterController;

    void Start()
    {
        playerUI = GetComponent<scr_PlayerUI>();
        characterController = GetComponent<scr_CharacterController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        playerUI.UpdateText(string.Empty);
        Ray ray = new Ray(characterController.camera.transform.position, characterController.camera.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * distance);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, distance, mask))
        {
            if (hitInfo.collider.GetComponent<scr_Interactable>() != null)
            {
                scr_Interactable interactable = hitInfo.collider.GetComponent<scr_Interactable>();
                playerUI.UpdateText(interactable.promptMessage);
                if(characterController.characterInput.Interact.triggered)
                {
                    interactable.BaseInteract();
                }
            }
        }
    }
}
