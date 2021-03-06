﻿using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace MealPlannerApp
{
    class Program
    {

        static void Main(string[] args)
        {
            using var db = new SqliteConnection("Data Source=MealPlannerApp.db;Cache=Shared");
            {
                db.Open();
                string cmd = "CREATE TABLE IF NOT EXISTS RecipeList (id TEXT, name TEXT, meal TEXT)";

                var createTable = new SqliteCommand(cmd, db);
                createTable.ExecuteReader();
            }
            MainMenu();
        }

        static void MainMenu()
        {
            Console.WriteLine("Welcome. What would you like to do?");
            Console.WriteLine("[1] - Add/View Meal Plan.");
            Console.WriteLine("[2] - Add/View Recipe.");
            Console.WriteLine("[3] - Add/View Grocery List. ");
            Console.WriteLine("[4] - Export file.");
            Console.WriteLine("[0] - Quit.");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    MealPlanMenu();
                    break;
                case "2":
                    RecipeMenu();
                    break;
                case "3":
                    GroceryMenu();
                    break;
                case "4":
                    WriteFile();
                    break;
                case "0":
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Select Menu Number.");
                    break;
            }
        }

        static void MealPlanMenu()
        {
            Console.WriteLine("MEAL PLAN MENU");
            Console.WriteLine("[1] - Create New Meal Plan.");
            Console.WriteLine("[2] - View Meal Plan.");
            Console.WriteLine("[3] - Delete Meal Plan.");
            Console.WriteLine("[0] - Main Menu.");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    AddMealPlan();
                    break;
                case "2":
                    ViewMealPlan();
                    break;
                case "3":
                    DeleteMealPlan();
                    break;
                case "0":
                    MainMenu();
                    break;
                default:
                    Console.WriteLine("Select Menu Number");
                    break;
            }
        }

        static void AddMealPlan()
        {
            Console.WriteLine("Enter the date (DD/MM/YYYY) for the first day of the meal plan.");
            string dateString = Console.ReadLine();
            var dateList = new List<DateTime>();

            DateTime date;
            if (DateTime.TryParse(dateString, out date))
                Console.WriteLine($"Date: {date.DayOfWeek}, " + $"{date:d}");
            else
            {
                Console.WriteLine("Value entered is not a valid date.");
                AddMealPlan();
            }

            Console.WriteLine("Enter the number of days you would like to plan.");
            int num = Convert.ToInt32(Console.ReadLine());
            using var db = new SqliteConnection("Data Source=MealPlannerApp.db;Cache=Shared");
            {
                db.Open();
                string name = $"[MealPlan{date:d}]";
                string cmd = "CREATE TABLE IF NOT EXISTS" + name + "(day TEXT, breakfast TEXT, lunch TEXT, dinner TEXT)";
                var createTable = new SqliteCommand(cmd, db);
                createTable.ExecuteNonQuery();
                for (int i = 0; i < num; i++)
                {
                    dateList.Add(date.AddDays(i));
                    Console.WriteLine(dateList[i].DayOfWeek);
                    string breakfast = GetString("Breakfast: ");
                    string lunch = GetString("Lunch: ");
                    string dinner = GetString("Dinner: ");
                    var insertCommand = new SqliteCommand("INSERT INTO" + name + "(day, breakfast, lunch, dinner) VALUES (@day, @breakfast, @lunch, @dinner)", db);
                    insertCommand.Parameters.AddWithValue("@day", dateList[i].DayOfWeek.ToString());
                    insertCommand.Parameters.AddWithValue("@breakfast", breakfast);
                    insertCommand.Parameters.AddWithValue("@lunch", lunch);
                    insertCommand.Parameters.AddWithValue("@dinner", dinner);
                    insertCommand.ExecuteNonQuery();
                }
                
            }
            Console.WriteLine("Meal Plan created!");
            MealPlanMenu();
        }

        static string GetString(string str)
        {
            Console.WriteLine(str);
            string input = Console.ReadLine();
            if (input == "ALL" || CheckRecipeExists(input))
                return input;
            else
                return GetString(str);

        }
        static bool CheckRecipeExists(string recipe)
        {
            using var db = new SqliteConnection("Data Source=MealPlannerApp.db;Cache=Shared");
            {
                db.Open();
                var checkCommand = new SqliteCommand("SELECT * FROM RecipeList WHERE name = (VALUES (@name))", db);
                checkCommand.Parameters.AddWithValue("@name", recipe);
                var recipeExists = checkCommand.ExecuteScalar();
                if (recipeExists == null)
                {
                    Console.WriteLine("Entry does not exist. Please enter a valid recipe.");
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        static void ViewMealPlan()
        {            
            using var db = new SqliteConnection("Data Source=MealPlannerApp.db;Cache=Shared");
            {
                db.Open();
                Console.WriteLine("Enter the starting date (DD/MM/YYYY) of the Meal Plan you want to view.");
                Console.WriteLine("Or type ALL to view a list of all Meal Plans");
                string dateString = Console.ReadLine();
                string name = dateString;
                if (dateString != "ALL")
                {
                    DateTime date;
                    if (!DateTime.TryParse(dateString, out date))
                    {
                        Console.WriteLine("Value entered is not a valid date.");
                        ViewMealPlan();
                    }

                    name = $"[MealPlan{date:d}]";
                }
                
                var insertCommand = new SqliteCommand("SELECT day, breakfast, lunch, dinner FROM " + name, db);
                if (name == "ALL")
                {
                    insertCommand = new SqliteCommand("SELECT name FROM sqlite_master WHERE type = 'table' AND name LIKE 'MealPlan%' ORDER BY 1", db);
                }
                try
                {
                    insertCommand.ExecuteNonQuery();
                }
                catch (SqliteException)
                {
                    Console.WriteLine("\nMeal Plan beginning with that date does not exist.\n");
                    ViewMealPlan();
                }
                var query = insertCommand.ExecuteReader();

                if (name != "ALL")
                    Console.WriteLine($"{query.GetName(0)}          {query.GetName(1)}    {query.GetName(2)}    {query.GetName(3)}");
                while (query.Read())
                {
                    if (name == "ALL")
                        Console.WriteLine($"{query.GetString(0)}");
                    else
                        Console.WriteLine($"{query.GetString(0)}    {query.GetString(1)}    {query.GetString(2)}    {query.GetString(3)}");
                    
                }

            }
            Console.WriteLine("\n");
            MealPlanMenu();
        }

        static void DeleteMealPlan()
        {
            Console.Write("Meal Plan Name: ");
            string name = "[" + Console.ReadLine() + "]";

            using var db = new SqliteConnection("Data Source=MealPlannerApp.db;Cache=Shared");
            {
                db.Open();
                var deleteCommand = new SqliteCommand("DROP TABLE IF EXISTS " + name, db);
                deleteCommand.ExecuteNonQuery();

                Console.WriteLine("\n" + name + " has been deleted.");

               
            }
            MealPlanMenu();
        }
        static void RecipeMenu()
        {
            Console.WriteLine("RECIPE MENU");
            Console.WriteLine("[1] - Add New Recipe.");
            Console.WriteLine("[2] - View Recipe.");
            Console.WriteLine("[3] - Delete Recipe.");
            Console.WriteLine("[0] - Main Menu.");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    AddRecipe();
                    break;
                case "2":
                    ViewRecipe();
                    break;
                case "3":
                    DeleteRecipe();
                    break;
                case "0":
                    MainMenu();
                    break;
                default:
                    Console.WriteLine("Select Menu Number.");
                    break;
            }
        }

        static void AddRecipe()
        {
            Console.Write("Recipe Name: ");
            string name = Console.ReadLine();
            string id;
            Console.WriteLine("Enter the meal (breakfast, lunch, dinner) this recipe is normally for.");
            string meal = Console.ReadLine();

            using var db = new SqliteConnection("Data Source=MealPlannerApp.db;Cache=Shared");
            {
                db.Open();
                var countCommand = new SqliteCommand("SELECT COUNT(id) FROM RecipeList", db);
                var query = countCommand.ExecuteReader();
                int idCount = 1;
                while (query.Read())
                {
                    idCount += query.GetInt32(0);
                    
                }
                id = string.Format("{0:000}", idCount);
                var insertCommand = new SqliteCommand("INSERT INTO RecipeList(id, name, meal) VALUES (@id, @name, @meal)", db);
                insertCommand.Parameters.AddWithValue("@id", id);
                insertCommand.Parameters.AddWithValue("@name", name);
                insertCommand.Parameters.AddWithValue("@meal", meal);

                insertCommand.ExecuteNonQuery();

                Console.WriteLine("\nRecipe added.");

            }
            Console.WriteLine("\nWould you like to add ingredients?");
            Console.WriteLine("[1] - Yes.");
            Console.WriteLine("[2] - No.");
            string choice = Console.ReadLine();
            if (choice == "1")
                AddIngredient(id);
            else if (choice == "2")
                RecipeMenu();
        }

        static void AddIngredient(string id)
        {
            Console.WriteLine("Enter the Ingredient name first, you'll then be asked for the quantity, and then you'll be asked for the appropriate unit.");
            Console.WriteLine("Ingredient: ");
            string name = Console.ReadLine();
            Console.WriteLine("Quantity: ");
            string qty = Console.ReadLine();
            Console.WriteLine("Unit:");
            string unit = Console.ReadLine();

            using var db = new SqliteConnection("Data Source=MealPlannerApp.db;Cache=Shared");
            {
                db.Open();
                string cmd = "CREATE TABLE IF NOT EXISTS IngredientsList (recipe_id INTEGER, ingredient TEXT, quantity INTEGER, unit TEXT)";
                var createTable = new SqliteCommand(cmd, db);
                createTable.ExecuteNonQuery();
                string insert = "INSERT INTO IngredientsList(recipe_id, ingredient, quantity, unit) VALUES (@recipe_id, @ingredient, @quantity, @unit)";
                var insertCommand = new SqliteCommand(insert, db);
                insertCommand.Parameters.AddWithValue("@recipe_id", id);
                insertCommand.Parameters.AddWithValue("@ingredient", name);
                insertCommand.Parameters.AddWithValue("@quantity", qty);
                insertCommand.Parameters.AddWithValue("@unit", unit);

                insertCommand.ExecuteNonQuery();

                Console.WriteLine("Ingredient added.");
            
            }
            Console.WriteLine("\nWould you like to add another ingredient?");
            Console.WriteLine("[1] - Yes.");
            Console.WriteLine("[2] - No.");
            string choice = Console.ReadLine();
            if (choice == "1")
                AddIngredient(id);
            else if (choice == "2")
                RecipeMenu();

        }
        
        static void ViewRecipe()
        {
            Console.WriteLine("Search for recipe or type ALL to view all recipes.");
            string name = GetString("Recipe Name: ");
            string cmd = "SELECT id, name, ingredient, quantity, unit FROM RecipeList JOIN IngredientsList ON RecipeList.id = IngredientsList.recipe_id WHERE name = (VALUES(@name))";

            using var db = new SqliteConnection("Data Source=MealPlannerApp.db;Cache=Shared");
            {
                db.Open();
                if (name == "ALL")
                    cmd = "SELECT id, name FROM RecipeList";
                var selectCommand = new SqliteCommand(cmd, db);
                selectCommand.Parameters.AddWithValue("@name", name);
                var query = selectCommand.ExecuteReader();
                if (name == "ALL")
                    Console.WriteLine($"{query.GetName(0)}, {query.GetName(1)}");
                while (query.Read())
                {
                    if (name != "ALL")
                        Console.WriteLine($"{query.GetString(3)} {query.GetString(4)} {query.GetString(2)}");
                    else
                        Console.WriteLine($"{query.GetString(0)} {query.GetString(1)}");
                    
                }
                Console.WriteLine("\n\n\n");
            
            }
            RecipeMenu();
        }

        static void DeleteRecipe()
        {
            string name = GetString("Recipe Name: ");

            using var db = new SqliteConnection("Data Source=MealPlannerApp.db;Cache=Shared");
            {
                db.Open();
                var deleteCommand = new SqliteCommand("DELETE FROM RecipeList WHERE name = (VALUES (@name))", db);
                deleteCommand.Parameters.AddWithValue("@name", name);
                deleteCommand.ExecuteNonQuery();

                Console.WriteLine("\n" + name + " has been deleted.");

            
            }
            RecipeMenu();
        }

        static void GroceryMenu()
        {
            Console.WriteLine("GROCERY LIST MENU");
            Console.WriteLine("[1] - Add New Grocery List.");
            Console.WriteLine("[2] - View Grocery List.");
            Console.WriteLine("[3] - Delete Grocery List.");
            Console.WriteLine("[0] - Main Menu.");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    AddGrocery();
                    break;
                case "2":
                    ViewGrocery();
                    break;
                case "3":
                    DeleteGrocery();
                    break;
                case "0":
                    MainMenu();
                    break;
                default:
                    Console.WriteLine("Select Menu Number.");
                    break;
            }
        }

        struct Ingredient
        {
            public string name;
            public float quantity;
            public string unit;
        }
        static void AddGrocery()
        {

            Console.WriteLine("Enter the date (MM/DD/YYYY) of the Meal Plan you would like to generate a Grocery List for");
            var mealsList = new List<string>();
            var ingredientsList = new List<Ingredient>();
            var groceryList = new List<Ingredient>();
            string dateString = Console.ReadLine();
            DateTime date;

            if (!DateTime.TryParse(dateString, out date))
            {
                Console.WriteLine("Value entered is not a valid date.");
                AddGrocery();
            }

            string name = $"[GroceryList{date:d}]";
            string mealPlan = $"[MealPlan{date:d}]";

            using var db = new SqliteConnection("Data Source=MealPlannerApp.db;Cache=Shared");
            {
                db.Open();
                string cmd = "CREATE TABLE IF NOT EXISTS" + name + "(quantity INTEGER, unit TEXT, ingredient TEXT)";
                var createTable = new SqliteCommand(cmd, db);
                createTable.ExecuteNonQuery();
                string getMealsCommand = "SELECT breakfast, lunch, dinner FROM " + mealPlan;
                var getMeals = new SqliteCommand(getMealsCommand, db);
                var getMealsQuery = getMeals.ExecuteReader();
                while (getMealsQuery.Read())
                {
                    mealsList.Add(getMealsQuery.GetString(0));
                    mealsList.Add(getMealsQuery.GetString(1));
                    mealsList.Add(getMealsQuery.GetString(2));
                }
                for (int i = 0; i < mealsList.Count; i++)
                {
                    string getIngredientsCommand = $"SELECT id, name, ingredient, quantity, unit FROM RecipeList JOIN IngredientsList ON RecipeList.id = IngredientsList.recipe_id WHERE RecipeList.name = (VALUES(@name))";
                    var getIngredients = new SqliteCommand(getIngredientsCommand, db);
                    getIngredients.Parameters.AddWithValue("@name", mealsList[i]);
                    var getIngredientsQuery = getIngredients.ExecuteReader();
                    while (getIngredientsQuery.Read())
                    {
                        Ingredient temp = new Ingredient();
                        temp.name = getIngredientsQuery.GetString(2);
                        temp.quantity = getIngredientsQuery.GetInt32(3);
                        temp.unit = getIngredientsQuery.GetString(4);
                        ingredientsList.Add(temp);
                    }
                }

                for (int j = 0; j < ingredientsList.Count; j++)
                {
                    bool isInGrocery = false;
                    if (groceryList.Count == 0)
                    {
                        groceryList.Add(ingredientsList[j]);
                    }
                    for (int k = 0; k < groceryList.Count; k++)
                    {
                        if (ingredientsList[j].name == groceryList[k].name && ingredientsList[j].unit == groceryList[k].unit)
                        {
                            isInGrocery = true;
                            Ingredient temp = new Ingredient();
                            temp.name = groceryList[k].name;
                            temp.quantity = groceryList[k].quantity + ingredientsList[j].quantity;
                            temp.unit = groceryList[k].unit;
                            groceryList[k] = temp;
                        }
                    }
                    if (!isInGrocery)
                        groceryList.Add(ingredientsList[j]);

                }

                for (int l = 0; l < groceryList.Count; l++)
                {
                    Console.WriteLine(groceryList[l].name + "   " + groceryList[l].quantity + "   " + groceryList[l].unit);
                    var addGrocery = new SqliteCommand("INSERT INTO " + name + "(ingredient, quantity, unit) VALUES(@ingredient, @quantity, @unit)", db);
                    addGrocery.Parameters.AddWithValue("@ingredient", groceryList[l].name);
                    addGrocery.Parameters.AddWithValue("@quantity", groceryList[l].quantity);
                    addGrocery.Parameters.AddWithValue("@unit", groceryList[l].unit);
                    addGrocery.ExecuteNonQuery();
                }

            }
            Console.WriteLine("\n\nWould you like to add another ingredient?");
            Console.WriteLine("[1] - Yes.");
            Console.WriteLine("[2] - No.");
            string choice = Console.ReadLine();

            if (choice == "1")
                AddItem(name);
            else
                GroceryMenu ();


        }

        static void AddItem(string listName)
        {
            Console.Write("Ingredient: ");
            string name = Console.ReadLine();
            Console.Write("Quantity: ");
            string quantity = Console.ReadLine();
            Console.Write("Unit: ");
            string unit = Console.ReadLine();
            using var db = new SqliteConnection("Data Source=MealPlannerApp.db;Cache=Shared");
            {
                db.Open();
                string cmd = "INSERT INTO" + listName + "(ingredient, quantity, unit) VALUES(@ingredient, @quantity, @unit)";
                var insertCmd = new SqliteCommand(cmd, db);
                insertCmd.Parameters.AddWithValue("@ingredient", name);
                insertCmd.Parameters.AddWithValue("@quantity", quantity);
                insertCmd.Parameters.AddWithValue("@unit", unit);

                insertCmd.ExecuteNonQuery();
            }
            Console.WriteLine(name + " added.");
            Console.WriteLine("\n\nWould you like to add another ingredient?");
            Console.WriteLine("[1] - Yes.");
            Console.WriteLine("[2] - No.");
            string choice = Console.ReadLine();

            if (choice == "1")
                AddItem(listName);
            else
                GroceryMenu();
        }

        static void ViewGrocery()
        {
            Console.WriteLine("Enter the date (MM/DD/YYYY) of the Grocery List you would like to view.");
            Console.WriteLine("Or type ALL to view a list of all Grocery Lists");
            string dateString = Console.ReadLine();
            string name = dateString;
            if (dateString != "ALL")
            {
                DateTime date;
                if (!DateTime.TryParse(dateString, out date))
                {
                    Console.WriteLine("Value entered is not a valid date.");
                    ViewMealPlan();
                }

                name = $"[GroceryList{date:d}]";
            }
            using var db = new SqliteConnection("Data Source=MealPlannerApp.db;Cache=Shared");
            {
                db.Open();
                var selectCmd = new SqliteCommand("SELECT ingredient, quantity, unit FROM" + name, db);
                if (dateString == "ALL")
                    selectCmd = new SqliteCommand("SELECT name FROM sqlite_master WHERE type = 'table' AND name LIKE 'GroceryList%' ORDER BY 1", db);
                try
                {
                    selectCmd.ExecuteNonQuery();
                }
                catch(SqliteException)
                {
                    Console.WriteLine("No Grocery List with that date exists");
                    ViewGrocery();
                }

                var query = selectCmd.ExecuteReader();
                while (query.Read())
                {
                    if (dateString == "ALL")
                        Console.WriteLine(query.GetString(0));
                    else
                        Console.WriteLine(query.GetString(0) + "   " + query.GetString(1) + "   " + query.GetString(2));
                }
            }
            if (dateString != "ALL")
            {
                Console.WriteLine("\n\nWould you like to add another ingredient?");
                Console.WriteLine("[1] - Yes.");
                Console.WriteLine("[2] - No.");
                string choice = Console.ReadLine();

                if (choice == "1")
                    AddItem(name);

            }
            GroceryMenu();
        }

        static void DeleteGrocery()
        {
            Console.Write("Grocery List Name: ");
            string name = "[" + Console.ReadLine() + "]";

            using var db = new SqliteConnection("Data Source=MealPlannerApp.db;Cache=Shared");
            {
                db.Open();
                var deleteCommand = new SqliteCommand("DROP TABLE IF EXISTS " + name, db);
                deleteCommand.ExecuteNonQuery();

                Console.WriteLine("\n" + name + " has been deleted.");


            }
            GroceryMenu();
        }

        static void WriteFile()
        {
            Console.WriteLine("Would you like to export a Meal Plan or Grocery List?");
            Console.WriteLine("[1] - Meal Plan.");
            Console.WriteLine("[2] - Grocery List.");
            Console.WriteLine("[0] - Main Menu.");
            string choice = Console.ReadLine();
            string name = "MealPlan";

            switch (choice)
            {
                case "1":
                    name = "MealPlan";
                    break;
                case "2":
                    name = "GroceryList";
                    break;
                case "3":
                    MainMenu();
                    break;
                default:
                    Console.WriteLine("Please select menu number.");
                    WriteFile();
                    break;
            }

            Console.WriteLine("Please enter the date (MM/DD/YYYY) of the " + name + "you wan to export.");
            string dateString = Console.ReadLine();
            DateTime date;
            if (!DateTime.TryParse(dateString, out date))
            {
                Console.WriteLine("value is not a valid date.");
                WriteFile();
            }
            string queryName = "[" + name + $"{date:d}]";

            using var db = new SqliteConnection("Data Source=MealPlannerApp.db;Cache=Shared");
            {
                db.Open();
                var selectCmd = new SqliteCommand("SELECT * FROM" + queryName, db);
                var reader = selectCmd.ExecuteReader();

                string fileName = "temp.csv";
                StreamWriter sw = new StreamWriter(fileName);
                object[] output = new object[reader.FieldCount];

                for (int i = 0; i < reader.FieldCount; i++)
                    output[i] = reader.GetName(i);
                sw.WriteLine(string.Join(",", output));

                while (reader.Read())
                {
                    reader.GetValues(output);
                    sw.WriteLine(string.Join(",", output));
                }

                sw.Close();
                reader.Close();

            }
            Console.WriteLine(queryName + "has been exported.");
            MainMenu();
        }

    }
}
