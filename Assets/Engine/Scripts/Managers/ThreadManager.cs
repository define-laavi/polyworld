using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Threading;

public class ThreadManager : MonoBehaviour
{
    static ThreadManager singleton;
    Queue<ThreadInfo> dataQueue = new Queue<ThreadInfo>();

    void Awake()
    {
        singleton = this;
    }

    public static void Add(Action threaded, Action callback = null)
    {
        ThreadStart threadStart = delegate {
            singleton.DataThread(threaded, callback);
        };

        new Thread(threadStart).Start();
    }

    void DataThread(Action threaded, Action callback)
    {
        threaded();
        lock (dataQueue)
        {
            dataQueue.Enqueue(new ThreadInfo(callback));
        }
    }


    void Update()
    {
        if (dataQueue.Count > 0)
            for (int i = 0; i < dataQueue.Count; i++)
                dataQueue.Dequeue().callback?.Invoke();
    }

    struct ThreadInfo
    {
        public readonly Action callback;

        public ThreadInfo(Action callback)
        {
            this.callback = callback;
        }

    }
}
