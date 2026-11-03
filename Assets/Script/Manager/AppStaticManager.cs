using UnityEngine;

public static class AppStaticManager
{
    //Nuestro usuario actual
    public static UserDataClass CurrentUser { get; private set; }
    public static evaluationValues CurrentScore { get; private set; }

    public static void SetCurrentUser(UserDataClass user)
    {
        CurrentUser = user;
    }

    //Se llama al iniciar el quizz, sin esto CurrentScore es null
    public static void ResetScore()
    {
        CurrentScore = new evaluationValues();
    }


}
