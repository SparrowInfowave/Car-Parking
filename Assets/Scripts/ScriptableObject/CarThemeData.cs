using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CarTheme",fileName = "NewCarTheme")]
public class CarThemeData : ScriptableObject
{
    public Material material;
    public List<Mesh> mesh;
    public List<Texture> textures;
}
