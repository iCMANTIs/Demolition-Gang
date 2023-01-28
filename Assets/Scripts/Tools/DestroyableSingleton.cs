using UnityEngine;

public class  DestroyableSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private T instance;
    public T Instance => instance;


    protected virtual void Awake()
    {
        if (instance == null)
            instance = GameObject.FindObjectOfType<T>();

        if (instance != null && instance.gameObject.GetInstanceID() == GetInstanceID())
            Destroy(gameObject);

    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {

    }

    protected virtual void OnDestroy()
    {
        instance = null;    
    }
}
