using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserRegistry : MonoBehaviour
{
    public TMP_InputField usernameInputField;
    public TMP_InputField emailInputField;

    [Header("Feedback")]
    public TMP_Text messageText;
    [Tooltip("Shake del panel cuando falta un dato")]
    public MMF_Player errorFeedback;

    private void Start()
    {
       Init();
    }

    public void Init()
    {
        // Inicializar los campos de entrada si es necesario
        usernameInputField.text = "";
        emailInputField.text = "";
        if (messageText != null) messageText.text = "";
    }

    public bool RegisterUser()
    {
        string userName = usernameInputField.text.Trim();
        string userEmail = emailInputField.text.Trim();
        if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userEmail))
        {
            ShowError("Completa tu nombre y tu correo");
            return false;
        }
        if (!IsValidEmail(userEmail))
        {
            ShowError("Revisa tu correo, parece incompleto");
            return false;
        }

        UserDataClass newUser = new UserDataClass(userName, userEmail);
        AppStaticManager.SetCurrentUser(newUser);
        Debug.Log($"Usuario registrado: {newUser.userName}, {newUser.userEmail}");
        if (messageText != null) messageText.text = "";
        return true;
    }

    //Validacion simple: algo@algo.algo
    private bool IsValidEmail(string _email)
    {
        int at = _email.IndexOf('@');
        int dot = _email.LastIndexOf('.');
        return at > 0 && dot > at + 1 && dot < _email.Length - 1 && !_email.Contains(" ");
    }

    private void ShowError(string _message)
    {
        Debug.LogWarning(_message);
        if (messageText != null) messageText.text = _message;
        if (errorFeedback != null) errorFeedback.PlayFeedbacks();
    }

    public void CompleteRegistration()
    {
        if (RegisterUser())
        {
            Debug.Log("Registro completado con éxito.");

            StaticMenu.LoadSceneSafe("1");
        }
    }

}
