using UnityEngine;

[System.Serializable]
public class UserDataClass
{
    public string userName;
    public string userEmail;

    public UserDataClass(string name, string email)
    {
        userName = name;
        userEmail = email;
    }
}
