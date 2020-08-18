using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Duo1J
{
    [RequireComponent(typeof(AVGController))]
    public class AVGFrame : MonoBehaviour, Duo1JAVG
    {
        private AVGController controller;

        private Duo1JLoader loader;
        private Duo1JAnimation anim;
        private Duo1JAround around;
        private Duo1JAction action;
        private Duo1JInput input;

        private List<AVGModel> modelList;

        public string imageUrlPrefix = "Image";
        public string audioUrlPrefix = "Audio";

        public TextAsset textAsset;

        [Header("UI Component")]
        public Canvas canvas;
        public Text text;
        public Text nameText;
        public Image characterImage;
        public Image backgroundImage;
        public AudioSource characterVoice;
        public AudioSource backgroundMusic;
        [Header("Choose Button")]
        public GameObject buttonPanel;
        public Button[] buttons;

        //实现接口后挂在在此脚本同一物体后自动注入，否则使用默认
        public Duo1JAnimation Anim { set { anim = value; controller.Anim = anim; } }
        public Duo1JAround Around { set { around = value; controller.Around = around; } }
        public Duo1JAction Action { set { action = value; controller.Action = action; } }
        public Duo1JLoader Loader { set { loader = value; controller.Loader = loader; } }
        public Duo1JInput Input0 { set { input = value; controller.Input0 = input; } }

        private bool isReady = false;
        private Coroutine startCoro = null;

        private void Awake()
        {
            ComponentsInit();
        }

        private void Start()
        {
            if (loader.Analysis(out modelList, textAsset))
            {
                controller.ModelList = modelList;
                isReady = true;
                Begin();
            }
            else
            {
                Debug.LogError("TextAsset analysis failed!");
            }
        }

        public void Begin()
        {
            if (startCoro == null)
            {
                StartCoroutine(BeginCoro());
            }
        }

        private IEnumerator BeginCoro()
        {
            while (!isReady)
            {
                yield return null;
            }
            yield return null;
            controller.Begin();
        }

        private void ComponentsInit()
        {
            controller = GetComponent<AVGController>();
            if (controller == null)
            {
                controller = gameObject.AddComponent<AVGController>();
            }
            controller.SetPrefix(imageUrlPrefix, audioUrlPrefix);
            controller.InitUIComponent(canvas, nameText, text, characterImage, backgroundImage,
                characterVoice, backgroundMusic);
            controller.SetChooseButton(buttonPanel, buttons);

            Loader = GetComponent<Duo1JLoader>();
            if (loader == null)
            {
                Loader = gameObject.AddComponent<AVGLoader>();
            }

            Anim = GetComponent<Duo1JAnimation>();
            if (anim == null)
            {
                Anim = gameObject.AddComponent<AVGAnimationDefault>();
            }

            Action = GetComponent<Duo1JAction>();
            if (action == null)
            {
                Action = gameObject.AddComponent<AVGActionDefault>();
            }

            Around = GetComponent<Duo1JAround>();
            if (around == null)
            {
                Around = gameObject.AddComponent<AVGAroundDefault>();
            }

            Input0 = GetComponent<Duo1JInput>();
            if (input == null)
            {
                Input0 = null;
            }
        }
    }
}
