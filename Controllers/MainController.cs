using drinkgogo.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace drinkgogo.Controllers
{
    [Route("api/[controller]")]  // 定義此 API 的路徑為 api/Main（Controller 名稱為 Main）
    [ApiController]
    public class MainController : ControllerBase
    {
        private readonly string? _channelAccessToken;   // 存放 LINE 頻道的 Access Token，用來驗證和授權發送訊息
        private readonly List<string> Shops;  // 存放飲料店清單，從設定檔中讀取

        public MainController(IConfiguration configuration)
        {
            // 從設定檔（appsettings.json）讀取 LINE 頻道的 Access Token 和飲料店清單
            _channelAccessToken = configuration["LineBot:ChannelAccessToken"];
            Shops = configuration.GetSection("DrinkShops").Get<List<string>>() ?? new List<string>();
        }

        // 建立一個亂數生成器，用於隨機選擇飲料店
        private readonly Random _random = new Random();

        // 允許 GET 請求（用於測試）
        [HttpGet("RandomShop")]
        public IActionResult GetRandomShop()
        {
            // 從飲料店清單中隨機選擇一家店
            string randomShop = Shops[_random.Next(Shops.Count)];
            return Ok(new { shop = randomShop });  // 回傳隨機選擇的店名
        }

        [HttpPost("webhook")]  // 設定此路徑為處理 webhook 的 POST 請求
        public async Task<IActionResult> Post([FromBody] LineWebhookRequest webhookRequest)
        {
            if (webhookRequest.Events == null || webhookRequest.Events.Count == 0)
            {
                return Ok(); // 確保 LINE 的測試請求不會導致錯誤
            }

            // 取得用戶傳來的訊息
            var userMessage = webhookRequest.Events[0].Message.Text;

            // 使用正則表達式來匹配用戶訊息，例如 "$推薦" 或 "$飲料推薦"
            var pattern = @"^(\$推薦|\$飲料推薦)$";

            // 如果訊息符合正則表達式模式
            if (Regex.IsMatch(userMessage, pattern))
            {
                // 在飲料店清單中隨機選擇一家店
                string randomShop = Shops[_random.Next(Shops.Count)];

                // 準備要回覆給用戶的訊息，這裡是文字訊息
                var replyMessage = new
                {
                    replyToken = webhookRequest.Events[0].ReplyToken,  // 用來回覆訊息的 replyToken
                    messages = new[] {
                        new { type = "text", text = $"今天你可以喝 {randomShop}！" }  // 回傳隨機選擇的飲料店名稱
                    }
                };

                using (var client = new HttpClient())
                {
                    // 設定 HTTP 請求的認證頭，使用 Bearer Token 認證
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", _channelAccessToken);

                    // 發送 POST 請求給 LINE API，用來回覆訊息
                    var response = await client.PostAsJsonAsync("https://api.line.me/v2/bot/message/reply", replyMessage);

                    // 檢查回應是否成功，如果不成功則可以記錄錯誤訊息
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorResponse = await response.Content.ReadAsStringAsync();
                    }

                    // 確保請求成功，否則會拋出異常
                    response.EnsureSuccessStatusCode();
                }

                return Ok();  // 返回成功響應
            }

            return NoContent();  // 如果訊息不符合要求，返回 204 No Content，表示無需回應
        }
    }
}
