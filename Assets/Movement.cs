using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Analytics;


//[RequireComponent(typeof(Rigidbody))] stuff to play with later
//[RequireComponent(typeof(CapsuleCollider))]


public class Movement : MonoBehaviour {

    //if (Application.platform != RuntimePlatform.WindowsEditor)
    //          Debug.Log("Do something special here!"); 


    private Vector3 forward;
    private RaycastHit nearestHitInfo;
    private Rigidbody mainRigidbody;
    public float jumpForce; // = 12F;
    public float groundCheckDistance; // = 0.1f;
    public float wallCheckDistance; // = 0.1f;
    public GameObject m_Cam;
    public float walkingSpeed; // = 1.0F;

    private Vector3 move;
    private bool jumpPressed;
    private bool isGrounded = true;
    private bool isWallRunable = false;
    private bool isWallRunning = false;
    private bool isWallJumpable = false;
    private bool isWallJumping = false;
    private uint numWallJumped = 0;
    public float wallRunAngleToleranceInDegrees;
    public float wallJumpAngleToleranceInDegrees;
    public float wallRunHorizontalMultiplier;
    public float gravityMinimizingMultiplier;

    private float cosineAngleBetweenColliderAndPlayer;

    private Vector3 respawnPos;
    
	// Use this for initialization
	void Start () 
    {
        
        if (walkingSpeed < 0.0F)
            walkingSpeed = 1.0F;

        if (jumpForce < 0.0f)
            jumpForce = 0.0f;

        if (!m_Cam)
        {
            Debug.LogError("No Camera Attached !");
            return;
        }

        mainRigidbody = GetComponent<Rigidbody>();
        respawnPos = this.transform.position;

	}
	
