using UnityEngine;
using UnityEditor;
using System.Collections;

public class Light2DMenu : Editor
{
    [MenuItem("GameObject/Create Other/Light2D/Radial Light", false, 50)]
    public static void CreateNewRadialLight()
    {
        Light2D light = Light2D.Create(Vector3.zero, new Color(0f, 0f, 1f, 0f));
        light.ShadowLayer = -1;

        Selection.activeGameObject = light.gameObject;
    }

    [MenuItem("GameObject/Create Other/Light2D/Spot Light", false, 51)]
    public static void CreateNewSpotLight()
    {
        Light2D light = Light2D.Create(Vector3.zero, new Color(0f, 1f, 0f, 0f));
        light.LightConeAngle = 45;
        light.LightDetail = Light2D.LightDetailSetting.Rays_100;
        light.ShadowLayer = -1;

        Selection.activeGameObject = light.gameObject;
    }

    [MenuItem("GameObject/Create Other/Light2D/Shadow Emitter", false, 51)]
    public static void CreateNewShadowLight()
    {
        Light2D light = Light2D.Create(Vector3.zero, new Color(1f, 0f, 0f, 0f));
        light.ShadowLayer = -1;
        light.LightColor = Color.black;
        light.IsShadowEmitter = true;
        light.LightMaterial = (Material)Resources.Load("RadialShadow", typeof(Material));

        Selection.activeGameObject = light.gameObject;
    }

    [MenuItem("GameObject/Create Other/Light2D/Online Help", false, 61)]
    public static void SeekHelp()
    {
        Application.OpenURL("http://reverieinteractive.com/2DVLS/Documentation/");
    }
}
