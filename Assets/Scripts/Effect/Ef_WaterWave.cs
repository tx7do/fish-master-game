using UnityEngine;

namespace Effect
{
    public class Ef_WaterWave : MonoBehaviour
    {
        public Texture[] textures;
        private Material _material;
        private int _index = 0;

        private void Start()
        {
            _material = GetComponent<MeshRenderer>().material;
            InvokeRepeating(nameof(ChangeTexture), 0, 0.1f);
        }

        private void ChangeTexture()
        {
            _material.mainTexture = textures[_index];
            _index = (_index + 1) % textures.Length;
        }
    }
}