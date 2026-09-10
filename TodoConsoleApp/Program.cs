using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient; // NuGet package: Microsoft.Data.SqlClient

namespace TodoConsoleApp
{
    // Simple class representing one row in the Todos table.
    class TodoItem
    {
        public int Id { get; set; }
        public string Task { get; set; } = "";
        public bool IsComplete { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    class Program
    {

        private const string ConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=TodoDemo;Trusted_Connection=True;Encrypt=False;";

        static void Main(string[] args)
        {
            Console.WriteLine("=== Todo List (SQL Server Demo) ===");

            bool running = true;
            while (running)
            {
                PrintMenu();
                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ListTodos();
                        break;
                    case "2":
                        AddTodo();
                        break;
                    case "3":
                        CompleteTodo();
                        break;
                    case "4":
                        DeleteTodo();
                        break;
                    case "5":
                        running = false;
                        break;
                    default:
                        Console.WriteLine("Not a valid option, try again.\n");
                        break;
                }
            }

            Console.WriteLine("Goodbye!");
        }

        static void PrintMenu()
        {
            Console.WriteLine("\n1) List todos");
            Console.WriteLine("2) Add todo");
            Console.WriteLine("3) Mark todo complete");
            Console.WriteLine("4) Delete todo");
            Console.WriteLine("5) Exit");
            Console.Write("Choose an option: ");
        }

        // ---- Data access methods ----
        // Each method opens its own connection, runs one command, and closes automatically
        // thanks to the "using" statement. This is the standard beginner-friendly ADO.NET pattern.

        static List<TodoItem> GetAllTodos()
        {
            var todos = new List<TodoItem>();

            using var connection = new SqlConnection(ConnectionString);
            connection.Open();

            string sql = "SELECT Id, Task, IsComplete, CreatedAt FROM Todos ORDER BY Id";
            using var command = new SqlCommand(sql, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                todos.Add(new TodoItem
                {
                    Id = reader.GetInt32(0),
                    Task = reader.GetString(1),
                    IsComplete = reader.GetBoolean(2),
                    CreatedAt = reader.GetDateTime(3)
                });
            }

            return todos;
        }

        static void ListTodos()
        {
            var todos = GetAllTodos();

            if (todos.Count == 0)
            {
                Console.WriteLine("No todos yet.");
                return;
            }

            Console.WriteLine();
            foreach (var todo in todos)
            {
                string status = todo.IsComplete ? "[x]" : "[ ]";
                Console.WriteLine($"{todo.Id,3} {status} {todo.Task}");
            }
        }

        static void AddTodo()
        {
            Console.Write("What do you need to do? ");
            string? task = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(task))
            {
                Console.WriteLine("Task can't be empty.");
                return;
            }

            using var connection = new SqlConnection(ConnectionString);
            connection.Open();

            string sql = "INSERT INTO Todos (Task) VALUES (@Task)";
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Task", task);
            command.ExecuteNonQuery();

            Console.WriteLine("Added.");
        }

        static void CompleteTodo()
        {
            ListTodos();
            Console.Write("\nEnter the Id to mark complete: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("That's not a valid Id.");
                return;
            }

            using var connection = new SqlConnection(ConnectionString);
            connection.Open();

            string sql = "UPDATE Todos SET IsComplete = 1 WHERE Id = @Id";
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);
            int rows = command.ExecuteNonQuery();

            Console.WriteLine(rows > 0 ? "Marked complete." : "No todo found with that Id.");
        }

        static void DeleteTodo()
        {
            ListTodos();
            Console.Write("\nEnter the Id to delete: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("That's not a valid Id.");
                return;
            }

            using var connection = new SqlConnection(ConnectionString);
            connection.Open();

            string sql = "DELETE FROM Todos WHERE Id = @Id";
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);
            int rows = command.ExecuteNonQuery();

            Console.WriteLine(rows > 0 ? "Deleted." : "No todo found with that Id.");
        }
    }
}