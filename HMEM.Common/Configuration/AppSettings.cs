namespace HMEM.Common.Configuration
{
    public class AppSettings
    {
        public MongoDBSettings MongoDB { get; set; } = new MongoDBSettings();
        public TelegramSettings Telegram { get; set; } = new TelegramSettings();
    }
}
