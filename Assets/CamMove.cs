using UnityEngine;
using System.Collections;

public class CamMove : MonoBehaviour {

    public GameObject camTarget;
    public bool invertableX;
    public bool invertableY;
    public float radiusTarget;
    private float azim = 0.0f, elev = 40.0f;

	// Use this for initialization
	void Start () {

        if (!camTarget)
        {
            Debug.LogError("No Target Attached !");
            return;
        }

        if (radiusTarget < 1.0f)
            radiusTarget = 1.0f;

        float azimAngle = Mathf.Deg2Rad * (azim - 90.0f);
        float elevAngle = Mathf.Deg2Rad * (elev - 20.0f);

        Vector3 playerPos = camTarget.transform.position;

        Vector3 camPos = new Vector3(radiusTarget * Mathf.Cos(azimAngle), radiusTarget * Mathf.Sin(elevAngle), radiusTarget * Mathf.Sin(azimAngle));

        this.transform.position = camPos;
        this.transform.LookAt(playerPos);
        
	}
	
	// Update is called once per frame
	void Update () {
        if (!camTarget)  return;

        UpdateCamPosition();

        float azimAngle = Mathf.Deg2Rad * (azim - 90.0f);
        float elevAngle = Mathf.Deg2Rad * (elev - 20.0f);
        
        Vector3 playerPos = camTarget.transform.position;

        Vector3 deltaCamPos = new Vector3(radiusTarget * Mathf.Cos(azimAngle), radiusTarget * Mathf.Sin(elevAngle), radiusTarget * Mathf.Sin(azimAngle));
        Vector3 camPos = playerPos + deltaCamPos;

        this.transform.position = camPos;
        this.transform.LookAt(playerPos);

	}

    void UpdateCamPosition()
    {
        float h = Input.GetAxis("Camera Horizontal");
        //float h2 = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Camera Vertical");

        //if (h2 > h)
            //h = h2;

        int invertedX = 1;
        int invertedY = 1;

        if (invertableX)
            invertedX = -1;
        if (invertableY)
            invertedY = -1;

        azim += 100 * invertedX * h * Time.deltaTime;
        elev += 100 * invertedY * v * Time.deltaTime;

        if (azim > 360.0f)
            azim = 0.0f;

        if (elev > 110.0f)
            elev = 110.0f;

        if (elev < 0.0f)
            elev = 0.0f;
    }
}
