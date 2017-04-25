using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace Devdog.InventorySystem
{
    [AddComponentMenu("InventorySystem/UI Helpers/DraggableWindow")]
    public partial class DraggableWindow : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        public float dragSpeed = 1.0f;

        private Vector2 dragOffset;


        public void OnBeginDrag(PointerEventData eventData)
        {
            if (InventorySettingsManager.instance.isUIWorldSpace)
                dragOffset = transform.position - eventData.worldPosition;            
            else
                dragOffset = new Vector2(transform.position.x, transform.position.y) - eventData.position;
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            transform.position = new Vector3(eventData.position.x + dragOffset.x * dragSpeed, eventData.position.y + dragOffset.y * dragSpeed, 0.0f);
        }
    }
}