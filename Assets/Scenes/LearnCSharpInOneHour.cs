using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Data;

public class LearnCSharpInOneHour : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    /*C#是什么，C#是一种编程语言，用来开发计算机程序的工具，可以用它编写各种各样的程序，手机程序，电脑程序等等。
     * 写好一段C#代码之后，需要将它变成程序，变成程序的过程叫编译，用来编译代码的程序叫IDE。就是能生程序的程序。
     * 
     * 电脑里的程序是由很多个类和结构体组成的，很多个类共同运行，才能保证程序正常运行。
     * 一个类由很多个变量和方法组成，类的变量就相当于这个类有啥特点，类的方法就相当于这个类能干啥。
     * 变量可以是很多种类型：byte, char, string, int, float, double, bool等。
     * 方法的返回类型分为两类，一类是没有返回值，方法名前加void, 一类有返回值，
     * 在方法名前指定返回值的类型，在方法里返回和指定的类型一样的变量，比如返回类型指定int, 在方法里就要return 一个int值。
    */

    /// <summary>
    /// 01_画一个形状
    /// </summary>
    void DrawAShape()
    {
        //Order matters
        Console.WriteLine("     /|");
        Console.WriteLine("    / |");
        Console.WriteLine("   /  |");
        Console.WriteLine("  /   |");
        Console.WriteLine(" /    |");
        Console.WriteLine("/_____|");
    }

    /// <summary>
    /// 02_变量
    /// </summary>
    void Varialbles()
    {
        string characterName = "John";
        int characterAge;
        characterAge = 35;

        Console.WriteLine("There once was a man named " + characterName);
        Console.WriteLine("He was " + characterAge + " years old");
        Console.WriteLine("He really liked the name George");
        Console.WriteLine("But didn't like being " + characterAge);
    }

    /// <summary>
    /// 03_数据类型
    /// </summary>
    void DataTypes()
    {
        string phrase = "Giraffe Academy";
        char grade = 'A';
        int age = 30;
        float fenshu = 59.5f;
        double gpa = 3.3;
        bool isMale = true;
    }

    /// <summary>
    /// 04_字符串操作
    /// </summary>
    void WorkingWithStrings()
    {
        string phrase = "Giraffe Academy " + "is cool!";
        //Console.WriteLine(phrase.Length);
        //Console.WriteLine(phrase.ToUpper());
        //Console.WriteLine(phrase.Contains('s'));
        //Console.WriteLine(phrase[1]);
        //Console.WriteLine(phrase.IndexOf('f'));
    }

    /// <summary>
    /// 05_数的操作
    /// </summary>
    void WorkingWithNumbers()
    {
        //Console.WriteLine( 2 + 8 );
        //Console.WriteLine( 8 / 6 );
        //Console.WriteLine( (4 + 2) * 3 );
        //Console.WriteLine(5 + 8.1);

        //int num = 6;
        //num++;
        //Console.WriteLine(num);
        //Console.WriteLine(Math.Pow(3, 2));
        Console.WriteLine(Math.Max(3, 2));
    }

    /// <summary>
    /// 06_获取用户输入
    /// </summary>
    void GettingUserInput()
    {
        Console.Write("Enter your name: ");
        string name = Console.ReadLine();
        Console.Write("Enter your age: ");
        string age = Console.ReadLine();

        Console.WriteLine("Hey " + name + " you are " + age);
    }

    /// <summary>
    /// 07_制作一个计算器
    /// </summary>
    void BuildingACalculator()
    {
        double numOne;
        double numTwo;

        Console.Write("Enter a number: ");
        numOne = Convert.ToDouble(Console.ReadLine());

        Console.Write("Enter another number: ");
        numTwo = Convert.ToDouble(Console.ReadLine());

        Console.WriteLine(numOne + numTwo);
    }

    /// <summary>
    /// 08_制作一个疯狂的库
    /// </summary>
    void BuildAMadLib()
    {
        string color, pluralNoun, celebrity;

        Console.Write("Enter a color: ");
        color = Console.ReadLine();

        Console.Write("Enter a plural noun: ");
        pluralNoun = Console.ReadLine();

        Console.Write("Enter a celebrity: ");
        celebrity = Console.ReadLine();


        Console.WriteLine("Roses are " + color);
        Console.WriteLine(pluralNoun + " are blue");
        Console.WriteLine("I Love " + celebrity);
    }

    /// <summary>
    /// 09_数组
    /// </summary>
    void LearnArrays()
    {
        int[] luckyNumbers = { 1, 2, 3 };
        string[] friends = new string[5];

        friends[0] = "Jim";
        friends[1] = "Kelly";
        friends[2] = "Josh";
        friends[3] = "Jess";
        friends[4] = "Sophie";

        luckyNumbers[2] = 900;
        Console.WriteLine(luckyNumbers[2]);
    }

    /// <summary>
    /// 10_方法
    /// </summary>
    void LearnMethods()
    {
        SayHi("Ricardo", 27);
    }
    void SayHi(string name, int age)
    {
        Console.WriteLine("Hi " + name + " you are " + age);
    }

    /// <summary>
    /// 11_返回语句
    /// </summary>
    void ReturnStatement()
    {
        int cubedNumber = cube(5);
        Console.WriteLine(cubedNumber);
    }
    int cube(int num)
    {
        int result = num * num * num;
        return result;
    }

    /// <summary>
    /// 12_if判断语句
    /// </summary>
   void IfStatements()
    {
        bool isMale = true;
        bool isTall = false;

        if (isMale && isTall)
        {

            Console.WriteLine("You're a tall male.");

        }
        else if (isMale && !isTall)
        {
            Console.WriteLine("You're a short male");
        }
        else if (!isMale && isTall)
        {
            Console.WriteLine("You are not male but you're tall");
        }
        else
        {
            Console.WriteLine("You're not male and not tall");
        }
    }

    /// <summary>
    ///13_ if语句比大小
    /// </summary>
    void IfStatementsCopare()
    {
        Console.WriteLine(GetMax(2, 10, 50));
    }
    static int GetMax(int num1, int num2, int num3)
    {
        int result;

        if (num1 >= num2 && num1 >= num3)
        {
            result = num1;
        }
        else if (num2 >= num1 && num2 >= num3)
        {
            result = num2;
        }
        else
        {
            result = num3;
        }

        return result;
    }

    /// <summary>
    /// 14_制作一个更好的计算器
    /// </summary>
    void BuildABetterCalculator()
    {
        Console.Write("Enter a number: ");
        double num1 = Convert.ToDouble(Console.ReadLine());

        Console.Write("Enter a Operator: ");
        string op = Console.ReadLine();

        Console.Write("Enter a number: ");
        double num2 = Convert.ToDouble(Console.ReadLine());


        if (op == "+")
        {
            Console.WriteLine(num1 + num2);
        }
        else if (op == "-")
        {
            Console.WriteLine(num1 - num2);
        }
        else if (op == "/")
        {
            Console.WriteLine(num1 / num2);
        }
        else if (op == "*")
        {
            Console.WriteLine(num1 * num2);
        }
        else
        {
            Console.WriteLine("Invalid operator");
        }
    }

    /// <summary>
    /// 15_Switch语句
    /// </summary>
    void SwitchYuJu()
    {
        Console.Write("Enter a day number: ");
        int dayNumber = Convert.ToInt32(Console.ReadLine());
        Console.WriteLine(GetDay(dayNumber));
    }
    static string GetDay(int dayNum)
    {
        string dayName;

        switch (dayNum)
        {
            case 0:
                dayName = "Sunday";
                break;
            case 1:
                dayName = "Monday";
                break;
            case 2:
                dayName = "Tuesday";
                break;
            case 3:
                dayName = "Wednesday";
                break;
            case 4:
                dayName = "Thursday";
                break;
            case 5:
                dayName = "Friday";
                break;
            case 6:
                dayName = "Saturday";
                break;
            default:
                dayName = "Invalid Day Number";
                break;
        }

        return dayName;
    }

    /// <summary>
    /// 16_While循环
    /// </summary>
    void WhileLoops()
    {
        int index = 0;

        //while (index <= 10) {
        //    Console.WriteLine(index);
        //    index++;
        //}

        do
        {
            index++;
            Console.WriteLine(index);

        }
        while (index <= 10);
    }

    /// <summary>
    /// 17_做一个猜谜游戏
    /// </summary>
    void BuildingAGuessGame()
    {
        string secretWord = "giraffe";
        string guess = "";
        int guessLimit = 5;

        do
        {
            Console.WriteLine("You have " + guessLimit + " chances remaining");
            Console.Write("Enter a guess: ");
            guess = Console.ReadLine().ToLower();
            guessLimit--;
            Console.Clear();

        } while (guess != secretWord && guessLimit != 0);


        if (secretWord == guess)
        {
            Console.WriteLine("Congratulations you won!");
            return;
        }

        if (guessLimit == 0)
        {
            Console.WriteLine("Game Over");
            return;

        }
    }

    /// <summary>
    /// 18_For循环语句
    /// </summary>
    void ForLoops()
    {
        int[] luckyNumbers = { 4, 8, 15, 16, 23, 42 };

        for (int i = 0; i <= luckyNumbers.Length; i++)
        {
            Console.WriteLine(luckyNumbers[i]);
        }
    }

    /// <summary>
    /// 19_做一个次方计算器
    /// </summary>
    void BuildAExponentMethod()
    {
        Console.WriteLine(GetPow(3, 2));
    }
    static int GetPow(int baseNum, int powNum)
    {
        int result = 1;

        for (int i = 0; i < powNum; i++)
        {
            result = result * baseNum;
        }

        return result;
    }

    /// <summary>
    /// 20_二维数组
    /// </summary>
    void TwoDArrays()
    {
        int[,] numberGrid = {
                {1, 2},
                {3, 4 }, //1 1
                {5, 6 },
            };


        Console.WriteLine(numberGrid[1, 1]);
    }

    /// <summary>
    /// 21_异常处理
    /// </summary>
    void ExceptionHandles()
    {
        try
        {
            int numOne;
            int numTwo;

            Console.Write("Enter a number: ");
            numOne = Convert.ToInt32(Console.ReadLine());

            Console.Write("Enter another number: ");
            numTwo = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine(numOne / numTwo);
        }
        catch (DivideByZeroException e)
        {
            Console.WriteLine(e.Message);
        }
        catch (FormatException e)
        {
            Console.WriteLine(e.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        finally
        {
            Console.WriteLine("Bye");
        }
    }

    /// <summary>
    /// 22_类和物体(或者叫对象，Object)
    /// </summary>
    void ClassesAndObjects()
    {
        Book bookOne = new Book();
        bookOne.title = "Harry Potter";
        bookOne.author = "J. K. Rowling";
        bookOne.pages = 400;

        Book bookTwo = new Book();
        bookTwo.title = "Lord of the Rings";
        bookTwo.author = "Tolkein";
        bookTwo.pages = 700;

        Console.WriteLine(bookOne.title);
        Console.WriteLine(bookTwo.title);
    }

    /// <summary>
    /// 23_构造方法
    /// </summary>
    void Constructors()
    {
        CustructorBook bookOne = new CustructorBook("Harry Potter", "J.K Rowling", 300);

        CustructorBook bookTwo = new CustructorBook("Lord Of the Rings", "Tolkein", 700);

        Console.WriteLine(bookOne.title);
        Console.WriteLine(bookTwo.title);
    }

    /// <summary>
    /// 24_对象的方法
    /// </summary>
    void ObjectMethods()
    {
        Student StudentOne = new Student("Jim", "Business", 2.8);
        Student StudentTwo = new Student("Pam", "Art", 3.6);


        Console.WriteLine(StudentOne.HasHonors());
        Console.WriteLine(StudentTwo.HasHonors());
    }

    /// <summary>
    /// 25_属性
    /// </summary>
    void GettersAndSetters()
    {
        // G, PG, PG-13, R, NR
        Movie avengers = new Movie("The Avengers", "Joss", "PG-13");
        Movie shrek = new Movie("Shrek", "Adam Adamson", "PG");

        avengers.Rating = "Cat";

        Console.WriteLine(avengers.Rating);
    }

    /// <summary>
    /// 26_类的静态属性
    /// </summary>
    void StaticClassAttributes()
    {
        Song holiday = new Song("Holiday", "Green Day", 200);
        Song kasmir = new Song("Kashmir", "Led Zeppelin", 150);

        Console.WriteLine("Was created " + Song.songCount + " sounds.");
        Console.WriteLine(kasmir.getSongCount());
    }

    /// <summary>
    /// 27_
    /// </summary>
    void StaticMethodsAndClasses()
    {
        //If class is defined as static, is not allowed to create an instance of it.
        //UsefulTools tools = new UsefulTools();

        //With static methods we can use that method without create an instance of this class.
        UsefulTools.SayHi("Ricardo");
    }

    /// <summary>
    /// 28_继承
    /// </summary>
    void Inheritance()
    {
        Chef chef = new Chef();
        chef.MakeChiken();

        //Italian Chef can do everything that normal Chef can do and can also do another stuff.
        ItalianChef italianChef = new ItalianChef();
        italianChef.MakeChiken();
        italianChef.MakePasta();
        italianChef.MakeSpecialDish();
    }

    /// <summary>
    /// 29_委托和事件
    /// </summary>
    void DelegateEvent()
    {
        Teacher teacher = new Teacher();
        var tom = new ExamStudent("Tom");
        var jerry = new ExamStudent("Jerry");
        var spark = new ExamStudent("Spark");
        var tyke = new ExamStudent("Tyke");

        // 订阅teacher的TestTimeUp事件
        teacher.TestTimeUp += tom.HandInTestPaper;
        teacher.TestTimeUp += jerry.HandInTestPaper;

        teacher.TestTimeComing += spark.HandInQingJia;

        // invoke TestTimeUp 事件
        teacher.OnTestTimeComing("大家做好准备，定于明天下午两点考试");
        teacher.OnTestTimeUp();

        Console.WriteLine();

        teacher.TestTimeUp -= tom.HandInTestPaper;
        teacher.TestTimeUp += tyke.HandInTestPaper;

        teacher.OnTestTimeUp();

        Console.ReadKey();
    }
}

public class Book
{
    public string title;
    public string author;
    public int pages;
}

public class CustructorBook
{
    public string title;
    public string author;
    public int pages;

    public CustructorBook(string aTitle, string aAuthor, int aPages)
    {
        title = aTitle;
        author = aAuthor;
        pages = aPages;
    }
}

public class Student
{
    public string name;
    public string major;
    public double gpa;

    public Student(string aName, string aMajor, double aGpa)
    {
        name = aName;
        major = aMajor;
        gpa = aGpa;
    }

    public bool HasHonors()
    {
        return gpa >= 2.5 ? true : false;
    }
}

public class Movie
{
    public string title;
    public string director;
    private string rating;

    public Movie(string aTitle, string aDirector, string aRating)
    {
        title = aTitle;
        director = aDirector;
        Rating = aRating;
    }

    public string Rating
    {
        get { return rating; }
        set
        {
            if (value == "G" || value == "PG" || value == "PG-13" || value == "R" || value == "NR")
            {
                rating = value;
            }
            else
            {
                rating = "NR";
            }

        }
    }
}

public class Song
{
    public string title;
    public string artist;
    public int duration;
    public static int songCount = 0;

    public Song(string aTitle, string aArtist, int aDuration)
    {
        title = aTitle;
        artist = aArtist;
        duration = aDuration;
        songCount++;
    }

    public int getSongCount()
    {
        return songCount;
    }
}

static class UsefulTools
{
    public static void SayHi(string name)
    {
        Console.WriteLine("Hello " + name);
    }
}
