using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BehaviourInject;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class Benchmark : MonoBehaviour
{
    [Inject]
    public IReader _dependency;
    [Inject]
    public IReader _dependency2;
    [Inject]
    public IReader _dependency3;
    [Inject]
    public IReader _dependency4;

    public int _iterations = 1000;

    private Injector _injector;

    void Start()
    {
        _injector = GetComponent<Injector>();

        Stopwatch watch = new Stopwatch();
        watch.Start();

        for (int i = 0; i < _iterations; i++)
        {
            _dependency = null;
            _dependency2 = null;
            _dependency3 = null;
            _dependency4 = null;
            _injector.FindAndResolveDependencies();
        }
        watch.Stop();
        Debug.Log(String.Format("{0} ms for {1} operations", watch.ElapsedMilliseconds, _iterations));
    }
}
