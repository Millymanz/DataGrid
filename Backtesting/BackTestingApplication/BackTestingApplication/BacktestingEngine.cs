using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using AnswerLibrary;
using System.Collections.Concurrent;

namespace BackTestingApplication
{
    public enum MODE
    {
        Live = 0,
        Test = 1,
        LiveTest = 2
    }


    public class BacktestingEngine
    {
         public static MODE Mode;

         private List<string> _timeFrameList;
         private List<string> _symbolIDList = new List<string>();

         private int _batchRunId = 0;

         public BacktestingEngine()
         {
             _timeFrameList = System.Configuration.ConfigurationManager.AppSettings["TimeFrameList"].Split(',').ToList();

             GetSymbolList();            

             //Register the batch run
             //obtain batch run id from database
         }

         public bool RunIndicatorRankingProcess()
         {
             DatabaseModel model = new DatabaseModel(Mode);
             _batchRunId = model.StartLogIndicatorPerformanceStats();            

             foreach (var symbolID in _symbolIDList)
             {
                 foreach (var timeframe in _timeFrameList)
                 {
                     GenerateBackTest(symbolID, timeframe);
                 }
             }

             model.EndLogIndicatorPerformanceStats(_batchRunId);

             return false;
         }

         private void GenerateBackTest(string symbolID, string timeframe)
         {
             IndicatorCalRSI(symbolID, timeframe);
         }

         private void IndicatorCalRSI(string symbolID, string timeFrame)
         {
             //14 day/datapoint strategy
             var pe = new ParameterEntity();
             pe.TimeFrame = timeFrame;
             pe.SymbolID.Add(symbolID);

             pe.IsRealTime = false;
             pe.MethodName = "rsi";
             pe.Exchange = "Forex";

             pe.DataPoints.Add(14); //RSI 14

             DatabaseModel model = new DatabaseModel(Mode);

             var dates = model.GetDataExtents(symbolID, timeFrame);
             if (dates.Any())
             {
                 pe.StartDatetime = dates[0];
                 pe.EndDatetime = dates[1];
             }
            

             //Get the most recent and then 12 months back for that data
             //
             var rsiCal = new RelativeStrengthIndexCalculator(pe);
             var result = rsiCal.Calculate();

             if (result.Any())
             {

                 var buySignals = result.FirstOrDefault().DerivedData.Where(m => m.Value < 30); //Buy Signal
                 var sellSignals = result.FirstOrDefault().DerivedData.Where(m => m.Value > 70); //Sell Signal

                 var buySignalBatch = new BatchRunLogItem();
                 buySignalBatch.RunId = _batchRunId;
                 buySignalBatch.Returns = buySignals.Count();
                 buySignalBatch.Indicator = "Relative Strength Index - RSI 14";
                 buySignalBatch.Description = "RSI 14 - Buy Signals is based on currency pair being oversold when RSI is below 30";

                 buySignalBatch.Categorization = "Momentum";
                 buySignalBatch.Scenario = "Buy";
                 buySignalBatch.TimeFrame = pe.TimeFrame;
                 buySignalBatch.SymbolID = pe.SymbolID.FirstOrDefault();

                 var tradeData = result.FirstOrDefault().TempDataCollection as List<TradeSummary>;

                 //CalculateEveryOtherPriceChange(buySignalBatch, tradeData, buySignals);

                 CalculateEveryOtherPriceChange(buySignalBatch, tradeData.Take(5000).ToList(), buySignals.Take(2000)); //temp
                 model.SavePerformanceStatistics(buySignalBatch);


                 var sellSignalBatch = new BatchRunLogItem();
                 sellSignalBatch.RunId = _batchRunId;
                 sellSignalBatch.Returns = sellSignals.Count();

                 //save to database  
             }
         }

         //private List<KeyValuePair<TradeSummary, TradeSummary>> GetTradesWithSignals(List<TradeSummary> tradeSummary, IEnumerable<KeyValuePair<DateTime, Double>> signals)
         //{
         //    //tradeSummary.Where(m => signals.ContainsKey(m.DateTime));
         //    List<KeyValuePair<TradeSummary, TradeSummary>> tradeSignalResult = new List<KeyValuePair<TradeSummary, TradeSummary>>();

         //    for (int i = 0; i < tradeSummary.Count; i++)
         //    {         
         //        if (signals.Any(k => k.Key == tradeSummary[i].DateTime))
         //        {
         //            int indexAfter = i + 1;
         //            if (indexAfter < tradeSummary.Count)
         //            {
         //                var tradeSignal = new KeyValuePair<TradeSummary, TradeSummary>(tradeSummary[i], tradeSummary[indexAfter]);

         //                tradeSignalResult.Add(tradeSignal);
         //            }
         //        }
         //    }
         //    return tradeSignalResult;
         //    //CalculateEveryOtherPriceChange
         //}

         private ConcurrentBag<KeyValuePair<TradeSummary, TradeSummary>> GetTradesWithSignals(List<TradeSummary> tradeSummary, IEnumerable<KeyValuePair<DateTime, Double>> signals)
         {
             ConcurrentBag<KeyValuePair<TradeSummary, TradeSummary>> tradeSignalResult = new ConcurrentBag<KeyValuePair<TradeSummary, TradeSummary>>();

             Parallel.For(0, tradeSummary.Count,
             index =>
             {
                 if (signals.Any(k => k.Key == tradeSummary[index].DateTime))
                 {
                     int indexAfter = index + 1;
                     if (indexAfter < tradeSummary.Count)
                     {
                         var tradeSignal = new KeyValuePair<TradeSummary, TradeSummary>(tradeSummary[index], tradeSummary[indexAfter]);

                         tradeSignalResult.Add(tradeSignal);
                     }
                 }
             });
             return tradeSignalResult;
         }

