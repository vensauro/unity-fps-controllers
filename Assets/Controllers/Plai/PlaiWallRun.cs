using UnityEngine;
using Cinemachine;
using System.Threading.Tasks;

public class PlaiWallRun : MonoBehaviour
{
  private Rigidbody rb;

  [Range(-1, 1)]
  private int wallAxis = 0;
  private bool isInAir = false;

  private RaycastHit wallHit;

  [Header("Movement")]

  [SerializeField] private Transform orientation;


  [Header("Detection")]
  [SerializeField] private float wallDistance = .6f;
  [SerializeField] private float minimumJumpHeight = 1.5f;


  [Header("Wall Running")]
  [SerializeField] private float wallRunGravity;
  [SerializeField] private float wallRunJumpForce;

  [Header("Camera")]
  [SerializeField] private CinemachineVirtualCamera cam;
  [SerializeField] private float fov;
  [SerializeField] private float wallRunFov;
  [SerializeField] private AnimationCurve fovCurve;
  [SerializeField] private float camTilt;
  [SerializeField] private float camTiltTime;


  [Header("Debug")]
  public bool showRaycasts = true;
  public bool showValues = true;

  void Start()
  {
    rb = GetComponent<Rigidbody>();
  }

  void Update()
  {
    CheckConstraintsToWallRun();

    if (isInAir && wallAxis != 0)
    {
      StartWallRun();
    }
    else
    {
      StopWallRun();
    }
  }

  void CheckConstraintsToWallRun()
  {
    if (Physics.Raycast(transform.position, -orientation.right, out wallHit, wallDistance))
    {
      wallAxis = -1;
    }
    else if (Physics.Raycast(transform.position, orientation.right, out wallHit, wallDistance))
    {
      wallAxis = 1;
    }
    else
    {
      wallAxis = 0;
    }

    isInAir = !Physics.Raycast(transform.position, Vector3.down, minimumJumpHeight);
  }

  private void StartWallRun()
  {
    rb.useGravity = false;
    rb.AddForce(Vector3.down * wallRunGravity);

    IncreaseFov();

    cam.m_Lens.Dutch = Mathf.Lerp(cam.m_Lens.Dutch, camTilt * wallAxis, camTiltTime * Time.deltaTime);

    if (Input.GetButtonDown("Jump"))
    {
      Vector3 wallRunJumpDirection = transform.up + wallHit.normal;
      rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
      rb.AddForce(wallRunJumpDirection * wallRunJumpForce * 100);
    }
  }

  private void StopWallRun()
  {
    rb.useGravity = true;
    ResetFov();
    cam.m_Lens.Dutch = Mathf.Lerp(cam.m_Lens.Dutch, 0, camTiltTime * Time.deltaTime);
  }

  async void IncreaseFov()
  {
    var fovTime = 0f;
    do
    {
      fovTime += Time.deltaTime;
      cam.m_Lens.FieldOfView = Mathf.Lerp(cam.m_Lens.FieldOfView, wallRunFov, fovCurve.Evaluate(fovTime));
      await Task.Yield();
    } while (fovTime < 1);
  }

  async void ResetFov()
  {
    var fovTime = 0f;

    do
    {
      fovTime += Time.deltaTime;
      cam.m_Lens.FieldOfView = Mathf.Lerp(cam.m_Lens.FieldOfView, fov, fovCurve.Evaluate(fovTime));
      await Task.Yield();
    } while (fovTime < 1);
  }


  private void OnDrawGizmos()
  {
    if (showRaycasts)
    {
      Gizmos.color = Color.magenta;
      Gizmos.DrawRay(transform.position, -orientation.right * wallDistance);

      Gizmos.color = Color.cyan;
      Gizmos.DrawRay(transform.position, orientation.right * wallDistance);

      Gizmos.color = Color.black;
      Gizmos.DrawRay(transform.position, Vector3.down * minimumJumpHeight);
    }
  }

  private void OnGUI()
  {
    if (!showValues) return;

    if (wallAxis != 0)
      GUI.Label(new Rect(10, 10, 150, 100), wallAxis == 1 ? "Right" : "Left");

    if (isInAir)
      GUI.Label(new Rect(10, 40, 150, 100), "Height to wallrun ok");

    GUI.Label(new Rect(160, 10, 150, 100), $"Gravity activated: {rb.useGravity}");
    GUI.Label(new Rect(160, 40, 150, 100), $"FOV: {cam.m_Lens.FieldOfView}");
  }

}
