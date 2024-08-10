using UnityEngine;
using UnityEngine.Serialization;

/**
 * 枪旋转脚本
 * 枪管的方向随着鼠标的位置旋转
 * 挂在每一个枪上
 **/
public class GunFollow : MonoBehaviour
{
    [FormerlySerializedAs("UGUICanvas")] public RectTransform uguiCanvas;
    public Camera mainCamera;

    private void Update()
    {
        // 将屏幕中的鼠标点坐标转为世界坐标系（该方法解决了UGUI缩放的问题）
        RectTransformUtility.ScreenPointToWorldPointInRectangle(uguiCanvas,
            new Vector2(Input.mousePosition.x, Input.mousePosition.y), mainCamera, out var mousePos);

        float z; // 枪管应该循转的角度，from和to的连线和它们一个指定轴向的夹角
        if (mousePos.x > transform.position.x) // 右边 mousePos - transform.position是负数
        {
            // from和to的连线和它们一个指定轴向的夹角
            z = -Vector3.Angle(Vector3.up, mousePos - transform.position);
        }
        else
        {
            // 左边
            z = Vector3.Angle(Vector3.up, mousePos - transform.position);
        }

        transform.localRotation = Quaternion.Euler(0, 0, z);
    }
}