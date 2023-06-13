using UnityEngine;
using TMPro;

public class scr_PlayerUI : MonoBehaviour
{
    public TextMeshProUGUI promptText;

    public void UpdateText(string promptMessage)
    {
        promptText.text = promptMessage;
    }
}
