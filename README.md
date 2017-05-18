# Unity-coroutine-wrapper
A simple but useful unity coroutine wrapper

# Features:
    1. Create coroutine anywhere 
    2. Stop/Pause a coroutine
    3. Fished and exception event Listening

# Usage:
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
    
    /// Yiled a task
    IEnumerator _doTestYieldTask()
    {
        ....
        yield return myTask.coroutine;
        ....
    }
