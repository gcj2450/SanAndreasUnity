using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static LearnCSharpInOneHour;

public class Chef
{
    public void MakeChiken()
    {
        Console.WriteLine("The Chef makes chicken");
    }

    public void MakeSalad()
    {
        Console.WriteLine("The Chef makes salad");
    }

    //Virtual means this method can be override in any sub-class
    public virtual void MakeSpecialDish()
    {
        Console.WriteLine("The Chef makes bbq ribs");
    }
}

public class ItalianChef : Chef
{
    public void MakePasta()
    {
        Console.WriteLine("The Chef makes pasta");
    }

    //Override MakeSpecialDish
    public override void MakeSpecialDish()
    {
        Console.WriteLine("The Chef makes chicken parm");
    }
}

/// <summary>
/// 委托，考试时间快要到的Handler
/// </summary>
/// <returns></returns>
public delegate void  TestTimeComingHander(string _contents);

/// <summary>
/// 委托，考试时间到的Handler
/// </summary>
public delegate void TestTimeUpHander();

/// <summary>
/// 教师类
/// </summary>
public class Teacher
{
    public event TestTimeComingHander TestTimeComing;

    public event TestTimeUpHander TestTimeUp;

    public Action TestTimeAction;

    /// <summary>
    /// 学校发通知，考试时间快到了
    /// </summary>
    /// <param name="_content"></param>
    public void OnTestTimeComing(string _content)
    {
        if (TestTimeComing!=null)
        {
            TestTimeComing(_content);
        }
    }

    /// <summary>
    /// 考试时间到了
    /// </summary>
    public void OnTestTimeUp()
    {
        if (TestTimeUp!=null)
        {
            TestTimeUp();
        }

        if (TestTimeAction!=null)
        {
            TestTimeAction();
        }
    }

}

/// <summary>
/// 学生类
/// </summary>
public class ExamStudent
{
    public string Name { get; private set; }

    public ExamStudent(string name)
    {
        this.Name = name;
    }

    public void HandInQingJia(string _contents)
    {
        Console.WriteLine($"{this.Name}\' _contents");
        Console.WriteLine($"{this.Name}\'要考试了，我要请假");
    }

    public void HandInTestPaper()
    {
        Console.WriteLine($"{this.Name}\' paper has been handed in");
    }

}
