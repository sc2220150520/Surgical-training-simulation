using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Devdog.InventorySystem;
using UnityEngine;


namespace Devdog.InventorySystem
{
    [RequireComponent(typeof(UIWindow))]
    [AddComponentMenu("InventorySystem/UI Helpers/Object trigger UI")]
    public partial class ObjectTriggererUI : MonoBehaviour
    {
        public UnityEngine.UI.Image imageIcon;
        public UnityEngine.UI.Text shortcutText;

        public UIWindow window { get; protected set; }

        public void Awake()
        {
            window = GetComponent<UIWindow>();

            //window.OnHide += () =>
            //{
                
            //};
        }

        public void Repaint(Sprite sprite, string text)
        {
            if(window.isVisible == false)
            {
                window.Show();
            }

            if (imageIcon != null && imageIcon.sprite == sprite)
                imageIcon.sprite = sprite;

            if (shortcutText != null && shortcutText.text == text)
                shortcutText.text = text;
        }
    }    
}
