using UnityEngine;


public class DimensionSwitchCollider: MonoBehaviour, DimensionSwitcher.DimensionSwitch {
    private Collider c;

    public void Start() {
        c = GetComponent<Collider>();
    }

    private void interpolate2d3d(float t, Vector3 direction) {
        if (t >= 1.0f) {
            c.enabled = false;
        } else {
            c.enabled = true;
        }

        transform.LookAt(transform.position + direction);
        Vector3 localScale = transform.localScale;
        localScale.z = Mathf.Lerp(1000.0f, 1.0f, t);
        transform.localScale = localScale;
    }

    public void dimensionSwitch(
        DimensionSwitcher.Dimension switchTo,
        float progress, Vector3 position, Vector3 direction)
    {
        switch (switchTo) {
            case DimensionSwitcher.Dimension.Two:
                interpolate2d3d(1.0f - progress, direction);
                break;
            case DimensionSwitcher.Dimension.Three:
                interpolate2d3d(progress, direction);
                break;
        }
    }
}
