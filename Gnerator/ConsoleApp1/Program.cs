using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Timers;
using System.Threading;
using System.Diagnostics;
using System.Linq;
using System.IO;

//SELECT AKCESORIA_ID FROM akcesoria ORDER BY DBMS_RANDOM.VALUE FETCH NEXT 1 ROWS ONLY;

namespace ConsoleApp1
{
    class Program
    {
        public static int day;
        public static int day_end;
        public static int month; 
        public static int year;
        public static int counter;
        public static OracleConnection connection;
        public static Stopwatch stopWatch;
        public static int rows_to_insert;
        const string all_chars = "ABCDEFGHIJKLMNOPQRSTUVW XYZabcdefghijklmnoprstuwxyz0123456789";
        const string letters = "ABCDEFGHIJKLMNOPQRSTUVW XYZabcdefghijklmnoprstuwxyz";
        const string numbers = "1234567890";        
        private static Random random = new Random();

        private static void SetTimer()
        {
            stopWatch = new Stopwatch();
            stopWatch.Start();
        }
        private static void StopTimer()
        {            
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}h:{1:00}m:{2:00}s{3:00}ms",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds);
            stopWatch.Restart();
            Console.WriteLine("Czas przetwarzania żądania: " + elapsedTime);           
        }

        public static int GetRandomNumberInt(int min, int max)
        {
            lock (random) // synchronize
            {
                return random.Next(min, max);
            }
        }
        public static double GetRandomNumberDouble(int min, int max)
        {
            lock (random) // synchronize
            {
                return Math.Round(random.NextDouble()*max+min, 2, MidpointRounding.AwayFromZero);
            }
        }

        public static string GetRandomSingleValue(string first, string second)
        {
            if (GetRandomNumberInt(1,9) != 1)
            {
                return first;
            }
            else
            {
                return second;
            }
        }
        public static string GetNameFromTxt()
        {
            string[] allLines = File.ReadAllLines(@AppDomain.CurrentDomain.BaseDirectory + "names.txt");
            Random rnd1 = new Random();
            string name = allLines[rnd1.Next(allLines.Length)];
            return name;
        }
        public static void HowManyRows()
        {
            Console.WriteLine();
            Console.WriteLine("Ile wpisów chcesz dodać? potwierdź ENTER");
            string rows_to_add = Console.ReadLine();
            try 
            { 
                rows_to_insert = Convert.ToInt32(rows_to_add);
            }
            catch (Exception)
            {
                Console.WriteLine("Podaj Liczbę");
                HowManyRows();
            }
                        
        }

