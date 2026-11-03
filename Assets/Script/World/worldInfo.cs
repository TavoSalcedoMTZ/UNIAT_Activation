using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "worldInfo", menuName = "Scriptable Objects/worldInfo")]
public class worldInfo : ScriptableObject
{
    public CareerType carrera;
    public string worldName;
    public Sprite worldImage;
    [Tooltip("Nombre exacto de la escena del planeta, debe estar agregada en Build Settings")]
    public string sceneName;
    [Tooltip("Color del planeta cuando no hay worldImage asignada")]
    public Color planetColor = Color.white;

    public static worldInfo FindByCareer(IEnumerable<worldInfo> _worlds, CareerType _career)
    {
        foreach (worldInfo world in _worlds)
        {
            if (world != null && world.carrera == _career)
            {
                return world;
            }
        }
        return null;
    }
}
