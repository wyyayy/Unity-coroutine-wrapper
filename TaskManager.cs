/*
Features:
    1. Create coroutine anywhere 
    2. Stop/Pause a coroutine
    3. Fished and exception event Listening

Usage:
    myTask = new Task(_doSomething());

    IEnumerator _doSomething()
    {
        ....
    }

    /// Listening fished event
    myTask.Finished += isManual=>
    {
        Debug.Log("Task is finished!");
    };

    /// Listening exception event
    myTask.ExceptionHandler += exception=>
    {
        Debug.LogErr("Exception occurred during executing!");
    };

    /// Pause task 
    myTask.Pause();

    /// Stop task
    myTask.Stop();

*/

using System;
using UnityEngine;
using System.Collections;

/// A Task object represents a coroutine.  Tasks can be started, paused, and stopped.
/// Task's fished and exception event can be listened.
/// 
/// It is an error to attempt to start a task that has been stopped or which has
/// naturally terminated.
public class Task
{
    /// Returns true if and only if the coroutine is running.  Paused tasks
    /// are considered to be running.
    public bool isRunning { get { return _coroutineWrapper.Running; } }
    /// Returns true if and only if the coroutine is currently paused.
    public bool isPaused { get { return _coroutineWrapper.Paused; } }

    public Coroutine coroutine { get { return _coroutineWrapper._internalCo; } }

    /// Delegate for termination subscribers.  manual is true if and only if
    /// the coroutine was stopped with an explicit call to Stop().
    public delegate void FinishedHandler(bool manual);

    /// Termination event.  Triggered when the coroutine completes execution.
    public event FinishedHandler Finished;

    /// Exception handler
    public event Action<Exception> ExceptionHandler;

    /// Internal coroutine wrapper
    private CoroutineWrapper _coroutineWrapper;

    /// Creates a new Task object for the given coroutine.
    ///
    /// If autoStart is true (default) the task is automatically started
    /// upon construction.
    public Task(IEnumerator enumerator, bool autoStart = true)
    {
        CoroutineWrapper._Init();
        _coroutineWrapper = new CoroutineWrapper(enumerator, this);
        _coroutineWrapper.Finished += TaskFinished;

        if (autoStart) Start();
    }

    /// Begins execution of the coroutine
    public void Start()
    {
        _coroutineWrapper.Start();
    }

    /// Discontinues execution of the coroutine at its next yield.
    public void Stop()
    {
        _coroutineWrapper.Stop();
    }

    public void Pause()
    {
        _coroutineWrapper.Pause();
    }

    public void Unpause()
    {
        _coroutineWrapper.Unpause();
    }

    internal void OnException(Exception e)
    {
        if (ExceptionHandler != null) ExceptionHandler(e);
        else Debug.LogError(e);
    }

    void TaskFinished(bool manual)
    {
        FinishedHandler handler = Finished;
        if (handler != null)
            handler(manual);
    }
}

public class CoroutineWrapper
{
    public class _Helper : MonoBehaviour { }

    static _Helper _CoroutineHelper;

    internal static void _Init()
    {
        if (_CoroutineHelper == null)
        {
            GameObject go = new GameObject("_CoroutineHelper");
            GameObject.DontDestroyOnLoad(go);
            _CoroutineHelper = go.AddComponent<_Helper>();
        }
    }

    public bool Running { get { return running; } }
    public bool Paused { get { return paused; } }

    public delegate void FinishedHandler(bool manual);
    public event FinishedHandler Finished;

    IEnumerator coroutine;
    internal Coroutine _internalCo;
    bool running;
    bool paused;
    bool stopped;

    Task _task;

    public CoroutineWrapper(IEnumerator c, Task task)
    {
        coroutine = c;
        _task = task;
    }

    public void Pause()
    {
        paused = true;
    }

    public void Unpause()
    {
        paused = false;
    }

    public void Start()
    {
        running = true;
        _internalCo = _CoroutineHelper.StartCoroutine(CallWrapper());
    }

    public void Stop()
    {
        stopped = true;
        running = false;

        if (_internalCo != null)
        {
            _CoroutineHelper.StopCoroutine(_internalCo);
            _internalCo = null;
        }
    }

    IEnumerator CallWrapper()
    {
        yield return null;
        IEnumerator enumerator = coroutine;
        while (running)
        {
            if (paused) yield return null;
            else
            {
                if (enumerator != null)
                {
                    bool yieldNextSucess = false;

                    try
                    {
                        yieldNextSucess = enumerator.MoveNext();
                    }
                    catch (System.Exception err)
                    {
                        _task.OnException(err);
                    }

                    if (yieldNextSucess)
                    {
                        yield return enumerator.Current;
                        continue;
                    }
                    else running = false;
                }
                else
                {
                    running = false;
                }
            }
        }

        FinishedHandler handler = Finished;
        if (handler != null) handler(stopped);

        _internalCo = null;
    }
}