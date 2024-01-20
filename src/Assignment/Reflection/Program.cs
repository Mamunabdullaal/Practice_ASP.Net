using System;
using System.Collections.Generic;
using System.Linq;


    
        House house = new House
        {
            
            Rooms = new List<Room>
    {
        new Room
        {
            RoomNumber = "200",
            Windows = new List<Window>
            {
                new Window { Width = 200, Height = 300 },
                new Window { Width = 44, Height = 88 }
            }
        },
        new Room
        {
            RoomNumber = "300",
            Windows = new List<Window>
            {
                new Window { Width = 100, Height = 200 },
                new Window { Width = 350, Height = 500 }
            }
        }
    }, BuildingHistory = new List<string> { "2012"}

        };

       
        Building building = new Building();
        SimpleMapper mapper = new SimpleMapper();
        mapper.Copy(house, building);


        // Print information about the building and its nested properties
        
        Console.WriteLine("BuildingHistory in House: " + string.Join(", ", house.BuildingHistory));

        foreach (var room in building.Rooms)
        {
            Console.WriteLine("Room Number: " + room.RoomNumber);

            foreach (var window in room.Windows)
            {
                Console.WriteLine("Window - Width: " + window.Width + ", Height: " + window.Height);
            }
        }

        // Create a User object to check it will handle primitive and non primitive data type properties
        var user = new User
        {
            Username = "Abdulla Al Mamun",
            Address = "Dhaka, Bangladesh",
            Name = "Abdulla Al ",
            Lastname = "Mamun",
            Friends = new List<string> { "Adnan", "Don" }
        };

        // Create a Person object
        var person = new Person();
        mapper.Copy(user, person);

        // Print information about the person and its properties
        Console.WriteLine("\nPerson:");
        Console.WriteLine(person.Name);
        Console.WriteLine(person.Lastname);
        Console.WriteLine("\nFriends:");
        foreach (var friend in person.Friends)
        {
            Console.WriteLine(friend);
        }

        // Create a School object and populate its properties and check array of object
        School school = new School
        {
            TeacherNames = new string[] { "Teacher1", "Teacher2", "Teacher3" },
            StudentNames = new string[] { "Student1", "Student2", "Student3" }
        };

        // Create Teacher, Student, and Staff objects
        Teacher teacher = new Teacher();
        Student student = new Student();
        mapper.Copy(school, teacher);
        mapper.Copy(school, student);


        // Print the mapped array properties
        Console.WriteLine("\nTeacher Names:");
        foreach (var name in teacher.TeacherNames)
        {
            Console.WriteLine(name);
        }

        Console.WriteLine("\nStudent Names:");
        foreach (var name in student.StudentNames)
        {
            Console.WriteLine(name);
        }

        // Pause to see the output
        Console.ReadLine();
    

public class User
{
    public string Username { get; set; }
    public string Address { get; set; }
    public string Name { get; set; }
    public string Lastname { get; set; }
    public List<string> Friends { get; set; }
}

public class Person
{
    public string Name { get; set; }
    public string Lastname { get; set; }
    public List<string> Friends { get; set; }
}

public class Building
{
    public string BuildingNumber { get; set; }
    public int HouseNumber { get; set; }
    public List<Room> Rooms { get; set; }
    public List<string> BuildingHistory { get; set; }
    public List<string> HouseNames { get; set; }
}
public class House
{
    public int BuildingNumber { get; set; }
    public List<Room> Rooms { get; set; }
    public List<string> BuildingHistory { get; set; }


}

public class Room
{
    public string RoomNumber { get; set; }
    public List<Window> Windows { get; set; }
}

public class Window
{
    public int Width { get; set; }
    public int Height { get; set; }

    
}
public class School
{
    public string[] TeacherNames { get; set; }
    public string[] StudentNames { get; set; }
}

public class Teacher
{
    public string[] TeacherNames { get; set; }
}

public class Student
{
    public string[] StudentNames { get; set; }
}

public class Staff
{
    public string[] StaffNames { get; set; }
}