        public static void BackToMenu()
        {
            Console.WriteLine();
            Console.WriteLine("Naciśnij ENTER by wrócić do menu lub dowolny klawisz by zamknąć");
            if (Console.ReadKey().Key == ConsoleKey.Enter)
            {
                Main();
            }          
        }
        public static string RandomStringAlphanumeric(int length)
        {           
            return new string(Enumerable.Repeat(all_chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static string RandomStringLetters(int length)
        {
            return new string(Enumerable.Repeat(letters, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static string RandomStringNumbers(int length)
        {
            return new string(Enumerable.Repeat(numbers, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static void ConnectToDataBase()
        {
            connection = new OracleConnection();
            connection.ConnectionString = "User Id=s95574;Password=muson8;Data Source=217.173.198.135:1522/orcltp.iaii.local";
            connection.Open();
            //Console.WriteLine();
            //Console.WriteLine("Połączono z Oracle" + connection.ServerVersion);
        }
        public static int FindNextId(string table_name)
        {            
            OracleCommand cmd_count = new OracleCommand();
            string max_row = "SELECT MAX("+table_name+"_ID) FROM "+table_name;
            cmd_count.CommandText = max_row;
            cmd_count.Connection = connection;
            OracleDataReader dr = cmd_count.ExecuteReader();
            dr.Read();
            int next_row;
            try
            {
                return next_row = dr.GetInt32(0)+1;
            }
            catch (System.InvalidCastException)
            {
                return next_row = 1;
            }
        }
        public static int GetRandomId(string table_name)
        {
            OracleCommand cmd_count = new OracleCommand();
            string random_id = "SELECT " + table_name + "_ID FROM " + table_name + " ORDER BY DBMS_RANDOM.VALUE FETCH NEXT 1 ROWS ONLY";
            cmd_count.CommandText = random_id;
            cmd_count.Connection = connection;
            OracleDataReader dr = cmd_count.ExecuteReader();
            dr.Read();
            int random_id2;
            try
            {
                return random_id2 = dr.GetInt32(0);
            }
            catch (Exception)
            {
                if (table_name == "klienci")
                {                                      
                    InsertKlienci(10);                   
                    return GetRandomId(table_name);                    
                }
                if (table_name == "uslugi")
                {
                    InsertUslugi(10);
                    return GetRandomId(table_name);
                }
                if (table_name == "zlecenia")
                {
                    InsertZlecenia(10);
                    return GetRandomId(table_name);
                }
                if (table_name == "pracownicy")
                {
                    InsertPracownicy(10);
                    return GetRandomId(table_name);
                }
                if (table_name == "akcesoria")
                {
                    InsertAkcesoria(10);
                    return GetRandomId(table_name);
                }
                if (table_name == "specjalizacje")
                {
                    InsertSpecjalizacje(10);
                    return GetRandomId(table_name);
                }
                return 0; 
            }
        }

        public static string GenerateDataStart()
        {            
            string data;
            string day_string;
            string month_string;
            if (day > 31)
            {
                day = 1;
                month++;
            }
            if (month == 2)
            {
                if (day > 28)
                {
                    day = 1;
                    month++;
                }
            }
            if (month == 4 || month == 6 || month == 9 || month == 11)
            {
                if (day > 30)
                {
                    day = 1;
                    month++;
                }
            }       
            if (month > 12)
            {
                month = 1;
                year++;
            }            
            if (day < 10)
            {
                 day_string = "0" + day + "";
            }
            else { day_string = "" + day + ""; }
            if (month < 10)
            {
                month_string = "0" + month + "";
            }
            else { month_string = "" + month + ""; }

            return data=""+day_string+"-"+month_string+"-"+year+"";
        }
        public static string GenerateDataEnd()
        {    
            
            string data;
            string day_string;
            string month_string;
            if (day + day_end > 31)
            {
                day = 1;
                month++;
            }
            if (month == 2)
            {
                if (day+day_end > 28)
                {
                    day = 1;
                    month++;
                }
            }
            if (month == 4 || month == 6 || month == 9 || month == 11)
            {
                if (day+day_end > 30)
                {
                    day = 1;
                    month++;
                }
            }
            if (month > 12)
            {
                month = 1;
                year++;
            }            
            if ((day+day_end) < 10)
            {
                day_string = "0" + (day+day_end) + "";
            }
            else { day_string = "" + (day+day_end) + ""; }
            if (month < 10)
            {
                month_string = "0" + month + "";
            }
            else { month_string = "" + month + ""; }

            return data = "" + day_string + "-" + month_string + "-" + year + "";
        }

        public static void CloseDatabaseConnection()
        {
            connection.Close();
            connection.Dispose();
        }
        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

        public static void Loader()
        {           
            counter++;
            switch (counter % 4)
            {
                case 0: Console.Write("."); break;
                case 1: Console.Write(".."); break;
                case 2: Console.Write("..."); break;
                case 3: ClearCurrentConsoleLine(); break;
            }
            Console.SetCursorPosition(0, Console.CursorTop);
        }

        public static string GetDateTime()
        {
            DateTime thisDay = DateTime.Now;
            string datetime = thisDay.ToString();
            datetime = datetime.Replace(":", "-");
            return datetime;
        }

        static void InsertAkcesoria(int rows_to_insert)
        {
            string datetime = GetDateTime();
            OracleCommand cmd = new OracleCommand();                   
            SetTimer();
            int next_row = FindNextId("akcesoria");
            for (int i = (next_row); i <= next_row + rows_to_insert-1; i++)
            {
                string nazwa = RandomStringAlphanumeric(GetRandomNumberInt(10, 41));
                string producent = RandomStringLetters(GetRandomNumberInt(2, 16)) + ".inc";
                string marka_docelowa;
                if (GetRandomNumberInt(1, 5) == 2)
                {
                    marka_docelowa = producent;
                }
                else
                {
                    marka_docelowa = RandomStringLetters(GetRandomNumberInt(2, 16)) + ".inc";
                }
                string cena = GetRandomNumberDouble(3, 3001).ToString();
                cena = cena.Replace(',', '.');
                string stan = GetRandomSingleValue("NOWY", "UŻYWANY");
                int liczba_sztuk = GetRandomNumberInt(0, 51);
                int akcesoria_kat_id = GetRandomNumberInt(1, 51);
                string opis = RandomStringAlphanumeric(GetRandomNumberInt(20, 300));
                string kategoria = RandomStringAlphanumeric(GetRandomNumberInt(5, 30));                
                string query = "INSERT INTO akcesoria VALUES ('" + nazwa + "','" + producent + "','" + marka_docelowa + "'," + cena + ",'" + stan + "'," + liczba_sztuk + ",'" + opis + "'," + i + ","+ akcesoria_kat_id+")";

                using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory+"insert_akcesoria "+datetime+".txt", true))
                {
                    file.WriteLine(query);
                }

                cmd.CommandText = query;
                cmd.Connection = connection;                
                cmd.ExecuteNonQuery();
                Loader();
            }            
            ClearCurrentConsoleLine();
            Console.WriteLine("Dodano "+rows_to_insert+" wierszy do tabeli AKCESORIA");
            StopTimer();
            Console.WriteLine();
        }
        static void InsertZlecenia(int rows_to_insert)
        {
            string datetime = GetDateTime();
            OracleCommand cmd = new OracleCommand();        
            try
            {
                string max_day = "Select extract(day from data_przyjecia) from zlecenia WHERE data_przyjecia=(Select max(data_przyjecia) from zlecenia)";
                cmd.CommandText = max_day;
                cmd.Connection = connection;
                OracleDataReader dr = cmd.ExecuteReader();
                dr = cmd.ExecuteReader();
                dr.Read();
                day = Convert.ToInt32(dr.GetFieldValue<decimal>(0));
                string max_month = "Select extract(month from data_przyjecia) from zlecenia WHERE data_przyjecia=(Select max(data_przyjecia) from zlecenia)";
                cmd.CommandText = max_month;
                cmd.Connection = connection; dr = cmd.ExecuteReader();
                dr.Read();
                month = Convert.ToInt32(dr.GetFieldValue<decimal>(0));
                string max_year = "Select extract(year from data_przyjecia) from zlecenia WHERE data_przyjecia=(Select max(data_przyjecia) from zlecenia)";
                cmd.CommandText = max_year;
                cmd.Connection = connection;
                dr = cmd.ExecuteReader();
                dr.Read();
                year = Convert.ToInt32(dr.GetFieldValue<decimal>(0));
            }
            catch (Exception)
            {
                day = 1;
                month = 1;
                year = 2017;
            }
            SetTimer();
            int next_row = FindNextId("zlecenia");
            for (int i = (next_row); i <= next_row + rows_to_insert - 1; i++)
            {
                day_end = GetRandomNumberInt(0, 6);
                string opis = RandomStringAlphanumeric(GetRandomNumberInt(20, 20));
                string data_przyjecia = GenerateDataStart();
                string data_zakonczenia = GenerateDataEnd();
                string zakonczono = GetRandomSingleValue("1", "0");
                int klienci_klienci_ID = GetRandomId("klienci");
                
                string query = "INSERT INTO zlecenia VALUES ('" + opis + "',TO_DATE('" + data_przyjecia + "','DD-MM-YYYY'),TO_DATE('" + data_zakonczenia + "','DD-MM-YY'),'" + zakonczono + "','" + i + "','" + klienci_klienci_ID +"')";
                
                using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + "insert_zlecenia" + datetime + ".txt", true))
                {
                    file.WriteLine(query);
                }

                cmd.CommandText = query;
                cmd.Connection = connection;                
                cmd.ExecuteNonQuery();
                day += GetRandomNumberInt(0,3);
                Loader();
            }          
            ClearCurrentConsoleLine();
            Console.WriteLine("Dodano " + rows_to_insert + " wierszy do tabeli ZLECENIA");
            StopTimer();
            Console.WriteLine();
        }
        static void InsertKlienci(int rows_to_insert)
        {
            string datetime = GetDateTime();
            OracleCommand cmd = new OracleCommand();         
            SetTimer();
            int next_row = FindNextId("klienci");                 
            for (int i = (next_row); i <= next_row + rows_to_insert - 1; i++)
            {               
                string imie = GetNameFromTxt();
                string nazwisko = RandomStringLetters(GetRandomNumberInt(2, 20));
                string nr_telefonu = RandomStringNumbers(GetRandomNumberInt(9, 12));                

                string query = "INSERT INTO klienci VALUES ('" + imie + "','" + nazwisko + "','" + nr_telefonu + "'," + i +")";

                using (System.IO.StreamWriter file =
               new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + "insert_klienci " + datetime + ".txt", true))
                {
                    file.WriteLine(query);
                }

                cmd.CommandText = query;
                cmd.Connection = connection;
                cmd.ExecuteNonQuery();
                Loader();
            }           
            ClearCurrentConsoleLine();
            Console.WriteLine("Dodano " + rows_to_insert + " wierszy do tabeli KLIENCI");
            StopTimer();
            Console.WriteLine();
        }
        static void InsertUslugi(int rows_to_insert)
        {
            string datetime = GetDateTime();
            OracleCommand cmd = new OracleCommand();
            SetTimer();
            int next_row = FindNextId("uslugi");
            for (int i = (next_row); i <= next_row + rows_to_insert - 1; i++)
            {
                string nazwa = RandomStringAlphanumeric(GetRandomNumberInt(10, 50));
                string cena = GetRandomNumberDouble(10, 500).ToString();
                cena = cena.Replace(',', '.');              

                string query = "INSERT INTO uslugi VALUES ('" + nazwa + "'," + cena + "," + i + ")";

                using (System.IO.StreamWriter file =
               new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + "insert_uslugi " + datetime + ".txt", true))
                {
                    file.WriteLine(query);
                }

                cmd.CommandText = query;
                cmd.Connection = connection;
                cmd.ExecuteNonQuery();
                Loader();
            }
            ClearCurrentConsoleLine();
            Console.WriteLine("Dodano " + rows_to_insert + " wierszy do tabeli USŁUGI");
            StopTimer();
            Console.WriteLine();
        }

        static void InsertSpecjalizacje(int rows_to_insert)
        {
            string datetime = GetDateTime();
            OracleCommand cmd = new OracleCommand();
            SetTimer();
            int next_row = FindNextId("specjalizacje");
            for (int i = (next_row); i <= next_row + rows_to_insert - 1; i++)
            {
                string nazwa = RandomStringAlphanumeric(GetRandomNumberInt(2, 30));
                string query = "INSERT INTO specjalizacje VALUES ('" + nazwa + "'," + i + ")";

                using (System.IO.StreamWriter file =
               new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + "insert_specjalizacje " + datetime + ".txt", true))
                {
                    file.WriteLine(query);
                }

                cmd.CommandText = query;
                cmd.Connection = connection;
                cmd.ExecuteNonQuery();
                Loader();
            }
            ClearCurrentConsoleLine();
            Console.WriteLine("Dodano " + rows_to_insert + " wierszy do tabeli SPECJALIZACJE");
            StopTimer();
            Console.WriteLine();
        }

        static void InsertPracownicy(int rows_to_insert)
        {
            string datetime = GetDateTime();
            OracleCommand cmd = new OracleCommand();
            SetTimer();
            int next_row = FindNextId("pracownicy");
            for (int i = (next_row); i <= next_row + rows_to_insert - 1; i++)
            {
                string imie = GetNameFromTxt();
                string nazwisko = RandomStringLetters(GetRandomNumberInt(2, 20));
                string wynagrodzenie = GetRandomNumberDouble(2200, 6800).ToString();
                wynagrodzenie = wynagrodzenie.Replace(',', '.');
                string nr_telefonu = RandomStringNumbers(GetRandomNumberInt(9, 12));
                string pesel= RandomStringNumbers(11);
                string ulica = RandomStringAlphanumeric(GetRandomNumberInt(7,40));
                string kod_pocztowy = RandomStringAlphanumeric(GetRandomNumberInt(5, 5));
                kod_pocztowy = kod_pocztowy.Insert(2, "-");
                string nr_konta = RandomStringNumbers(28);

                string query = "INSERT INTO pracownicy VALUES ('" + imie + "','" + nazwisko + "'," + wynagrodzenie + ",'" + nr_telefonu + "','" + pesel + "','" + ulica + "','" + kod_pocztowy + "','" + nr_konta + "'," + i + ")";

                using (System.IO.StreamWriter file =
               new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + "insert_pracownicy " + datetime + ".txt", true))
                {
                    file.WriteLine(query);
                }

                cmd.CommandText = query;
                cmd.Connection = connection;
                cmd.ExecuteNonQuery();
                Loader();
            }
            ClearCurrentConsoleLine();
            Console.WriteLine("Dodano " + rows_to_insert + " wierszy do tabeli PRACOWNICY");
            StopTimer();
            Console.WriteLine();
        }

        static void InsertUslugi_Zlecenia(int rows_to_insert)
        {
            string datetime = GetDateTime();
            OracleCommand cmd = new OracleCommand();
            SetTimer();           
            for (int i = 0; i <= rows_to_insert; i++)
            {
                int uslugi_id = GetRandomId("uslugi");
                int zlecenia_id= GetRandomId("zlecenia");

                string query = "INSERT INTO uslugi_zlecenia VALUES (" + uslugi_id + "," + zlecenia_id +")";

                using (System.IO.StreamWriter file =
               new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + "insert_uslugi_zlecenia " + datetime + ".txt", true))
                {
                    file.WriteLine(query);
                }

                cmd.CommandText = query;
                cmd.Connection = connection;
                cmd.ExecuteNonQuery();
                Loader();
            }
            ClearCurrentConsoleLine();
            Console.WriteLine("Dodano " + rows_to_insert + " wierszy do tabeli uslugi_zlecenia");          
            StopTimer();
            Console.WriteLine();
        }
        static void InsertAkcesoria_Zlecenia(int rows_to_insert)
        {
            string datetime = GetDateTime();
            OracleCommand cmd = new OracleCommand();
            SetTimer();
            for (int i = 0; i <= rows_to_insert; i++)
            {
                int akcesoria_id = GetRandomId("akcesoria");
                int zlecenia_id = GetRandomId("zlecenia");

                string query = "INSERT INTO akcesoria_zlecenia VALUES (" + akcesoria_id + "," + zlecenia_id + ")";

                using (System.IO.StreamWriter file =
               new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + "insert_akcesoria_zlecenia " + datetime + ".txt", true))
                {
                    file.WriteLine(query);
                }

                cmd.CommandText = query;
                cmd.Connection = connection;
                cmd.ExecuteNonQuery();
                Loader();
            }
            ClearCurrentConsoleLine();
            Console.WriteLine("Dodano " + rows_to_insert + " wierszy do tabeli akcesoria_zlecenia");
            StopTimer();
            Console.WriteLine();
        }
        static void InsertSpecjalizacje_Pracownicy(int rows_to_insert)
        {
            string datetime = GetDateTime();
            OracleCommand cmd = new OracleCommand();
            SetTimer();
            for (int i = 0; i <= rows_to_insert; i++)
            {
                int specjalizacje_id = GetRandomId("specjalizacje");
                int pracownicy_id = GetRandomId("pracownicy");

                string query = "INSERT INTO specjalizacje_pracownicy VALUES (" + specjalizacje_id + "," + pracownicy_id + ")";

                using (System.IO.StreamWriter file =
               new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + "insert_specjalizacje_pracownicy " + datetime + ".txt", true))
                {
                    file.WriteLine(query);
                }

                cmd.CommandText = query;
                cmd.Connection = connection;
                cmd.ExecuteNonQuery();
                Loader();
            }
            ClearCurrentConsoleLine();
            Console.WriteLine("Dodano " + rows_to_insert + " wierszy do tabeli specjalizacje_pracownicy");
            StopTimer();
            Console.WriteLine();
        }
        static void InsertPracownicy_Zlecenia(int rows_to_insert)
        {
            string datetime = GetDateTime();
            OracleCommand cmd = new OracleCommand();
            SetTimer();
            for (int i = 0; i <= rows_to_insert; i++)
            {
                int pracownicy_id = GetRandomId("pracownicy");
                int zlecenia_id = GetRandomId("zlecenia");

                string query = "INSERT INTO pracownicy_zlecenia VALUES (" + zlecenia_id + "," + pracownicy_id + ")";

                using (System.IO.StreamWriter file =
               new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + "insert_pracownicy_zlecenia " + datetime + ".txt", true))
                {
                    file.WriteLine(query);
                }

                cmd.CommandText = query;
                cmd.Connection = connection;
                cmd.ExecuteNonQuery();
                Loader();
            }
            ClearCurrentConsoleLine();
            Console.WriteLine("Dodano " + rows_to_insert + " wierszy do tabeli pracownicy_zlecenia");
            StopTimer();
            Console.WriteLine();
        }

        static void InsertAkcesoria_Kat(int rows_to_insert)
        {
            string datetime = GetDateTime();
            OracleCommand cmd = new OracleCommand();
            SetTimer();
            int next_row = FindNextId("kategoria_akcesorium");
            for (int i = (next_row); i <= next_row + rows_to_insert - 1; i++)
            {
                string nazwa = RandomStringAlphanumeric(GetRandomNumberInt(3, 30));
                string producent = RandomStringLetters(GetRandomNumberInt(2, 16)) + ".inc";
                string marka_docelowa;
                if (GetRandomNumberInt(1, 5) == 2)
                {
                    marka_docelowa = producent;
                }
                else
                {
                    marka_docelowa = RandomStringLetters(GetRandomNumberInt(2, 16)) + ".inc";
                }
                string cena = GetRandomNumberDouble(3, 3001).ToString();
                cena = cena.Replace(',', '.');
                string stan = GetRandomSingleValue("NOWY", "UŻYWANY");
                int liczba_sztuk = GetRandomNumberInt(0, 51);
                string opis = RandomStringAlphanumeric(GetRandomNumberInt(20, 300));
                string kategoria = RandomStringAlphanumeric(GetRandomNumberInt(5, 30));

                string query = "INSERT INTO kategoria_akcesorium VALUES ('" + nazwa + "'," + i + ")";

                using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + "insert_kategoria_akcesorium " + datetime + ".txt", true))
                {
                    file.WriteLine(query);
                }

                cmd.CommandText = query;
                cmd.Connection = connection;
                cmd.ExecuteNonQuery();
                Loader();
            }
            ClearCurrentConsoleLine();
            Console.WriteLine("Dodano " + rows_to_insert + " wierszy do tabeli KAT_AKCESORIA");
            StopTimer();
            Console.WriteLine();
        }
        static void Main()
        {
            Console.Clear();
            Console.WriteLine("Wybierz którą tabele chcesz uzupełnić");
            Console.WriteLine("1. AKCESORIA");
            Console.WriteLine("2. ZLECENIA");
            Console.WriteLine("3. KLIENCI");
            Console.WriteLine("4. USŁUGI");
            Console.WriteLine("5. SPECJALIZACJE");
            Console.WriteLine("6. PRACOWNICY");
            Console.WriteLine("7. uslugi-zlecenia");
            Console.WriteLine("8. akcesoria-zlecenia");
            Console.WriteLine("9. specjalizacje-pracownicy");
            Console.WriteLine("0. pracownicy_zlecenia");
            Console.WriteLine("a. CAŁA BAZA");

            switch (Console.ReadKey().KeyChar)
            {
                case '1':
                    HowManyRows();
                    ConnectToDataBase();
                    InsertAkcesoria(rows_to_insert);
                    CloseDatabaseConnection();
                    BackToMenu();
                    break;
                case '2':
                    HowManyRows();
                    ConnectToDataBase();
                    InsertZlecenia(rows_to_insert);
                    CloseDatabaseConnection();
                    BackToMenu();
                    break;
                case '3':
                    HowManyRows();
                    ConnectToDataBase();
                    InsertKlienci(rows_to_insert);
                    CloseDatabaseConnection();
                    BackToMenu();
                    break;
                case '4':
                    HowManyRows();
                    ConnectToDataBase();
                    InsertUslugi(rows_to_insert);
                    CloseDatabaseConnection();
                    BackToMenu();
                    break;
                case '5':
                    HowManyRows();
                    ConnectToDataBase();
                    InsertSpecjalizacje(rows_to_insert);
                    CloseDatabaseConnection();
                    BackToMenu();
                    break;
                case '6':
                    HowManyRows();
                    ConnectToDataBase();
                    InsertPracownicy(rows_to_insert);
                    CloseDatabaseConnection();
                    BackToMenu();
                    break;
                case '7':
                    HowManyRows();
                    ConnectToDataBase();
                    InsertUslugi_Zlecenia(rows_to_insert);
                    CloseDatabaseConnection();
                    BackToMenu();
                    break;
                case '8':
                    HowManyRows();
                    ConnectToDataBase();
                    InsertAkcesoria_Zlecenia(rows_to_insert);
                    CloseDatabaseConnection();
                    BackToMenu();
                    break;
                case '9':
                    HowManyRows();
                    ConnectToDataBase();
                    InsertSpecjalizacje_Pracownicy(rows_to_insert);
                    CloseDatabaseConnection();
                    BackToMenu();
                    break;
                case '0':
                    HowManyRows();
                    ConnectToDataBase();
                    InsertPracownicy_Zlecenia(rows_to_insert);
                    CloseDatabaseConnection();
                    BackToMenu();
                    break;
                case 'a':
                    HowManyRows();
                    ConnectToDataBase();
                    InsertAkcesoria_Kat(rows_to_insert);
                    InsertAkcesoria(rows_to_insert);
                    InsertUslugi(rows_to_insert);
                    InsertKlienci(rows_to_insert);
                    InsertZlecenia(rows_to_insert);
                    InsertPracownicy(rows_to_insert);
                    InsertSpecjalizacje(rows_to_insert);
                    InsertAkcesoria_Zlecenia(rows_to_insert);
                    InsertPracownicy_Zlecenia(rows_to_insert);
                    InsertSpecjalizacje_Pracownicy(rows_to_insert);
                    InsertUslugi_Zlecenia(rows_to_insert);
                    CloseDatabaseConnection();
                    BackToMenu();
                    break;

                default:
                    Console.WriteLine("Wybierz Tabele do uzupełnienia");
                    Main();
                    break;
            }
        }
    }
}
