using UnityEngine;
using UnityEditor;
using System.Collections;

[CanEditMultipleObjects()]
[CustomEditor(typeof(Light2D))]
public class Light2DEditor : Editor
{
    SerializedObject sObj;

    SerializedProperty sweepStart;
    SerializedProperty sweepSize;
    SerializedProperty lightRadius;
    SerializedProperty lightMaterial;
    SerializedProperty lightDetail;
    SerializedProperty lightColor;
    SerializedProperty useEvents;
    SerializedProperty shadowLayer;
    SerializedProperty isShadow;
    //SerializedProperty allowHide;

    void OnEnable()
    {
        sObj = new SerializedObject(targets);

        isShadow = sObj.FindProperty("isShadowCaster");
        lightDetail = sObj.FindProperty("lightDetail");
        lightColor = sObj.FindProperty("lightColor");
        sweepSize = sObj.FindProperty("sweepSize");
        sweepStart = sObj.FindProperty("sweepStart");
        lightRadius = sObj.FindProperty("lightRadius");
        lightMaterial = sObj.FindProperty("lightMaterial");
        useEvents = sObj.FindProperty("useEvents");
        shadowLayer = sObj.FindProperty("shadowLayer");
        //allowHide = sObj.FindProperty("allowHiddenLights");

        (target as Light2D).Draw(true, true, true, true);

    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(shadowLayer, new GUIContent("Shadow Layer", "Objects on this layer will cast shadows."));
        EditorGUILayout.PropertyField(isShadow, new GUIContent("Is Shadow Caster", "If 'TRUE' this light will only emit a shadow."));
        EditorGUILayout.PropertyField(lightDetail, new GUIContent("Light Detail", "The number of rays the light checks for when generating shadows. Rays_500 will cast 500 raycasts."));
        EditorGUILayout.PropertyField(lightColor);
        EditorGUILayout.PropertyField(lightMaterial);

        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(sweepStart, new GUIContent("Light Cone Start"));
        EditorGUILayout.PropertyField(sweepSize, new GUIContent("Light Cone Angle", ""));
        sweepSize.floatValue = Mathf.Clamp(sweepSize.floatValue, 0, 360);
        EditorGUILayout.PropertyField(lightRadius);
        lightRadius.floatValue = Mathf.Clamp(lightRadius.floatValue, 0.001f, Mathf.Infinity);

        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(useEvents);
        //EditorGUILayout.PropertyField(allowHide, new GUIContent("Is Shadow Caster", "If 'TRUE' this light will disable when inside of object bounding volumes. Keep in mind that bounds are cube shaped."));

        if (sObj.ApplyModifiedProperties())
        {
            sObj.Update();
            UpdateLight();
        }
    }

    void OnSceneGUI()
    {
        Light2D l = (Light2D)target;
        EditorUtility.SetSelectedWireframeHidden(l.renderer, !l.EDITOR_SHOW_MESH);

        Handles.color = Color.green;
        float widgetSize = Vector3.Distance(l.transform.position, SceneView.lastActiveSceneView.camera.transform.position) * 0.1f;
        float rad = (l.LightRadius);
        Handles.DrawWireDisc(l.transform.position, l.transform.forward, rad);
        lightRadius.floatValue = Mathf.Clamp(Handles.ScaleValueHandle(l.LightRadius, l.transform.TransformPoint(Vector3.right * rad), Quaternion.identity, widgetSize, Handles.CubeCap, 1), 0.001f, Mathf.Infinity);

        Handles.color = Color.red;
        Vector3 sPos = l.transform.TransformDirection(Mathf.Cos(Mathf.Deg2Rad * -((l.LightConeAngle / 2f) - l.LightConeStart)), Mathf.Sin(Mathf.Deg2Rad * -((l.LightConeAngle / 2f) - l.LightConeStart)), 0);
        Handles.DrawWireArc(l.transform.position, l.transform.forward, sPos, l.LightConeAngle, (rad * 0.8f));
        sweepSize.floatValue = Mathf.Clamp(Handles.ScaleValueHandle(l.LightConeAngle, l.transform.position - l.transform.right * (rad * 0.8f), Quaternion.identity, widgetSize, Handles.CubeCap, 1), 0, 360);

        if (sObj.ApplyModifiedProperties())
        {
            sObj.Update();
            UpdateLight();
        }
    }

    void UpdateLight()
    {
        Light2D l2d = (Light2D)target;

        //l2d.LightConeStart = sweepStart.floatValue;
        l2d.LightConeAngle = sweepSize.floatValue;
        l2d.LightRadius = lightRadius.floatValue;
        l2d.LightMaterial = (Material)lightMaterial.objectReferenceValue;
        l2d.LightDetail = (Light2D.LightDetailSetting)lightDetail.intValue;
        l2d.LightColor = lightColor.colorValue;
        l2d.EnableEvents = useEvents.boolValue;
        l2d.ShadowLayer = shadowLayer.intValue;
        l2d.IsShadowEmitter = isShadow.boolValue;
    }
}
