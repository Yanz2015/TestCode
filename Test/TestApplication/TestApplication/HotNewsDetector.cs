using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TestApplication
{
    public class HotNewsDetector
    {
        public HotNewsDetector(int windowSize)
        {
            this.Messages = new List<Message>();
            this.MessageWindows = new List<MessageWindow>();
            this.Keywords = new Dictionary<string, Keyword>();
            this.FullWordList = new HashSet<string>();
            this.MessageReader = new MessageReader();
            this.Result = new PredicationResult();
            this.WindowSize = windowSize;
        }

        public int WindowSize { get; set; }

        public int PredicationLength { get; set; }

        private List<Message> Messages { get; set; }

        private List<MessageWindow> MessageWindows { get; set; }

        private MessageReader MessageReader { get; set; }

        private Dictionary<string, Keyword> Keywords { get; set; }

        private PredicationResult Result { get; set; }

        private HashSet<string> FullWordList { get; set; }

        private List<MessageWindow> PredictionWindows { get; set; }


        public void ReadMessages(string file)
        {
            this.Messages = this.MessageReader.ReadMessages(file);
        }

        public List<MessageWindow> BreakIntoWindows()
        {
            int index = 0;
            MessageWindow messageWindow = null;
            foreach (var message in this.Messages)
            {
                if (index % WindowSize == 0)
                {
                    messageWindow = new MessageWindow
                    {
                        Size = WindowSize
                    };
                    this.MessageWindows.Add(messageWindow);
                }

                messageWindow.Messages.Add(message);
                index++;
            }

            return this.MessageWindows;
        }

        public PredicationResult Predict(int startIndex, int length)
        {
            var result = new PredicationResult();
            var prdtWindows = new List<MessageWindow>();
            for (int i = startIndex; i < startIndex + length; i++)
            {
                var window = this.MessageWindows[i];
                prdtWindows.Add(window);
            }

            this.PredictionWindows = prdtWindows;
            CalculateWordWeight(prdtWindows);
            CalculateRate();
            CalculateHotList();
            CalculateMessageRanking();

            return this.Result;
        }

        private void CalculateWordWindowCount(MessageWindow window)
        {
            foreach (var message in window.Messages)
            {
                foreach (var word in message.Words)
                {
                    if (!window.Keywords.ContainsKey(word))
                    {
                        window.Keywords[word] = new KeywordWindowInfo
                        {
                            Keyword = new Keyword
                            {
                                Value = word
                            },
                            Count = 1
                        };
                    }
                    else
                    {
                        var keywordState = window.Keywords[word];
                        keywordState.Count++;
                    }

                    if (!window.Keywords[word].Keyword.Messages.Contains(message.Id))
                    {
                        window.Keywords[word].Keyword.Messages.Add(message.Id);
                    }
                }
            }
        }


        private void CalculateWordWindowWeight(MessageWindow window)
        {
            var totalCount = window.GetTotalWordCount();

            foreach (var item in window.Keywords.Values)
            {
                item.Weight = (double)item.Count / (double)totalCount;
            }
        }

        private void CalculateWordWeight(List<MessageWindow> prdtWindows)
        {
            var wordList = this.FullWordList;
            foreach (var window in prdtWindows)
            {
                CalculateWordWindowCount(window);
                CalculateWordWindowWeight(window);
                var windowWords = window.Keywords.Select(k => k.Key);
                foreach (var word in windowWords)
                {
                    if (!wordList.Contains(word))
                    {
                        wordList.Add(word);
                    }
                }
            }

            foreach (var word in wordList)
            {
                var weightList = new List<double>();
                foreach (var window in prdtWindows)
                {
                    var totalCount = window.GetTotalWordCount();
                    if (!window.Keywords.ContainsKey(word))
                    {
                        weightList.Add(1d / (double)totalCount);
                    }
                    else
                    {
                        weightList.Add(window.Keywords[word].Weight);
                    }
                }

                this.Result.Weight[word] = weightList;
            }
        }

        private void CalculateRate()
        {
            var weightDict = this.Result.Weight;
            var wordList = this.FullWordList;
            var rateDict = this.Result.Rate;

            foreach (var word in wordList)
            {
                var list = new List<double>();
                var wordWeights = weightDict[word];
                for (int i = 0; i < wordWeights.Count; i++)
                {
                    var rate = 1d;
                    if (i > 0)
                    {
                        var sum = 0d;
                        for (int j = 0; j < i; j++)
                        {
                            sum += wordWeights[j];
                        }
                        var divide = sum / i;
                        rate = wordWeights[i] / divide;
                    }

                    list.Add(rate);
                }
                rateDict[word] = list;
            }
        }

        private void CalculateHotList()
        {
            var result = this.Result.Rate.Select(i => new { Key = i.Key, Value = i.Value.ElementAt(i.Value.Count - 1) }).OrderByDescending(j=> j.Value);
            foreach (var item in result)
            {
                this.Result.HotList.Add(item.Key, item.Value);
            }
        }

        private void CalculateMessageRanking()
        {
            foreach (var window in this.PredictionWindows)
            {
                foreach (var message in window.Messages)
                {
                    var sum = 0d;
                    foreach (var word in message.Words)
                    {
                        sum += this.Result.HotList[word];
                    }

                    var rank = sum / message.WordCount;
                    this.Result.MessageRanking.Add(message, rank);
                }
            }
        }

        

        public void WriteResult(string fileName)
        {
            var weights = this.Result.Weight;
            var rates = this.Result.Rate;

            var weightfile = fileName + "_weight.txt";
            var ratefile = fileName + "_rate.txt";
            var rankfile = fileName + "_rank.txt";
            var messagerankfile = fileName + "_msgrank.txt";

            using (StreamWriter sw = new StreamWriter(weightfile))
            {
                foreach (var line in weights)
                {
                    sw.WriteLine("{0}:{1}", line.Key, string.Join(",", line.Value.ToArray()));
                }
            }

            using (StreamWriter sw = new StreamWriter(ratefile))
            {
                foreach (var line in rates)
                {
                    sw.WriteLine("{0}:{1}", line.Key, string.Join(",", line.Value.ToArray()));
                }
            }

            using (StreamWriter sw = new StreamWriter(rankfile))
            {
                foreach (var line in this.Result.HotList)
                {
                    sw.WriteLine("{0}:{1}", line.Key, line.Value);
                }
            }

            var messageRanking = this.Result.MessageRanking.OrderByDescending(i => i.Value);

            using (StreamWriter sw = new StreamWriter(messagerankfile))
            {
                foreach (var line in messageRanking)
                {
                    sw.WriteLine("{0},{1},{2}", line.Key.Id, line.Value, line.Key.Text);
                }
            }
        }
    }

    public class MessageReader
    {
        Regex puncationRegx = new Regex(@"[。？！，、；：‘’“”（）〔〕【】「」『』_—,|…–．《》〈〉]");

        public List<Message> ReadMessages(string file)
        {
            var lines = File.ReadAllLines(file).Where(i=> i.Trim(new char[] { '\t', ' ', '\r', '\n'}).Length > 0).ToList();
            File.WriteAllLines(file, lines);
            var messages = lines.Select(MessageFromLine).ToList();
            return messages;
        }

        public Message MessageFromLine(string line)
        {
            Message msg = new Message();
            var properties = line.Split(',');
            msg.Id = properties[2];
            msg.Time = Convert.ToDateTime(properties[0]);
            var wordsarg = properties[1].Split('/');
            msg.Words = wordsarg.Select(i => puncationRegx.IsMatch(i) ? null : i).Where(i => i != null).ToList();
            msg.Text = line;
            return msg;
        }
    }

    public class Message
    {
        public string Id { get; set; }

        public string Text { get; set; }

        public DateTime Time { get; set; }

        public List<string> Words { get; set; }

        public int WordCount { get { return Words.Count; } }
    }

    public class MessageWindow
    {
        public MessageWindow()
        {
            this.Messages = new HashSet<Message>();
            this.Keywords = new Dictionary<string, KeywordWindowInfo>();
        }

        public int Id { get; set; }


        public int Size { get; set; }


        public HashSet<Message> Messages { get; private set; }

        public Dictionary<string, KeywordWindowInfo> Keywords { get; private set; }

        public int GetTotalWordCount()
        {
            return this.Keywords.Values.Sum(i => i.Count);
        }

    }

    public class KeywordWindowInfo
    {
        public Keyword Keyword { get; set; }

        public int Count { get; set; }

        public double Weight { get; set; }
    }

    public class Keyword
    {
        public Keyword()
        {
            this.Messages = new HashSet<string>();
        }

        public string Value
        {
            get; set;
        }

        public string ClusterVal
        {
            get; set;
        }

        public HashSet<string> Messages { get; private set; }
    }

    public class PredicationResult
    {
        private Dictionary<string, List<double>> weight = new Dictionary<string, List<double>>();

        private Dictionary<string, List<double>> rate = new Dictionary<string, List<double>>();

        private Dictionary<string, double> hotlist = new Dictionary<string, double>();

        private Dictionary<Message, double> messageRanking = new Dictionary<Message, double>();

        public int StartIndex { get; set; }

        public int SequenceLength { get; set; }

        public Dictionary<string, List<double>> Weight
        {
            get { return this.weight; }
        }

        public Dictionary<string, List<double>> Rate
        {
            get { return this.rate; }
        }

        public Dictionary<string, double> HotList
        {
            get { return this.hotlist; }
        }

        public Dictionary<Message, double> MessageRanking
        {
            get { return this.messageRanking; }
        }
    }


}
