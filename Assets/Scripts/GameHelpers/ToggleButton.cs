using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace FluurMat.GameHelpers
{
    public class ToggleButton : MonoBehaviour
    {
        [SerializeField] private Button tButton;
        [SerializeField] private GameObject tEnable;
        private bool isOn = true;

        private Image[] relatedImages;
        private Color[] imageColors;

        [FormerlySerializedAs("tUnicodeText")] [SerializeField] private UITextUnicode tUnicodeUIText;
        [SerializeField] private string enabledUnicode = "xf028";
        [SerializeField] private string disabledUnicode = "xf6a9"; //xf026

        [SerializeField] private float inactiveAlpha = 0.2f;
        [SerializeField] private ToggleButton linkedToggleButton;

        [SerializeField] Sprite onImage;
        [SerializeField] Sprite offImage;
        [SerializeField] private Image tImage;

        void Awake()
        {
            if (tButton && (relatedImages == null || relatedImages.Length == 0))
            {
                List<Image> imgList = new List<Image>();
                Image[] images = tButton.gameObject.GetComponentsInChildren<Image>();
                Image bImage = tButton.gameObject.GetComponent<Image>();

                for (int i = 0; i < images.Length; i++)
                {
                    if (bImage)
                    {
                        if (images[i] != bImage)
                        {
                            imgList.Add(images[i]);
                        }
                    }
                    else
                    {
                        imgList.Add(images[i]);
                    }
                }

                relatedImages = imgList.ToArray();
                imageColors = new Color[relatedImages.Length];

                for (int i = 0; i < relatedImages.Length; i++)
                {
                    imageColors[i] = relatedImages[i].color;
                }
            }

            if (onImage && offImage && !tImage)
            {
                if (tButton && tButton.GetComponent<Image>())
                {
                    tImage = tButton.GetComponent<Image>();
                }
                else
                {
                    tImage = this.gameObject.GetComponent<Image>();
                }
            }
        }

        public void DoUpdate(bool updateTo)
        {
            if (tEnable)
            {
                tEnable.SetActive(!updateTo);
            }
            else if (tUnicodeUIText)
            {
                tUnicodeUIText.text = updateTo ? enabledUnicode : disabledUnicode;
            }
            else if (tImage && onImage && offImage)
            {
                tImage.sprite = updateTo ? onImage : offImage;
            }
            else if (tButton)
            {
                tButton.interactable = updateTo;
            }

            if (linkedToggleButton)
            {
                linkedToggleButton.DoUpdate(updateTo);
            }
        }

        public void DoFullUpdate(bool updateTo)
        {
            DoUpdate(updateTo);

            if (linkedToggleButton)
            {
                linkedToggleButton.DoFullUpdate(updateTo);
            }

            if (tButton)
            {
                tButton.interactable = updateTo;
            }

            if (!tButton || relatedImages == null || relatedImages.Length < 1)
                return;

            for (int i = 0; i < relatedImages.Length; i++)
            {
                if (!updateTo)
                {
                    relatedImages[i].color =
                        new Color(imageColors[i].r, imageColors[i].g, imageColors[i].b, inactiveAlpha);
                }
                else
                {
                    relatedImages[i].color = imageColors[i];
                }
            }
        }
    }
}