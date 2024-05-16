using UnityEngine;
using UnityEngine.UI;

namespace KMTW
{
    public class UITween : MonoBehaviour
    {
        private bool initialValue = false;

        private CanvasGroup canvasGroup;
        private RectTransform targetTransform;

        [SerializeField] private RectTransform target;

        private float lerpZ = 0f;
        [SerializeField] private bool atIni = true;
        private bool doRest = true;

        private Vector2 initialSize = Vector2.zero;
        private Vector3 initialPosition = Vector3.zero;
        private Vector3 initialScale = Vector3.zero;
        private Quaternion initialRotation = Quaternion.identity;
        private Color initialColor = Color.white;
        private Object colorTarget;

        [SerializeField] private AnimationCurve theCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] private float speed = 1f;

        [SerializeField] private bool tweenOpacity = true;
        [SerializeField] private bool invOpacity = false;

        [SerializeField] private bool tweenSize = false;
        [SerializeField] private Vector2 targetSize = Vector2.one;

        [SerializeField] private bool tweenScale = false;
        [SerializeField] private Vector3 targetScale = Vector3.one;

        [SerializeField] private bool tweenPosition = false;
        [SerializeField] private Vector3 targetPostion = Vector3.zero;

        [SerializeField] private bool tweenRotation = false;
        [SerializeField] private Vector3 targetRotation = Vector3.zero;

        [SerializeField] private bool tweenColor = false;
        [SerializeField] private Color targetColor = Color.white;

        public delegate void StartReached();

        public event StartReached OnStartReached;

        public delegate void EndReached();

        public event EndReached OnEndReached;

        [SerializeField] private bool useUnscaledTime = false;

        void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            targetTransform = GetComponent<RectTransform>();

            initialSize = targetTransform.sizeDelta;
            initialPosition = targetTransform.localPosition;
            initialScale = targetTransform.localScale;
            initialRotation = targetTransform.localRotation;

            if (!(colorTarget = gameObject.GetComponent<Image>()))
            {
                colorTarget = gameObject.GetComponent<Text>();
            }

            if (colorTarget != null)
            {
                if (colorTarget is Image)
                {
                    initialColor = ((Image)colorTarget).color;
                }
                else if (colorTarget is Text)
                {
                    initialColor = ((Text)colorTarget).color;
                }
            }

            initialValue = atIni;
        }

        void Start()
        {
            if (atIni)
            {
                lerpZ = 0f;
            }
            else
            {
                lerpZ = 1f;
            }

            doRest = true;

            if (target)
            {
                targetSize = target.sizeDelta;
                targetPostion = target.localPosition;
                targetScale = target.localScale;
                targetRotation = target.localEulerAngles;
            }

            Evaluate();
        }

        public void ResetTweens()
        {
            atIni = initialValue;
            if (atIni)
            {
                lerpZ = 0f;
            }
            else
            {
                lerpZ = 1f;
            }

            Evaluate();
        }

        void Update()
        {
            float deltaTime = 0f;

            if (useUnscaledTime)
            {
                deltaTime = Time.unscaledDeltaTime;
            }
            else
            {
                deltaTime = Time.deltaTime;
            }


            if (atIni)
            {
                lerpZ -= deltaTime * speed;
            }
            else
            {
                lerpZ += deltaTime * speed;
            }

            lerpZ = Mathf.Clamp01(lerpZ);

            if (((atIni && Mathf.Approximately(lerpZ, 0f)) || (!atIni && Mathf.Approximately(lerpZ, 1f))) && doRest)
            {
                return;
            }

            Evaluate();

            if (atIni && Mathf.Approximately(lerpZ, 0f))
            {
                if (OnStartReached != null)
                {
                    OnStartReached();
                }

                doRest = true;
            }
            else if (!atIni && Mathf.Approximately(lerpZ, 1f))
            {
                if (OnEndReached != null)
                {
                    OnEndReached();
                }

                doRest = true;
            }
            else
            {
                doRest = false;
            }
        }

        private void Evaluate()
        {
            if (targetTransform)
            {
                if (tweenSize)
                {
                    targetTransform.sizeDelta = InterpolateOverCurve(initialSize, targetSize, lerpZ);
                }

                if (tweenScale)
                {
                    targetTransform.localScale = InterpolateOverCurve(initialScale, targetScale, lerpZ);
                }

                if (tweenPosition)
                {
                    targetTransform.localPosition = InterpolateOverCurve(initialPosition, targetPostion, lerpZ);
                }

                if (tweenRotation)
                {
                    targetTransform.localRotation = Quaternion.Lerp(initialRotation, Quaternion.Euler(targetRotation),
                        InterpolateOverCurve(0f, 1f, lerpZ));
                }
            }

            if (tweenColor && colorTarget != null)
            {
                if (colorTarget is Image)
                {
                    ((Image)colorTarget).color = InterpolateOverCurve(initialColor, targetColor, lerpZ);
                }
                else if (colorTarget is Text)
                {
                    ((Text)colorTarget).color = InterpolateOverCurve(initialColor, targetColor, lerpZ);
                }
            }

            if (canvasGroup && tweenOpacity)
            {
                canvasGroup.alpha = InterpolateOverCurve(0f, 1f, invOpacity ? 1f - lerpZ : lerpZ);

                if (!atIni && Mathf.Approximately(lerpZ, 1f) /* > 0.5f */)
                {
                    canvasGroup.blocksRaycasts = true;
                    canvasGroup.interactable = true;
                }
                else
                {
                    canvasGroup.blocksRaycasts = false;
                    canvasGroup.interactable = false;
                }
            }
        }

        public void PopUp()
        {
            atIni = false;
        }

        public void PopBack()
        {
            atIni = true;
        }

        private float InterpolateOverCurve(float from, float to, float t)
        {
            return from + theCurve.Evaluate(t) * (to - from);
        }

        private Vector2 InterpolateOverCurve(Vector2 from, Vector2 to, float t)
        {
            float x = from.x + theCurve.Evaluate(t) * (to.x - from.x);
            float y = from.y + theCurve.Evaluate(t) * (to.y - from.y);
            return new Vector2(x, y);
        }

        private Vector3 InterpolateOverCurve(Vector3 from, Vector3 to, float t)
        {
            float x = from.x + theCurve.Evaluate(t) * (to.x - from.x);
            float y = from.y + theCurve.Evaluate(t) * (to.y - from.y);
            float z = from.z + theCurve.Evaluate(t) * (to.z - from.z);
            return new Vector3(x, y, z);
        }

        private Color InterpolateOverCurve(Color from, Color to, float t)
        {
            float r = from.r + theCurve.Evaluate(t) * (to.r - from.r);
            float g = from.g + theCurve.Evaluate(t) * (to.g - from.g);
            float b = from.b + theCurve.Evaluate(t) * (to.b - from.b);
            float a = from.a + theCurve.Evaluate(t) * (to.a - from.a);
            return new Color(r, g, b, a);
        }
    }
}