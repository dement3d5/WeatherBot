using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Args;

var botClient = new TelegramBotClient(token: "5553659992:AAEqG7xOCD04V9SZfa2DX8-sRQuSV-Ciq7s");
using var cts = new CancellationTokenSource();

var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = { }

};



botClient.StartReceiving(
    HandleUpdateAsync, 
    HandleErrorAsync, 
    receiverOptions, 
    cancellationToken: cts.Token);
var me = botClient.GetMeAsync().Result;
Console.WriteLine($"Bot_Id: {me.Id} \nBot_Name: {me.FirstName} ");
Console.ReadLine();
cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    if (update.Type == UpdateType.Message && update?.Message?.Text != null)
    {
        await HandleMessage(botClient, update.Message);
        return;

    }
    if (update.Type ==UpdateType.CallbackQuery)
    {
        await HandleCallbackQuery(botClient, update.CallbackQuery);
        return;

    }

}
async Task HandleMessage(ITelegramBotClient botClient, Message message)
{
    if (message.Text == "/start")
    {
        await botClient.SendTextMessageAsync(message.Chat.Id, text: "Choose commands: /inline | /keyboard");
    }

    if (message.Text == "/keyboard")
    {
        ReplyKeyboardMarkup keyboard = new(new[]
        {

            new KeyboardButton[] {"Владивосток"},
        })
        {

            ResizeKeyboard = true

        };
        await botClient.SendTextMessageAsync(message.Chat.Id, text: "Choose:", replyMarkup: keyboard);
        return;

    }
}


async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
{
    if (callbackQuery.Data.StartsWith("buy"))
    {
        await botClient.SendTextMessageAsync(
            callbackQuery.Message.Chat.Id,
            $"Вы хотите купить?"
        );
        return;
    }
    if (callbackQuery.Data.StartsWith("sell"))
    {
        await botClient.SendTextMessageAsync(
            callbackQuery.Message.Chat.Id,
            $"Вы хотите продать?"
        );
        return;
    }
    await botClient.SendTextMessageAsync(
        callbackQuery.Message.Chat.Id,
        $"You choose with data: {callbackQuery.Data}"
        );
    return;
}


Task HandleErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Ошибка телеграм АПИ:\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
        _ => exception.ToString()
    };
    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}

namespace WeatherBot
{
    class Program
    {
        private static string token { get; set; } = "5553659992:AAEqG7xOCD04V9SZfa2DX8-sRQuSV-Ciq7s";
        private static TelegramBotClient client;

        static string NameCity;
        static float tempOfCity;
        static string nameOfCity;

        static string answerOnWether;

        public static void Main(string[] args)
        {
            client = new TelegramBotClient(token) { Timeout = TimeSpan.FromSeconds(10) };

            var me = client.GetMeAsync().Result;
            Console.WriteLine($"Bot_Id: {me.Id} \nBot_Name: {me.FirstName} ");
            сlient.OnMessage += Bot_OnMessage;
            client.StartReceiving();
            Console.ReadLine();
            client.StopReceiving();
        }
        private static async void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            var message = e.Message;

            if (message.Type == MessageType.Text)
            {
                NameCity = message.Text;
                Weather(NameCity);
                Celsius(tempOfCity);
                await client.SendTextMessageAsync(message.Chat.Id, $"{answerOnWether} \n\nТемпература в {nameOfCity}: {Math.Round(tempOfCity)} °C");

                Console.WriteLine(message.Text);
            }
        }
        public static void Weather(string cityName)
        {
            try
            {
                string url = "https://api.openweathermap.org/data/2.5/weather?q=" + cityName + "&unit=metric&appid=5d18a196257406fa3ed24f17c35f9682";
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest?.GetResponse();
                string response;

                using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                {
                    response = streamReader.ReadToEnd();
                }
                ResponseWeather weatherResponse = JsonConvert.DeserializeObject<ResponseWeather>(response);

                nameOfCity = weatherResponse.Name;
                tempOfCity = weatherResponse.Main.Temp - 273;
            }
            catch (System.Net.WebException)
            {
                Console.WriteLine("Возникло исключение");
                return;
            }

        }
        public static void Celsius(float celsius)
        {
            if (celsius <= 10)
                answerOnWether = "Сегодня холодно одевайся потеплее!";
            else
                answerOnWether = "Сегодня очень жарко, так что можешь одеть маечку и шортики)";
        }
    }



}



