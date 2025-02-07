namespace ConverterAPI.Services;

public static class ConfigurationHelper
{
    private static readonly IConfiguration _configuration;

    static ConfigurationHelper()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        _configuration = builder.Build();
    }

    public static string GetConnectionString(string name)
    {
        var connectionString = _configuration.GetConnectionString(name);
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException($"The connection string '{name}' has not been initialized.");
        }
        return connectionString;
    }
}

/*
Эти 4 строки посвящаются победителю легендарной постоянной рубрики
"Рандомная загадка от Бредихина", Ëкарному Бабаю, успешно ответившему
на все поставленные вопросы! Насколько правильными были его ответы мне похрен,
по вашему я правда их проверял и знаю, какой там ответ?
*/