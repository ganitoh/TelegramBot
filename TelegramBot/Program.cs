using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

const string _token = "6134644949:AAEK7PaCC9RLSNjNfhIPALmlaQd7J_zh3FA";

var botClient = new TelegramBotClient(_token);
using var cts = new CancellationTokenSource();

List<string> banWord = new List<string>();

string path = @"banWord.txt";

using (StreamReader reader = new StreamReader(path))
{
    string? line;
    while ((line = await reader.ReadLineAsync()) != null)
        banWord.Add(line.ToLower());
}

var reciverOptions = new ReceiverOptions
{
    AllowedUpdates = { }
};

botClient.StartReceiving(
    HandleUpdateAsynk,
    HandleErroreAsynk,
    reciverOptions,
    cancellationToken: cts.Token);

var me = await botClient.GetMeAsync();

Console.WriteLine($"начинаем работу с @{me.Username}");
Console.ReadLine();
cts.Cancel();

async Task HandleUpdateAsynk(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    if (update.Type == UpdateType.Message)
    {
        var chatId = update.Message!.Chat.Id;
        var messageText = update.Message!.Text;
        var messageId = update.Message.MessageId;
        var fromId = update.Message.From!.Id;

        var chat = update.Message.Chat;

        if (messageText is not null && IsBanWord(messageText))
        {
            if (chat.Type == ChatType.Supergroup)
            {
                string messageInfo = $"пользователь: {update.Message.From.Username} | время: {DateTime.Now.ToShortTimeString()} | сообщение: {messageText} | чат: {chat.Title}";

                try
                {
                    await botClient.RestrictChatMemberAsync(
                    chatId: chatId,
                    userId: fromId,
                    permissions: new ChatPermissions()
                    {
                        CanSendMediaMessages = false
                    },
                    untilDate: DateTime.Now.AddMinutes(30),
                    cancellationToken: cancellationToken
                    );

                    await botClient.DeleteMessageAsync(chatId, messageId, cancellationToken);
                    
                    Console.WriteLine($"сообщение удалено и выдан бан | {messageInfo}");
                }
                catch (ApiRequestException ex)
                {
                    Console.WriteLine($"{ex.Message} | {messageInfo}");
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message + $" | время: {DateTime.Now.ToShortTimeString()}");
                }
            }
            else 
            {
                try
                {
                    await botClient.DeleteMessageAsync(chatId, messageId, cancellationToken);
                    Console.WriteLine($"сообщение удалено " + $" | время: {DateTime.Now.ToShortTimeString()} | сообщение: {messageText}11");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + $" |  пользователь: {update.Message.From.Username} | время: {DateTime.Now.ToShortTimeString()} | сообщение: {messageText}");
                }
            }
        }
    }
}
bool IsBanWord(string message)
{
    string? messageLower = message.ToLower();

    foreach (var banMessage in banWord)
    {
        if (messageLower.Contains(banMessage))
            return true;
    }
    return false;
}
Task HandleErroreAsynk(ITelegramBotClient botClient, Exception exception, CancellationToken canalLocationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}
