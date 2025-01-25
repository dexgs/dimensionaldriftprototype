using UnityEngine;
using UnityEngine.InputSystem;


public class DimensionSwitcher: MonoBehaviour {
    public enum Dimension { Two, Three };

    public interface DimensionSwitch {
        void dimensionSwitch(
                Dimension switchTo,
                float progress, Vector3 position, Vector3 direction);
    }
    public delegate void dimensionSwitch(Dimension switchTo, float progress, Vector3 position, Vector3 direction);
    private event dimensionSwitch onDimensionSwitch;

    public InputActionReference increaseDimensionAction;
    public InputActionReference decreaseDimensionAction;
    public GameObject player;
    public GameObject playerCamera;

    private Transform playerTransform;
    private Transform cameraTransform;
    private float progress;
    private float switchSpeed;
    private Vector3 position;
    private Vector3 direction;
    private Dimension dimension;
    private Dimension nextDimension;

    void Start() {
        increaseDimensionAction.action.performed += OnIncreaseDimension;
        decreaseDimensionAction.action.performed += OnDecreaseDimension;

        // set up events
        onDimensionSwitch += playerCamera.GetComponent<DimensionSwitch>().dimensionSwitch;
        onDimensionSwitch += player.GetComponent<DimensionSwitch>().dimensionSwitch;

        playerTransform = player.transform;
        cameraTransform = playerCamera.transform;

        progress = 1.0f;
        switchSpeed = 3.0f;
        dimension = Dimension.Three;
        nextDimension = dimension;
        position = Vector3.zero;
        direction = Vector3.zero;
    }

    void Update() {
        if (dimension != nextDimension) {
            progress += Time.deltaTime * switchSpeed;
            position = playerTransform.position;
            direction = cameraTransform.forward;
            direction.y = 0.0f;
            direction.Normalize();
            
            if (progress >= 1.0f) {
                progress = 1.0f;
                dimension = nextDimension;
            }

            onDimensionSwitch(nextDimension, progress, position, direction);
        }
    }

    private void switchDimension(Dimension switchTo) {
        dimension = nextDimension;
        nextDimension = switchTo;
        progress = 0.0f;
    }

    public void OnIncreaseDimension(InputAction.CallbackContext c) {
        if (dimension == nextDimension) {
            switch (dimension) {
                case Dimension.Two:
                    switchDimension(Dimension.Three);
                    break;
                default:
                    break;
            }
        }
    }

    public void OnDecreaseDimension(InputAction.CallbackContext c) {
        if (dimension == nextDimension) {
            switch (dimension) {
                case Dimension.Three:
                    switchDimension(Dimension.Two);
                    break;
                default:
                    break;
            }
        }
    }
}
