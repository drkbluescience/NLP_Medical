using ExcelDataReader;
using net.zemberek.erisim;
using net.zemberek.tr.yapi;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Processes
{
    class Processes
    {

        List<string> report = new List<string>();
        List<string> zemberek_dict = new List<string>();
        List<string> stopwords = new List<string>();
        List<string> roots_All = new List<string>();
        List<string> medicineWords = new List<string>();
        static List<string> dictionary_Turkish = new List<string>();
        //List<string> last = new List<string>();
        List<Form1.Datas> allData_oyku = new List<Form1.Datas>();
        List<string> features = new List<string>();

        List<Form1.Difference> differences = new List<Form1.Difference>();
        List<Form1.Oykus> oykus = new List<Form1.Oykus>();
        public List<Form1.Oykus> oykus_found = new List<Form1.Oykus>();
        List<string> rest_data = new List<string>();
        List<string> rest_data_tip = new List<string>();
        List<string> rest_data_no = new List<string>();
        List<Form1.Oykus> original_oyku = new List<Form1.Oykus>();
        string[] points = new string[] { ".", "!", "?", ",", "'", "-", "~", "[", "]", ":", 
            ";", "(", ")", "/", "&", "+", "*", "=", "<", ">", "%", "½", "_", "#", "^" };
        string path = "Data Source = DESKTOP-QIO9SE6; Initial Catalog = Gastro; Integrated Security = SSPI";

        public void CompoundTexts()
        {
            string[] file1 = File.ReadAllLines(@"datafile.txt", Encoding.GetEncoding("iso-8859-9"));
            string[] file2 = File.ReadAllLines(@"datafileMed.txt", Encoding.GetEncoding("iso-8859-9"));

            string path = @"AllData.txt";
            if (File.Exists(path))
            {
                for (int i = 0; i < file1.Length; i++)
                {
                    string datas = file1[i].ToUpper(new CultureInfo("tr-TR", false));
                    using (StreamWriter sr = File.AppendText(path))
                    {
                        sr.WriteLine(datas);
                        sr.Close();
                    }
                }
                for (int i = 0; i < file2.Length; i++)
                {
                    string datas = file2[i].ToUpper(new CultureInfo("tr-TR", false));
                    using (StreamWriter sr = File.AppendText(path))
                    {
                        sr.WriteLine(datas);
                        sr.Close();
                    }
                }
            }
        }
        public void DeleteDataB()
        {
            SqlConnection con;
            SqlCommand cmd;
            con = new SqlConnection(path);

            for (int i = 13942; i < 14080; i++)
            {
                string delete = "DELETE FROM dbo.Oyku";
                cmd = new SqlCommand(delete, con);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
                cmd.Parameters.Clear();
            }
        }
        public void DataSetRead()
        {
            StreamReader sw = new StreamReader(String.Format(@"dataset.txt", System.Windows.Forms.Application.StartupPath), Encoding.GetEncoding("iso-8859-9"), false);
            string word = sw.ReadLine();

            while (word != null)
            {
                word = word.ToLower();
                features.Add(word);
                word = sw.ReadLine();
            }

            sw.Close();
        }
        public void DataSetAdd(string data_)
        {
            string path = @"parsedDataset.txt";
            if (File.Exists(path))
            {
                using (StreamWriter sr = File.AppendText(path))
                {
                    sr.WriteLine(data_);
                    sr.Close();
                }
            }
        }
        private void KNNAlg(List<Form1.Datas> allData_set, string P_no)
        {
            int sum_extract = 0;
            for (int p = 0; p < features.Count; p++)
            {
                for (int i = 0; i < allData_oyku.Count; i++)
                {
                    if (allData_oyku[i].Name.Equals(features[p]))
                    {
                        for (int j = 0; j < allData_set.Count; j++)
                        {
                            if (allData_set[j].Name.Equals(features[p]))
                            {
                                int extract = Math.Abs(allData_set[j].Count - allData_oyku[i].Count);
                                sum_extract += extract;
                            }
                        }
                    }
                }
            }
            differences.Add(new Form1.Difference { pNo = P_no, difference = sum_extract });
        }
        public void FunctionOykuNew(List<string> oyku)
        {
            //read features
            DataSetRead();

            FileZemberek();

            FileStopWords();

            ReadDataBase_x();

            //ReadDataBase_();
           
              string oyku_new = String.Empty;
            for (int i = 0; i < oyku.Count; i++)
            {
                oyku_new += oyku[i] + " ";
            }

            List<string> number_oyku = ParseData_x_newOyku(oyku_new);
            for (int i = 0; i < number_oyku.Count; i++)
            {
                allData_oyku.Add(new Form1.Datas { Name = number_oyku[i].Split(' ')[0], Count = Convert.ToInt32(number_oyku[i].Split(' ')[1]) });
            }
            Read_DataBase();
             
        }
        public void FileDictionary()
        {

            string[] file_dictionary = File.ReadAllLines(@"sozluk.txt", Encoding.GetEncoding("iso-8859-9"));
            for (int i = 0; i < file_dictionary.Length; i++)
            {
                string datas = file_dictionary[i].ToUpper(new CultureInfo("tr-TR", false));
                dictionary_Turkish.Add(datas);
            }

        }
        public void FileZemberek()
        {
            StreamReader sw = new StreamReader(String.Format(@"zemb.txt", System.Windows.Forms.Application.StartupPath), Encoding.GetEncoding("iso-8859-9"), false);
            string word = sw.ReadLine();
            while (word != null)
            {
                word = word.ToLower();
                dictionary_Turkish.Add(word);
                word = sw.ReadLine();
            }

            sw.Close();
        }
        public void FileStopWords()
        {
            StreamReader sw = new StreamReader(String.Format(@"stopWords.txt", System.Windows.Forms.Application.StartupPath), Encoding.GetEncoding("iso-8859-9"), false);
            string word = sw.ReadLine();
            while (word != null)
            {
                word = word.ToLower();
                stopwords.Add(word);
                word = sw.ReadLine();
            }

            sw.Close();
        }
        public void ReadDataBase_x()
        {
            //string All_oyku = string.Empty;

            SqlDataReader dataReader;
            SqlCommand cmd;
            SqlConnection connection = new SqlConnection(path);

            string comment = "SELECT * FROM dbo.Oyku";
            cmd = new SqlCommand(comment, connection);
            connection.Open();
            cmd.ExecuteNonQuery();
            dataReader = cmd.ExecuteReader();

            if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {
                    //All_oyku += dataReader.GetValue(0).ToString();
                    original_oyku.Add(new Form1.Oykus { Oyku = dataReader.GetValue(0).ToString(), PNo = dataReader.GetValue(2).ToString(), Tip_Adi = dataReader.GetValue(1).ToString() });
                }
            }
            else
            {
                /////
            }
            dataReader.Close();
            cmd.Dispose();
            connection.Close();

            //ParseData_x(All_oyku);
        }
        public void Read_DataBase()
        {
            SqlDataReader dataReader;
            SqlCommand cmd;
            SqlConnection connection = new SqlConnection(path);

            string comment = "SELECT * FROM dbo.OykuSet";
            cmd = new SqlCommand(comment, connection);
            connection.Open();
            cmd.ExecuteNonQuery();
            dataReader = cmd.ExecuteReader();

            if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {
                    oykus.Add(new Form1.Oykus { Oyku = dataReader.GetValue(2).ToString(), Tip_Adi = dataReader.GetValue(1).ToString(), PNo = dataReader.GetValue(0).ToString() });
                }
            }
            else
            {
                /// Error
            }
            dataReader.Close();
            cmd.Dispose();
            connection.Close();

            for (int i = 0; i < oykus.Count; i++)
            {
                List<Form1.Datas> allData_set = new List<Form1.Datas>();
                string[] vs = oykus[i].Oyku.Split('/');
                for (int j = 0; j < vs.Length; j++)
                {
                    allData_set.Add(new Form1.Datas { Name = vs[j].Split(' ')[0], Count = Convert.ToInt32(vs[j].Split(' ')[1]) });
                }
                KNNAlg(allData_set, oykus[i].PNo);
            }
            CompareDiff();
        }
        public void ReadDataBase_()
        {
            for (int i = 330; i < 350; i++)
            {
                List<string> parsed = ParseData_x(original_oyku[i].Oyku);
                List<Form1.Datas> allData_set = new List<Form1.Datas>();
                string input_ = String.Empty;
                for (int j = 0; j < parsed.Count; j++)
                {
                    input_ += parsed[j].Split(' ')[0] + " " + parsed[j].Split(' ')[1];
                    if (j != parsed.Count - 1)
                    {
                        input_ += "/";
                    }
                    //allData_set.Add(new Form1.Datas { Name = parsed[j].Split(' ')[0], Count = Convert.ToInt32(parsed[j].Split(' ')[1]) });
                }
                rest_data.Add(input_);
                rest_data_tip.Add(original_oyku[i].Tip_Adi);
                rest_data_no.Add(original_oyku[i].PNo);

                //KNNAlg(allData_set, oykus[i].PNo);////go on
            }
            InsertData_();
            //CompareDiff();
        }
        public void InsertData_()
        {
            SqlConnection connection = new SqlConnection(path);
            SqlCommand cmd;
            try
            {
                connection.Open();
                //MessageBox.Show("Connection Opened ! ");
                connection.Close();
            }
            catch (Exception)
            {
            }
            try
            {
                for (int i = 0; i < rest_data.Count; i++)
                {

                    string comment = "INSERT INTO dbo.OykuSet(TipAdi,PNo, Oyku_set) VALUES(@tipAdi,@pNo,@oykuset)";

                    cmd = new SqlCommand(comment, connection);

                    cmd.Parameters.AddWithValue("@tipAdi", rest_data_tip[i]);

                    cmd.Parameters.AddWithValue("@pNo", rest_data_no[i]);

                    cmd.Parameters.AddWithValue("@oykuset", rest_data[i]);

                    connection.Open();
                    cmd.ExecuteNonQuery();
                    connection.Close();
                    cmd.Parameters.Clear();

                }

            }
            catch (Exception)
            {
            }
        }
        public void CompareDiff()
        {
            List<int> all_diff = new List<int>();

            for (int i = 0; i < differences.Count; i++)
            {
                all_diff.Add(differences[i].difference);
            }

            int[] diff_array = all_diff.ToArray();
            Array.Sort(diff_array);
            int count_element = 0;

            for (int i = 0; i < diff_array.Length; i++)
            {
                for (int j = 0; j < differences.Count; j++)
                {
                    if (differences[j].difference == diff_array[i])
                    {
                        int index = oykus.FindIndex(p => p.PNo == differences[j].pNo);
                        int index_ = original_oyku.FindIndex(p => p.PNo == differences[j].pNo);
                        oykus_found.Add(new Form1.Oykus { PNo = differences[j].pNo, Oyku = original_oyku[index_].Oyku, Tip_Adi = oykus[index].Tip_Adi });
                        count_element++;
                    }
                    if (count_element == 3)
                    {
                        break;
                    }
                }
                if (count_element == 3)
                {
                    break;
                }
            }
        }
        public List<string> ParseData_x(string oykus)
        {
            List<string> roots_Alls = new List<string>();
            List<string>  oykuParsed_set = ParseDictionary(oykus);
            List<string> fixed_oyku = FixedWords(oykuParsed_set);
            for (int i = 0; i < fixed_oyku.Count; i++)
            {
                if (!stopwords.Contains(fixed_oyku[i]))
                {
                    string root = RootofWords(fixed_oyku[i]);
                    roots_Alls.Add(root);
                }
            }
            List<string> number_oyku = StringNum(roots_Alls.ToArray());
            List<string> vad_values = ExcelRead(roots_Alls);
            for (int i = 0; i < vad_values.Count; i++)
            {
                number_oyku.Add(vad_values[i]);
            }
            return number_oyku;
        }
        public List<string> ParseData_x_newOyku(string oyku)
        {
            List<string> roots_Alls = new List<string>();
            List<string>  oykuParsed = ParseDictionary(oyku);
            List<string> fixed_oyku = FixedWords(oykuParsed);
            for (int i = 0; i < fixed_oyku.Count; i++)
            {
                if (!stopwords.Contains(fixed_oyku[i]))
                {
                    string root = RootofWords(fixed_oyku[i]);
                    roots_Alls.Add(root);
                }
            }
            List<string> number_oyku = StringNum(roots_Alls.ToArray());
            List<string> vad_values = ExcelRead(roots_Alls);
            for (int i = 0; i < vad_values.Count; i++)
            {
                number_oyku.Add(vad_values[i]);
            }
            //WriteFile(number_oyku);
            return number_oyku;
        }

        // Oyku to dataset for database
        public void DataSetDatabase()
        {
            string[] file_oyku = File.ReadAllLines(@"Oyku.csv", Encoding.GetEncoding("iso-8859-9"));

            List<string> p_number = new List<string>();
            List<string> tipAdi = new List<string>();

            string oykus = string.Join(" ", file_oyku);
            List<string> oykuParsed = ParseDictionary(oykus);
            List<string> vals = new List<string>();
            List<string> oykusAll = new List<string>();
            string oys = "";
            for (int i = 4; i < oykuParsed.Count; i++)
            {
                if (oykuParsed[i].All(char.IsDigit) && oykuParsed[i].Length > 4)
                {
                    if (!p_number.Contains(oykuParsed[i]))
                    {
                        p_number.Add(oykuParsed[i]);
                        tipAdi.Add(oykuParsed[i - 1]);
                        vals.Add(oys);
                        oys = String.Empty;
                    }
                    else
                    {
                        oys = String.Empty;
                    }
                }
                else
                {
                    oys += oykuParsed[i] + " ";
                }
            }
            //InsertData(p_number, vals, tipAdi);
        }
        public void InsertData(List<string> p_number, List<string> vals, List<string> tipAdi)
        {
            SqlConnection connection = new SqlConnection(path);
            SqlCommand cmd;
            try
            {
                connection.Open();
                //MessageBox.Show("Connection Opened ! ");
                connection.Close();
            }
            catch (Exception)
            {
            }
            try
            {
                for (int i = 0; i < p_number.Count; i++)
                {

                    string comment = "INSERT INTO dbo.Oyku(Oyku,TipAdi,PNo) VALUES(@oyku,@tipAdi,@pNo)";

                    cmd = new SqlCommand(comment, connection);

                    cmd.Parameters.AddWithValue("@oyku", vals[i]);

                    cmd.Parameters.AddWithValue("@tipAdi", tipAdi[i]);

                    cmd.Parameters.AddWithValue("@pNo", p_number[i]);

                    connection.Open();
                    cmd.ExecuteNonQuery();
                    connection.Close();
                    cmd.Parameters.Clear();

                }

            }
            catch (Exception)
            {
            }
        }

        // Recursive Function for parsing
        public List<string> ParseAgain(List<string> values, int count)
        {
            List<string> vs = new List<string>();
            int size_values = values.Count;
            int count_ = 0;

            if (count != -1)
            {
                for (int j = 0; j < values.Count; j++)
                {
                    int control = 0;
                    for (int i = 0; i < points.Length; i++)
                    {
                        if (values[j].Contains(points[i]))
                        {
                            char[] char_string = points[i].ToCharArray();
                            string[] parse_value = values[j].Split(char_string[0]);
                            for (int k = 0; k < parse_value.Length; k++)
                            {
                                if (!String.IsNullOrEmpty(parse_value[k]))
                                {
                                    vs.Add(parse_value[k]);
                                }
                            }
                            control++;
                            break;
                        }
                    }
                    if (control == 0)
                    {
                        if (!String.IsNullOrEmpty(values[j]))
                        {
                            vs.Add(values[j]);
                        }
                    }
                }
                if (size_values == vs.Count)
                {
                    count_ = -1;
                }
                return ParseAgain(vs, count_);
            }
            else
            {
                return values;
            }
        }
        public List<string> ParseDictionaryNewOyku(List<string> splitDictionary)
        {
            List<string> dictionary = new List<string>();

            for (int i = 0; i < splitDictionary.Count; i++)
            {
                string value = splitDictionary[i];
                value = value.Trim();
                value = value.Replace("\n", "");
                int control = 0;

                for (int j = 0; j < points.Length; j++)
                {
                    if (value.Contains(points[j]))
                    {
                        List<string> values = new List<string>();
                        values.Add(value);
                        int count = 0;

                        string[] parse_value = ParseAgain(values, count).ToArray();
                        for (int t = 0; t < parse_value.Length; t++)
                        {
                            if (!string.IsNullOrEmpty(parse_value[t]))
                            {
                                if (parse_value[t].Any(char.IsDigit))
                                {
                                    count++;
                                }
                                else
                                {
                                    int control_ = 0;
                                    for (int p = 0; p < points.Length; p++)
                                    {
                                        if (parse_value[t].Contains(points[p]))
                                        {
                                            control_++;
                                        }
                                    }
                                    if (control_ == 0)
                                    {
                                        dictionary.Add(TrimPunctuation(parse_value[t]));
                                        count++;
                                    }
                                }

                            }
                        }
                        control++;
                        if (count == parse_value.Length)
                        {
                            break;
                        }
                    }
                }
                if (control == 0)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        if (!value.Any(char.IsDigit))
                        {
                            dictionary.Add(TrimPunctuation(value));
                        }
                    }
                }
            }
            return dictionary;
        }
        public List<string> ParseDictionary(string dict)
        {
            List<string> dictionary = new List<string>();
            string[] splitDictionary = dict.Split(' ');
            for (int i = 0; i < splitDictionary.Length; i++)
            {
                string value = splitDictionary[i];
                value = value.Trim();
                value = value.Replace("\n", "");
                int control = 0;

                for (int j = 0; j < points.Length; j++)
                {
                    if (value.Contains(points[j]))
                    {
                        List<string> values = new List<string>();
                        values.Add(value);
                        int count = 0;

                        string[] parse_value = ParseAgain(values, count).ToArray();
                        for (int t = 0; t < parse_value.Length; t++)
                        {
                            if (!string.IsNullOrEmpty(parse_value[t]))
                            {
                                if (parse_value[t].Any(char.IsDigit))
                                {
                                    count++;
                                }
                                else
                                {
                                    int control_ = 0;
                                    for (int p = 0; p < points.Length; p++)
                                    {
                                        if (parse_value[t].Contains(points[p]))
                                        {
                                            control_++;
                                        }
                                    }
                                    if (control_ == 0)
                                    {
                                        dictionary.Add(TrimPunctuation(parse_value[t]));
                                        count++;
                                    }
                                }

                            }
                        }
                        control++;
                        if (count == parse_value.Length)
                        {
                            break;
                        }
                    }
                }
                if (control == 0)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        if (!value.Any(char.IsDigit))
                        {
                            dictionary.Add(TrimPunctuation(value));
                        }
                    }
                }
            }
            return dictionary;
        }
        static string TrimPunctuation(string value)
        {
            // Count start punctuation.
            int removeFromStart = 0;
            for (int i = 0; i < value.Length; i++)
            {
                if (char.IsPunctuation(value[i]))
                {
                    removeFromStart++;
                }
                else
                {
                    break;
                }
            }

            // Count end punctuation.
            int removeFromEnd = 0;
            for (int i = value.Length - 1; i >= 0; i--)
            {
                if (char.IsPunctuation(value[i]))
                {
                    removeFromEnd++;
                }
                else
                {
                    break;
                }
            }
            // No characters were punctuation.
            if (removeFromStart == 0 &&
                removeFromEnd == 0)
            {
                return value;
            }
            // All characters were punctuation.
            if (removeFromStart == value.Length &&
                removeFromEnd == value.Length)
            {
                return "";
            }
            // Substring.
            return value.Substring(removeFromStart,
                value.Length - removeFromEnd - removeFromStart);
        }

        public void DataAdd(string data)
        {
            string path = @"datafile.txt";
            if (File.Exists(path))
            {
                using (StreamWriter sr = File.AppendText(path))
                {
                    sr.WriteLine(data);
                    sr.Close();
                }
            }
        }

        public void FileDataMed()
        {
            string datafile = @"datafileMed.txt";
            try
            {
                if (!File.Exists(datafile))
                {
                    FileStream fs = File.Create(datafile);
                }


            }
            catch (Exception)
            {

            }
        }
        public void DataAddMed(string data)
        {
            string path = @"datafileMed.txt";
            if (File.Exists(path))
            {
                using (StreamWriter sr = File.AppendText(path))
                {
                    sr.WriteLine(data);
                    sr.Close();
                }
            }
        }

        public List<string> FixedWords(List<string> words)
        {
            Zemberek zemberek = new Zemberek(new TurkiyeTurkcesi());
            List<string> fixedwords = new List<string>();

            for (int i = 0; i < words.Count; i++)
            {
                string found_word = String.Empty;
                if (!String.IsNullOrEmpty(words[i]))
                {
                    string word = words[i];
                    if (dictionary_Turkish.Contains(word))
                    {
                        fixedwords.Add(word);
                    }
                    else
                    {

                        int lower = word.Length;
                        for (int j = 0; j < dictionary_Turkish.Count; j++)
                        {
                            if (dictionary_Turkish[j][0].Equals(word[0]))
                            {
                                int m = LevenshteinDistance(word, dictionary_Turkish[j]);
                                if (m < lower)
                                {
                                    lower = m;
                                    found_word = dictionary_Turkish[j];
                                }
                                else
                                {
                                    continue;
                                }
                            }
                        }
                        if (!String.IsNullOrEmpty(found_word))
                        {
                            fixedwords.Add(found_word);
                        }
                        else
                        {
                            medicineWords.Add(word);
                        }
                    }
                }
            }
            return fixedwords;
        }
        public List<string> FixWords(List<string> words)
        {
            Zemberek zemberek = new Zemberek(new TurkiyeTurkcesi());
            List<string> fixedwords = new List<string>();
            string found = String.Empty;
            int lower = 5;
            for (int i = 0; i < words.Count; i++)
            {

                if (!String.IsNullOrEmpty(words[i]) && words[i].Length > 1 && !words[i].Equals("Batur"))
                {
                    if (Processes.dictionary_Turkish.Contains(words[i]))
                    {
                        fixedwords.Add(words[i]);
                    }
                    else
                    {
                        string word = words[i];
                        var suggestions = zemberek.oner(word);
                        if (suggestions.Any())
                        {
                            List<string> offerings = suggestions.ToList<string>();
                            foreach (string offer in offerings)

                            {
                                if (offer[0].Equals(word[0]))
                                {
                                    int m = LevenshteinDistance(word, offer);
                                    if (m <= lower)
                                    {
                                        lower = m;
                                        found = offer;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                            }
                            fixedwords.Add(found);
                        }
                        else
                        {
                            medicineWords.Add(word);
                        }
                    }
                }
            }
            return fixedwords;
        }
        // Minimum Edit Distance
        public int LevenshteinDistance(string source1, string source2)
        {
            int source1Length = source1.Length;
            int source2Length = source2.Length;

            var matrix = new int[source1Length + 1, source2Length + 1];

            if (source1Length == 0)
                return source2Length;

            if (source2Length == 0)
                return source1Length;

            for (int i = 0; i <= source1Length; matrix[i, 0] = i++) { }
            for (int j = 0; j <= source2Length; matrix[0, j] = j++) { }

            for (int i = 1; i <= source1Length; i++)
            {
                for (int j = 1; j <= source2Length; j++)
                {
                    int cost = (source2[j - 1] == source1[i - 1]) ? 0 : 1;

                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }
            return matrix[source1Length, source2Length];
        }

        public List<string> ExcelRead(List<string> roots_Alls)
        {
            List<string> vad_values = new List<string>();
            string rest = "";
            string filePath = @"ANEW_Turkish.xlsx";
            FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            IExcelDataReader excelReader;
            List<double> v = new List<double>();
            List<double> a = new List<double>();
            List<double> d = new List<double>();
            int counter = 0;
            int count = 0;
            if (Path.GetExtension(filePath).ToUpper() == ".XLS")
            {
                //Reading from a binary Excel file ('97-2003 format; *.xls)
                excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
            }
            else
            {
                //Reading from a OpenXml Excel file (2007 format; *.xlsx)
                excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            }
            var resultt = excelReader.AsDataSet(new ExcelDataSetConfiguration()
            {
                ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                {
                    UseHeaderRow = true
                }
            });
            while (excelReader.Read())
            {
                counter++;

                //ilk satır başlık olduğu için 2.satırdan okumaya başlıyorum.
                if (counter > 1)
                {
                    foreach (string root in roots_Alls)
                    {
                        if (root.Equals(excelReader.GetString(0)))
                        {
                            v.Add(excelReader.GetDouble(2));
                            a.Add(excelReader.GetDouble(3));
                            d.Add(excelReader.GetDouble(4));
                            count++;
                        }
                    }

                }
            }
            excelReader.Close();
            double valuev = 0;
            double valuea = 0;
            double valued = 0;

            if (count > 0)
            {
                foreach (var fr in v)
                {
                    valuev += fr;
                }
                double div_v = valuev / count;
                rest = "V: " + (valuev / count).ToString();
                vad_values.Add("valuev" + " " + Convert.ToInt32(div_v).ToString());

                foreach (var fs in a)
                {
                    valuea += fs;
                }
                double div_a = valuea / count;
                rest = "A: " + (valuea / count).ToString();
                vad_values.Add("valuea" + " " + Convert.ToInt32(div_a).ToString());

                foreach (var ft in d)
                {
                    valued += ft;
                }
                double div_d = valued / count;
                rest = "D: " + (valued / count).ToString();
                vad_values.Add("valued" + " " + Convert.ToInt32(div_d).ToString());
                //textBox3.AppendText("\r\n" + rest);
            }

            return vad_values;
        }
        public string[] CleanPoints(string word)
        {
            for (int i = 0; i < points.Length; i++)
            {
                word = word.Replace(points[i], "");
            }
            word = word.Replace('"', ' ');
            word = word.Replace('\"', ' ');
            string[] cleanedwords = word.Split(' ');
            //flagwords = cleanedwords.Length;
            return cleanedwords;
        }
        public string RootofWords(string word)
        {
            Zemberek zemberek = new Zemberek(new TurkiyeTurkcesi());

            if (zemberek.kelimeDenetle(word))
            {
                String root = zemberek.kelimeCozumle(word)[0].kok().icerik();
                return root;
            }
            else
                return word;
        }

        public List<string> Splitreport(string[] word)
        {
            List<string> report2 = new List<string>();
            string[] cleanedwords = null;
            for (int i = 0; i < word.Length; i++)
            {
                cleanedwords = word[i].Split(' ');
                for (int j = 0; j < cleanedwords.Length; j++)
                {
                    for (int k = 0; k < points.Length; k++)
                    {
                        cleanedwords[j] = cleanedwords[j].Replace(points[k], "");
                    }
                    cleanedwords[j] = cleanedwords[j].Replace('"', ' ');
                    cleanedwords[j] = cleanedwords[j].Replace('\"', ' ');
                    string last = Regex.Replace(cleanedwords[j], @"\d", "");
                    string[] lastwords = last.Split(' ');
                    for (int k = 0; k < lastwords.Length; k++)
                    {
                        if (!lastwords[k].Equals(" "))
                        {
                            report2.Add(lastwords[k]);
                        }
                    }

                }
            }
            return report2;
        }
        public string[] Sort(string[] array)
        {
            string[] lastarray = new string[array.Length];
            int temp = -1;
            int bound = 0;
            int foundindex = 0;

            while (bound < array.Length)
            {
                temp = -1;
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i] != "")
                    {
                        string[] numbers = array[i].Split(' ');
                        int number = Int32.Parse(numbers[numbers.Length - 1]);
                        // int number = array[i][array[i].Length - 1] - '0';
                        if (number >= temp)
                        {
                            temp = number;
                            foundindex = i;
                        }
                    }

                }
                lastarray[bound] = array[foundindex];  //max eklendi
                array[foundindex] = "";
                bound++;
            }
            return lastarray;
        }
        public List<string> Pro2(List<string> data_)
        {
            List<string> alldatas = new List<string>();
            string[] result = Launch2(Splitreport(data_.ToArray()).ToArray());
            // ExcelRead();
            if (result.Length == 0)
            {
                //textBox2.Text = "Empty";
            }
            else
            {
                string[] result2 = StringNum(result).ToArray();
                string[] newarray = Sort(result2);
                for (int i = 0; i < newarray.Length; i++)
                {
                    //DataAdd(newarray[i]);
                    alldatas.Add(newarray[i]);
                    // WriteFile(newarray[i]);
                    if (i == (newarray.Length - 1))
                    {
                        break;
                    }
                }

                /*
                 *  for (int i = 0; i < newarray.Length; i++)
                {
                    //  textBox2.Text += result2[i][0] + result2[i][1];              
                    //DataAdd(newarray[i]);
                    if (i == (newarray.Length - 1))
                    {
                        //MessageBox.Show("Done");
                        break;
                    }
                }
                 * 
                 * for (int i = 0; i < result.Length; i++)
                 {
                     //  textBox2.Text += result2[i][0] + result2[i][1];
                     DataAdd(result[i]);
                     if (i == (result.Length - 1))
                     {
                         MessageBox.Show("Done");
                     }
                 }*/
                /* string[] last = Splitreport(report.ToArray()).ToArray();
                 for (int i = 0; i < last.Length; i++)
                 {
                     textBox1.Text += " "+ last[i];
                 }*/
            }

            string[] lastmedicine = Sort(StringNum(medicineWords.ToArray()).ToArray());
            for (int j = 0; j < lastmedicine.Length; j++)
            {
                //DataAddMed(lastmedicine[j]);
                alldatas.Add(lastmedicine[j]);
                //WriteFileMedicine(lastmedicine[j]);
                if (j == (lastmedicine.Length - 1))
                {
                    //MessageBox.Show("Done 2");
                    break;
                }
            }
            return alldatas;
        }
        public List<string> StringNum(string[] array)
        {
            string[] newarray = array;
            int size = newarray.Length;
            List<string> result = new List<string>();
            int value = 1;
            for (int i = 0; i < size; i++)
            {
                if (!newarray[i].Equals("") && !newarray[i].Equals(" "))
                {
                    string word = newarray[i];
                    if (i < size - 1)
                    {
                        for (int j = i + 1; j < size; j++)
                        {
                            if (word.Equals(newarray[j]))
                            {
                                newarray[j] = "";
                                value++;
                            }
                        }
                        result.Add(word + " " + value.ToString());
                    }
                    else if (i == size - 1)
                    {
                        if (newarray[i].Equals(""))
                        {
                            break;
                        }
                        else
                        {
                            result.Add(word + " " + value.ToString());
                        }
                    }
                    value = 1;
                }
            }
            return result;
        }
        public string[] Launch2(string[] words)
        {
            List<string> roots = new List<string>();
            string[] fixedword = null;
            if (words == null)
            {
                //MessageBox.Show("NULL!");
            }
            else
            {
                List<string> fixedWords = FixWords(words.ToList<string>());

                foreach (string word in fixedWords)
                {
                    if (word != null)
                    {
                        if (stopwords.Contains(RootofWords(word)))
                        {
                            continue;
                        }
                        else if (!stopwords.Contains(RootofWords(word)))
                        {
                            string last = RootofWords(word);
                            roots.Add(last);
                            continue;
                        }

                    }
                    else
                    {
                        continue;
                    }
                }
            }
            fixedword = roots.ToArray();

            return fixedword;

        }
    }
}
