using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunForAllTrain
{
    using System.Data.SqlClient;
    using System.IO;

    class Program
    {
        static void Main(string[] args)
        {
            for (var i = 2; i <= 5; i++)
            {

                LinesFetcher fectch = new LinesFetcher();
                PredictSalesCalculater predictor = new PredictSalesCalculater();
                predictor.NFactor = i;
                LineDataWriter writer = new LineDataWriter();
                var lines = fectch.FetchAllLinesData();
                predictor.PredictSales(lines);
                predictor.CalculatePredictErrorRate(lines);
                predictor.CalculateTotalErrorRate(lines);
                writer.WriteLines($"N{i}ForTest", lines);
            }

            for (var i = 2; i <= 5; i++)
            {

                LinesFetcher fectch = new LinesFetcher();
                PredictSalesCalculater predictor = new PredictSalesCalculater();
                predictor.NFactor = i;
                predictor.Alogorithm = AloType.Weight;
                LineDataWriter writer = new LineDataWriter();
                var lines = fectch.FetchAllLinesData();
                predictor.PredictSales(lines);
                predictor.CalculatePredictErrorRate(lines);
                predictor.CalculateTotalErrorRate(lines);
                writer.WriteLines($"WeightN{i}ForTest", lines);
            }
        }
    }

    public class Config
    {
        public static int TotalDays
        {
            get
            {
                return 60;
            }
        }
    }

    public enum AloType
    {
        Simple,
        Weight,
        Log,
    }

    public class SingleLine
    {
        public string Code { get; set; }

        public string FromStation { get; set; }

        public string ToStation { get; set; }

        public string Type { get; set; }

        public string Date { get; set; }
    }

    public class SingleLineData
    {
        public SingleLine Line { get; } = new SingleLine();

        public Dictionary<int, int> ActualSales { get; } = new Dictionary<int, int>();

        public Dictionary<int, int> PredictSales { get; } = new Dictionary<int, int>();

        public Dictionary<int, double> ErrorRate { get; } = new Dictionary<int, double>();

        public double TotalErrorRate { get; set; }

        public string GetKey()
        {
            return $"{this.Line.Code}-{this.Line.Date}-{this.Line.FromStation}-{this.Line.ToStation}-{this.Line.Type}";
        }
    }

    public class PredictSalesCalculater
    {
        private int nFactor = 2;

        public PredictSalesCalculater()
        {
            this.ErrorRateDays = 30;
            this.SetNFactorFor();
        }

        public int NFactor {
            get
            {
                return this.nFactor;
            }
            set
            {
                this.nFactor = value;
                this.SetNFactorFor();
            }
        }

        public int ErrorRateDays { get; set; }

        public AloType Alogorithm { get; set; }

        public List<double> PredictWeights { get; } = new List<double>();

        private void SetNFactorFor()
        {
            double sum = 1;
            for (var i = 0; i < this.NFactor; i++)
            {
                this.PredictWeights.Add(1);
            }

            for (var i = this.NFactor; i > 0; i--)
            {
                var index = i - 1;
                if (index == 0)
                {
                    this.PredictWeights[index] = sum;
                }
                else
                {
                    this.PredictWeights[index] = sum * 0.9;
                    sum = sum - this.PredictWeights[index];
                }

            }
        }

        public void PredictSales(List<SingleLineData> list)
        {
            foreach (var line in list)
            {
                switch (this.Alogorithm)
                {
                    case AloType.Simple:
                        this.PredictSales(line);
                        break;
                    case AloType.Weight:
                        this.PredictSalesWithWeight(line);
                        break;
                    case AloType.Log:
                        this.PredictSalesWithLog(line);
                        break;

                }

            }
        }
        public void PredictSales(SingleLineData line)
        {
            foreach (var sales in line.ActualSales)
            {
                int start = sales.Key - this.NFactor ;
                start = start > 1 ? start : 1;
                int end = sales.Key;
                double size = (double)end - start;

                int sum = 0;
                for (var i = start; i < end; i++)
                {
                    sum += line.ActualSales[i];
                }

                line.PredictSales[sales.Key] = (int)Math.Ceiling((double)(sum) / size);
            }
        }

        public void PredictSalesWithWeight(SingleLineData line)
        {
            foreach (var sales in line.ActualSales)
            {
                int start = sales.Key - this.NFactor;
                start = start > 1 ? start : 1;
                int end = sales.Key;
                double size = (double)end - start;

                double sum = 0;
                if (size >= this.NFactor)
                {
                    for (var i = start; i < end; i++)
                    {
                        var index = i - start;
                        sum += line.ActualSales[i] * this.PredictWeights[index];
                    }
                }
                else
                {
                    this.PredictSales(line);
                }

                line.PredictSales[sales.Key] = (int)Math.Ceiling((double)(sum));
            }
        }

        public void PredictSalesWithLog(SingleLineData line)
        {
            foreach (var sales in line.ActualSales)
            {
                int start = sales.Key - this.NFactor - 1;
                start = start > 1 ? start : 1;
                int end = sales.Key;

                int sum = 0;
                for (var i = start; i < end; i++)
                {
                    sum += line.ActualSales[i];
                }

                line.PredictSales[sales.Key] = (int)Math.Ceiling((double)(sum) / (double)this.NFactor);
            }
        }

        public void CalculatePredictErrorRate(List<SingleLineData> list)
        {
            foreach (var line in list)
            {
                this.CalculatePredictErrorRate(line);
            }
        }

        public void CalculatePredictErrorRate(SingleLineData line)
        {
            for (int i = 1; i <= Config.TotalDays; i++)
            {
                if (line.ActualSales[i] != 0)
                {
                    var errorRate = (double)(line.PredictSales[i] - line.ActualSales[i]) / (double)line.ActualSales[i];
                    line.ErrorRate[i] = Math.Abs(errorRate);
                }
                else
                {
                    line.ErrorRate[i] = int.MinValue;
                }
            }
        }

        public void CalculateTotalErrorRate(List<SingleLineData> list)
        {
            foreach (var line in list)
            {
                this.CalculateTotalErrorRate(line);
            }
        }

        public void CalculateTotalErrorRate(SingleLineData line)
        {
            var sum = 0.0d;
            var factor = 0.0d;
            for (var i = Config.TotalDays; i > (Config.TotalDays - this.ErrorRateDays); i--)
            {
                var error = line.ErrorRate[i];
                if (Math.Abs(error - int.MinValue) > 0)
                {
                    factor += line.ActualSales[i];
                    var errorRate = line.ErrorRate[i] * line.ActualSales[i];
                    sum += errorRate;
                }
            }
            line.TotalErrorRate = sum / factor;
        }
    }

    public class LinesFetcher
    {
        public List<SingleLineData> FetchAllLinesData()
        {
            Dictionary<string, SingleLineData> dataSet = new Dictionary<string, SingleLineData>();
            var connStr = "server=cnyazha-svr;user id=sa;password=Passw0rd!;database=Test";
            using (var conn = new SqlConnection(connStr))
            {
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    conn.Open();
                }

                using (var sqlCmd = conn.CreateCommand())
                {
                    sqlCmd.CommandText =
                        "select code, ondate, [type], fromstation, tostation, presaleday, SUM(sales) as sales from AllData group by code, ondate, [type], fromstation, tostation, presaleday order by code, ondate, [type], fromstation, tostation, presaleday";
                    using (var reader = sqlCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var lineData = new SingleLineData();
                            lineData.Line.Code = reader.GetString(0);
                            lineData.Line.Date = reader[1].ToString();
                            lineData.Line.Type = reader.GetString(2);
                            lineData.Line.FromStation = reader.GetString(3);
                            lineData.Line.ToStation = reader.GetString(4);
                            var key = lineData.GetKey();
                            if (dataSet.ContainsKey(key))
                            {
                                lineData = dataSet[key];
                            }
                            else
                            {
                                dataSet[key] = lineData;
                            }

                            var presaleDay = Config.TotalDays + reader.GetInt32(5);
                            var sales = reader.GetInt32(6);
                            lineData.ActualSales[presaleDay] = sales;
                        }
                    }
                }
            }

            return dataSet.Values.ToList();
        }

        //public void SaveDataToFile(List<SingleLineData>)
    }

    public class LineDataWriter
    {

        public void WriteLines(string textPrex, List<SingleLineData> list)
        {
            using (var sw = new StreamWriter(textPrex + "result" + System.Environment.TickCount.ToString() + ".txt"))
            {
                foreach (var line in list)
                {
                    this.WriteLine(sw, line);
                }
            }
        }

        public void WriteLine(StreamWriter sw, SingleLineData data)
        {
            for (var i = 1; i <= Config.TotalDays; i++)
            {
                sw.WriteLine($"{data.Line.Code}, {data.Line.Date}, {data.Line.Type},{data.Line.FromStation}, {data.Line.ToStation},{i}, {data.ActualSales[i]}, {data.PredictSales[i]}, {data.ErrorRate[i]}, {data.TotalErrorRate}");
            }
        }
    }

}
