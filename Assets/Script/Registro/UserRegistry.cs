using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserRegistry : MonoBehaviour
{
    public TMP_InputField usernameInputField;
    public TMP_InputField emailInputField;

    private void Start()
    {
       Init();
    }

    public void Init()
    {
        // Inicializar los campos de entrada si es necesario
        usernameInputField.text = "";
        emailInputField.text = "";
    }

    public bool RegisterUser()
    {
        string userName = usernameInputField.text;
        string userEmail = emailInputField.text;
        if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(userEmail))
        {
            UserDataClass newUser = new UserDataClass(userName, userEmail);
            AppStaticManager.SetCurrentUser(newUser);
            Debug.Log($"Usuario registrado: {newUser.userName}, {newUser.userEmail}");
            return true;
        }
        else
        {
            Debug.LogWarning("Por favor, complete todos los campos.");
        }
        return false;
    }

    public void CompleteRegistration()
    {
        if (RegisterUser())
        {
            Debug.Log("Registro completado con éxito.");

            //Aqui usaremos nuestro SceneManager para cargar
           if(StaticMenu.Instance != null)
            {
                StaticMenu.Instance.LoadScene("1");
            }
        }
    }

}