         private void CalculateEveryOtherPriceChange(BatchRunLogItem batchRunLog, List<TradeSummary> tradeSummary, IEnumerable<KeyValuePair<DateTime, Double>> signals)
         {
             if (tradeSummary.Any() && signals.Any())
             {
                 var tradeSignalResult = GetTradesWithSignals(tradeSummary, signals);
                 List<double> percentDiffList = new List<double>();
                 double result = 0.0;

                 foreach (var item in tradeSignalResult)
                 {
                     var pricechange = item.Value.Close - item.Key.Close;
                     var percentDiff = (pricechange / item.Key.Close) * 100;
                     percentDiffList.Add(percentDiff);
                 }
                 percentDiffList.Sort();

                 if (percentDiffList.Any())
                 {
                     double total = 0.0;
                     foreach (var item in percentDiffList)
                     {
                         total += item;
                     }
                     result = total / percentDiffList.Count;
                 }

                 batchRunLog.AverageReturn = result;

                 int remainder = percentDiffList.Count % 2;

                 int medianIndex = 0;

                 if (remainder > 0)
                 {
                     double temp = percentDiffList.Count / 2;
                     var medianIndexTemp = Math.Round(temp);

                     medianIndex = Convert.ToInt32(medianIndexTemp);

                     batchRunLog.MedianReturn = percentDiffList[medianIndex];
                 }
                 else
                 {
                     if (percentDiffList.Count == 2)
                     {
                         batchRunLog.MedianReturn = (percentDiffList[0] + percentDiffList[1]);
                     }
                     else
                     {
                         int tempIndex = percentDiffList.Count / 2;
                         batchRunLog.MedianReturn = (percentDiffList[tempIndex] + percentDiffList[tempIndex + 1]) / 2;
                     }
                 }

                 batchRunLog.PercentPositive = (percentDiffList.Where(n => n > 0).Count() / batchRunLog.Returns) * 100;
                 batchRunLog.PercentNegative = 100 - batchRunLog.PercentPositive;
             }
         }

         //private List<KeyValuePair<TradeSummary,TradeSummary>> GetTradesWithSignals(List<TradeSummary> tradeSummary, Dictionary<DateTime, Double> signals)
         //{
         //    //tradeSummary.Where(m => signals.ContainsKey(m.DateTime));
         //    List<KeyValuePair<TradeSummary, TradeSummary>> tradeSignalResult = new List<KeyValuePair<TradeSummary, TradeSummary>>();

         //    for (int i = 0; i < tradeSummary.Count; i++)
         //    {
         //        if (signals.ContainsKey(tradeSummary[i].DateTime))
         //        {
         //            int indexAfter = i + 1;
         //            if (indexAfter < tradeSummary.Count)
         //            {
         //                var tradeSignal = new KeyValuePair<TradeSummary, TradeSummary>(tradeSummary[i], tradeSummary[indexAfter]);

         //                tradeSignalResult.Add(tradeSignal);
         //            }
         //        }
         //    }
         //    return tradeSignalResult;
         //    //CalculateEveryOtherPriceChange
         //}

         //private double CalculateEveryOtherPriceChange(BatchRunLogItem batchRunLog, List<TradeSummary> tradeSummary, Dictionary<DateTime, Double> signals)
         //{
         //    var tradeSignalResult = GetTradesWithSignals(tradeSummary, signals);
         //    List<double> percentDiffList = new List<double>();
         //    double result = 0.0;

         //    foreach (var item in tradeSignalResult)
         //    {
         //        var pricechange = item.Value.Close - item.Key.Close;
         //        var percentDiff = (pricechange / item.Key.Close) * 100;
         //        percentDiffList.Add(percentDiff);
         //    }

         //    if (percentDiffList.Any())
         //    {
         //        double total = 0.0;
         //        foreach (var item in percentDiffList)
         //        {
         //            total += item;
         //        }
         //        result = total / percentDiffList.Count;
         //    }

         //    batchRunLog.AverageReturns = result;

         //    int remainder = percentDiffList.Count % 2;
             
         //    int medianIndex = 0;

         //    if (remainder > 0)
         //    {
         //       double temp = percentDiffList.Count / 2;
         //      var medianIndexTemp = Math.Round(temp);

         //    }

         //    //batchRunLog.MedianReturn = percentDiffList.Sort()

         //    return result;
         //}

         private void GetSymbolList()
         {
             String path = System.Configuration.ConfigurationManager.AppSettings["DATALIST"];

             //file read
             try
             {
                 // Create an instance of StreamReader to read from a file.
                 // The using statement also closes the StreamReader.

                 //temp file location in future will be with exe
                 using (StreamReader sr = new StreamReader(path))
                 {
                     string line;
                     // Read and display lines from the file until the end of 
                     // the file is reached.

                     //Queue<String> symbols = new Queue<String>();
                     while ((line = sr.ReadLine()) != null)
                     {
                         line = line.Replace("/", "");

                         if (String.IsNullOrEmpty(line) == false)
                         {
                             _symbolIDList.Add(line);
                         }
                     }
                 }
             }
             catch (Exception e)
             {
                 // Let the user know what went wrong.
                 Console.WriteLine("The file could not be read:");
                 Console.WriteLine(e.Message);

             }            
         }
    }
}
