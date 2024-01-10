using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunLaterDemo : MonoBehaviour
{
    // Start is called before the first frame update
    private Util util;
    void Start()
    {
        this.util = new Util(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Click!");
            this.util.RunTaskLater(() =>
            {
                Debug.Log("Task that has been ran later.");
            }, 3f);
        }
    }
}
