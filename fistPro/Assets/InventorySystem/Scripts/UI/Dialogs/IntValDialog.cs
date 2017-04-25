using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Devdog.InventorySystem.Dialogs
{

    public delegate void IntValDialogCallback(int value);

    public partial class IntValDialog : InventoryUIDialogBase
    {
        [Header("UI int val")]
        public UnityEngine.UI.InputField inputField;
        
        // Leave blank if you don't want to use them
        public UnityEngine.UI.Button plusButton;
        // Leave blank if you don't want to use them
        public UnityEngine.UI.Button minusButton;

        private int minValue { get; set; }
        private int maxValue { get; set; }

        /// <summary>
        /// Show this dialog.
        /// <b>Don't forget to call dialog.Hide(); when you want to hide it, this is not done auto. just in case you want to animate it instead of hide it.</b>
        /// </summary>
        /// <param name="title">Title of the dialog.</param>
        /// <param name="description">The description of the dialog.</param>
        /// <param name="yes">The name of the yes button.</param>
        /// <param name="no">The name of the no button.</param>
        /// <param name="minValue">The minimal value allowed to be selected.</param>
        /// <param name="maxValue">The max value allowed to be selected.</param> 
        /// <param name="yesCallback"></param>
        /// <param name="noCallback"></param>
        public virtual void ShowDialog(string title, string description, string yes, string no, int min, int max, IntValDialogCallback yesCallback, IntValDialogCallback noCallback)
        {
            SetEnabledWhileActive(false);
      
        
            window.Show(); // Have to show it first, otherwise we can't use the elements, as they're disabled.


            minValue = min;
            maxValue = max;

            titleText.text = title;
            if (descriptionText != null)
                descriptionText.text = description;
        
            yesButton.GetComponentInChildren<Text>().text = yes;
            noButton.GetComponentInChildren<Text>().text = no;
            inputField.text = minValue.ToString();


            inputField.onEndEdit.RemoveAllListeners();
            inputField.onValueChange.AddListener((string result) =>
            {
                ValidateInputField(minValue, maxValue);
            });

            if (plusButton != null)
            {
                plusButton.onClick.RemoveAllListeners();
                plusButton.onClick.AddListener(() =>
                {
                    if (window.isVisible == false)
                        return;

                    if (inputField.text == "")
                        inputField.text = maxValue.ToString();

                    if (Input.GetKey(KeyCode.LeftShift))
                        inputField.text = (int.Parse(inputField.text) + 10).ToString();
                    else    
                        inputField.text = (int.Parse(inputField.text) + 1).ToString();
                });
            }
            if(minusButton != null)
            {
                minusButton.onClick.RemoveAllListeners();
                minusButton.onClick.AddListener(() =>
                {
                    if (window.isVisible == false)
                        return;

                    if (inputField.text == "")
                        inputField.text = minValue.ToString();

                    if(Input.GetKey(KeyCode.LeftShift))
                        inputField.text = (int.Parse(inputField.text) - 10).ToString();
                    else
                        inputField.text = (int.Parse(inputField.text) - 1).ToString();
                });
            }


        
            yesButton.onClick.RemoveAllListeners();
            yesButton.onClick.AddListener(() =>
            {
                if (window.isVisible == false)
                    return;

                if (ValidateInputField(minValue, maxValue) == false)
                    return;

                SetEnabledWhileActive(true);
                yesCallback(int.Parse(inputField.text));
                window.Hide();
            });

            noButton.onClick.RemoveAllListeners();
            noButton.onClick.AddListener(() =>
            {
                if (window.isVisible == false)
                    return;

                SetEnabledWhileActive(true);
                if (ValidateInputField(minValue, maxValue) == false)
                    noCallback(-1);
                else
                    noCallback(int.Parse(inputField.text));
                window.Hide();
            });
        }

        /// <summary>
        /// Show the dialog.
        /// <b>Don't forget to call dialog.Hide(); when you want to hide it, this is not done auto. just in case you want to animate it instead of hide it.</b>
        /// </summary>
        /// <param name="title">The title of the dialog. Note that {0} is the item ID and {1} is the item name.</param>
        /// <param name="description">The description of the dialog. Note that {0} is the item ID and {1} is the item name.</param>
        /// <param name="yes">The name of the yes button.</param>
        /// <param name="no">The name of the no button.</param>
        /// <param name="minValue">The minimal value allowed to be selected.</param>
        /// <param name="maxValue">The max value allowed to be selected.</param> 
        /// <param name="item">
        /// You can add an item, if you're confirming something for that item. This allows you to use {0} for the title and {1} for the description inside the title and description variables of the dialog.
        /// An example:
        /// 
        /// ShowDialog("Are you sure you want to drop {0}?", "{0} sure seems valuable..", ...etc..);
        /// This will show the item name at location {0} and the description at location {1}.
        /// </param>
        /// <param name="yesCallback"></param>
        /// <param name="noCallback"></param>
        public virtual void ShowDialog(string title, string description, string yes, string no, int minValue, int maxValue, InventoryItemBase item, IntValDialogCallback yesCallback, IntValDialogCallback noCallback)
        {
            ShowDialog(string.Format(string.Format(title, item.name, item.description)), string.Format(description, item.name, item.description), yes, no, minValue, maxValue, yesCallback, noCallback);
        }


        protected virtual bool ValidateInputField(int minValue, int maxValue)
        {
            if (inputField.text == "")
                return false;

            int r = int.Parse(inputField.text);
            if (r > maxValue)
                inputField.text = maxValue.ToString();
            else if (r < minValue)
                inputField.text = minValue.ToString();

            return true;
        }
    }
}