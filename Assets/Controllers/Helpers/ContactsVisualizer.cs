using UnityEngine;

public class ContactsVisualizer : MonoBehaviour
{

  void Update()
  {

  }
  public Material trailMaterial;

  /// <summary>
  /// Called when the ghost collides with a solid object
  /// </summary>
  /// <param name="collision">The collision information</param>
  private void OnCollisionStay(Collision collision)
  {
    foreach (ContactPoint contact in collision.contacts)
    {
      LeaveTrail(contact.point, 0.1f, trailMaterial);
    }
  }

  /// <summary>
  /// Places a single sphere at a specific point in space, and sets it to auto-destroy
  /// </summary>
  /// <param name="point">The world point at which to spawn the sphere</param>
  /// <param name="scale">The local scale of the sphere</param>
  /// <param name="material">The material to apply to the sphere</param>
  private void LeaveTrail(Vector3 point, float scale, Material material)
  {
    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    sphere.transform.localScale = Vector3.one * scale;
    sphere.transform.position = point;
    sphere.transform.parent = transform.parent;
    sphere.GetComponent<Collider>().enabled = false;
    sphere.GetComponent<Renderer>().material = material;
    Destroy(sphere, 10f);
  }
}
