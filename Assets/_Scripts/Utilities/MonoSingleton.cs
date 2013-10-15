using UnityEngine;

/// <summary>
/// Mono singleton. From http://wiki.unity3d.com/index.php?title=Singleton#Generic_Based_Singleton_for_MonoBehaviours
/// </summary>
public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    private static T m_Instance = null;
    public static T Instance
    {
        get
        {
            // Instance requiered for the first time, we look for it
            if( m_Instance == null )
            {
                m_Instance = GameObject.FindObjectOfType(typeof(T)) as T;

                // Object not found, we create a temporary one
                if( m_Instance == null )
                {
                    Debug.LogWarning("MonoSingleton: No instance of " + typeof(T).ToString() + ", a temporary one is created.");
                    m_Instance = new GameObject("MonoSingleton: Temp Instance of " + typeof(T).ToString(), typeof(T)).GetComponent<T>();

                    // Problem during the creation, this should not happen
                    if( m_Instance == null )
                    {
                        Debug.LogError("MonoSingleton: Problem during the creation of " + typeof(T).ToString());
                    }
                }
            }

            return m_Instance;
        }
    }

    /// <summary>
    /// Awake function. Override when necessary and call base.Awake() first.
    /// </summary>
    protected virtual void Awake()
    {
        if( m_Instance == null )
        {
            m_Instance = this as T;
        }
    }

    /// <summary>
    /// Clear the reference when the application quits. Override when necessary and call base.OnApplicationQuit() last.
    /// </summary>
    protected virtual void OnApplicationQuit()
    {
        m_Instance = null;
    }
}

