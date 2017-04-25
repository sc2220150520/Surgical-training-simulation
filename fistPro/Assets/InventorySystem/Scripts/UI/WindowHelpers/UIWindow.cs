using System;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace Devdog.InventorySystem
{
    /// <summary>
    /// Any window that you want to hide or show through key combination or a helper (UIShowWindow for example)
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [AddComponentMenu("InventorySystem/UI Helpers/UIWindow")]
    public partial class UIWindow : MonoBehaviour
    {

        public delegate void WindowShow();
        public delegate void WindowHide();


        #region Variables 

        /// <summary>
        /// Should the window be hidden when the game starts?
        /// </summary>
        [Header("Behavior")]
        public bool hideOnStart = true;

        /// <summary>
        /// Keys to toggle this window
        /// </summary>
        public KeyCode[] keyCombination;

        /// <summary>
        /// The animation played when showing the window, if null the item will be shown without animation.
        /// </summary>
        [Header("Audio & Visuals")]
        public AnimationClip showAnimation;

        /// <summary>
        /// The animation played when hiding the window, if null the item will be hidden without animation. 
        /// </summary>
        public AnimationClip hideAnimation;

        public AudioClip showAudioClip;
        public AudioClip hideAudioClip;


        /// <summary>
        /// The animator in case the user wants to play an animation.
        /// </summary>
        public Animator animator { get; set; }
        protected RectTransform rectTransform { get; set; }

        [NonSerialized]
        private bool _isVisible = false;
        /// <summary>
        /// Is the window visible or not? Used for toggling.
        /// </summary>
        public bool isVisible
        {
            get
            {
                return _isVisible;
            }
            protected set
            {
                _isVisible = value;
            }
        }

        private IEnumerator showCoroutine;
        private IEnumerator hideCoroutine;


        /// <summary>
        /// All the pages of this window
        /// </summary>
        [HideInInspector]
        private List<UIWindowPage> pages = new List<UIWindowPage>();

        public UIWindowPage defaultPage
        {
            get;
            private set;
        }

        #endregion

        #region Events

        /// <summary>
        /// Event is fired when the window is hidden.
        /// </summary>
        public event WindowHide OnHide;

        /// <summary>
        /// Event is fired when the window becomes visible.
        /// </summary>
        public event WindowShow OnShow;

        #endregion


        public void AddPage(UIWindowPage page)
        {
            pages.Add(page);

            if (page.isDefaultPage)
                defaultPage = page;
        }

        public void RemovePage(UIWindowPage page)
        {
            pages.Remove(page);
        }


        public virtual void Awake()
        {
            animator = GetComponent<Animator>();
            if (animator == null)
                animator = gameObject.AddComponent<Animator>();

            rectTransform = GetComponent<RectTransform>();

            if (hideOnStart)
                HideFirst();
            else
            {
                isVisible = true;
            }
        }

        public virtual void Update()
        {
            //sc注释
            //if (keyCombination.Length == 0)
            //    return;

            //bool allDown = true;
            //foreach (var key in keyCombination)
            //{
            //    if (Input.GetKeyDown(key) == false)
            //    {
            //        allDown = false;
            //    }
            //}
            //if (allDown)
            //    Toggle();
            //Debug.Log("isvis " + isVisible);
        }

        #region Usefull UI reflection functions 

        /// <summary>
        /// One of our children pages has been shown
        /// </summary>
        public void NotifyPageShown(UIWindowPage page)
        {
            foreach (var item in pages)
            {
                if (item.isVisible && item != page)
                    item.Hide();
            }
        }

        protected virtual void SetChildrenActive(bool active)
        {
            foreach (Transform t in transform)
            {
                t.gameObject.SetActive(active);
            }

            var img = gameObject.GetComponent<UnityEngine.UI.Image>();
            if(img != null)
                img.enabled = active;
        }

        public virtual void Toggle()
        {
            if (isVisible)
                Hide();
            else
                Show();
        }

        public virtual void Show()
        {
            if (isVisible)
                return;

            isVisible = true;
            animator.enabled = true;

            SetChildrenActive(true);
            if (showAnimation != null)
            {
                animator.Play(showAnimation.name);
                if (showCoroutine != null)
                {
                    StopCoroutine(showCoroutine);
                    
                }

                showCoroutine = _Show(showAnimation); 
                StartCoroutine(showCoroutine);
            }

            // Show pages
            foreach (var page in pages)
            {
                if (page.isDefaultPage)
                    page.Show();
                else if (page.isVisible)
                    page.Hide();
            }

            if (showAudioClip != null)
                InventoryUIUtility.AudioPlayOneShot(showAudioClip);

            if (OnShow != null)
                OnShow();
        }


        public virtual void HideFirst()
        {
            isVisible = false;
            animator.enabled = false;

            SetChildrenActive(false);
            rectTransform.anchoredPosition = Vector2.zero;
        }

        public virtual void Hide()
        {
            //Debug.Log("isfuckC# " + isVisible);
            if (isVisible == false)
            {
                return;
            }


            isVisible = false;
            if (OnHide != null)
                OnHide();

            if (hideAudioClip != null)
                InventoryUIUtility.AudioPlayOneShot(hideAudioClip);

            if (hideAnimation != null)
            {
                animator.enabled = true;
                animator.Play(hideAnimation.name);

                if (hideCoroutine != null)
                {
                    StopCoroutine(hideCoroutine);
                }

                hideCoroutine = _Hide(hideAnimation);
                StartCoroutine(hideCoroutine);
            }
            else
            {
                animator.enabled = false;
                SetChildrenActive(false);
            }
        }


        /// <summary>
        /// Hides object after animation is completed.
        /// </summary>
        /// <param name="animation"></param>
        /// <returns></returns>
        protected virtual IEnumerator _Hide(AnimationClip animation)
        {
            yield return new WaitForSeconds(animation.length + 0.1f);

            // Maybe it got visible in the time we played the animation?
            if (isVisible == false)
            {
                SetChildrenActive(false);
                animator.enabled = false;
            }
        }


        /// <summary>
        /// Hides object after animation is completed.
        /// </summary>
        /// <param name="animation"></param>
        /// <returns></returns>
        protected virtual IEnumerator _Show(AnimationClip animation)
        {
            yield return new WaitForSeconds(animation.length + 0.1f);

            if (isVisible)
                animator.enabled = false;
        }

        #endregion
    }
}