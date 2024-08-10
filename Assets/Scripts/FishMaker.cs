using UnityEngine;
using System.Collections;
using Effect;

/**
 * 自动生成鱼的脚本
 * 挂在ScriptsHolder这个GameObj上
 * 随机在某个指定位置随机生成某种鱼
 **/
public class FishMaker : MonoBehaviour
{
    public Transform fishHolder; // 生成鱼的容器
    public Transform[] genPositions; // 所有鱼的生成位置，在每个位置处事先设置了空的gameObj表示了生成位置
    public GameObject[] fishPrefabs; // 生成鱼的prefab

    public float fishGenWaitTime = 0.5f;
    public float waveGenWaitTime = 0.3f;

    protected void Start()
    {
        // 每隔固定时间生成fishes
        InvokeRepeating(nameof(MakeFishes), 0, waveGenWaitTime);
    }

    private void MakeFishes()
    {
        var genPosIndex = Random.Range(0, genPositions.Length); // 生成位置
        var fishPreIndex = Random.Range(0, fishPrefabs.Length); // 生成鱼的种类

        var maxNum = fishPrefabs[fishPreIndex].GetComponent<FishAttr>().maxNum; //生成的数量
        var maxSpeed = fishPrefabs[fishPreIndex].GetComponent<FishAttr>().maxSpeed;

        var num = Random.Range((maxNum / 2) + 1, maxNum);
        var speed = Random.Range(maxSpeed / 2, maxSpeed);
        var moveType = Random.Range(0, 2); // 0：直走；1：转弯

        if (moveType == 0) // 生成直走鱼
        {
            var angOffset = Random.Range(-22, 22); // 仅直走生效，直走的倾斜角
            StartCoroutine(GenStraightFish(genPosIndex, fishPreIndex, num, speed, angOffset));
        }
        else
        {
            // 生成转弯鱼
            var angSpeed = Random.Range(0, 2) == 0 ? Random.Range(-15, -9) : Random.Range(9, 15); // 仅转弯生效，转弯的角速度

            StartCoroutine(GenTurnFish(genPosIndex, fishPreIndex, num, speed, angSpeed));
        }
    }

    // 直线生成鱼
    private IEnumerator GenStraightFish(int genPosIndex, int fishPreIndex, int num, int speed, int angOffset)
    {
        for (var i = 0; i < num; i++)
        {
            var fish = Instantiate(fishPrefabs[fishPreIndex]);
            fish.transform.SetParent(fishHolder, false); // false,不会再次转化坐标
            fish.transform.localPosition = genPositions[genPosIndex].localPosition; // 在生成位置出生成该鱼
            fish.transform.localRotation = genPositions[genPosIndex].localRotation; // 鱼的方向调整为与生成点的方向一致
            fish.transform.Rotate(0, 0, angOffset);
            fish.GetComponent<SpriteRenderer>().sortingOrder += i; // 为了防止生成的鱼出现闪的情况，不同的鱼之间的sortingOrder不同
            fish.AddComponent<Ef_AutoMove>().speed = speed; // 给生成鱼添加移动的脚本
            yield return new WaitForSeconds(fishGenWaitTime); // 协程等待fishGenWaitTime,程序接着运行，为了让生成的鱼不要重叠
        }
    }

    // 转弯生成鱼
    private IEnumerator GenTurnFish(int genPosIndex, int fishPreIndex, int num, int speed, int angSpeed)
    {
        for (var i = 0; i < num; i++)
        {
            var fish = Instantiate(fishPrefabs[fishPreIndex]);
            fish.transform.SetParent(fishHolder, false);
            fish.transform.localPosition = genPositions[genPosIndex].localPosition;
            fish.transform.localRotation = genPositions[genPosIndex].localRotation;
            fish.GetComponent<SpriteRenderer>().sortingOrder += i; // 解决闪的问题
            fish.AddComponent<Ef_AutoMove>().speed = speed; // 挂上自动move的脚本
            fish.AddComponent<Ef_AutoRotate>().speed = angSpeed; // 挂上自动rotate的脚本
            yield return new WaitForSeconds(fishGenWaitTime);
        }
    }
}