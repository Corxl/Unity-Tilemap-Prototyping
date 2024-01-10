using System;
using System.Collections;
using UnityEngine;

public class Util
{
    private MonoBehaviour scriptExecuter;
    public Util (MonoBehaviour scriptExecuter)
    {
        this.scriptExecuter = scriptExecuter;
    }
    public void RunTaskLater(Action run, float delaySeconds)
    {
        scriptExecuter.StartCoroutine(taskRoutine(run, delaySeconds));
    }


    private IEnumerator taskRoutine(Action run, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        run?.Invoke();
    }
}