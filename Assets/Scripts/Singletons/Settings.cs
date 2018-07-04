using UnityEngine;

public class Settings : MonoBehaviour {
    public static Settings Instance;

    public float LookSensitivity;
    public KeyCode JumpKey = KeyCode.Space;
    public KeyCode SprintKey = KeyCode.LeftShift;
    public KeyCode EaseOverloadKey = KeyCode.Alpha2;
    
    private void Start() {
        if (Instance == null) Instance = this;
    }
}