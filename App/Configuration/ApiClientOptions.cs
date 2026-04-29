namespace App.Configuration
{
    public class ApiClientOptions
    {
        /// <summary>Базова адреса API (повинна закінчуватись на /)</summary>
        public string BaseUrl { get; set; } = "";

        /// <summary>Таймаут однієї спроби у секундах</summary>
        public int TimeoutSeconds { get; set; } = 5;

        /// <summary>Кількість повторних спроб після першої невдачі</summary>
        public int MaxRetryAttempts { get; set; } = 3;

        /// <summary>Час (секунди), на який Circuit Breaker блокує запити після спрацювання</summary>
        public int BreakDurationSeconds { get; set; } = 10;
    }
}
