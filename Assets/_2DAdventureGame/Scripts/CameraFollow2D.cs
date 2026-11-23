using UnityEngine;

// Просте слідування камери за гравцем без залежності від Cinemachine.
public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 6.0f;
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    public Vector2 minBounds = new Vector2(-11.2f, -2.5f);
    public Vector2 maxBounds = new Vector2(10.2f, 1.8f);

    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = target.position + offset;
        desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
        desiredPosition.y = Mathf.Clamp(desiredPosition.y, minBounds.y, maxBounds.y);

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}
