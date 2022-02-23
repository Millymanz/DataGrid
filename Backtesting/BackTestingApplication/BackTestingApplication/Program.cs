using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingApplication
{
    static public class RunApp
    {
        static public void Run(bool bLive)
        {
            String modeType = bLive ? "LIVE" : "TEST";

            if (bLive)
            {
                modeType = "LIVE";
            }
            else
            {
                var modeStr = System.Configuration.ConfigurationManager.AppSettings["MODE"];
                modeType = modeStr;

                if (modeStr == "LIVE") modeType = "TEST";
            }

            Console.Title = "Backtesting Engine";

            if (modeType == "UI")
            {
                BacktestingEngine backtestingEngine = new BacktestingEngine();

                Console.WriteLine("Select Runtime Mode - (1)TEST Or (2)LIVE-TEST");
                String readAns = Console.ReadLine().ToLower();

                MODE modeItem;
                var selection = (MODE)Convert.ToInt32(readAns);
                modeItem = (MODE)selection;
                if ((MODE)selection == MODE.Live) modeItem = MODE.Test;

                switch (modeItem)
                {
                    case MODE.Test:
                        {
                            BacktestingEngine.Mode = MODE.Test;
                            Console.Title += " :: TEST-MODE";
                        }
                        break;

                    case MODE.Live:
                        {
                            BacktestingEngine.Mode = MODE.Live;
                            Console.Title += " :: LIVE-MODE";
                        }
                        break;
                }

                Console.WriteLine("");

                Console.WriteLine("Do you want to run the Indicator Ranking Process? (Y)Yes (N)No \n");
                String answer = Console.ReadLine().ToLower(); ;

                Console.WriteLine("\n");

                if (answer == "y")
                {
                    backtestingEngine.RunIndicatorRankingProcess();
                }

            }
            else if (modeType == "LIVE")
            {
                BacktestingEngine.Mode = MODE.Live;
                Console.Title += " :: LIVE-MODE";
            }
            else
            {
                BacktestingEngine.Mode = MODE.Test;
                Console.Title += " :: TEST-MODE";
            }

            var backtesting = new BacktestingEngine();
            backtesting.RunIndicatorRankingProcess();

            Console.WriteLine("Backtesting Engine Running...\n");
            Console.ReadLine();
            Console.ReadLine();

        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            RunApp.Run(false);

        }
    }
}
