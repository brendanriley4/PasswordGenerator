
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main (string[] args)
    {
        string connectionString = "Data Source = passwords.db;Version=3;";
        InitializeDatabase(connectionString);

        Console.WriteLine("Welcome to password protector!");

        while (true)
        {
            Console.WriteLine("");
            Console.WriteLine("What would you like to do today?");
            Console.WriteLine("1. Input a password to storage");
            Console.WriteLine("2. View services and passwords");
            Console.WriteLine("3. Edit a stored password");
            Console.WriteLine("4. Quit");
            Console.Write("Choice: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.WriteLine("");
                    Console.WriteLine("Would you like to choose a password or have one generated?");
                    Console.WriteLine("1. Input my own password");
                    Console.WriteLine("2. Generate password");
                    Console.Write("Choice: ");
                    string new_pwd_type = Console.ReadLine();
                    
                    if (new_pwd_type == "1")
                    {
                        Console.WriteLine("");
                        Console.WriteLine("Please enter your login information:");
                        Console.Write("Service name: ");
                        string service_name = Console.ReadLine();
                        Console.Write("Username: ");
                        string username = Console.ReadLine();
                        Console.Write("Password: ");
                        string password = Console.ReadLine();
                        StorePassword(connectionString, service_name, username, password);
                    }
                    else if (new_pwd_type == "2")
                    {
                        Console.WriteLine("");
                        PasswordGenerator(connectionString);
                    }
                    else
                    {
                        Console.WriteLine("Invalid choice, please try again");
                    }
                    break;

                case "2":
                    ReadPassword(connectionString);
                    break;
                case "3":
                    // going to call store password here
                    break;
                case "4":
                    Console.WriteLine("Goodbye!");
                    return;
                default:
                    Console.WriteLine("Invalid choice, please try again");
                    Console.WriteLine("");
                    break;
            }
        }        
    }

    static Dictionary<int, int> pseudoIdMap = new Dictionary<int, int>();

    static void InitializeDatabase(string connectionString)
    {
        using (var db = new SQLiteConnection(connectionString))
        {
            db.Open();

            string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS Passwords (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Service TEXT NOT NULL,
                    Username TEXT NOT NULL,
                    Password TEXT NOT NULL
                )";

            using (var cmd = new SQLiteCommand(createTableQuery, db))
            {
                cmd.ExecuteNonQuery();
            }

            Console.WriteLine("Database Initialized Successfully!");
            Console.WriteLine("");
        }
    }

    static void PasswordGenerator (string connectionString)
    {
        while (true)
        {
            Console.Write("How many characters long would you like your new password to be? ");
            
            if (int.TryParse(Console.ReadLine(), out int pwd_len) && pwd_len > 0)
            {
                Console.WriteLine("");
                Console.WriteLine($"Generating password of length: {pwd_len}");
                Console.WriteLine("What character combinations would you like?");
                Console.WriteLine("1. All possible characters");
                Console.WriteLine("2. Non-numeric characters only (not including special characters)");
                Console.WriteLine("3. Alphanumeric characters only");
                Console.Write("Choice: ");
                string pwd_type = Console.ReadLine();
                var chars = "";

                if (pwd_type == "1")
                {
                    // logic to generate passwords of all possible characters
                    chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()-_=+";
                }
                else if (pwd_type == "2")
                {
                    chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
                }
                else if (pwd_type == "3")
                {
                    chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                }
                else
                {
                    Console.WriteLine("Invalid choice, please try again");
                    Console.WriteLine("");
                    break;
                }

                string password = SecurePassword(pwd_len, chars);

                Console.WriteLine("Does this password suffice?");
                Console.WriteLine("1. Yes");
                Console.WriteLine("2. No");
                Console.Write("Choice: ");
                string ans = Console.ReadLine();

                if (ans == "2")
                {
                    Console.WriteLine("");
                    Console.WriteLine("Let's try this again...");
                    Console.WriteLine("");
                    continue;
                }
                else if (ans == "1")
                {
                    Console.WriteLine("What service would you like to store this password under?");
                    Console.Write("Input: ");
                    string service = Console.ReadLine();
                    Console.WriteLine("What username would you like to store alongside this password?");
                    Console.Write("Input: ");
                    string username = Console.ReadLine();
                    StorePassword(connectionString, service, username, password.ToString());
                    return;
                }
                else
                {
                    Console.WriteLine("Invalid choice, trying again...");
                    Console.WriteLine("");
                }
                
            }
            else
            {
                Console.WriteLine("");
                Console.WriteLine("Invalid length input...");
                Console.WriteLine("Let's try this again...");
                Console.WriteLine("");
                continue;
            }        
        }
    }

    static string SecurePassword(int pwd_len, string chars)
    {
        StringBuilder password = new StringBuilder();
        byte[] pwd = new byte[pwd_len];

        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(pwd);
        }

        foreach (byte b in pwd)
        {
            password.Append(chars[b % chars.Length]);
        }

        Console.WriteLine("");
        Console.WriteLine($"Generated Password: {password.ToString()}");
        Console.WriteLine("");
        
        return password.ToString();
    }

    static void StorePassword (string connectionString, string service, string username, string password)
    {
        Console.WriteLine("");
        Console.WriteLine("Accessing Database...");
        Console.WriteLine("");
        Console.WriteLine("Information being entered:");
        Console.WriteLine($"Service: {service}");
        Console.WriteLine($"Username: {username}");
        Console.WriteLine($"Password: {password}");

        Console.WriteLine("Proceed?");
        Console.WriteLine("1. Yes");
        Console.WriteLine("2. No");
        string ans = Console.ReadLine();

        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();

            string insertQuery = "INSERT INTO Passwords (Service, Username, Password) VALUES (@Service, @Username, @Password)";
            using (var command = new SQLiteCommand(insertQuery, connection)) 
            {
                command.Parameters.AddWithValue("@Service", service);
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@Password", password);
                command.ExecuteNonQuery();
            }
            Console.WriteLine("Information successfully stored");
            Console.WriteLine("");
        }
    }

    static void ReadPassword (string connectionString)
    {
        Console.WriteLine("");
        Console.WriteLine("Accessing Database...");
        Console.WriteLine("");

        pseudoIdMap.Clear();

        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();

            string query = "SELECT Id, Service From Passwords";
            using (var command = new SQLiteCommand(query, connection))
            using (var reader = command.ExecuteReader()) 
            {
                Console.WriteLine("Services with stored passwords:");
                int pseudoId = 1;

                while (reader.Read())
                {
                    string name = reader["Service"].ToString();
                    int id = int.Parse(reader["Id"].ToString());
                    pseudoIdMap[pseudoId] = id;
                    Console.WriteLine($"{pseudoId}: {name}");
                    pseudoId++;
                }
                Console.WriteLine("");
            }

            Console.WriteLine("Would you like to view a password?");
            Console.WriteLine("1. Yes");
            Console.WriteLine("2. No");
            Console.Write("Input: ");
            string ans = Console.ReadLine();

            if (ans == "1")
            {
                Console.WriteLine("");
                Console.WriteLine("Which password would you like to view?");
                Console.Write("ID: ");
                string input = Console.ReadLine();
                int pwdId;
                if (int.TryParse(input, out pwdId))
                {
                    // Successfully parsed the input to an integer
                    if (pseudoIdMap.ContainsKey(pwdId))
                    {
                        int realId = pseudoIdMap[pwdId];
                        string getPasswordQuery = "SELECT Password FROM Passwords WHERE Id = @Id";

                        using (var cmd = new SQLiteCommand(getPasswordQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@Id", realId);
                            string password = cmd.ExecuteScalar()?.ToString();
                            Console.WriteLine($"Password for ID {pwdId}: {password}");
                            Console.WriteLine("Password retrieval successful.");
                            Console.WriteLine("");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid ID. Please try again...");
                        Console.WriteLine("");
                    }
                }
                else
                {
                    // Failed to parse input
                    Console.WriteLine("Invalid input. Breaking loop...");
                    Console.WriteLine("");
                }
            }
        }
    }
}