using MoleculeDataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Extentions;
using System.IO;
using System.Reflection;

namespace MoleculeServer
{
    class IP_Listener
    {
        bool Enabled = false;

        // Параметры БД
        static string DB_Server = "127.0.0.1";
        static string DB_Name = "mol_base";
        static string DB_User = "Mol_Base";
        static string DB_Pass = "Yrjksorybakpetudomgztyu73ju96m";

        /// <summary>
        /// База данных MySQL, из которой берётся вся информация
        /// </summary>
        static DB DataBase;

        /// <summary>
        /// Ключ и вектор для шифрования
        /// </summary>
        public static AES_Data CommonAES;

        /// <summary>
        /// Список активных пользователей
        /// </summary>
        public static List<User> Active_Users = new List<User>();

        /// <summary>
        /// Время бездействия пользователя до принудительного выхода
        /// </summary>
        const int UserTimeOut = 3600;



        /// <summary>
        /// Запускает прослушивание порта
        /// </summary>
        public void Start()
        {
            Enabled = true;

            try
            {
                //Подключаемся к БД
                DataBase = new DB(DB_Server, DB_Name, DB_User, DB_Pass);

                Log($"Подключение к службе MySQL: {DB_Server}:3306");
            }
            catch
            {
                Exception E = new Exception("Ошибка подключения к БД");
            }

            // Создаём классы для обработки команд
            List<Commands.ExecutableCommand> BaseCommands = new List<Commands.ExecutableCommand>()
            {
                new Commands.Account(DataBase),
                new Commands.Database(DataBase),
                new Commands.FileEngine(DataBase),
                new Commands.Laboratories(DataBase),
                new Commands.Log(DataBase),
                new Commands.Molecules(DataBase),
                new Commands.Users(DataBase),
                new Commands.Status(DataBase)
            };

            Log("Создание команд");

            Commands.Log LogCmd = (Commands.Log)(BaseCommands.Where(x => x.Name == "log").ToArray()[0]);

            Log("Создание команды Log");

            // Открываем файл-ключ
            try
            {
                CommonAES = (AES_Data)Serializable.LoadFromFile(Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "vector.bin"));
                Log("открытие ключа");
            }
            catch (Exception e)
            {
                Log(e.Message);
            }

            // Устанавливаем для сокета локальную конечную точку
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 11000);

            // Создаем сокет Tcp/Ip
            Socket sListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Log("Создание сокета");

