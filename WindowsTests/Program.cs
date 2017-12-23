using Newtonsoft.Json;
using Predict;
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
            "PREDICT \t PREDICT next word of inputing word",
            "MAKEXML \t MAKEXML",
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
            }
            catch (Exception ex)
            {
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

        public int CheckOverlapDatarow(DataRow[] dRows, string ReadSentence)
        {
            int check = -1;

            for (int i = 0; i < dRows.Count(); i++)
            {
                if (dRows[i]["Code"].ToString() == KoreanHelper.DivideJaso(ReadSentence))
                {
                    check = i;
                }
            }
            return check;
        }

        string CenterString(string str, int length, char pad = ' ')
        {
            int left, right;
            int strLen = str.Length;
            foreach (var item in str)
            {
                if (item >= 256)
                    strLen++;
            }
            left = right = (int)Math.Max(0, Math.Floor((float)(length - strLen) / 2));
            right += Math.Max(0, length - strLen - left - right);
            string ret = str;
            for (int i = 0; i < left; i++)
            {
                ret = pad + ret;
            }
            for (int i = 0; i < right; i++)
            {
                ret = ret + pad;
            }
            return ret;
        }

        public void PredictWord()
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                DefaultExt = ".xml",
                Filter = "XML File (*.xml)|*.xml",
                Title = "Select your sentence dataset"
            };
            ofd.ShowDialog();

            var corrector = new WordCorrecter(ofd.FileName);

            while (true)
            {
                Console.Write("Input> ");
                var read = Console.ReadLine().Trim();
                if (read.Length > 0)
                {
                    var result = corrector.Correcting(read);
                    if (result != null)
                    {
                        foreach (var item in result)
                        {
                            Console.WriteLine($"{item.Index.ToString().PadRight(5)} | {CenterString(item.Name, 30)} | {item.UsedCount}");
                        }

                        var message = $"Select suggestion [0~{result.Length - 1}";
                        bool contain = corrector.Contains(read);
                        if (!contain)
                            message += " -1 to Add";
                        message += "]> ";
                        Console.Write(message);
                        try
                        {
                            var indRead = Console.ReadLine().Trim();
                            var ind = Convert.ToInt32(indRead);

                            if (ind == -1)
                            {
                                corrector.Used(read);
                            }
                            else
                            {
                                Console.WriteLine("Corrected to " + result[ind].Name);
                                corrector.Used(result[ind].Name);
                            }
                        }
                        catch (FormatException) { }
                        catch (IndexOutOfRangeException) { }
                    }
                    else
                    {
                        Console.Write("Add new? [Y/n] ");
                        bool add = true;
                        var addRead = Console.ReadLine();

                        if (addRead != null && addRead != "")
                        {
                            add = false;
                            if (addRead.ToLower() == "y")
                                add = true;
                        }

                        if (add)
                            corrector.Used(read);
                    }
                }
            }
        }

        public void MakeXML()
        {
            DataSet Sentence;
            DataTable ResultSentence;

            string DatasetPath = "";

            OpenFileDialog ofd = new OpenFileDialog
            {
                DefaultExt = ".xml",
                Filter = "XML File (*.xml)|*.xml",
                Title = "Select your sentence dataset"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                DatasetPath = ofd.FileName;
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
                string s = KoreanHelper.DivideJaso(dRow["Name"].ToString());

                DataRow newRow = ResultSentence.NewRow();

                newRow["Name"] = dRow["Name"].ToString();
                newRow["Code"] = s;
                newRow["Count"] = 0;

                ResultSentence.Rows.Add(newRow);
            }

            ResultSentence.WriteXml(DatasetPath);
        }
    }
}