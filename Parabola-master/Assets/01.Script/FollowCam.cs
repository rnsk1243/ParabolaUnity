using UnityEngine;
using System.Collections;

public class FollowCam : MonoBehaviour {

    ObjectMath math;

    public delegate void OnCameraEvent(Vector3 position, float height);
    public OnCameraEvent CameraSetUp;
    Camera camera;
	// Use this for initialization
	void Start () {
        camera = GetComponent<Camera>();
        addEvent(SetUp);

    }

    void addEvent(OnCameraEvent ev)
    {
        CameraSetUp += ev;
    }
	
    void SetUp(Vector3 pos, float power)
    {
        Debug.Log("이벤트 호출 됨.");
        this.transform.position = new Vector3(pos.x, pos.y, -1.0f);
        camera.orthographicSize = power / 20.0f;
    }

}