	// Update is called once per frame
	void Update ()
    {
        if (!m_Cam) return;

        CheckGroundStatus();
        if (DoWallCheckHitInfo())
        {
            CheckWallStatus();
        }
        else
        {
            isWallRunable = false;
            //isWallRunning = false;
            isWallJumpable = false;
            //isWallJumping = false;
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        jumpPressed = Input.GetButtonDown("Jump");

        if (!isWallRunning)
        {
            Transform camTransform = m_Cam.transform;

            forward = Vector3.Scale(camTransform.forward, new Vector3(1, 0, 1)).normalized;
            move = v * forward + h * camTransform.right;
        }//*/
    }

    //FixedUpdate should be called in fixed intervalls if need be more than once before the next frame
    //accomodates the physics evaluation needed for collisions and rigidbody in general
    void FixedUpdate() 
    {
        if (!m_Cam) return;

        if (isGrounded && jumpPressed)
        {
            if (isWallJumpable && numWallJumped < 4) // && numWallJumped < 3)
            {
                //print("WallJump is here");
                DoWallJump();
            }
            else if (isWallRunable)
            {
                //print("WallRun is here");
                DoWallRun();
            }
            else
            {
                //print("Jump");
                mainRigidbody.velocity = new Vector3(move.x, jumpForce, move.z);
                isGrounded = false;

                SendJumpHeatmaps();
            }
        }
        else if (isWallJumping || (!isGrounded && isWallJumpable)) //???
        {
            if (mainRigidbody.velocity.y < 1.0f)
            {
                Vector3 newMove = move * walkingSpeed;
                newMove.y = mainRigidbody.velocity.y;

                mainRigidbody.velocity = newMove;

                if (isWallJumpable && numWallJumped < 4 && jumpPressed) //numWallJumped < 3 && 
                {
                    DoWallJump();
                }
            }
        }
        else if (isWallRunning)
        {
            Vector3 newMove = wallRunHorizontalMultiplier * move * walkingSpeed;
            newMove.y = mainRigidbody.velocity.y;

            mainRigidbody.velocity = newMove;

            Vector3 extraGravityForce = (Physics.gravity * gravityMinimizingMultiplier) - Physics.gravity;
            mainRigidbody.AddForce(extraGravityForce * mainRigidbody.mass);

            if (!isWallRunable)
                isWallRunning = false;

            
        }
        else
        {
            Vector3 newMove = move * walkingSpeed;
            newMove.y = mainRigidbody.velocity.y;

            mainRigidbody.velocity = newMove;
        }

        /*Vector3 pos = this.transform.position;
        Vector3 newPos = new Vector3(Mathf.Clamp(pos.x, -250, 250), 
                                     Mathf.Clamp(pos.y, -10, 100),  
                                     Mathf.Clamp(pos.z, -250, 250));

       this.transform.position = newPos; //*/

    }

    void CheckGroundStatus()
    {
        //player.transform.position ist in the middle of the render

        RaycastHit hitInfo;

#if UNITY_EDITOR
        Debug.DrawLine(transform.position, transform.position + (Vector3.down * groundCheckDistance), Color.red, 20f, false);
#endif
        
        if (Physics.Raycast(this.transform.position, Vector3.down, out hitInfo, groundCheckDistance))
        {
            if (!isGrounded)
            {
                if (hitInfo.collider.tag == "Pit")
                {
                    var dict = new Dictionary<string, object>();
                    dict["WasWallRunning"] = isWallRunning;
                    dict["WasWallJumping"] = isWallJumping;

                    print("DeathHeatMap");
                    print(isWallRunning);
                    UnityAnalyticsHeatmap.HeatmapEvent.Send("PlayerDeathv2", mainRigidbody.position, Time.time, dict);

                    this.transform.position = respawnPos;
                }
                else
                {
                    var dict = new Dictionary<string, object>();
                    dict["WasWallRunning"] = isWallRunning;
                    dict["WasWallJumping"] = isWallJumping;

                    print("LandedHeatmap");
                    UnityAnalyticsHeatmap.HeatmapEvent.Send("PlayerLandedv2", mainRigidbody.position, Time.time, dict);
                }

                if (numWallJumped > 0)
                {
                    var dict = new Dictionary<string, object>();
                    dict["TimesWallJumped"] = numWallJumped;

                    print("WallJumpEndHeatmap");
                    UnityAnalyticsHeatmap.HeatmapEvent.Send("WallJumpEndv2", mainRigidbody.position, Time.time, dict);
                } 
            }

            isGrounded = true;
            numWallJumped = 0;
            isWallRunning = false;
            isWallJumping = false;

        }
        else
        {
            isGrounded = false;
        }
    }

    bool DoWallCheckHitInfo()
    {
        int numRayCastsWall = 8;
        bool result = false;
        RaycastHit[] hits = new RaycastHit[numRayCastsWall];
        bool[] res = new bool[numRayCastsWall];

        Vector3 side90 = Quaternion.Euler(0, 90, 0) * forward;
        Vector3 side45 = Quaternion.Euler(0, 45, 0) * forward;
        Vector3 side135 = Quaternion.Euler(0, 135, 0) * forward;

        Vector3 checkPosition = this.transform.position - 0.75f * Vector3.up;

#if UNITY_EDITOR
        Debug.DrawLine(checkPosition, checkPosition + (forward * wallCheckDistance), Color.red, 20f, false);
        Debug.DrawLine(checkPosition, checkPosition + (-forward * wallCheckDistance), Color.red, 20f, false);
        Debug.DrawLine(checkPosition, checkPosition + (side45 * wallCheckDistance), Color.red, 20f, false);
        Debug.DrawLine(checkPosition, checkPosition + (-side45 * wallCheckDistance), Color.red, 20f, false);
        Debug.DrawLine(checkPosition, checkPosition + (side90 * wallCheckDistance), Color.red, 20f, false);
        Debug.DrawLine(checkPosition, checkPosition + (-side90 * wallCheckDistance), Color.red, 20f, false);
        Debug.DrawLine(checkPosition, checkPosition + (side135 * wallCheckDistance), Color.red, 20f, false);
        Debug.DrawLine(checkPosition, checkPosition + (-side135 * wallCheckDistance), Color.red, 20f, false);
#endif

        res[0] = Physics.Raycast(checkPosition, forward, out hits[0], wallCheckDistance);
        res[1] = Physics.Raycast(checkPosition, -forward, out hits[1], wallCheckDistance);
        res[2] = Physics.Raycast(checkPosition, side45, out hits[2], wallCheckDistance);
        res[3] = Physics.Raycast(checkPosition, -side45, out hits[3], wallCheckDistance);
        res[4] = Physics.Raycast(checkPosition, side90, out hits[4], wallCheckDistance);
        res[5] = Physics.Raycast(checkPosition, -side90, out hits[5], wallCheckDistance);
        res[6] = Physics.Raycast(checkPosition, side135, out hits[6], wallCheckDistance);
        res[7] = Physics.Raycast(checkPosition, -side135, out hits[7], wallCheckDistance);

        for (int i = 0; i < numRayCastsWall; i++)
        {
            if (res[i])
            {
                nearestHitInfo = hits[i];
                result = true;
                break;                
            }
        }

        if (result)
        {
            for (int j = 0; j < numRayCastsWall; j++)
            {
                if (res[j])
                {
                    if (nearestHitInfo.distance > hits[j].distance)
                    {
                        nearestHitInfo = hits[j];
                    }

                }
            } 
        }

        return result;
    }

    void CheckWallStatus()
    {

        if (move == Vector3.zero || nearestHitInfo.collider.tag != "Wall")
        {
            //cosineAngleBetweenColliderAndPlayer = -1.2345f;
            isWallRunable = false;
            //isWallRunning = false;
            isWallJumpable = false;
            //isWallJumping = false;

            return;
        }

        Vector3 hitNormal = nearestHitInfo.normal;
        Vector3 newMove = transform.TransformVector(move).normalized;

        cosineAngleBetweenColliderAndPlayer = Vector3.Dot(hitNormal, newMove);
        float cosineWallJump = Mathf.Cos(Mathf.Deg2Rad * (0.0f + wallJumpAngleToleranceInDegrees));
        float cosineWallRun = Mathf.Cos(Mathf.Deg2Rad * (90.0f - wallRunAngleToleranceInDegrees));

        if (cosineAngleBetweenColliderAndPlayer > -cosineWallRun && cosineAngleBetweenColliderAndPlayer < cosineWallRun)
        {
            isWallJumpable = false;
            isWallRunable = true;
            //print("is runable");
        }
        else if (cosineAngleBetweenColliderAndPlayer < -cosineWallJump || cosineAngleBetweenColliderAndPlayer > cosineWallJump)
        {
            isWallJumpable = true;
            isWallRunable = false;
            //print("is jumpable");
        }
        else
        {
            isWallJumpable = false;
            isWallRunable = false;
            //print("no luck :(");
        }

        return;
    }

    void DoWallJump()
    {
        var dict = new Dictionary<string, object>();
        dict["WallJumpAngle"] = cosineAngleBetweenColliderAndPlayer;

        print("WallJumpHeatmap");
        UnityAnalyticsHeatmap.HeatmapEvent.Send("WallJumpStartv2", mainRigidbody.position, Time.time, dict);

        isWallJumping = true;
        isWallRunning = false;
        isGrounded = false;

        Vector3 newMove = - move * walkingSpeed;

        mainRigidbody.velocity = new Vector3(0.5f * newMove.x, jumpForce, 0.5f * newMove.z);
        numWallJumped++;
    }

    void DoWallRun()
    {
        var dict = new Dictionary<string, object>();
        dict["WallRunAngle"] = cosineAngleBetweenColliderAndPlayer;

        print("WallRunHeatmap");
        UnityAnalyticsHeatmap.HeatmapEvent.Send("WallRunStartv2", mainRigidbody.position, Time.time, dict);

        isWallRunning = true;
        isWallJumping = false;
        isGrounded = false;

        Vector3 newMove = Vector3.Cross(nearestHitInfo.normal, Vector3.up);
        newMove.y = 0.0f;
        newMove.Normalize();

        if (Vector3.Dot(move.normalized, newMove) < 0)
            newMove = -newMove;

        move = newMove;

        mainRigidbody.velocity = new Vector3(wallRunHorizontalMultiplier * move.x, jumpForce, wallRunHorizontalMultiplier * move.z);
        //print("warum klappt das nicht?");
    }

    void SendJumpHeatmaps()
    {
        var dict = new Dictionary<string, object>();
        dict["IsWallRunable"] = isWallRunable;
        dict["IsWallJumpable"] = isWallJumpable;

        if (isWallJumpable || isWallRunable)
            dict["WallAngle"] = cosineAngleBetweenColliderAndPlayer;

        print("JumpHeatmap");
        UnityAnalyticsHeatmap.HeatmapEvent.Send("Jumpv2", mainRigidbody.position, Time.time, dict);
    }

}
