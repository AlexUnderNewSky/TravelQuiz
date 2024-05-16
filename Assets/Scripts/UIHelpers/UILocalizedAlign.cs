using UnityEngine;
using UnityEngine.UI;

namespace FluurMat.UI
{
    public class UILocalizedAlign : MonoBehaviour
    {
        [SerializeField] private Text textTarget;
        [SerializeField] private bool moveToRight = true;
        [SerializeField] private int refCharsNum = 0;
        [SerializeField] private int maxCharsNum = 0;
        [SerializeField] private float subMult = 0.5f;

        private int minCharsNum = 1;
        private int curCharsNum = 0;
        private int fontSize = 0;
        private Vector3 iniPos = Vector3.zero;

        void Start()
        {
            if (!textTarget)
            {
                textTarget = this.GetComponent<Text>();
            }

            fontSize = textTarget.fontSize;

            curCharsNum = textTarget.text.ToCharArray().Length;
            curCharsNum = Mathf.Clamp(curCharsNum, minCharsNum, maxCharsNum);

            iniPos = transform.position;
            UpdatePosition(transform.localScale.x);
        }

        public void UpdatePosition(float scale = 1f)
        {
            float moveWith = fontSize * scale * 0.5f;
            moveWith = Mathf.Round(((float)(curCharsNum - Mathf.Clamp(refCharsNum, minCharsNum, maxCharsNum)) *
                                    Mathf.Max(0f, subMult)) * moveWith);
            if (!moveToRight)
            {
                moveWith *= -1f;
            }

            transform.position = new Vector3(iniPos.x + moveWith, iniPos.y, iniPos.z);
        }
    }
}