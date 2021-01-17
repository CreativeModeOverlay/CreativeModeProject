using ThreeDISevenZeroR.XmlUI;
using UnityEngine;
using UnityEngine.UI;

namespace CreativeMode
{
    public class NotificationManager : MonoBehaviour
    {
        public TextAsset notificationLayout;
        
        public LayoutInflater inflater;
        public XmlLayoutElement notificationContainer;
        public float currentRotation;
        private bool animateNotificationContainer;

        private void Awake()
        {
            notificationContainer.SetChildAnimator(new DoTweenChildAnimator());
        }

        private void Update()
        {
            MoveNotificationToPosition(Input.mousePosition, animateNotificationContainer);
            animateNotificationContainer = true;

            if (Input.GetButtonDown("Fire1"))
            {
                ShowNotification(null, "Test1", "Test2");
            }
        }

        private void MoveNotificationToPosition(Vector2 target, bool animate)
        {
            if (animate)
            {
                var position = notificationContainer.RectTransform.anchoredPosition;
                var newPosition = Vector2.Lerp(position, target, Time.deltaTime * 10);

                var positionDiff = (newPosition - position).x * Time.deltaTime;
                currentRotation = Mathf.Lerp(currentRotation, positionDiff * 60, Time.deltaTime * 10);

                notificationContainer.RectTransform.anchoredPosition = newPosition;
                notificationContainer.RectTransform.eulerAngles = new Vector3(0, 0, currentRotation);
            }
            else
            {
                currentRotation = 0;
                notificationContainer.RectTransform.anchoredPosition = target;
                notificationContainer.RectTransform.eulerAngles = Vector3.zero;
            }
        }

        public void ShowNotification(Sprite icon, string title, string subtitle)
        {
            var layout = inflater.InflateChild(notificationContainer, notificationLayout.text);
            var iconImage = layout.FindComponentById<Image>("Icon");
            var titleText = layout.FindComponentById<Text>("Title");
            var subtitleText = layout.FindComponentById<Text>("Subtitle");

            //iconImage.sprite = icon;
            titleText.text = title;
            subtitleText.text = subtitle;
        }
    }
}