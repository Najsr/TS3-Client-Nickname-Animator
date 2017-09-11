using System;
using System.Text.RegularExpressions;
using System.Timers;
using PrimS.Telnet;

namespace TeamspeakNameAnimator
{
    class Program
    {
        static Client client;
        static Timer timer = new Timer();
        static Regex regex;
        static Match match;
        static string nickname;
        static string current_nickname = string.Empty;
        static int counting_index = -1;

        static bool new_client = false;
        static string api_key = "";

        static void Main(string[] args)
        {
            Console.Title = "TS3 Nickname animator by Nicer";
            timer.Elapsed += Elapsed;
            if (args.Length != 0)
            {
                if (args[0] != null)
                {
                    uint time;
                    if (uint.TryParse(args[0], out time))
                        timer.Interval = time;
                    else
                        timer.Interval = 150;
                }
                if (args[1] != null)
                {
                    new_client = true;
                    api_key = args[1];
                }
            }
            try
            {
                client = new Client("localhost", 25639, new System.Threading.CancellationToken());
            }
            catch
            {
                Console.WriteLine("Failed to connect, please check that your client query is running");
                Console.ReadLine();
                return;
            }

            string welcome = client.Read();
            if (!welcome.Contains("TS3 Client"))
            {
                Console.WriteLine("Check that nothing is running on port 25639!");
                Console.ReadLine();
                return;
            }
            if (new_client)
                client.WriteLine("auth apikey=" + api_key);

            client.WriteLine("whoami");
            string whoami = client.Read();
            if (whoami.Contains("error id=1796"))
            {
                Console.WriteLine("You need to enter an API key to start parameters! (./TeamSpeakNameAnimator.exe 150 API_KEY_HERE)");
                Console.ReadLine();
                return;
            }
            regex = new Regex(@".*clid=([0-9]*)");
            match = regex.Match(whoami);
            client.WriteLine(string.Format("clientvariable clid={0} client_nickname", match.Groups[1].Value));

            nickname = client.Read().Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)[0];
            regex = new Regex(@".*client_nickname=(.*)");
            match = regex.Match(nickname);
            nickname = Unescape(match.Groups[1].Value);
            match = null;
            regex = null;
            Console.WriteLine("Animating nickname {0}", nickname);
            timer.Enabled = true;
            for (;;)
                Console.ReadKey(true);
        }

        private static void Elapsed(object sender, ElapsedEventArgs e)
        {
            current_nickname = string.Empty;

            for (int i = 0; i < counting_index; i++)
                current_nickname += " ";

            current_nickname += nickname;

            if (current_nickname == nickname)
                current_nickname = current_nickname.Substring(Math.Abs(counting_index));

            if (current_nickname.Length > 28)
                current_nickname = current_nickname.Substring(0, 28);

            client.WriteLine(@"clientupdate client_nickname=!\s" + Escape(current_nickname));
            counting_index--;

            if (counting_index == (0 - nickname.Length))
                counting_index = 27;
        }

        static string Unescape(string text)
        {
            return text.Replace(@"\s", " ").Replace(@"\n", "");
        }

        static string Escape(string text)
        {
            return text.Replace(" ", @"\s").Replace(@"\n", "");
        }

    }
}
