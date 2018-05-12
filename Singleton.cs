using System.Collections;
using UnityEngine;

public class Singleton<T>  : MonoBehaviour where T: MonoBehaviour
{
    private static T _instance;  //定义静态的泛型<T>的类型变量_instance
    private static object _lock = new object();//定义一个静态的object类型的_lock并且实例化
    //private static bool applicationIsQuitting = false;//定义bool类型的变量判断程序是否结束

    private void Awake()
    {
        _instance = this.GetComponent<T>();
        DontDestroyOnLoad(this.gameObject);//切换场景保存
    }


    public static T Instance  //定义静态的泛型<T>类型的属性
    {
        get
        {
            //if (applicationIsQuitting)
            //    return null;

            lock (_lock)//_lock 关键字可以用来保存代码完成运行，而不会被其他线程中断。
            {
                if (_instance == null)
                {
                    _instance = (T)FindObjectOfType(typeof(T));

                    //if (FindObjectOfType(typeof(T)).Length>1)
                    //{
                    //    return _instance;
                    //}

                    if (_instance == null)
                    {
                        GameObject singleton = new GameObject();
                        singleton.AddComponent<T>();
                        _instance.name = "singleton" + typeof(T).ToString();
                    }
                }
                return _instance;
            }
        }

    }

    public void OnDestroy()
    {
        _instance = null;
        //applicationIsQuitting=true;
    }

}
