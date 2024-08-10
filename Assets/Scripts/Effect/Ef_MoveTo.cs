using UnityEngine;

namespace Effect
{
    /**
    * 此处主要用来处理鱼死亡时获得金币有个效果：在死亡处生成的金币飞向左下角的goldCollect处
    * 挂在大小金币预制体上
    **/
    public class Ef_MoveTo : MonoBehaviour
    {
        private GameObject _goldCollect;

        private void Start()
        {
            _goldCollect = GameObject.Find("GoldCollect");
        }

        private void Update()
        {
            // MoveTowards插值移动的方式
            transform.position =
                Vector3.MoveTowards(transform.position, _goldCollect.transform.position, 5 * Time.deltaTime);
        }
    }
}