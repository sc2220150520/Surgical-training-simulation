﻿using System;
using System.Collections;
using System.Collections.Generic;
using Devdog.InventorySystem.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Devdog.InventorySystem.UI.Models
{
    /// <summary>
    /// A single message inside the message displayer
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public partial class NoticeMessageUI : MonoBehaviour, IPoolableObject
    {
        public UnityEngine.UI.Text title;
        public UnityEngine.UI.Text message;
        public UnityEngine.UI.Text time;

        public AnimationClip showAnimation;
        public AnimationClip hideAnimation;

        [HideInInspector]
        public float showTime = 4.0f;

        public DateTime dateTime { get; private set; }

        [NonSerialized]
        protected Animator animator;
        [NonSerialized]
        protected bool isHiding = false; // In the process of hiding

        public virtual void Awake()
        {
            animator = GetComponent<Animator>();

            if (showAnimation != null)
                animator.Play(showAnimation.name);
        }

        public virtual void SetMessage(InventoryNoticeMessage message)
        {
            this.showTime = (int)message.duration;
            this.dateTime = message.time;

            if (string.IsNullOrEmpty(message.title) == false)
            {
                if (this.title != null)
                {
                    this.title.text = string.Format(message.title, message.parameters);
                    this.title.color = message.color;
                }
            }
            else
                title.gameObject.SetActive(false);


            this.message.text = string.Format(message.message, message.parameters);
            this.message.color = message.color;

            if (this.time != null)
            {
                this.time.text = dateTime.ToShortTimeString();
                this.time.color = message.color;
            }
        }

        public virtual void Hide()
        {
            // Already hiding
            if (isHiding)
                return;

            isHiding = true;

            if (hideAnimation != null)
                animator.Play(hideAnimation.name);
        }

        public void Reset()
        {
            isHiding = false;
        }
    }
}