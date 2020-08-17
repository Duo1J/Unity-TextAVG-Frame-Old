﻿using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Duo1J
{
    public class AVGController : MonoBehaviour, Duo1JAVG
    {
        //当前播放的索引
        private int index = 0;
        //模型列表长度
        private int maxIndex = 0;
        //是否正在运行
        private bool run = false;

        //图片文件夹前缀
        private string imagePrefix = "";
        //音频文件夹前缀
        private string audioPrefix = "";

        //画布半宽度
        private float canvasHalfWidth;
        //画布半高度
        private float canvasHalfHeight;

        //是否播放完毕
        private bool textEnd = true;
        //下一句标志
        private bool next = false;

        //模型列表
        private List<AVGModel> modelList;
        public List<AVGModel> ModelList { set => modelList = value; }

        //组件
        private Canvas canvas;
        private Text nameText;
        private Text text;
        private Image characterImage;
        private Image backgroundImage;
        private AudioSource characterVoice;
        private AudioSource backgroundMusic;

        //选择按钮
        private GameObject buttonPanel;
        private Button[] buttons;
        private bool chooseButtonEnable = false;
        private bool choosing = false;

        //接口均从AVGFrame注入
        //前后环绕方法接口
        private Duo1JAround around;
        //行为接口
        private Duo1JAction action;
        //动画方法接口
        private Duo1JAnimation anim;
        //用户输入自定义
        private Duo1JInput input;
        //资源加载
        private Duo1JLoader loader;

        public Duo1JAround Around { set => around = value; }
        public Duo1JAction Action { set => action = value; }
        public Duo1JInput Input0 { set => input = value; }
        public Duo1JLoader Loader { set => loader = value; }
        public Duo1JAnimation Anim
        {
            set
            {
                anim = value;
                if (anim != null && anim.GetType() == typeof(AVGAnimationDefault))
                {
                    ((AVGAnimationDefault)anim).Init(characterImage, backgroundImage, Camera.main);
                }
            }
        }

        //文本显示协程
        private Coroutine textCoro;
        //角色语音过渡协程
        private Coroutine charAudioCoro;
        //背景音乐过渡协程
        private Coroutine bkAudioCoro;
        //角色立绘过渡协程
        private Coroutine imageCoro;

        private void LateUpdate()
        {
            if (run && !choosing)
            {
                if ((Input_Next() && textEnd) || next)
                {
                    DoNext();
                }
            }
        }

        private void Next()
        {
            next = true;
        }

        //下一句
        private void DoNext()
        {
            next = false;
            index++;
            if (index >= maxIndex)
            {
                run = !run;
                if (around != null)
                {
                    around.After();
                }
            }
            else
            {
                UpdateShow();
            }
        }

        //接收是否下一句，可实现Duo1JInput接口自定义input.Input_Next()
        private bool Input_Next()
        {
            if (input != null)
            {
                return input.Input_Next();
            }
            return Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space);
        }


        //显示模型的识别和更新
        private void UpdateShow()
        {
            AVGModel model = modelList[index];
            if (model.GetType() == typeof(TextModel))
            {
                TextModel tmp = (TextModel)model;
                if (tmp.Text != null && tmp.Text != "")
                {
                    TextControl(text, nameText, tmp.Name, tmp.Text, tmp.DelayList);
                }
                if (tmp.Voice != null && tmp.Voice != "")
                {
                    AudioControl(characterVoice, tmp.Voice, AudioType.CHARACTER_VOICE);
                }
            }
            else if (model.GetType() == typeof(CommandModel))
            {
                CommandModel tmp = (CommandModel)model;
                if (tmp.Background_image != null && tmp.Background_image != "")
                {
                    ImageControl(backgroundImage, tmp.Background_image, ImagePos.SKIP, 0, 0, true);
                }
                if (tmp.Background_music != null && tmp.Background_music != "")
                {
                    AudioControl(backgroundMusic, tmp.Background_music, AudioType.BACKGROUND_MUSIC);
                }
                if (tmp.ImageModel0 != null)
                {
                    int pos = tmp.ImageModel0.Pos;
                    if (pos > 2 || pos < 0)
                    {
                        pos = 1;
                    }
                    ImageControl(characterImage, tmp.ImageModel0.Image, (ImagePos)pos, tmp.ImageModel0.X_offset,
                        tmp.ImageModel0.Y_offset, true, true, true);
                }
                ClearTextPanel();
            }
            else if (model.GetType() == typeof(ChooseModel))
            {
                if (!chooseButtonEnable)
                {
                    Debug.LogError("Choose Button Panel Error! at UpdateShow() at AVGController.cs");
                }
                else
                {
                    ChooseModel tmp = (ChooseModel)model;
                    ChooseButtonControl(tmp);
                }
            }
            else if (model.GetType() == typeof(AnimationModel))
            {
                AnimationControl(((AnimationModel)model).Type);
            }
            else
            {
                Debug.LogError("AVGController~UpdateShow-Error: Unknown Type of AVGModel!");
            }
        }

        //动画控制
        private void AnimationControl(string type)
        {
            anim.StartAnimation(type);
            Next();
        }

        //选择按钮控制
        private void ChooseButtonControl(ChooseModel chooseModel)
        {
            List<Choose> chooses = chooseModel.Chooses;

            for (int index = 0; index < buttons.Length; index++)
            {
                if (index < chooses.Count)
                {
                    buttons[index].gameObject.SetActive(true);
                    buttons[index].GetComponentInChildren<Text>().text = chooses[index].Text;
                    int i = index;
                    buttons[index].onClick.AddListener(delegate
                    {
                        ChooseButtonCall(chooseModel.EventTag, chooses[i]);
                    });
                }
                else
                {
                    buttons[index].gameObject.SetActive(false);
                }
            }
            buttonPanel.SetActive(true);
            choosing = true;
        }

        //按钮的选择接收
        private void ChooseButtonCall(string eventTag, Choose choose)
        {
            if (action != null)
            {
                action.GetButtonChoose(eventTag, choose);
            }
            else
            {
                Debug.Log("选择按钮的选择未接收: " + choose.Text);
            }
            choosing = false;
            buttonPanel.SetActive(false);
            Next();
        }

        private void TextControl(Text target, Text nameTarget, string name, string text, List<TextModel.TextDelay> delayList)
        {
            if (textCoro != null)
            {
                StopCoroutine(textCoro);
            }
            if (delayList == null)
            {
                textCoro = StartCoroutine(TextChange(target, text));
            }
            else if (delayList.Count == 0)
            {
                textCoro = StartCoroutine(TextChange(target, text));
            }
            else
            {
                textCoro = StartCoroutine(TextChange(target, text, delayList));
            }
            if (name != null && name != "")
            {
                nameTarget.text = name;
            }
            textEnd = false;
        }

        public void ClearTextPanel()
        {
            text.text = "";
            nameText.text = "";
        }

        //文本显示，不带延迟
        private IEnumerator TextChange(Text target, string text)
        {
            float length = 0;
            int maxLength = text.Length;
            while (length < maxLength)
            {
                length = length + AVGSetting.TEXT_CHANGE_SPEED;
                if (length > maxLength)
                {
                    length = maxLength;
                }
                target.text = text.Substring(0, (int)length);
                yield return null;
            }
            textEnd = true;
        }

        //带语句延迟的文本切换
        private IEnumerator TextChange(Text target, string text, List<TextModel.TextDelay> delayList)
        {
            float length = 0;
            int maxLength = text.Length;
            int currentDelayIndex = 0;
            //下一个延时对应的文本(text)索引
            int delayIndex = delayList[currentDelayIndex].DelayIndex;
            while (length < maxLength)
            {
                //若当前播放长度小于延时所在索引
                //表示可能会执行延时
                if (length < delayIndex)
                {
                    length = length + AVGSetting.TEXT_CHANGE_SPEED;
                    if (length > maxLength)
                    {
                        length = maxLength;
                    }
                    //如果加上AVGSetting.TEXT_CHANGE_SPEED后大于看延时索引
                    //表示延时索引应该在中间
                    if (length >= delayIndex)
                    {
                        target.text = text.Substring(0, delayIndex);
                        length = delayIndex;
                        yield return new WaitForSeconds(delayList[currentDelayIndex].DelayTime);
                        //将索引+1以继续执行后面的判断
                        if (currentDelayIndex < delayList.Count - 1)
                        {
                            currentDelayIndex++;
                            delayIndex = delayList[currentDelayIndex].DelayIndex;
                        }
                        length++;
                    }
                    //length小于延时索引，还没播放到延时位置
                    else
                    {
                        target.text = text.Substring(0, (int)length);
                    }
                }
                //播放长度已经大于延时的索引，则正常播放完毕即可
                else
                {
                    length = length + AVGSetting.TEXT_CHANGE_SPEED;
                    if (length > maxLength)
                    {
                        length = maxLength;
                    }
                    target.text = text.Substring(0, (int)length);
                }
                yield return null;
            }
            textEnd = true;
        }

        //获取已经播放了的文本
        public List<TextModel> GetPreviousText(int length)
        {
            List<TextModel> res = new List<TextModel>();
            int count = 0;
            for (int i = index; i > 0 && count < length; i--)
            {
                if (modelList[i].GetType() == typeof(TextModel))
                {
                    res.Add((TextModel)modelList[i]);
                    ++count;
                }
            }
            return res;
        }

        private void AudioControl(AudioSource target, string name, AudioType type)
        {
            switch (type)
            {
                case AudioType.BACKGROUND_MUSIC:
                    if (bkAudioCoro != null)
                    {
                        StopCoroutine(bkAudioCoro);
                    }
                    bkAudioCoro = StartCoroutine(AudioChange(target, name));
                    break;
                case AudioType.CHARACTER_VOICE:
                    if (charAudioCoro != null)
                    {
                        StopCoroutine(charAudioCoro);
                    }
                    charAudioCoro = StartCoroutine(AudioChange(target, name));
                    break;
                default:
                    Debug.LogError("Unknow AudioType at AudioControl() in AVGController.cs");
                    break;
            }
        }

        private IEnumerator AudioChange(AudioSource target, string name)
        {
            while (target.volume > 0.2f)
            {
                target.volume -= AVGSetting.BK_AUDIO_VOLUME_FADESPEED;
                yield return null;
            }
            ResourceRequest resReq = loader.LoadAudioAsync(audioPrefix + "/" + name.Split('.')[0]);
            //target.clip = Resources.LoadAsync<AudioClip>(audioPrefix + "/" + name.Split('.')[0]);
            while (!resReq.isDone)
            {
                yield return null;
            }
            target.clip = resReq.asset as AudioClip;
            target.Play();
            while (target.volume < AVGSetting.BK_AUDIO_VOLUME)
            {
                target.volume += AVGSetting.BK_AUDIO_VOLUME_FADESPEED;
                yield return null;
            }
        }

        private void ImageControl(Image target, string name, ImagePos pos = ImagePos.SKIP, int xOffset = 0,
            int yOffset = 0, bool fade = false, bool nativeSize = false, bool nextStep = false)
        {
            if (nextStep)
            {
                textEnd = false;
            }
            //if (imageCoro != null)
            //{
            //    StopCoroutine(imageCoro);
            //}
            imageCoro = StartCoroutine(ImageChange(target, name, pos, xOffset, yOffset, fade, nativeSize, nextStep));
        }

        private IEnumerator ImageChange(Image target, string name, ImagePos pos = ImagePos.MID, int xOffset = 0,
            int yOffset = 0, bool fade = false, bool nativeSize = false, bool nextStep = false)
        {
            Color tmp = target.color;
            while (target.color.a > 0 && fade)
            {
                tmp.a -= AVGSetting.CH_IMAGE_FADESPEED;
                target.color = tmp;
                yield return null;
            }

            target.sprite = loader.LoadImage(imagePrefix + "/" + name.Split('.')[0]);
            //Rect Pos fit
            if (pos != ImagePos.SKIP)
            {
                RectTransform rect = target.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(
                    (canvasHalfWidth - rect.rect.width * 0.5f * rect.localScale.x) * ((int)pos - 1)
                    + xOffset + AVGSetting.CH_IMAGE_XOFFSET,
                    rect.rect.height * 0.5f * rect.localScale.y - canvasHalfHeight
                    + yOffset + AVGSetting.CH_IMAGE_YOFFSET
                );
            }

            // Image x:y fit
            if (nativeSize)
            {
                target.SetNativeSize();
            }
            while (target.color.a < 1 && fade)
            {
                tmp.a += AVGSetting.CH_IMAGE_FADESPEED;
                target.color = tmp;
                yield return null;
            }
            //Move on to the next step
            if (nextStep)
            {
                Next();
                textEnd = true;
            }
        }

        //设置文件夹前缀
        public void SetPrefix(string imagePrefix, string audioPrefix)
        {
            this.imagePrefix = imagePrefix;
            this.audioPrefix = audioPrefix;
        }

        //组件初始化
        public void InitUIComponent(Canvas canvas, Text nameText, Text text, Image ch_Image, Image bk_Image,
            AudioSource ch_Voice, AudioSource bk_Music)
        {
            this.canvas = canvas;
            this.nameText = nameText;
            this.text = text;
            characterImage = ch_Image;
            backgroundImage = bk_Image;
            characterVoice = ch_Voice;
            backgroundMusic = bk_Music;

            //
            canvasHalfWidth = bk_Image.GetComponent<RectTransform>().rect.width * 0.5f;
            canvasHalfHeight = bk_Image.GetComponent<RectTransform>().rect.height * 0.5f;

            canvas.gameObject.SetActive(false);
        }

        //初始化并开始
        public void Begin()
        {
            canvas.gameObject.SetActive(true);
            run = true;

            index = 0;
            maxIndex = modelList.Count;

            if (around != null)
            {
                around.Before();
            }

            UpdateShow();
        }

        public void Continue()
        {
            canvas.gameObject.SetActive(true);
            run = true;
        }

        public void Stop()
        {
            canvas.gameObject.SetActive(false);
            run = false;
        }

        public void InitUIComponent(Canvas canvas, Text nameText, Text text, Image ch_Image, Image bk_Image,
            AudioSource ch_Voice, AudioSource bk_Music, GameObject buttonPanel, Button[] buttons)
        {
            InitUIComponent(canvas, nameText, text, ch_Image, bk_Image, ch_Voice, bk_Music);
            SetChooseButton(buttonPanel, buttons);
        }

        public void SetChooseButton(GameObject buttonPanel, Button[] buttons)
        {
            this.buttonPanel = buttonPanel;
            this.buttons = buttons;
            if (buttons != null && buttonPanel != null)
            {
                chooseButtonEnable = true;
                buttonPanel.SetActive(false);
            }
            else
            {
                chooseButtonEnable = false;
                Debug.LogError("Choose Button Init Failed at SetChooseButton() at AVGController.cs");
            }
        }

        public enum AudioType
        {
            CHARACTER_VOICE,
            BACKGROUND_MUSIC
        }

        public enum ImagePos
        {
            LEFT,
            MID,
            RIGHT,
            SKIP
        }
    }
}