            // Назначаем сокет локальной конечной точке и слушаем входящие сокеты
            try
            {
                sListener.Bind(ipEndPoint);
                sListener.Listen(10);

                Log("Старт сервера");

                // Завершаем все сеансы сообщением о падении сервера.
                DataBase.ExecuteQuery(@"UPDATE `sessions` 
                    SET `quit_date` = CURRENT_TIMESTAMP(), `reason_quit` = 'Server Fail – quit date of restart'
                    WHERE `quit_date` IS NULL;");

                // Начинаем слушать соединения
                while (Enabled)
                {
                    Log($"Ожидаем соединение через порт {ipEndPoint}");

                    // Программа приостанавливается, ожидая входящее соединение
                    Socket handler = sListener.Accept();
                    Log($"Сигнал получен. Передаём в отдельный поток");
                    //Task NewCommandRun = Task.Run(() => RunCommand(handler, BaseCommands, LogCmd));
                    RunCommand(handler, BaseCommands, LogCmd);
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
            finally
            {

            }
        }

        /// <summary>
        /// Останавливает прослушивание порта
        /// </summary>
        public void Stop()
        {
            Enabled = false;

            foreach (User U in Active_Users)
            {
                U.Quit("SYSTEM DOWN: SERVICE HAS BEEN STOPED");
            }

            // Удалим все устаревшие записи
            Active_Users.Clear();

        }

        /// <summary>
        /// Отправка текстового сообщения через сокет.
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="Msg"></param>
        public static void SendMsg(Socket handler, string Msg, User CurUser = null)
        {
            byte[] msg = CurUser == null
                ? Encoding.UTF8.GetBytes(Msg + "\n")
                : CurUser.Transport.Crypt.EncryptStringToBytes(Msg + "\n");
            handler.Send(BitConverter.GetBytes(msg.Length));
            using (FileStream FS = new FileStream("temp.dat", FileMode.Create))
            {
                FS.Write(msg, 0, msg.Length);
                FS.Close();
            };
            handler.Send(msg);
        }

        /// <summary>
        /// Завершение соединения через сокет.
        /// </summary>
        /// <param name="handler"></param>
        private static void FinishConnection(Socket handler)
        {
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();

            GC.Collect();
        }

        /// <summary>
        /// Выделение параметров из полного сообщения
        /// </summary>
        /// <param name="data_parse"></param>
        /// <returns></returns>
        private static string[] GetParameters(string[] data_parse)
        {
            string[] ShowParams = new string[data_parse.Count() - 3];
            for (int i = 3; i < data_parse.Count(); i++)
                ShowParams[i - 3] = data_parse[i];
            return ShowParams;
        }

        /// <summary>
        /// Выделение пользователя из списка активных пользователей по имени и кодовой фразе сессии
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="UserID"></param>
        /// <returns></returns>
        private static User GetCurUser(string UserName, string UserID)
        {
            User CurUser = Active_Users.Find(x => x.GetLogin() == UserName);
            if (CurUser == null) { return null; };
            if (CurUser.GetUserID() != UserID) { return null; };

            if ((DateTime.Now - CurUser.GetLastUse()).TotalSeconds > UserTimeOut)
            {
                CurUser.Quit("Time out");
                Active_Users.Remove(CurUser);
                return null;
            }

            // Удалим все устаревшие записи
            List<User> LU = Active_Users.FindAll(x => (DateTime.Now - x.GetLastUse()).TotalSeconds > UserTimeOut);
            foreach (User U in LU)
            {
                U.Quit("Time out");
                Active_Users.Remove(U);
            }

            // Продлим срок жизни пользователя. (Хе-хе-хе!)
            CurUser.Use();

            return CurUser;
        }

        /// <summary>
        /// Отсылает клиенту простое текстовое сообщение.
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="Message"></param>
        public static void SimpleMsg(Socket handler, string Message)
        {
            SendMsg(handler, Commands.Answer.StartMsg);
            SendMsg(handler, Message);
            SendMsg(handler, Commands.Answer.EndMsg);
        }

        /// <summary>
        /// Обработка полученной команды. Производится в другом потоке для использования многоядерности
        /// </summary>
        /// <param name="handler">Сокет</param>
        /// <param name="BaseCommands">Экземпляры команд</param>
        /// <param name="LogCmd">Журнал</param>
        private static void RunCommand(Socket handler, List<Commands.ExecutableCommand> BaseCommands,
            Commands.Log LogCmd)
        {
            try
            {
                string data = null;
                // Мы дождались клиента, пытающегося с нами соединиться
                LogCmd.DataBase.Log($"Приняли команду. Начали обработку");

                // Получаем длину текстового сообщения
                byte[] SL_Length_b = new byte[4];
                handler.Receive(SL_Length_b);
                LogCmd.DataBase.Log($"Получили размер команды");
                int SL_Length = BitConverter.ToInt32(SL_Length_b, 0);

                // Получаем текстовую часть сообщения
                byte[] bytes = new byte[SL_Length];
                int bytesRec = handler.Receive(bytes);
                LogCmd.DataBase.Log($"Получили команду");

                data += Encoding.UTF8.GetString(bytes, 0, bytesRec);

                string[] data_parse = data.Split("\n"[0]);

                // Показываем данные на консоли
                LogCmd.DataBase.Log($"Полученный текст: «{data}»\n\n");

                // Очистка ото всех уже не активных пользователей, которые почему-то не удалены из списка активных
                Active_Users.RemoveAll(x => x.Dead());
                LogCmd.DataBase.Log($"Удалили неактивных пользователей");

                // Ищем пользователя по его логину и защитной записи.
                // Если дана команда входа в систему, то поиск не производим.
                User CurUser = null;
                if (data_parse[0].Trim() != Commands.Account.LoginAll)
                {
                    LogCmd.DataBase.Log($"Получили команду на вход");
                    CurUser = GetCurUser(data_parse[1], data_parse[2]);
                    LogCmd.DataBase.Log($"Ищем пользователя с такими логином и паролем");
                    if (CurUser == null)
                    {
                        LogCmd.DataBase.Log($"Не нашли. Отправляем сообщение об отказе");
                        SendMsg(handler, Commands.Answer.StartMsg);
                        SendMsg(handler, Commands.Answer.LoginExp);
                        SendMsg(handler, Commands.Answer.EndMsg);
                        LogCmd.DataBase.Log($"Отправили");

                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();
                        LogCmd.DataBase.Log($"Закрыли соединение");

                        GC.Collect();
                        return;
                    }
                }

                // Записываем в журнал команду.
                // Сохраняем все переданные параметры в одну строку через перенос каретки
                string Params = "";
                for (int i = 3; i < data_parse.Count(); i++)
                {
                    if (i > 3) Params += "\n";
                    if ((data_parse[0].Trim() == Commands.Account.LoginAll) &&
                        (data_parse[i].StartsWith("password ")))
                        Params += "*****";
                    else Params += data_parse[i];
                }

                // И добавляем в лог
                int LogID = LogCmd.SaveQuery(handler, CurUser, data_parse[0], Params);

                // Обработка классом Commands.

                string[] Command = data_parse[0].Split('.');
                bool Executed = false;

                // Выполним операцию, если команда требует стандартных параметров
                foreach (Commands.IStandartCommand Block in BaseCommands.OfType<Commands.IStandartCommand>())
                {
                    if (Command[0].ToLower() == Block.To<Commands.ExecutableCommand>().Name)
                    {
                        Block.Execute(handler, CurUser, Command, GetParameters(data_parse));
                        Executed = true;
                        break;
                    }
                }

                // Выполним операцию, если команда требует расширенных параметров
                foreach (Commands.IUserListCommand Block in BaseCommands.OfType<Commands.IUserListCommand>())
                {
                    if (Command[0].ToLower() == Block.To<Commands.ExecutableCommand>().Name)
                    {
                        Block.Execute(handler, CurUser, Command, GetParameters(data_parse),
                            Active_Users, LogID);
                        Executed = true;
                        break;
                    }
                }

                if (!Executed) new Commands.Global(DataBase).Execute(handler, CurUser, Command,
                    GetParameters(data_parse), LogID);

                FinishConnection(handler);
            }
            catch (Exception e)
            {
                LogCmd.DataBase.Log(e.Message);
            }
        }

        public void Log(string Message)
        {
            DataBase.Log(Message);
        }
    }
}
