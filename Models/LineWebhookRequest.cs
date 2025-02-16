namespace drinkgogo.Models
{
    public class LineWebhookRequest
    {
        // Events 屬性儲存多個事件（即多筆來自使用者的訊息或動作）
        public List<LineEvent> Events { get; set; }
    }

    // 來自 LINE 平台的事件
    public class LineEvent
    {
        // 用來回應該事件的訊息，LINE 平台會將每個事件都分配一個 ReplyToken
        public string ReplyToken { get; set; }

        // 事件中的訊息內容（包含發送到用戶的訊息）
        public LineMessage Message { get; set; }
    }

    // LINE 訊息的結構
    public class LineMessage
    {
        // 訊息的類型
        public string Type { get; set; }

        // 訊息的內容（用戶所發送的訊息）
        public string Text { get; set; }
    }
}
