using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace WindowsTests
{
    class Program
    {

        static string[] HelpMessages = new string[]
   {

            "PREDICT     \t PREDICT next word of inputing word",
            "MAKEXML    \t MAKEXML",
            "CLR     \t Clear console",
            "EXIT    \t Exit program"
   };

        [STAThread]
        static void Main(string[] args)
        {
            Program prg = new Program();
            prg.Run();
        }

        public void Run()
        {

            while (true)
            {
                Console.Write(">>> ");
                string read_raw = Console.ReadLine();
                string read = read_raw.ToLower();

                switch (read)
                {

                    case "predict":
                        PredictWord();
                        break;
                    case "makexml":
                        MakeXML();
                        break;
                    case "clr":
                        Console.Clear();
                        break;

                    case "help":
                        foreach (string line in HelpMessages)
                        {
                            Console.WriteLine(line);
                        }
                        break;
                    case "?":
                        foreach (string line in HelpMessages)
                        {
                            Console.WriteLine(line);
                        }
                        break;
                    case "exit":
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Unknown Command : \"{0}\"", read_raw);
                        break;
                }
            }
        }

        private DataSet GetData(string path)
        {
            DataSet temp = new DataSet();
            try
            {
                temp.ReadXml(path);
            } catch (Exception ex) {
                Console.Write("ERROR: " + ex.ToString());
            } 
            return temp;

        }

        private DataTable ConvertDataTable(DataRow[] dRows)

        {

            DataTable returnDataTable;

            if (dRows.Length > 0)
                returnDataTable = dRows[0].Table.Clone();
             else
                return null;

            foreach (DataRow dRow in dRows)

            {

                DataRow row = returnDataTable.NewRow();

                row.ItemArray = ((object[])dRow.ItemArray.Clone());

                returnDataTable.Rows.Add(row);

            }

            return returnDataTable;

        }

        static string ChoSungTbl = "ㄱㄲㄴㄷㄸㄹㅁㅂㅃㅅㅆㅇㅈㅉㅊㅋㅌㅍㅎ";
        static string JungSungTbl = "ㅏㅐㅑㅒㅓㅔㅕㅖㅗㅘㅙㅚㅛㅜㅝㅞㅟㅠㅡㅢㅣ";
        static string JongSungTbl = " ㄱㄲㄳㄴㄵㄶㄷㄹㄺㄻㄼㄽㄾㄿㅀㅁㅂㅄㅅㅆㅇㅈㅊㅋㅌㅍㅎ";
        static ushort UniCodeHangulBase = 0xAC00;
        static ushort UniCodeHangulLast = 0xD79F;

        private string DivideJaso(string str)
        {

            int FirstChar;

            int LastChar;

            int MiddleChar;

            string Result = "";

            for (int i = 0; i < str.Length; i++)
            {

                ushort Temp = Convert.ToUInt16(str[i]);

                if ((Temp < UniCodeHangulBase) || (Temp > UniCodeHangulLast))
                {
                    Result += str[i];
                }
                else
                {

                    LastChar = str[i] - UniCodeHangulBase;

                    FirstChar = LastChar / (21 * 28);

                    LastChar = LastChar % (21 * 28);

                    MiddleChar = LastChar / 28;

                    LastChar = LastChar % 28;

                    if (FirstChar >= 0)
                        Result += ChoSungTbl[FirstChar] + " ";

                    if (MiddleChar >= 0)
                        Result += JungSungTbl[MiddleChar] + " ";

                    if (LastChar != 0x0000 && LastChar >= 0)
                        Result += JongSungTbl[LastChar] + " ";

                }

            }

            return Result;

        }

        public int CheckOverlapDatarow(DataRow[] dRows, string ReadSentence)
        {
            int check = -1;

            for (int i = 0; i < dRows.Count(); i++)
            {
                if (dRows[i]["Code"].ToString() == DivideJaso(ReadSentence))
                {
                    check = i;
                }
            }
            return check;
        }
        public void PredictWord()
        {
            DataSet Sentence;
            DataTable ResultSentence;
            
            string DatasetPath = "";

            DataColumn dc1 = new DataColumn("Name", typeof(string));

            DataColumn dc2 = new DataColumn("Code", typeof(string));

            DataColumn dc3 = new DataColumn("Count", typeof(int));

            OpenFileDialog OFD = new OpenFileDialog();
            OFD.DefaultExt = ".xml";
            OFD.Filter = "XML File (*.xml)|*.xml";
            OFD.Title = "Select your sentence dataset";

            DialogResult OFDResult = OFD.ShowDialog();

            if (OFDResult == DialogResult.OK)
            {
                DatasetPath = OFD.FileName;
            }

            Sentence = GetData(DatasetPath);

            Console.Write("Sentences Dataset path is " + DatasetPath +"\n");

            ResultSentence = new DataTable();

            ResultSentence.Columns.Add(dc1);

            ResultSentence.Columns.Add(dc2);

            ResultSentence.Columns.Add(dc3);

            ResultSentence.ReadXml(DatasetPath);

            foreach (DataRow dRow in Sentence.Tables[0].Rows)

            {

                DataRow newRow = ResultSentence.NewRow();

                newRow["Name"] = dRow["Name"].ToString();

                newRow["Code"] = dRow["Code"].ToString();

                newRow["Count"] = dRow["Count"].ToString();

                ResultSentence.Rows.Add(newRow);

            }


            while (true)
            {

                string ReadSentence = Console.ReadLine();

                if (ReadSentence == "exit()")
                    break;
                else
                {
                   

                    DataRow[] dRows = ResultSentence.Select("Code LIKE '" + DivideJaso(ReadSentence.Trim()) + "' OR Code LIKE '" + DivideJaso(ReadSentence.Trim()) + "%'", "Count DESC");

                    DataTable dts = ConvertDataTable(dRows);

                    if(dts == null)
                    {
                        Console.Write("It doesn't exist! Do you want to add this word?\nT: True, F : False\n");
                        string check = Console.ReadLine().ToLower(); ;
                        if(check == "t")
                        {

                            Console.Write("Sentences Dataset path is " + DatasetPath + "\n");

                            DataRow temp = ResultSentence.NewRow();
                            temp["Name"] = ReadSentence;
                            temp["Code"] = DivideJaso(ReadSentence);
                            temp["Count"] = 0;
                            ResultSentence.Rows.Add(temp);
                            ResultSentence.TableName = "Data";
                            File.Delete(DatasetPath);
                            ResultSentence.WriteXml(DatasetPath);
                            ResultSentence.Reset();
                            ResultSentence.Columns.Add(dc1);
                            ResultSentence.Columns.Add(dc2);
                            ResultSentence.Columns.Add(dc3);
                            Sentence.Reset();
                            Sentence = GetData(DatasetPath);

                            foreach (DataRow dRow in Sentence.Tables[0].Rows)

                            {

                                string s = DivideJaso(dRow["Name"].ToString());

                                DataRow newRow = ResultSentence.NewRow();

                                newRow["Name"] = dRow["Name"].ToString();

                                newRow["Code"] = dRow["Code"].ToString();

                                newRow["Count"] = dRow["Count"].ToString();

                                ResultSentence.Rows.Add(newRow);

                            }
                            Console.Write(ReadSentence + " has added in database! (" + DatasetPath + ")\n");
                        } else {

                        }

                    } else {
                        int checker = CheckOverlapDatarow(dRows, ReadSentence);
                        if (checker != -1)
                        {
                            dRows[checker]["Count"] = Int32.Parse(dRows[checker]["Count"].ToString()) + 1;
                        }

                        if (dts.Rows.Count > 6)
                        {
                            for (int j = 0; j < 6; j++)
                            {
                                Console.Write(j + 1 + ". " + dts.Rows[j]["Name"] + " \t" + dts.Rows[j]["Count"] + "\n");
                            }
                        } else {

                            for (int j = 0; j < dts.Rows.Count; j++)
                            {
                                Console.Write(j + 1 + ". " + dts.Rows[j]["Name"] + " \t" + dts.Rows[j]["Count"] + "\n");
                            }
                        }
                        ResultSentence.TableName = "Data";
                        ResultSentence.WriteXml(DatasetPath);
                        ResultSentence.Reset();
                        ResultSentence.Columns.Add(dc1);
                        ResultSentence.Columns.Add(dc2);
                        ResultSentence.Columns.Add(dc3);
                        Sentence.Reset();
                        Sentence = GetData(DatasetPath);

                        foreach (DataRow dRow in Sentence.Tables[0].Rows)

                        {

                            string s = DivideJaso(dRow["Name"].ToString());

                            DataRow newRow = ResultSentence.NewRow();

                            newRow["Name"] = dRow["Name"].ToString();

                            newRow["Code"] = dRow["Code"].ToString();

                            newRow["Count"] = dRow["Count"].ToString();

                            ResultSentence.Rows.Add(newRow);

                        }
                    }
                }

            }
        }

        public void MakeXML()
        {
            DataSet Sentence;
            DataTable ResultSentence;

            string DatasetPath = "";

            OpenFileDialog OFD = new OpenFileDialog();
            OFD.DefaultExt = ".xml";
            OFD.Filter = "XML File (*.xml)|*.xml";
            OFD.Title = "Select your sentence dataset";

            DialogResult OFDResult = OFD.ShowDialog();

            if (OFDResult == DialogResult.OK)
            {
                DatasetPath = OFD.FileName;
            }

            Sentence = GetData(DatasetPath);

            Console.Write("Sentences Dataset path is " + DatasetPath + "\n");

            ResultSentence = new DataTable();

            ResultSentence.TableName = "Data";

            DataColumn dc1 = new DataColumn("Name", typeof(string));

            DataColumn dc2 = new DataColumn("Code", typeof(string));

            DataColumn dc3 = new DataColumn("Count", typeof(int));

            ResultSentence.Columns.Add(dc1);

            ResultSentence.Columns.Add(dc2);

            ResultSentence.Columns.Add(dc3);

            foreach (DataRow dRow in Sentence.Tables[0].Rows)

            {

                string s = DivideJaso(dRow["Name"].ToString());

                DataRow newRow = ResultSentence.NewRow();

                newRow["Name"] = dRow["Name"].ToString();

                newRow["Code"] = s;

                newRow["Count"] = 0;

                ResultSentence.Rows.Add(newRow);

            }

            ResultSentence.WriteXml(DatasetPath);

        }

        //TFSession sess;
        //TFGraph graph;
        //char[] chars = { ' ', 'e', 't', 'a', 'o', 'n', 'h', 'i', 's', 'r', 'd', 'l', 'u', '\n', '\r', 'w', 'm', 'c', 'f', 'y', 'g', ',', 'p', 'b', '.', 'v', 'k', 'I', '-', '’', 'T', ';', '‘', '“', '”', 'H', 'A', 'M', '_', '"', 'S', 'W', '!', 'x', 'B', 'D', 'E', 'L', '?', 'C', 'j', 'q', '\'', ':', 'N', 'P', 'G', 'O', 'z', 'Y', 'R', 'F', 'J', '*', 'V', 'U', '1', ')', '(', 'K', 'X', '2', 'Q', '3', '0', '5', '&', '4', '/', '8', '9', '6', '7', 'é', '[', ']', 'æ', 'á', 'ī', 'è', 'ο', 'í', 'Z', 'ó', 'ĕ', '}', 'ĭ', '$', '{', '@', '=', 'ö', 'œ', 'λ', '#', 'ē', 'ŭ', 'ā', '>', 'ε', 'κ', 'ς', 'à', 'ρ', 'α', 'ü', 'ô', 'ά', 'π', 'ν', '%', 'σ', 'δ', 'â', 'ú', '+', 'υ', 'ō', 'ŏ', 'ê', 'ū', 'ë', 'τ', 'ι', 'γ', 'ξ', '£', 'þ', 'μ', 'Æ', 'ç', 'ï', 'ý', 'ό', 'ἀ', 'ί', 'β', 'É', 'Œ', 'η', 'Þ', 'ω', 'ῆ', 'φ', 'ἐ', 'ἔ', 'ή', 'ñ', 'ῦ', 'ὶ', 'ῖ', 'ǽ', 'ἄ', 'Β', 'ἑ', 'θ', 'χ', 'ă', 'û' };

        //dynamic vocab = JsonConvert.DeserializeObject("{'ῦ': 158, ',': 21, '-': 28, 'ū': 130, 'ο': 90, 'i': 7, 'λ': 103, '1': 66, 'ἄ': 162, 'η': 149, '’': 29, 'þ': 137, 'æ': 86, 'ô': 116, 'τ': 132, 'l': 11, '?': 48, 'x': 43, 'ç': 140, ':': 53, 'J': 62, 'D': 45, 'ὶ': 159, 'ĕ': 94, 'X': 70, '$': 97, '#': 104, 'c': 17, 'H': 35, 'j': 50, ';': 31, 'ō': 127, 'χ': 166, 'ν': 119, 'ἀ': 144, '!': 42, 'œ': 102, 'E': 46, '2': 71, '0': 74, 'ê': 129, 'y': 19, 'm': 16, 'u': 12, 'ἔ': 155, 'p': 22, '[': 84, 'B': 44, 'ι': 133, 'a': 3, 'β': 146, 'ἐ': 154, 't': 2, 'k': 26, 'w': 15, 'ῖ': 160, '>': 108, 'ῆ': 152, 'F': 61, 'ŏ': 128, '9': 80, 'ŭ': 106, '“': 33, 'ǽ': 161, 'ī': 88, 'T': 30, '6': 81, 'Œ': 148, 'φ': 153, 'υ': 126, 'ë': 131, 'z': 58, 'b': 23, 'Z': 92, '%': 120, 'Y': 59, 'É': 147, 'Β': 163, 'ε': 109, 'q': 51, 'ρ': 113, '_': 38, 'ú': 124, 'ö': 101, 'Æ': 139, 'n': 5, '7': 82, 'W': 41, 'L': 47, 'û': 168, 'd': 10, 'π': 118, 'θ': 165, 'ē': 105, 'â': 123, ' / ': 78, 'μ': 138, 'ξ': 135, 'γ': 134, '8': 79, 'G': 56, ' + ': 125, 'K': 69, '£': 136, 'í': 91, 'r': 9, 'ñ': 157, 'ï': 141, ' = ': 100, 'ā': 107, 'O': 57, 'ă': 167, '{': 98, 'N': 54, ', ': 21, ' ': 0, 'e': 1, 'δ': 122, '\r': 14, 'Þ': 150, 'ĭ': 96, ' & ': 76, 'I': 27, 'C': 49, 'ü': 115, 'ἑ': 164, '5': 75, 'Q': 72, 'P': 55, ']': 85, 'ω': 151, 'à': 112, 'M': 37, 'κ': 110, 'g': 20, 'ό': 143, 'v': 25, 'R': 60, 'U': 65, '.': 24, '@': 99, '*': 63, ')': 67, '‘': 32, 'ý': 142, '\n': 13, 'f': 18, 'ó': 93, 'ά': 117, 'S': 40, 'ς': 111, 'α': 114, 'è': 89, 'h': 6, 'ή': 156, 'A': 36, 'V': 64, '(': 68, '4': 77, '\"': 39, 'o': 4, 's': 8, '3': 73, '”': 34, 'ί': 145, 'é': 83, '}': 95, 'á': 87, 'σ': 121}");

        //public void PredictWord()
        //{
        //    OpenFileDialog OFD = new OpenFileDialog();
        //    OFD.DefaultExt = ".pb";
        //    OFD.Filter = "PB File (*.pb)|*.pb";
        //    OFD.Title = "Select your model";

        //    DialogResult ofdresult = OFD.ShowDialog();

        //    string modelpath = "";
        //    byte[] model;
        //    graph = new TFGraph();

        //    if (ofdresult == DialogResult.OK)
        //    {

        //        var input = Console.ReadLine();

        //        TensorFlowSharp.Windows.NativeBinding.Init();

        //        Numpy np = new Numpy();

        //        modelpath = OFD.FileName;

        //        model = File.ReadAllBytes(modelpath);

        //        graph.Import(model, "");


        //        using (sess = new TFSession(graph))
        //        {

        //            Console.WriteLine("model start");

        //            string result = sample(input, 100);

        //            Console.WriteLine(result);

        //        }

        //    }

        //}

        //public string sample(string prime, int num)
        //{



        //    var inputs = new TFOutput[] {
        //                new TFOutput (graph["Placeholder"], 0),
        //                 new TFOutput (graph["Placeholder"], 0)
        //            };
        //    var input_values = new TFTensor[] {
        //                0, 1
        //            };
        //    var add_output = new TFOutput(graph["output"], 0);
        //    var state_output = new TFOutput(graph["concat"], 0);
        //    var outputs = new TFOutput[] {
        //                add_output,
        //                state_output
        //            };

        //    var output = sess.Run(runOptions: null,
        //                               inputs: inputs,
        //                              inputValues: input_values,
        //                              outputs: outputs,
        //                              targetOpers: null,
        //                              runMetadata: null,
        //                             status: null);

        //    var result = output[0].GetValue();

        //    int[,] x = new int[1, 1];




        //    for (int i = 0; i < prime.Length - 1; i++)
        //    {

        //        x[0, 0] = vocab[prime[i].ToString()];


        //         input_values = new TFTensor[] {
        //                x, output[0]
        //            };

        //        output= sess.Run(runOptions: null,
        //                               inputs: inputs,
        //                              inputValues: input_values,
        //                              outputs: outputs,
        //                              targetOpers: null,
        //                              runMetadata: null,
        //                             status: null);

        //        float[,] realresult = (float[,])output[0].GetValue();


        //    }

        //    char ch = prime[prime.Length - 1];

        //    for (int n = 0; n < num; n++)
        //    {


        //        x[0, 0] = vocab[ch.ToString()];

        //        input_values = new TFTensor[] { x, output[0]};

        //        var state2 = sess.GetRunner().AddInput(graph["Placeholder"][0], input_values).Fetch(graph["output"][0]).Fetch(graph["concat"][0]);


        //        output = sess.Run(runOptions: null,
        //                               inputs: inputs,
        //                              inputValues: input_values,
        //                              outputs: outputs,
        //                              targetOpers: null,
        //                              runMetadata: null,
        //                             status: null);

        //        float[,] realoutput = (float[,])output[0].GetValue();

        //        float[] weight = new float[169];

        //        for (int i = 0; i < 169; i++)
        //        {
        //            weight[i] = realoutput[0, i];
        //        }

        //        int sample = weighted_pick(weight);

        //         string pred = chars[sample].ToString();

        //        ch = pred[0];

        //        Console.Write(pred);

        //    }

        //    return "";
        //}

        //public int weighted_pick(float[] weight)
        //{
        //    Numpy np = new Numpy();
        //    Random r = new Random();

        //    float[] t = np.cumsum(weight);
        //    double s = weight.Sum();
        //    double rand = r.NextDouble();

        //    return np.searchsorted(t, s * rand);
        //}


    }
}