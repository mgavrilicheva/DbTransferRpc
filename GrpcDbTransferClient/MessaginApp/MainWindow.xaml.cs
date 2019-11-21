using ExchangeLibrary;
using Grpc.Core;
using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Net;
using System.Windows;

namespace MessaginApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Client client;
        private SQLiteConnectionStringBuilder csb;

        private static ConnectionTypes ConnectionType => Properties.Settings.Default.connectionType;
        private static string MqServerAddress => Properties.Settings.Default.mqServerAddress;
        private static string MqPassword => Properties.Settings.Default.mqPassword;
        private static int MqServerPort => Properties.Settings.Default.mqServerPort;
        private static string MqUser => Properties.Settings.Default.mqUser;
        private static string ServerIpAddress => Properties.Settings.Default.serverIpAddress;
        private static int ServerPort => Properties.Settings.Default.serverPort;

        public MainWindow()
        {
            InitializeComponent();
            serverLabel.Content = $"Адрес сервера: {ServerIpAddress}";
            InitializeSqliteConnectionBuilder("languages_db.db", 3);
        }

        private bool InitializeClient()
        {
            bool successfulInitialization = true;
            switch(ConnectionType)
            {
                case ConnectionTypes.SOCKETS:
                    {
                        client = new SocketClient(IPAddress.Parse(ServerIpAddress), ServerPort);
                        break;
                    }
                case ConnectionTypes.MESSAGE_QUERY:
                    {
                        client = new MessageQueryClient(MqServerAddress, MqServerPort, MqUser, MqPassword);
                        break;
                    }
                case ConnectionTypes.GRPC:
                    {
                        client = new GrpcClient(ServerIpAddress, ServerPort, new SslCredentials());
                        break;
                    }
                default:
                    {
                        MessageBox.Show("Увы, но используемый тип соединения не распознан. Покопайтесь в настройках, что ли.");
                        successfulInitialization = false;
                        break;
                    }
            }
            return successfulInitialization;
        }

        private void InitializeSqliteConnectionBuilder(string path, int version)
        {
            csb = new SQLiteConnectionStringBuilder
            {
                DataSource = path,
                Pooling = true,
                Version = version
            };
        }

        private void LoadDataFromDesktopDatabase()
        {
            using (SQLiteConnection sqliteConnection = new SQLiteConnection(csb.ToString()))
            {
                sqliteConnection.Open();
                using (SQLiteCommand command = new SQLiteCommand
                {
                    Connection = sqliteConnection,
                    CommandText =
                    @"SELECT * FROM languages
                    ORDER BY 
                        macrofamily_id, family_id, language_id, 
                        writing_type_id, writing_id, alphabet_id, 
                        country_id, lang_info_rec_id, language_by_countries_rec_id;"
                })
                {
                    using (SQLiteDataReader result = command.ExecuteReader())
                    {
                        var data = new DataTable("Languages");
                        data.Load(result);
                        dataTable.ItemsSource = data.DefaultView;
                        data.Dispose();
                    }
                }
            }
        }

        private byte[] ReadDatabase()
        {
            return File.ReadAllBytes(csb.DataSource);
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            sendButton.IsEnabled = false;
            byte[] dbBytes = ReadDatabase();
            try
            {
                client.Send(dbBytes);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Какие-то нелады с соединением. Попробуйте ещё раз, когда появится тырнет.");
                Console.WriteLine(ex.Message);
            }
            sendButton.IsEnabled = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(csb.DataSource))
                MessageBox.Show("База данных не найдена! Положите БД в папку с приложением и перезапустите приложение, что ли.");
            else
            {
                if (!InitializeClient())
                    return;
                LoadDataFromDesktopDatabase();
                sendButton.IsEnabled = true;
            }
        }
    }
}
