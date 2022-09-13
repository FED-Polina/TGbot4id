using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using System.Collections;
using System;
using System.Data;
using Microsoft.Data.Sqlite;
using NLog;
using MailKit;
using Telegram.Bot.Types.Enums;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.IO;

namespace TelegramBotExperiments
{
    struct BotUpdate
    {
        public string text;
        public long id;
        public string? username;
    }

    class Program
    {
        static string kod = System.IO.File.ReadAllText(@"tokenTG.txt");
        static ITelegramBotClient bot = new TelegramBotClient(kod.ToString());
        private static Logger logger = LogManager.GetCurrentClassLogger();
        static string fileName = "User.json";
        static List<BotUpdate> botUpdates = new List<BotUpdate>();

        static void Main(string[] args)
        {
            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);

            logger.Debug("log {1}", "EventHandler"); //лог

            //Read all saved updates
            try
            {
                var botUpdatesString = System.IO.File.ReadAllText(fileName);

                botUpdates = JsonConvert.DeserializeObject<List<BotUpdate>>(botUpdatesString) ?? botUpdates;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading or deserializing {ex}");
            }


            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            //подключение событий
            {
                AllowedUpdates = { }, //получать все типы обновлений
            };
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            Console.ReadLine();

            //подключение БД
            using (var connection = new SqliteConnection("Data Source=Films.db"))
            {
                connection.Open();
            }
            Console.Read();
        }

        public void SaveJson(object sender, EventArgs e)//Сохранение списка в файл Json
        {
            DataContractJsonSerializer jsonList = new DataContractJsonSerializer(typeof(List<BotUpdate>));
            FileStream fileList = new FileStream("User.json", FileMode.Create);
            jsonList.WriteObject(fileList, botUpdates);
            fileList.Close();
            Console.WriteLine("Файл успешно сохранен!");
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Некоторые действия
            logger.Debug("log {0}", "Start/Info/Help.Debug"); //лог
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;
                System.IO.File.WriteAllText("text.txt", $"Message:{message.Text}, message_id:{message.MessageId}");

                logger.Debug("log {0}", "Кнопка Start"); //лог
                if (message.Text.ToLower() == "/start")
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Добро пожаловать, юный киноман!");
                    return;
                }
                logger.Debug("log {0}", "Кнопка Info"); //лог
                if (message.Text.ToLower() == "/info")
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Моя задача помочь пользователю в подборке фильма.");
                    return;
                }
                logger.Debug("log {0}", "Кнопка Help"); //лог
                if (message.Text.ToLower() == "/help")
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Для работы с ботом необходимо воспользоваться кнопками: выбрать категорию жанр, затем сам фильм из предложенных.");
                    return;
                }
                await botClient.SendTextMessageAsync(message.Chat, "Извините, я не могу Вас понять.");
            }
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));

        }


    }

}
