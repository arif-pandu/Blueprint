using UnityEngine;

/*
 *  DONT FORGET TO IMPLEMENT BaseAwake() and BaseOnDestroy()
 *  ON THE CHILDREN CLASSES!
 * 
 */

public abstract class StaticReference<T> : MonoBehaviour
{
    protected static T instance;

    protected virtual void BaseAwake(T tInstance)
    {
        if (instance == null)
        {
            instance = tInstance;
        }
    }
    public static T Instance => instance;

    protected virtual void BaseOnDestroy()
    {
        instance = default(T);
    }
}