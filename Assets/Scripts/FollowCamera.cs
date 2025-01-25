using UnityEngine;
using UnityEngine.InputSystem;


public class FollowCamera: MonoBehaviour, DimensionSwitcher.DimensionSwitch {
    public InputActionReference lookAction;
    public GameObject target;
    public float distance;
    public float fov;
    public float lookSpeed;

    private float theta;
    private float phi;
    private Vector2 inputDirection;

    private DimensionSwitcher.Dimension switchTo;
    private float dimensionSwitchProgress;

    void Start() {
        theta = 0.0f;
        phi = 0.0f;
        inputDirection = Vector2.zero;
        lookAction.action.performed += OnLook;
        lookAction.action.canceled += OnLook;

        // hard-coded start in 3D for now...
        switchTo = DimensionSwitcher.Dimension.Three;
        dimensionSwitchProgress = 1.0f;
    }

    // t is a value on the interval [0, 1] representing progress from 2D to
    // 3D. 0 is fully 2D, 1 is fully 3D.
    private void interpolate2d3d(float t) {
        float l = distance * Mathf.Tan(Mathf.Deg2Rad * fov / 2.0f);
        Camera camera = GetComponent<Camera>();

        theta -= inputDirection.x * Time.deltaTime * lookSpeed * t;
        theta = theta % (2.0f * Mathf.PI);
        phi -= inputDirection.y * Time.deltaTime * lookSpeed * t;
        phi = Mathf.Min(Mathf.Max(phi, -Mathf.PI / 4.0f), Mathf.PI / 4.0f);

        float fov3D = fov;
        float fov2D = 1.0f;
        float fovLerp = Mathf.Lerp(fov2D, fov3D, t);

        Vector3 position3D =
            target.transform.position + 
            distance * (
             Mathf.Cos(phi) * new Vector3(Mathf.Cos(theta), 0.0f, Mathf.Sin(theta))
             + new Vector3(0.0f, Mathf.Sin(phi), 0.0f));
        Vector3 target3D = target.transform.position;

        Vector3 position2D =
            target.transform.position + 
            (l / Mathf.Tan(Mathf.Deg2Rad * fovLerp / 2.0f)) * (new Vector3(Mathf.Cos(theta), 0.0f, Mathf.Sin(theta)));
            //distance * (new Vector3(Mathf.Cos(theta), 0.0f, Mathf.Sin(theta)));
        Vector3 target2D = target3D;

        transform.position = Vector3.Lerp(position2D, position3D, t);
        transform.LookAt(Vector3.Lerp(target2D, target3D, t));
        camera.fieldOfView = fovLerp;

        if (t > 0.0f) {
            camera.orthographic = false;
        } else {
            camera.orthographicSize = l;
            camera.orthographic = true;
        }
    }

    void LateUpdate() {
        /*
        theta -= inputDirection.x * Time.deltaTime * lookSpeed;
        theta = theta % (2.0f * Mathf.PI);
        phi -= inputDirection.y * Time.deltaTime * lookSpeed;
        phi = Mathf.Min(Mathf.Max(phi, -Mathf.PI / 4.0f), Mathf.PI / 4.0f);

        transform.position =
            target.transform.position + 
            distance * (
             Mathf.Cos(phi) * new Vector3(Mathf.Cos(theta), 0.0f, Mathf.Sin(theta))
             + new Vector3(0.0f, Mathf.Sin(phi), 0.0f));
        transform.LookAt(target.transform);
        */
        switch (switchTo) {
            case DimensionSwitcher.Dimension.Two:
                interpolate2d3d(1.0f - dimensionSwitchProgress);
                break;
            case DimensionSwitcher.Dimension.Three:
                interpolate2d3d(dimensionSwitchProgress);
                break;
        }
    }

    public void OnLook(InputAction.CallbackContext c) {
        inputDirection = c.ReadValue<Vector2>();
    }

    public void dimensionSwitch(
        DimensionSwitcher.Dimension switchTo,
        float progress, Vector3 position, Vector3 direction)
    {
        this.switchTo = switchTo;
        dimensionSwitchProgress = progress;
    }
}
