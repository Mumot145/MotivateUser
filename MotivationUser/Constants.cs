namespace MotivationUser
{
	public static class Constants
	{
		// Replace strings with your mobile services and gateway URLs.
		public static string ApplicationURL = @"http://motivationchat.azurewebsites.net";
        public const string SenderID = "293616591997"; // Google API Project Number
        public const string ListenConnectionString = "Endpoint=sb://motivation.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=yynG2WLZdtVEvl7PIGS3fT4RX9tHq9f60K+eeMIFoWY=";
        public const string NotificationHubName = "MotivationalNotificationHub";
        public const string AzureSQLConnection = "Server=tcp:motivationserver.database.windows.net,1433;Initial Catalog=MotivationDB;Persist Security Info=False;User ID=patrick@motivationserver;Password=malwina145!;MultipleActiveResultSets=False;Connection Timeout=30;";
    }
}
