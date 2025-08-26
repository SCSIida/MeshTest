using UnityEngine;
using UnityEngine.EventSystems;

namespace LocalMultiplayerUITest
{
    public class CustomReactionTest : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log($"{nameof(CustomReactionTest)}: {nameof(OnPointerEnter)}: {eventData.position}, {eventData.currentInputModule}: ", this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Debug.Log($"{nameof(CustomReactionTest)}: {nameof(OnPointerExit)}: {eventData.position}, {eventData.currentInputModule}: ", this);
        }
    }
}
