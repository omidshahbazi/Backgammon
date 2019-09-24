using UnityEngine;

namespace GamePlay
{
    public class Beed : MonoBehaviour
    {
        public enum Color
        {
            White,
            Black
        }

        [SerializeField]
        public Color BeedColor;
    }

}