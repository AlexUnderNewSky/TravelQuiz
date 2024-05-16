using UnityEngine;

// *

namespace FluurMat.GameHelpers
{
    public class ResetTime : MonoBehaviour
    {
        void Start()
        {
            if (Time.timeScale != 1f)
            {
                Time.timeScale = 1f;
            }
        }
    }
}