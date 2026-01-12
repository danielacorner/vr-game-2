using UnityEngine;

/// <summary>
/// Simple test script to verify ANY script can execute
/// </summary>
public class TestAwake : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("========================================");
        Debug.Log("========================================");
        Debug.Log("========================================");
        Debug.Log("TEST AWAKE CALLED!");
        Debug.Log("========================================");
        Debug.Log("========================================");
        Debug.Log("========================================");
    }

    private void Start()
    {
        Debug.Log("TEST START CALLED!");
    }

    private void Update()
    {
        if (Time.frameCount % 300 == 0) // Every ~5 seconds
        {
            Debug.Log("TEST UPDATE RUNNING!");
        }
    }
}
