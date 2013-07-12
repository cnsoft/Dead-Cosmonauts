using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum LightEventListenerType { OnEnter, OnStay, OnExit }
public delegate void Light2DEvent(Light2D lightObject, GameObject objectInLight);

[ExecuteInEditMode()]
[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class Light2D : MonoBehaviour
{
    public enum LightDetailSetting
    {
        Rays_50 = 49,
        Rays_100 = 99,   // Base - (100 * 4)
        Rays_200 = 199,  // Base - (100 * 3)
        Rays_300 = 299,  // Base - (100 * 2)
        Rays_400 = 399,  // Base - (100 * 1)
        Rays_500 = 499,  // Base
        Rays_600 = 599,  // Base + (100 * 1)
        Rays_700 = 699,  // Base + (100 * 2)
        Rays_800 = 799,  // Base + (100 * 3)
        Rays_900 = 899,  // Base + (100 * 4)
        Rays_1000 = 999,  // Base + (100 * 5)
        Rays_2000 = 1999, // Base + (100 * 10)
        Rays_3000 = 2999, // Base + (100 * 25)
        Rays_4000 = 3999, // Base + (100 * 35)
        Rays_5000 = 4999, // Base + (100 * 45)
    }

    [HideInInspector()]
    /// <summary>Variable used by editor. If 'TRUE' the bounds of the light will be drawn in grey</summary>
    public bool EDITOR_SHOW_BOUNDS = true;
    [HideInInspector()]
    /// <summary>Variable used by editor script. If 'TRUE' mesh gizmos will be drawn.</summary>
    public bool EDITOR_SHOW_MESH = false;

    private MeshRenderer _renderer;
    private MeshFilter _filter;
    private Mesh _mesh;
    private RaycastHit rh1, rh2;

    private float s = 0;
    private int targetVertexCount = 0;
    private Vector3 point;
    private Quaternion nRot = Quaternion.identity;
    private Quaternion pRot = Quaternion.identity;
    private float[] dst = new float[1024];
    private Collider[] objs = new Collider[512];

    private List<GameObject> identifiedObjects = new List<GameObject>();
    private List<GameObject> unidentifiedObjects = new List<GameObject>();

    private static event Light2DEvent OnBeamEnter = null;
    private static event Light2DEvent OnBeamStay = null;
    private static event Light2DEvent OnBeamExit = null;

    private Vector3[] circleLookup = new Vector3[0];
    private Vector3[] vertices = new Vector3[0];
    private Vector3[] normals = new Vector3[0];
    private Vector2[] uvs = new Vector2[0];
    private Color32[] mColors = new Color32[0];
    private int[] triangles = new int[0];

    [SerializeField]
    private float lightRadius = 1;
    [SerializeField]
    private float sweepStart = 0;
    [SerializeField]
    private float coneStart = 0;
    [SerializeField]
    private float sweepSize = 360f;
    [SerializeField]
    private Color lightColor = new Color(0.8f, 1f, 1f, 0);
    [SerializeField]
    private LightDetailSetting lightDetail = LightDetailSetting.Rays_300;
    [SerializeField]
    private Material lightMaterial;
    [SerializeField]
    private LayerMask shadowLayer = -1; // -1 = EVERYTHING, 0 = NOTHING, 1 = DEFAULT
    [SerializeField]
    private bool useEvents = false;
    [SerializeField]
    private bool isShadowCaster = false;
    [SerializeField]
    private bool lightEnabled = true;


    private static int totalLightsRendered = 0;
    private static int totalLightsUpdated = 0;


    [HideInInspector()]
    /// <summary>Returns the number of Render updates currently occuring</summary>
    public static int TotalLightsRendered { get { return totalLightsRendered; } }
    [HideInInspector()]
    /// <summary>Retunrs the number of light meshes being updated occuring</summary>
    public static int TotalLightsUpdated { get { return totalLightsUpdated; } }


    /// <summary>Sets the Radius of the light. Value clampedbetween 0.001f and Mathf.Infinity</summary>
    public float LightRadius { get { return lightRadius; } set { lightRadius = Mathf.Clamp(value, 0.001f, Mathf.Infinity); flagMeshUpdate = true; } }
    /// <summary>Sets the Radius of the light. Value clampedbetween 0.001f and Mathf.Infinity</summary>
    public float LightConeStart { get { return sweepStart; } set { sweepStart = value; flagCircleUpdate = true; flagMeshUpdate = true; } }
    /// <summary>Sets the Radius of the light. Value clampedbetween 0.001f and Mathf.Infinity</summary>
    public float LightConeAngle { get { return sweepSize; } set { sweepSize = Mathf.Clamp(value, 0f, 360f); coneStart = -(value / 2f); flagCircleUpdate = true; flagMeshUpdate = true; } }
    /// <summary>Sets the Radius of the light. Value clampedbetween 0.001f and Mathf.Infinity</summary>
    public Color LightColor { get { return lightColor; } set { lightColor = value; flagColorUpdate = true; flagMeshUpdate = true; } }
    /// <summary>Sets the Radius of the light. Value clampedbetween 0.001f and Mathf.Infinity</summary>
    public LightDetailSetting LightDetail { get { return lightDetail; } set { lightDetail = value; flagTrianglesUpdate = true; flagNormalsUpdate = true; flagCircleUpdate = true; flagColorUpdate = true; flagMeshUpdate = true; } }
    /// <summary>Sets the Radius of the light. Value clampedbetween 0.001f and Mathf.Infinity</summary>
    public Material LightMaterial { get { return lightMaterial; } set { lightMaterial = value; flagMeshUpdate = true; } }
    /// <summary>The layer which responds to the raycasts. If a collider is on the same layer then a shadow will be cast from that collider</summary>
    public LayerMask ShadowLayer { get { return shadowLayer; } set { shadowLayer = value; flagMeshUpdate = true; } }
    /// <summary>When set to 'TRUE' the light will produce inverse of what the light produces which is shadow.</summary>
    public bool IsShadowEmitter { get { return isShadowCaster; } set { isShadowCaster = value; flagMeshUpdate = true; } }
    /// <summary>When set to 'TRUE' the light will use events such as 'OnBeamEnter(Light2D, GameObject)', 'OnBeamStay(Light2D, GameObject)', and 'OnBeamExit(Light2D, GameObject)'</summary>
    public bool EnableEvents { get { return useEvents; } set { useEvents = value; } }
    /// <summary>Returns 'TRUE' when light is enabled</summary>
    public bool LightEnabled { get { return lightEnabled; } set { if (value != lightEnabled) { lightEnabled = value; if (isShadowCaster) UpdateMesh_RadialShadow(); else UpdateMesh_Radial(); } } }
    /// <summary>Returns 'TRUE' when light is visible</summary>
    public bool IsVisible { get { if (_renderer) return _renderer.isVisible; else return false; } }


    // Depriciated Variables
    [System.Obsolete("Depreciated. Use 'LightConeAngle' instead.")]
    /// <summary>[Depreciated] Use 'LightConeAngle' instead.</summary>
    public float SweepSize { get { return LightConeAngle; } set { LightConeAngle = value; } }
    [System.Obsolete("Depreciated. Use 'transform.Rotate()' to rotate your light. This value is now calculated automatically via 'LightConeAngle'.")]
    /// <summary>[Depreciated] Use 'LightConeStart' instead.</summary>
    public float SweepStart { get { return sweepStart; } set { sweepStart = value; } }
    [System.Obsolete("Depreciated. No Longer Used.")]
    /// <summary>[Depreciated] No Longer Supported</summary>
    public bool ignoreOptimizations = false;
    [System.Obsolete("Depreciated. Use 'AllowLightsToHide' instead.")]
    /// <summary>[Depreciated] No Longer Supported.</summary>
    public bool allowHideInsideColliders { get { return false; } set { } }
    [System.Obsolete("Depreciated. Use 'gameObject.isStatic' instead.")]
    /// <summary>[Depreciated] Use 'gameObject.isStatic' instead.</summary>
    public bool IsStatic { get { return gameObject.isStatic; } set { gameObject.isStatic = value; } }


    private bool flagCircleUpdate = true;
    private bool flagColorUpdate = true;
    private bool flagMeshUpdate = true;
    private bool flagNormalsUpdate = true;
    private bool flagTrianglesUpdate = true;
    private bool flagFinalUpdate = false;
    private bool initialized = false;


    void OnDrawGizmosSelected()
    {
        if (_renderer && EDITOR_SHOW_BOUNDS)
        {
            Gizmos.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            Gizmos.DrawWireCube(_renderer.bounds.center, _renderer.bounds.max - _renderer.bounds.min);
        }
    }

    void OnDrawGizmos()
    {
        if (!isShadowCaster)
            Gizmos.DrawIcon(transform.position, "Light.png", true);
        else
            Gizmos.DrawIcon(transform.position, "Shadow.png", true);
    }

    void OnDestroy()
    {
        if (Application.isPlaying)
        {
            Destroy(_mesh);
            Destroy(_renderer);
            Destroy(_filter);
        }
        else
        {
            DestroyImmediate(_mesh);
            _mesh = null;
            _renderer = null;
            _filter = null;
        }
    }

    void Awake()
    {
        nRot = Quaternion.FromToRotation(Vector3.forward, Vector3.right) * Quaternion.Euler(-90, 0, 0);

        totalLightsRendered = 0;
        totalLightsUpdated = 0;
    }

    void Update()
    {
        totalLightsRendered = 0;
        totalLightsUpdated = 0;
    }

    void LateUpdate()
    {
        if (_renderer)
        {
            _renderer.enabled = lightEnabled;
            objs = Physics.OverlapSphere(transform.position, lightRadius, shadowLayer);

            if (objs.Length > 0)
            {
                flagFinalUpdate = true;

                if (Quaternion.Angle(transform.rotation, pRot) != 0)
                    flagMeshUpdate = true;
                pRot = transform.rotation;
            }
            else if (flagFinalUpdate == true)
            {
                flagMeshUpdate = true;
                flagFinalUpdate = false;
            }

            for (int c = 0; c < objs.Length; c++)
            {
                for (int i = 0; i < 2; i++)
                {
                    s = (gameObject.transform.position - ((i % 2 == 0) ? objs[c].bounds.min : objs[c].bounds.max)).sqrMagnitude;

                    if (dst[(c * 2) + i] != s)
                    {
                        dst[(c * 2) + i] = s;
                        flagMeshUpdate = true;
                        break;
                    }
                }
            }
            objs = null;
        }

        Draw();
    }

    /// <summary>
    /// Only call Draw within an editor function. You would not normally need to make a call to Draw when in game.
    /// </summary>
    public void Draw()
    {
        Draw(flagCircleUpdate, flagTrianglesUpdate, flagColorUpdate, flagNormalsUpdate, flagMeshUpdate);
    }

    /// <summary>
    /// Only call Draw within an editor function. You would not normally need to make a call to Draw when in game.
    /// </summary>
    /// <param name="_flagCircleUpdate">A 'TRUE' flag forces the circle lookup table to update.</param>
    /// <param name="_flagTrianglesUpdate">A 'TRUE' flag forces the triangles list to update.</param>
    /// <param name="_flagColorUpdate">A 'TRUE' flag forces the colors list to update.</param>
    /// <param name="_flagNormalsUpdate">A 'TRUE' flag forces the normals list to update.</param>
    /// <param name="_flagMeshUpdate">A 'TRUE' flag forces the mesh to udpate. [Default: TRUE]</param>
    public void Draw(bool _flagCircleUpdate, bool _flagTrianglesUpdate, bool _flagColorUpdate, bool _flagNormalsUpdate, bool _flagMeshUpdate = true)
    {
        // === Assign Rebuild Flags ============================
        flagCircleUpdate = _flagCircleUpdate;
        flagColorUpdate = _flagColorUpdate;
        flagNormalsUpdate = _flagNormalsUpdate;
        flagTrianglesUpdate = _flagTrianglesUpdate;
        flagMeshUpdate = _flagMeshUpdate;

        // === Initialize Variables =============================
        targetVertexCount = (int)lightDetail + 2;

        _filter = gameObject.GetComponent<MeshFilter>();
        if (_filter == null)
            _filter = gameObject.AddComponent<MeshFilter>();

        _renderer = gameObject.GetComponent<MeshRenderer>();
        if (_renderer == null)
            _renderer = gameObject.AddComponent<MeshRenderer>();

        _filter.hideFlags = HideFlags.DontSave;
        _renderer.hideFlags = HideFlags.DontSave;

        if (initialized && Application.isPlaying)
        {
            if (!_renderer.isVisible)
                return;

            totalLightsRendered++;

            if (gameObject.isStatic)
                return;
        }

        // === Begin Building Mesh ============================
        if (circleLookup.Length < 1 || flagCircleUpdate)     // CIRCLE TABLE
            UpdateCircleLookup();
        if (normals.Length < 1 || flagNormalsUpdate)        // NORMALS
            UpdateNormals();
        if (mColors.Length < 1 || flagColorUpdate)          // COLORS
            UpdateColors();
        if (triangles.Length < 1 || flagTrianglesUpdate)    // TRIANGLES
            UpdateTriangles_Radial();
        if (_mesh == null || flagMeshUpdate)
        {
            // Check for Udpates and Clear Events
            if (Application.isPlaying && useEvents)
                unidentifiedObjects.Clear();

            if (_mesh == null)
            {
                _mesh = new Mesh();
                _mesh.name = "LightMesh_" + gameObject.GetInstanceID();
                _mesh.hideFlags = HideFlags.HideAndDontSave;
            }

            if (isShadowCaster)
                UpdateMesh_RadialShadow();
            else
                UpdateMesh_Radial();

            totalLightsUpdated++;
        }

        // === Finish Event Checks
        if (Application.isPlaying && useEvents)
        {
            for (int i = 0; i < unidentifiedObjects.Count; i++)
            {
                if (identifiedObjects.Contains(unidentifiedObjects[i]))
                {
                    if (OnBeamStay != null)
                        OnBeamStay(this, unidentifiedObjects[i]);
                }

                if (!identifiedObjects.Contains(unidentifiedObjects[i]))
                {
                    identifiedObjects.Add(unidentifiedObjects[i]);

                    if (OnBeamEnter != null)
                        OnBeamEnter(this, unidentifiedObjects[i]);
                }
            }

            for (int i = 0; i < identifiedObjects.Count; i++)
            {
                if (!unidentifiedObjects.Contains(identifiedObjects[i]))
                {
                    if (OnBeamExit != null)
                        OnBeamExit(this, identifiedObjects[i]);

                    identifiedObjects.Remove(identifiedObjects[i]);
                }
            }
        }

        initialized = true;

        _filter.hideFlags = HideFlags.HideInInspector;
        _renderer.hideFlags = HideFlags.HideInInspector;
    }

    void UpdateCircleLookup()
    {
        if (circleLookup.Length != ((int)lightDetail + 1))
            circleLookup = new Vector3[(int)lightDetail + 1];

        float ld = ((float)sweepSize / (int)lightDetail);
        for (int i = 0; i < circleLookup.Length; i++)
            circleLookup[i] = new Vector3((Mathf.Cos(((sweepStart + coneStart) + (ld * (i))) * Mathf.Deg2Rad)), (Mathf.Sin(((sweepStart + coneStart) + (ld * (i))) * Mathf.Deg2Rad)), 0);

        flagCircleUpdate = false;
    }

    void UpdateNormals()
    {
        if (normals.Length != targetVertexCount)
            normals = new Vector3[targetVertexCount];

        for (int i = 0; i < normals.Length; i++)
            normals[i] = -Vector3.forward;

        flagNormalsUpdate = false;
    }

    void UpdateColors()
    {
        if (mColors.Length != targetVertexCount)
            mColors = new Color32[targetVertexCount];

        for (int c = 0; c < mColors.Length; c++)
            mColors[c] = lightColor;

        flagColorUpdate = false;
    }

    void UpdateTriangles_Radial()
    {
        if (triangles.Length != ((int)lightDetail * 3))
            triangles = new int[(int)lightDetail * 3];

        for (int i = 0, v = 0; i < triangles.Length; i += 3, v++)
        {
            triangles[i + 0] = 0;
            triangles[i + 1] = v + 2;
            triangles[i + 2] = v + 1;
        }

        flagTrianglesUpdate = false;
    }

    List<Vector3> hVerts = new List<Vector3>();
    List<Vector2> hUvs = new List<Vector2>();
    List<int> hTri = new List<int>();
    List<Vector3> hNorm = new List<Vector3>();
    List<Color32> hColor = new List<Color32>();
    void UpdateMesh_RadialShadow()
    {
        hVerts.Clear();
        hUvs.Clear();
        hTri.Clear();
        hNorm.Clear();
        hColor.Clear();

        int triCount = 0;

        for (int i = 0; i < circleLookup.Length - 1; i++)
        {
            Vector3 p1 = circleLookup[i] * lightRadius;
            Vector3 p2 = circleLookup[i + 1] * lightRadius;

            if (Physics.Raycast(transform.position, circleLookup[i], out rh1, lightRadius, shadowLayer)
                && Physics.Raycast(transform.position, circleLookup[i + 1], out rh2, lightRadius, shadowLayer))
            {
                Vector3 uvp;

                if (Application.isPlaying && useEvents && !unidentifiedObjects.Contains(rh1.transform.gameObject))
                    unidentifiedObjects.Add(rh1.transform.gameObject);

                uvp = transform.InverseTransformPoint(rh1.point);  // 0 
                hVerts.Add(uvp);
                hNorm.Add(-Vector3.forward);
                hColor.Add(lightColor);
                hUvs.Add(new Vector2((0.5f + (uvp.x * 0.5f) / lightRadius), (0.5f + (uvp.y * 0.5f) / lightRadius)));

                uvp = p1;
                hVerts.Add(uvp);
                hNorm.Add(-Vector3.forward);
                hColor.Add(lightColor);
                hUvs.Add(new Vector2((0.5f + (uvp.x * 0.5f) / lightRadius), (0.5f + (uvp.y * 0.5f) / lightRadius)));

                uvp = transform.InverseTransformPoint(rh2.point);  // 2
                hVerts.Add(uvp);
                hNorm.Add(-Vector3.forward);
                hColor.Add(lightColor);
                hUvs.Add(new Vector2((0.5f + (uvp.x * 0.5f) / lightRadius), (0.5f + (uvp.y * 0.5f) / lightRadius)));

                uvp = p2;
                hVerts.Add(uvp);
                hNorm.Add(-Vector3.forward);
                hColor.Add(lightColor);
                hUvs.Add(new Vector2((0.5f + (uvp.x * 0.5f) / lightRadius), (0.5f + (uvp.y * 0.5f) / lightRadius)));

                hTri.Add(triCount + 2);
                hTri.Add(triCount + 1);
                hTri.Add(triCount + 0);

                hTri.Add(triCount + 2);
                hTri.Add(triCount + 3);
                hTri.Add(triCount + 1);

                triCount += 4;
            }
        }

        _mesh.Clear();
        _mesh.vertices = hVerts.ToArray();
        _mesh.triangles = hTri.ToArray();
        _mesh.normals = hNorm.ToArray();
        _mesh.uv = hUvs.ToArray();
        _mesh.colors32 = hColor.ToArray();

        _mesh.RecalculateBounds();

        if (lightMaterial == null)
            lightMaterial = (Material)Resources.Load("RadialLight");

        if (!Application.isPlaying)
        {
            _filter.sharedMesh = _mesh;
            _renderer.sharedMaterial = lightMaterial;
        }
        else
        {
            _filter.mesh = _mesh;
            _renderer.material = lightMaterial;
        }

        flagMeshUpdate = false;
    }

    void UpdateMesh_Radial()
    {
        if (vertices.Length != targetVertexCount)
            vertices = new Vector3[targetVertexCount];
        if (uvs.Length != targetVertexCount)
            uvs = new Vector2[targetVertexCount];

        vertices[0] = Vector3.zero;
        uvs[0] = new Vector2(0.5f, 0.5f);

        for (int i = 0; i < circleLookup.Length; i++)
        {
            point = circleLookup[i] * lightRadius;

            if (Physics.Raycast(transform.position, transform.TransformDirection(point), out rh1, lightRadius, shadowLayer))
            {
                point = transform.InverseTransformPoint(rh1.point);

                if (Application.isPlaying && useEvents && !unidentifiedObjects.Contains(rh1.transform.gameObject))
                    unidentifiedObjects.Add(rh1.transform.gameObject);
            }

            vertices[i + 1] = point;

            if (i > 0)
                uvs[i] = new Vector2(0.5f + (vertices[i].x * 0.5f) / lightRadius, 0.5f + (vertices[i].y * 0.5f) / lightRadius);
        }
        uvs[vertices.Length - 1] = new Vector2(0.5f + (vertices[vertices.Length - 1].x * 0.5f) / lightRadius, 0.5f + (vertices[vertices.Length - 1].y * 0.5f) / lightRadius);

        _mesh.Clear();
        _mesh.vertices = vertices;
        _mesh.triangles = triangles;
        _mesh.normals = normals;
        _mesh.uv = uvs;
        _mesh.colors32 = mColors;

        _mesh.RecalculateBounds();

        if (lightMaterial == null)
            lightMaterial = (Material)Resources.Load("RadialLight");

        if (!Application.isPlaying)
        {
            _filter.sharedMesh = _mesh;
            _renderer.sharedMaterial = lightMaterial;
        }
        else
        {
            _filter.mesh = _mesh;
            _renderer.material = lightMaterial;
        }

        flagMeshUpdate = false;
    }

    /// <summary>
    /// A custom 'LookAt' funtion which looks along the lights 'Right' direction. This function was implimented for those unfamiliar with Quaternion math as
    /// without that math its nearly impossible to get the right results using the typical 'transform.LookAt' function.
    /// </summary>
    /// <param name="_target">The GameObject you want the light to look at.</param>
    public void LookAt(GameObject _target)
    {
        LookAt(_target.transform.position);
    }
    /// <summary>
    /// A custom 'LookAt' funtion which looks along the lights 'Right' direction. This function was implimented for those unfamiliar with Quaternion math as
    /// without that math its nearly impossible to get the right results using the typical 'transform.LookAt' function.
    /// </summary>
    /// <param name="_target">The Transform you want the light to look at.</param>
    public void LookAt(Transform _target)
    {
        LookAt(_target.position);
    }
    /// <summary>
    /// A custom 'LookAt' funtion which looks along the lights 'Right' direction. This function was implimented for those unfamiliar with Quaternion math as
    /// without that math its nearly impossible to get the right results using the typical 'transform.LookAt' function.
    /// </summary>
    /// <param name="_target">The Vecto3 position you want the light to look at.</param>
    public void LookAt(Vector3 _target)
    {
        transform.rotation = Quaternion.LookRotation(transform.position - _target, Vector3.forward) * nRot;
    }

    /// <summary>
    /// Toggles the light on or off
    /// </summary>
    /// <param name="_updateMesh">If 'TRUE' mesh will be forced to update. Use this if your light is dynamic when toggling it on.</param>
    /// <returns>'TRUE' if light is on.</returns>
    public bool ToggleLight(bool _updateMesh = false)
    {
        lightEnabled = !lightEnabled;

        if (_updateMesh)
        {
            if (isShadowCaster)
                UpdateMesh_RadialShadow();
            else
                UpdateMesh_Radial();
        }

        return lightEnabled;
    }

    /// <summary>
    /// Provides and easy way to register your event method. The delegate takes the form of 'Foo(Light2D, GameObject)'.
    /// </summary>
    /// <param name="_eventType">Choose from 3 event types. 'OnEnter', 'OnStay', or 'OnExit'. Does not accept flags as argument.</param>
    /// <param name="_eventMethod">A callback method in the form of 'Foo(Light2D, GameObject)'.</param>
    public static void RegisterEventListener(LightEventListenerType _eventType, Light2DEvent _eventMethod)
    {
        if (_eventType == LightEventListenerType.OnEnter)
            OnBeamEnter += _eventMethod;

        if (_eventType == LightEventListenerType.OnStay)
            OnBeamStay += _eventMethod;

        if (_eventType == LightEventListenerType.OnExit)
            OnBeamExit += _eventMethod;
    }

    /// <summary>
    /// Provides and easy way to unregister your events. Usually used in the 'OnDestroy' and 'OnDisable' functions of your gameobject.
    /// </summary>
    /// <param name="_eventType">Choose from 3 event types. 'OnEnter', 'OnStay', or 'OnExit'. Does not accept flags as argument.</param>
    /// <param name="_eventMethod">The callback method you wish to remove.</param>
    public static void UnregisterEventListener(LightEventListenerType _eventType, Light2DEvent _eventMethod)
    {
        if (_eventType == LightEventListenerType.OnEnter)
            OnBeamEnter -= _eventMethod;

        if (_eventType == LightEventListenerType.OnStay)
            OnBeamStay -= _eventMethod;

        if (_eventType == LightEventListenerType.OnExit)
            OnBeamExit -= _eventMethod;
    }

    /// <summary>
    /// Easy static function for creating 2D lights.
    /// </summary>
    /// <param name="_position">Sets the position of the created light</param>
    /// <param name="_lightColor">Sets the color of the created light</param>
    /// <param name="_lightRadius">Sets the radius of the created light</param>
    /// <param name="_lightConeAngle">Sets the cone angle of the light</param>
    /// <param name="_lightDetail">Sets the detail of the light</param>
    /// <param name="_useEvents">If 'TRUE' event messages will be sent.</param>
    /// <param name="_isShadow">If 'TRUE' light will be inverted to generate shadow mesh.</param>
    /// <returns>Returns the created Light2D object, NOT the gameobject.</returns>
    public static Light2D Create(Vector3 _position, Color _lightColor, float _lightRadius = 1, int _lightConeAngle = 360, LightDetailSetting _lightDetail = LightDetailSetting.Rays_500, bool _useEvents = false, bool _isShadow = false)
    {
        return Create(_position, (Material)Resources.Load("RadialLight"), _lightColor, _lightRadius, _lightConeAngle, _lightDetail, _useEvents, _isShadow);
    }

    /// <summary>
    /// Easy static function for creating 2D lights.
    /// </summary>
    /// <param name="_position">Sets the position of the created light</param>
    /// <param name="_lightMaterial">Sets the Material of the light</param>
    /// <param name="_lightColor">Sets the color of the created light</param>
    /// <param name="_lightRadius">Sets the radius of the created light</param>
    /// <param name="_lightConeAngle">Sets the cone angle of the light</param>
    /// <param name="_lightDetail">Sets the detail of the light</param>
    /// <param name="_useEvents">If 'TRUE' event messages will be sent.</param>
    /// <param name="_isShadow">If 'TRUE' light will be inverted to generate shadow mesh.</param>
    /// <returns>Returns the created Light2D object, NOT the gameobject.</returns>
    public static Light2D Create(Vector3 _position, Material _lightMaterial, Color _lightColor, float _lightRadius = 1, int _lightConeAngle = 360, LightDetailSetting _lightDetail = LightDetailSetting.Rays_500, bool _useEvents = false, bool _isShadow = false)
    {
        GameObject obj = new GameObject("New Light");
        obj.transform.position = _position;

        Light2D l2D = obj.AddComponent<Light2D>();
        l2D.LightMaterial = _lightMaterial;
        l2D.LightColor = _lightColor;
        l2D.LightDetail = _lightDetail;
        l2D.LightRadius = _lightRadius;
        l2D.LightConeAngle = _lightConeAngle;
        l2D.ShadowLayer = -1;
        l2D.EnableEvents = _useEvents;
        l2D.IsShadowEmitter = _isShadow;

        return l2D;
    }
}
