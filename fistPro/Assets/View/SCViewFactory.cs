using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CustomView
{
    public class SCCanvs:System.Object
    {
        public GameObject _canvs;
        public SCCanvs(int layer,string name,Transform parent)
        {
            _canvs = new GameObject();
            _canvs.transform.parent = parent;
            _canvs.AddComponent<Canvas>();
            _canvs.AddComponent<CanvasScaler>();
            //设置UI的渲染模式
            _canvs.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            _canvs.AddComponent<GraphicRaycaster>();
            _canvs.layer = layer;
            _canvs.name = name;
        }
        public void setlayer(int layer)
        {
            _canvs.layer = layer;
        }
        public void setName(string name)
        {
            _canvs.name = name;
        }
        public void setRenderMode(RenderMode mode)
        {
            _canvs.GetComponent<Canvas>().renderMode = mode;
        }
        //~SCCanvs()
        //{
        //    GameObject.Destroy(_canvs);
        //}
    }

    public class SCButton: System.Object
    {
        public GameObject _btn;

        public SCButton(int layer,string name,Transform parent,UnityAction call, Sprite btnImage,Vector2 anMin,Vector2 anMax,Vector2 offMin,Vector2 offMax)
        {
            _btn = new GameObject();
            _btn.layer = 5;
            _btn.transform.SetParent(parent);
            _btn.AddComponent<Button>();
            _btn.AddComponent<Image>();
            _btn.GetComponent<Button>().onClick.AddListener(call);
            _btn.name = name;
            //设置button的锚框
            _btn.GetComponent<RectTransform>().anchorMin = anMin;
            _btn.GetComponent<RectTransform>().anchorMax = anMax;
            _btn.GetComponent<RectTransform>().offsetMin = offMin;
            _btn.GetComponent<RectTransform>().offsetMax = offMax;
            //设置button的图片
            _btn.GetComponent<Button>().targetGraphic = _btn.GetComponent<Image>();
            _btn.GetComponent<Image>().overrideSprite = btnImage;
            _btn.GetComponent<Image>().type = Image.Type.Sliced;
        }
        public void setlayer(int layer)
        {
            _btn.layer = layer;
        }
        public void setName(string name)
        {
            _btn.name = name;
        }
        public void setCallFun(UnityAction call)
        {
            _btn.GetComponent<Button>().onClick.AddListener(call);
        }
        public void setSprite(Sprite btnImage)
        {
            _btn.GetComponent<Image>().overrideSprite = btnImage;
        }
        public void setImageType(Image.Type type)
        {
            _btn.GetComponent<Image>().type = type;
        }
        public void setAnchorMin(Vector2 vc)
        {
            _btn.GetComponent<RectTransform>().anchorMin = vc;
        }
        public void setAnchorMax(Vector2 vc)
        {
            _btn.GetComponent<RectTransform>().anchorMax = vc;
        }
        public void setOffsetMin(Vector2 vc)
        {
            _btn.GetComponent<RectTransform>().offsetMin = vc;
        }
        public void setOffsetMax(Vector2 vc)
        {
            _btn.GetComponent<RectTransform>().offsetMax = vc;
        }
        //~SCButton()
        //{
        //    GameObject.Destroy(_btn);
        //}

    }

    public class SCText:Object
    {
        public GameObject _text;
        public SCText(int layer, string name, Transform parent,string text,Font textFont,FontStyle style,Color textColor,TextAnchor pos,Vector2 anchorMin,Vector2 anchorMax,Vector2 offsetMin,Vector2 offsetMax)
        {
            _text = new GameObject();
            _text.layer = layer;
            _text.name = name;
            _text.transform.SetParent(parent);
            _text.AddComponent<Text>();
            _text.GetComponent<Text>().text = text;
            _text.GetComponent<Text>().font = textFont;
            _text.GetComponent<Text>().fontStyle = style;
            _text.GetComponent<Text>().color = textColor;
            _text.GetComponent<Text>().alignment = pos;
            _text.GetComponent<RectTransform>().anchorMin = anchorMin;
            _text.GetComponent<RectTransform>().anchorMax = anchorMax;
            _text.GetComponent<RectTransform>().offsetMin = offsetMin;
            _text.GetComponent<RectTransform>().offsetMax = offsetMax;
        }
        ~SCText()
        {
            //Debug.Log("yes");
        }
    }
}


