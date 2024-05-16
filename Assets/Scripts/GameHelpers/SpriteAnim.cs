using UnityEngine;

namespace FluurMat.GameHelpers
{
    public class SpriteAnim : MonoBehaviour
    {
        [SerializeField] private Material animMat;
        [SerializeField] private float numFrames = 4f;
        [SerializeField] private string vecName = "_Offset";
        [SerializeField] private Vector2 animXY = Vector2.zero;
        private float frame = 0f;
        [SerializeField] private float tickTime = 0.15f;
        private float time = 0f;

        void Update()
        {
            if (Time.timeScale > 0f)
            {
                time += Time.deltaTime;
                if (time >= tickTime)
                {
                    time = 0f;
                    frame = Mathf.Repeat(frame + 1f, numFrames);
                    animMat.SetVector(vecName, new Vector4(animXY.x * frame, animXY.y * frame, numFrames, numFrames));
                }
            }
        }
    }
}