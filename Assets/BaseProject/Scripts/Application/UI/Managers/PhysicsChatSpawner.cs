using System.Collections.Generic;
using System.Linq;
using CreativeMode;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

public class PhysicsChatSpawner : MonoBehaviour
{
    public float chatMessageDuration = 300f;
    public float chatEmoteDuration = 60f;
    
    public PhysicsWidgetContainer physicsWidgetPrefab;
    public ChatMessageWidget chatMessagePrefab;
    public ChatMessageWidget emoteMessagePrefab;
    public RectTransform containerRoot;
    public int maxMessagesPerUser;
    public int emoteMessageEmoteThreshold;

    public Vector2Int maxTextMessageSize;
    public Vector2Int maxEmoteMessageSize;

    public float spawnSidePadding;
    public float spawnHeight;
    public float spawnFlashDuration;
    public float spawnAngleRange;
    
    private Dictionary<string, List<SpawnedMessage>> chatMessagesByUser 
        = new Dictionary<string, List<SpawnedMessage>>();

    private List<SpawnedMessage> spawnedMessages = new List<SpawnedMessage>();

    private IChatInteractor Chat => Instance<IChatInteractor>.Get();
    
    private void Awake()
    {
        // Workaround, so messages can be spawned in local space without additional calculations
        ((RectTransform) transform).pivot = Vector2.zero;
        
        Chat.ChatMessages
            .Where(CanSpawnMessage)
            .Subscribe(SpawnChatMessage)
            .AddTo(this);

        Chat.OnClearChatMessages
            .Subscribe(_ => { RemoveAllMessages(); })
            .AddTo(this);
    }

    private void Update()
    {
        RemoveOldMessages();
    }

    private void RemoveOldMessages()
    {
        while (spawnedMessages.Count > 0)
        {
            var message = spawnedMessages.First();
            if (message.destroyTime > Time.time)
                return;
        
            RemoveMessage(message);
        }
    }

    private void RemoveAllMessages()
    {
        while (spawnedMessages.Count > 0)
        {
            var message = spawnedMessages.Last();
            RemoveMessage(message);
        }
    }

    private bool CanSpawnMessage(ChatMessage message)
    {
        return message.canDropOnDesktop;
    }

    private void SpawnChatMessage(ChatMessage message)
    {
        var isEmoteMessage = message.message.IsIconsOnly() && 
                             message.message.GetTagCount<IconTag>() <= emoteMessageEmoteThreshold;
        
        var messagePrefab = isEmoteMessage
            ? emoteMessagePrefab : chatMessagePrefab;

        var messageSize = isEmoteMessage 
            ? maxEmoteMessageSize : maxTextMessageSize;

        var spawnRotation = isEmoteMessage
            ? Quaternion.identity
            : Quaternion.Euler(0, 0, Random.Range(-spawnAngleRange, spawnAngleRange));
        
        var container = Instantiate(physicsWidgetPrefab, containerRoot);
        var widget = Instantiate(messagePrefab, container.transform);
        container.transform.localPosition = GetWidgetSpawnPosition();
        container.transform.localRotation = spawnRotation;

        widget.SetMessage(message);
        widget.Flash(spawnFlashDuration);
        widget.OnRemove = () =>
        {
            container.SetEnableCollisions(false);
            Destroy(container.gameObject, 0.5f);
        };

        container.maxWidth = messageSize.x;
        container.maxHeight = messageSize.y;
        container.UpdateContainerSize();

        OnUserMessageSpawned(new SpawnedMessage
        {
            authorId = message.author,
            widget = widget,
            message = message,
            destroyTime = Time.time + (isEmoteMessage ? chatEmoteDuration : chatMessageDuration)
        });
    }

    private Vector2 GetWidgetSpawnPosition()
    {
        var containerWidth = containerRoot.rect.width;
        var position = Input.mousePosition;

        position.y = spawnHeight;
        position.x = Mathf.Lerp(spawnSidePadding, 
            containerWidth - spawnSidePadding, 
            position.x / containerWidth);

        return position;
    }

    private void OnUserMessageSpawned(SpawnedMessage m)
    {
        spawnedMessages.Add(m);
        spawnedMessages.Sort((l, r) => 
            l.destroyTime.CompareTo(r.destroyTime));

        if (!chatMessagesByUser.TryGetValue(m.message.author, out var list))
        {
            list = new List<SpawnedMessage>();
            chatMessagesByUser[m.authorId] = list;
        }

        list.Add(m);

        if (list.Count > maxMessagesPerUser)
        {
            RemoveMessage(list.First());
        }
    }

    private void RemoveMessage(SpawnedMessage message)
    {
        spawnedMessages.Remove(message);
        if (chatMessagesByUser.TryGetValue(message.authorId, out var list))
            list.Remove(message);

        message.widget.Remove();
    }
    
    private class SpawnedMessage
    {
        public string authorId;
        public ChatMessageWidget widget;
        public ChatMessage message;
        public float destroyTime;
    }
}
